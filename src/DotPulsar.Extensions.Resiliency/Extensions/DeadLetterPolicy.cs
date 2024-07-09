// /*
//  * Licensed under the Apache License, Version 2.0 (the "License")
//  * you may not use this file except in compliance with the License.
//  * You may obtain a copy of the License at
//  *
//  *   http://www.apache.org/licenses/LICENSE-2.0
//  *
//  * Unless required by applicable law or agreed to in writing, software
//  * distributed under the License is distributed on an "AS IS" BASIS,
//  * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  * See the License for the specific language governing permissions and
//  * limitations under the License.
//  */

using System.Buffers;
using System.Globalization;
using DotPulsar.Abstractions;
using Polly;

namespace DotPulsar.Extensions;

public class DeadLetterPolicy : IDeadLetterPolicy, IAsyncDisposable
{
	// From https://github.com/apache/pulsar/blob/master/pulsar-client/src/main/java/org/apache/pulsar/client/util/RetryMessageUtil.java
	public const string ReconsumeTimeMetadataKey = "RECONSUMETIMES";
	public const string DelayTimeMetadataKey = "DELAY_TIME";
	public const string RealTopicMetadataKey = "REAL_TOPIC";
	public const string RealSubscriptionMetadataKey = "REAL_SUBSCRIPTION";
	public const string OriginMessageIdMetadataKey = "ORIGIN_MESSAGE_ID";
	public const int DefaultMaxReconsumeTimes = 16;
	public const string RetryTopicSuffix = "-RETRY";
	public const string DeadLetterTopicSuffix = "-DLQ";

	private readonly Lazy<IProducer<ReadOnlySequence<byte>>> deadProducer;
	private readonly Lazy<IProducer<ReadOnlySequence<byte>>>? retryProducer;

	public DeadLetterPolicy(
		IProducerBuilder<ReadOnlySequence<byte>> deadLetterProducerBuilder,
		IProducerBuilder<ReadOnlySequence<byte>>? retryLetterProducerBuilder = null,
		int maxRedeliveryCount = 16,
		TimeSpan? retryDelay = null,
		ResiliencePipeline? resiliencePipeline = null) {
		ArgumentNullException.ThrowIfNull(deadLetterProducerBuilder);

		deadProducer = new Lazy<IProducer<ReadOnlySequence<byte>>>(() => deadLetterProducerBuilder.ToResilientProducer(resiliencePipeline));
		retryProducer = retryLetterProducerBuilder != null
			? new Lazy<IProducer<ReadOnlySequence<byte>>>(() => retryLetterProducerBuilder.ToResilientProducer(resiliencePipeline))
			: null;
		MaxRedeliveryCount = maxRedeliveryCount;
		RetryDelay = retryDelay;
	}

	public int MaxRedeliveryCount {
		get;
	}

	public TimeSpan? RetryDelay {
		get;
	}

	public async ValueTask DisposeAsync() {
		try {
			if (retryProducer != null && retryProducer.IsValueCreated) {
				await retryProducer.Value.DisposeAsync().ConfigureAwait(false);
			}
		} finally {
			if (deadProducer.IsValueCreated) {
				await deadProducer.Value.DisposeAsync().ConfigureAwait(false);
			}
		}
		GC.SuppressFinalize(this);
	}

	public async ValueTask ReconsumeLater(IMessage message, TimeSpan? delayTime = null, IEnumerable<KeyValuePair<string, string?>>? customProperties = null, CancellationToken cancellationToken = default) {
		ArgumentNullException.ThrowIfNull(message);

		var metadata = PrepareMetadata(message, delayTime ?? RetryDelay, customProperties);
		if (retryProducer != null) {
			var reconsumeTimes = GetReconsumeAndUpdate(metadata);
			if (reconsumeTimes <= MaxRedeliveryCount) {
				try {
					await retryProducer.Value.Send(metadata, message.Data, cancellationToken).ConfigureAwait(false);
					return;
#pragma warning disable CA1031
				} catch {
#pragma warning restore CA1031
					// TODO: what to do with the exception?
					// For now, just let it fall through and go to the dead letter queue
				}
			}
		}

		await deadProducer.Value.Send(metadata, message.Data, cancellationToken).ConfigureAwait(false);

		static MessageMetadata PrepareMetadata(IMessage message, TimeSpan? delayTime, IEnumerable<KeyValuePair<string, string?>>? customProperties) {
			var metadata = new MessageMetadata {
				EventTime = message.EventTime,
			};
			if (message.HasKey) {
				if (message.HasBase64EncodedKey) {
					metadata.KeyBytes = message.KeyBytes;
				} else {
					metadata.Key = message.Key;
				}
			}

			if (message.HasOrderingKey) {
				metadata.OrderingKey = message.OrderingKey;
			}

			foreach (var (key, value) in message.Properties) {
				metadata[key] = value;
			}

			if (message.Properties.TryGetValue(DelayTimeMetadataKey, out var delayValue) && int.TryParse(delayValue, out var delayMillis)) {
				delayTime = TimeSpan.FromMilliseconds(delayMillis);
			}

			if (delayTime != null) {
				metadata.DeliverAtTimeAsDateTimeOffset = DateTimeOffset.UtcNow.Add(delayTime.Value);
				metadata[DelayTimeMetadataKey] = delayTime.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
			}

			if (!message.Properties.TryGetValue(RealTopicMetadataKey, out var originTopicNameStr)) {
				originTopicNameStr = message.MessageId.Topic;
			}
			metadata[RealTopicMetadataKey] = originTopicNameStr;
			metadata[OriginMessageIdMetadataKey] = message.MessageId.ToString();

			if (customProperties != null) {
				foreach (var (key, value) in customProperties) {
					if (value != null) {
						metadata[key] = value;
					}
				}
			}

			return metadata;
		}

		static int GetReconsumeAndUpdate(MessageMetadata metadata) {
			if (!int.TryParse(metadata[ReconsumeTimeMetadataKey], out var reconsumeTimes)) {
				reconsumeTimes = 1;
			} else {
				reconsumeTimes++;
			}
			metadata[ReconsumeTimeMetadataKey] = reconsumeTimes.ToString(CultureInfo.InvariantCulture);
			return reconsumeTimes;
		}
	}
}
