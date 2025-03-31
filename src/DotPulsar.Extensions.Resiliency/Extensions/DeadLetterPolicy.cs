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
	public const string ExceptionTypeMetadataKey = "EXCEPTION_TYPE";
	public const string ExceptionMessageMetadataKey = "EXCEPTION_MESSAGE";
	public const string StackTraceMetadataKey = "STACK_TRACE";
	public const int DefaultMaxReconsumeTimes = 16;
	public const string RetryTopicSuffix = "-RETRY";
	public const string DeadLetterTopicSuffix = "-DLQ";

	private readonly Func<MessageMetadata, ReadOnlySequence<byte>, CancellationToken, ValueTask>? deadLetterProducer;
	private readonly Func<ValueTask>? disposeDeadLetterProducer;
	private readonly Func<MessageMetadata, ReadOnlySequence<byte>, CancellationToken, ValueTask>? retryProducer;
	private readonly Func<ValueTask>? disposeRetryProducer;

	public DeadLetterPolicy(
		Func<MessageMetadata, ReadOnlySequence<byte>, CancellationToken, ValueTask>? deadLetterProducer,
		Func<MessageMetadata, ReadOnlySequence<byte>, CancellationToken, ValueTask>? retryProducer = null,
		int maxRedeliveryCount = DefaultMaxReconsumeTimes,
		TimeSpan? retryDelay = null) {
		this.deadLetterProducer = deadLetterProducer;
		this.retryProducer = retryProducer;
		MaxRedeliveryCount = maxRedeliveryCount;
		RetryDelay = retryDelay;
	}

	public DeadLetterPolicy(
		IProducerBuilder<ReadOnlySequence<byte>>? deadLetterProducerBuilder = null,
		IProducerBuilder<ReadOnlySequence<byte>>? retryProducerBuilder = null,
		int maxRedeliveryCount = DefaultMaxReconsumeTimes,
		TimeSpan? retryDelay = null,
		ResiliencePipeline? resiliencePipeline = null) {
		if (deadLetterProducerBuilder != null) {
			var lazyDeadLetterProducer = new Lazy<IProducer<ReadOnlySequence<byte>>>(() => deadLetterProducerBuilder.CreateResilient(resiliencePipeline));
			disposeDeadLetterProducer = () => lazyDeadLetterProducer.IsValueCreated
				? lazyDeadLetterProducer.Value.DisposeAsync()
#if NET6_0_OR_GREATER
				: ValueTask.CompletedTask;
#else
				: default;
#endif
			deadLetterProducer = async (metadata, message, ct) => await lazyDeadLetterProducer.Value.Send(metadata, message, ct).ConfigureAwait(false);
		}

		if (retryProducerBuilder != null) {
			var lazyRetryProducer = new Lazy<IProducer<ReadOnlySequence<byte>>>(() => retryProducerBuilder.CreateResilient(resiliencePipeline));
			disposeRetryProducer = () => lazyRetryProducer.IsValueCreated
				? lazyRetryProducer.Value.DisposeAsync()
#if NET6_0_OR_GREATER
				: ValueTask.CompletedTask;
#else
				: default;
#endif
			retryProducer = async (metadata, message, ct) => await lazyRetryProducer.Value.Send(metadata, message, ct).ConfigureAwait(false);
		}

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
			if (disposeRetryProducer != null) {
				await disposeRetryProducer().ConfigureAwait(false);
			}
		} finally {
			if (disposeDeadLetterProducer != null) {
				await disposeDeadLetterProducer().ConfigureAwait(false);
			}
		}
		GC.SuppressFinalize(this);
	}

	public async ValueTask ReconsumeLater(IMessage message, TimeSpan? delayTime = null, IEnumerable<KeyValuePair<string, string?>>? customRetryProperties = null, IEnumerable<KeyValuePair<string, string?>>? customDlqProperties = null, bool preventRetry = false, CancellationToken cancellationToken = default) {
#if NET6_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(message);
#else
		if (message == null) {
			throw new ArgumentNullException(nameof(message));
		}
#endif

		var metadata = PrepareMetadata(message, delayTime ?? RetryDelay, customRetryProperties);
		if (retryProducer != null && !preventRetry) {
			var reconsumeTimes = GetReconsumeAndUpdate(metadata);
			if (reconsumeTimes <= MaxRedeliveryCount) {
				try {
					await retryProducer(metadata, message.Data, cancellationToken).ConfigureAwait(false);
					return;
#pragma warning disable CA1031
				} catch {
#pragma warning restore CA1031
					// TODO: what to do with the exception?
					// For now, just let it fall through and go to the dead letter queue
				}
			}
		}

		if (deadLetterProducer != null) {
			AddProperties(metadata, customDlqProperties);
			await deadLetterProducer(metadata, message.Data, cancellationToken).ConfigureAwait(false);
		}

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

			foreach (var property in message.Properties) {
				metadata[property.Key] = property.Value;
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

			AddProperties(metadata, customProperties);
			return metadata;
		}

		static void AddProperties(MessageMetadata metadata, IEnumerable<KeyValuePair<string, string?>>? customProperties) {
			if (customProperties != null) {
				foreach (var property in customProperties) {
					if (property.Value != null) {
						metadata[property.Key] = property.Value;
					}
				}
			}
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

	public static IEnumerable<KeyValuePair<string, string?>> SerializeException(Exception exception) {
#if NET6_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(exception);
#else
		if (exception == null) {
			throw new ArgumentNullException(nameof(exception));
		}
#endif
		yield return new(ExceptionTypeMetadataKey, exception.GetType().FullName);
		yield return new(ExceptionMessageMetadataKey, exception.Message);
		yield return new(StackTraceMetadataKey, exception.StackTrace);
	}
}
