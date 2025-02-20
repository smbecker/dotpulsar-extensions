using DotPulsar.Abstractions;
using DotPulsar.Exceptions;
using DotPulsar.Internal;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace DotPulsar.Extensions;

public static class PulsarResilientExtensions
{
	public static ValueTask Process<TMessage>(
		this IConsumer<TMessage> consumer,
		Func<IMessage<TMessage>, CancellationToken, ValueTask> processor,
		ResiliencePipeline resiliencePipeline,
		ProcessingOptions options,
		IConsumerFailureHandler? failureHandler = null,
		CancellationToken cancellationToken = default) {
		return consumer.Process(async (message, token) => {
			try {
				await resiliencePipeline.ExecuteAsync(static (state, ct) => {
					var (processor, message) = state;
					return processor(message, ct);
				}, (processor, message), token).ConfigureAwait(false);
			} catch (Exception e) when (failureHandler != null) {
				await failureHandler.HandleAsync(message, e, token).ConfigureAwait(false);
				// Propagate so that the Activity is marked as faulted
				throw;
			}
		}, options, cancellationToken);
	}

	public static ValueTask Process<TMessage>(
		this IConsumer<TMessage> consumer,
		Func<IMessage<TMessage>, CancellationToken, ValueTask> processor,
		ResiliencePipeline resiliencePipeline,
		IConsumerFailureHandler? failureHandler = null,
		CancellationToken cancellationToken = default)
		=> Process(consumer, processor, resiliencePipeline, new ProcessingOptions(), failureHandler, cancellationToken);

	public static ResiliencePipelineBuilder AddResilientProducer(this ResiliencePipelineBuilder pipelineBuilder, Action<RetryStrategyOptions>? configureRetry = null, Action<TimeoutStrategyOptions>? configureTimeout = null) {
		return pipelineBuilder.AddProducerRetry(configureRetry).AddProducerTimeout(configureTimeout);
	}

	public static ResiliencePipelineBuilder AddProducerTimeout(this ResiliencePipelineBuilder pipelineBuilder, Action<TimeoutStrategyOptions>? configure = null) {
		var options = new TimeoutStrategyOptions {
			Timeout = TimeSpan.FromSeconds(30)
		};
		configure?.Invoke(options);
		return pipelineBuilder.AddTimeout(options);
	}

	public static ResiliencePipelineBuilder AddProducerRetry(this ResiliencePipelineBuilder pipelineBuilder, Action<RetryStrategyOptions>? configure = null) {
		var options = new RetryStrategyOptions {
			MaxRetryAttempts = 10,
			Delay = TimeSpan.FromMilliseconds(100),
			MaxDelay = TimeSpan.FromMilliseconds(5000),
			BackoffType = DelayBackoffType.Exponential,
			ShouldHandle = static args => {
				var ex = args.Outcome.Exception;
				if (ShouldHandleProducerException(ex)) {
					return new ValueTask<bool>(true);
				}

				return new ValueTask<bool>(false);
			}
		};
		configure?.Invoke(options);
		return pipelineBuilder.AddRetry(options);
	}

	public static bool ShouldHandleProducerException(Exception? exception) {
		return exception is not ResilientProducerDisposedException
			&& exception is not PulsarClientDisposedException
			&& exception is ProducerFaultedException or ProducerClosedException or ObjectDisposedException;
	}

	public static IProducer<TMessage> CreateResilient<TMessage>(this IProducerBuilder<TMessage> producerBuilder, ResiliencePipeline? resiliencePipeline = null) {
		ArgumentNullException.ThrowIfNull(producerBuilder);

		if (resiliencePipeline == null || Equals(resiliencePipeline, ResiliencePipeline.Empty)) {
			return producerBuilder.Create();
		}
		return new ResilientProducer<TMessage>(producerBuilder, resiliencePipeline);
	}

	public static IProducer<TMessage> CreateResilient<TMessage>(this IProducerBuilder<TMessage> producerBuilder, Action<ResiliencePipelineBuilder>? configurePipelineBuilder = null) {
		var pipelineBuilder = new ResiliencePipelineBuilder();
		if (configurePipelineBuilder != null) {
			configurePipelineBuilder(pipelineBuilder);
		} else {
			pipelineBuilder.AddResilientProducer();
		}

		return CreateResilient(producerBuilder, pipelineBuilder.Build());
	}
}
