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

using DotPulsar.Abstractions;
using DotPulsar.Exceptions;
using DotPulsar.Internal.Exceptions;
using Polly;

namespace DotPulsar.Internal;

public sealed class ResilientProducer<TMessage> : IProducer<TMessage>
{
	private readonly IProducerBuilder<TMessage> producerBuilder;
	private readonly ResiliencePipeline resiliencePipeline;
	private bool disposed;
#pragma warning disable CA2213
	private IProducer<TMessage>? currentProducer;
#pragma warning restore CA2213

	public ResilientProducer(IProducerBuilder<TMessage> producerBuilder, ResiliencePipeline? resiliencePipeline = null) {
		this.producerBuilder = producerBuilder ?? throw new ArgumentNullException(nameof(producerBuilder));
		this.resiliencePipeline = resiliencePipeline ?? ResiliencePipeline.Empty;
		currentProducer = GetOrCreateProducer();
		ServiceUrl = currentProducer.ServiceUrl;
		Topic = currentProducer.Topic;
		State = new ResilienceState(this);
		SendChannel = new ResilienceSendChannel(this);
	}

	public Uri ServiceUrl {
		get;
	}

	public string Topic {
		get;
	}

	public IState<ProducerState> State {
		get;
	}

	public ISendChannel<TMessage> SendChannel {
		get;
	}

	public ValueTask<MessageId> Send(MessageMetadata metadata, TMessage message, CancellationToken cancellationToken = new CancellationToken()) {
		ThrowIfDisposed();
		return resiliencePipeline.ExecuteAsync(static (state, ct) => {
			var (topicProducer, message, metadata) = state;
			return topicProducer.GetOrCreateProducer().Send(metadata, message, ct);
		}, (this, message, metadata), cancellationToken);
	}

	private IProducer<TMessage> GetOrCreateProducer() {
		ThrowIfDisposed();

		var current = currentProducer;
		if (current != null) {
			return current;
		}

		var created = producerBuilder.Create();
		var result = Interlocked.CompareExchange(ref currentProducer, created, null);
		if (result == null) {
			_ = StateMonitor.MonitorProducer(created, new StateChangedHandler(this));
			return created;
		}

		if (!ReferenceEquals(result, created)) {
			DisposeProducer(created);
		}

		return result;

		static void DisposeProducer(IProducer producer) {
			var task = DisposeAndIgnoreException(producer);
			if (!task.IsCompleted) {
				try {
					task.AsTask().Wait();
#pragma warning disable CA1031
				} catch (Exception) {
#pragma warning restore CA1031
					// Ignore
				}
			}
		}
	}

	private void ThrowIfDisposed() {
		if (disposed) {
			throw new ResilientProducerDisposedException("DotPulsar.Internal.ResilientProducer");
		}
	}

#pragma warning disable CA1816
	public ValueTask DisposeAsync() {
		if (!disposed) {
			disposed = true;
			var last = Interlocked.Exchange(ref currentProducer, null);
			if (last != null) {
				return DisposeAndIgnoreException(last);
			}
		}
#if NET6_0_OR_GREATER
		return ValueTask.CompletedTask;
#else
		return default;
#endif
	}
#pragma warning restore CA1816

	private static async ValueTask DisposeAndIgnoreException(IProducer producer) {
		try {
			await producer.DisposeAsync().ConfigureAwait(false);
#pragma warning disable CA1031
		} catch (Exception) {
#pragma warning restore CA1031
			// Ignore
		}
	}

	private sealed class StateChangedHandler : IHandleStateChanged<ProducerStateChanged>
	{
		private readonly ResilientProducer<TMessage> instance;

		public StateChangedHandler(ResilientProducer<TMessage> instance) {
			this.instance = instance;
		}

		public ValueTask OnStateChanged(ProducerStateChanged stateChanged, CancellationToken cancellationToken = new CancellationToken()) {
			if (stateChanged.Producer.State.IsFinalState(stateChanged.ProducerState)) {
				var toDispose = Interlocked.CompareExchange(ref instance.currentProducer, null, (IProducer<TMessage>)stateChanged.Producer);
				if (toDispose != null) {
					return DisposeAndIgnoreException(toDispose);
				}
			}

#if NET6_0_OR_GREATER
			return ValueTask.CompletedTask;
#else
			return default;
#endif
		}

		public CancellationToken CancellationToken => CancellationToken.None;
	}

	private sealed class ResilienceState : IState<ProducerState>
	{
		private readonly ResilientProducer<TMessage> producer;

		public ResilienceState(ResilientProducer<TMessage> producer) {
			this.producer = producer;
		}

		public bool IsFinalState() => producer.disposed;

		public bool IsFinalState(ProducerState state) => producer.GetOrCreateProducer().State.IsFinalState(state);

		public ValueTask<ProducerState> OnStateChangeTo(ProducerState state, CancellationToken cancellationToken) {
			producer.ThrowIfDisposed();
			return producer.resiliencePipeline.ExecuteAsync(async static (args, ct) => {
				var (instance, state) = args;
				return await instance.GetOrCreateProducer().State.OnStateChangeTo(state, ct).ConfigureAwait(false);
			}, (producer, state), cancellationToken);
		}

		public ValueTask<ProducerState> OnStateChangeFrom(ProducerState state, CancellationToken cancellationToken = new CancellationToken()) {
			producer.ThrowIfDisposed();
			return producer.resiliencePipeline.ExecuteAsync(async static (args, ct) => {
				var (instance, state) = args;
				return await instance.GetOrCreateProducer().State.OnStateChangeFrom(state, ct).ConfigureAwait(false);
			}, (producer, state), cancellationToken);
		}
	}

	private sealed class ResilienceSendChannel : ISendChannel<TMessage>
	{
		private readonly ResilientProducer<TMessage> producer;
		private int isCompleted;

		public ResilienceSendChannel(ResilientProducer<TMessage> producer) {
			this.producer = producer;
		}

		public ValueTask Send(MessageMetadata metadata, TMessage message, Func<MessageId, ValueTask>? onMessageSent, CancellationToken cancellationToken) {
			if (isCompleted != 0) {
				throw new SendChannelCompletedException();
			}
			producer.ThrowIfDisposed();
			return producer.resiliencePipeline.ExecuteAsync(static (state, ct) => {
				var (sendChannel, message, metadata, onMessageSent) = state;
				var producer = sendChannel.producer.GetOrCreateProducer();
				return producer.SendChannel.Send(metadata, message, onMessageSent, ct);
			}, (this, message, metadata, onMessageSent), cancellationToken);
		}

		public void Complete() {
			isCompleted = 1;
			producer.currentProducer?.SendChannel.Complete();
		}

		public ValueTask Completion(CancellationToken cancellationToken) {
			return producer.currentProducer?.SendChannel.Completion(cancellationToken) ?? default;
		}
	}
}
