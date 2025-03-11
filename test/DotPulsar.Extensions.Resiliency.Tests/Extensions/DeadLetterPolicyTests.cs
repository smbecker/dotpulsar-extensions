using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DotPulsar.Abstractions;
using DotPulsar.Schemas;
using Testcontainers.Pulsar;
// ReSharper disable AccessToDisposedClosure

namespace DotPulsar.Extensions;

[Trait("Category", "FUNCTIONAL")]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public class DeadLetterPolicyTests
{
	[Fact]
	public async Task can_retry_messages() {
		var pulsarContainer = new PulsarBuilder().Build();
		await pulsarContainer.StartAsync();
		try {
			await using var client = PulsarClient.Builder()
				.ServiceUrl(new Uri(pulsarContainer.GetBrokerAddress()))
				.Build();
			await using var originProducer = client.NewProducer()
				.Topic("persistent://public/default/origin")
				.Create();
			await using var originConsumer = client.NewConsumer()
				.Topic("persistent://public/default/origin")
				.SubscriptionName("test")
				.SubscriptionType(SubscriptionType.Exclusive)
				.Create();
			await using var retryConsumer = client.NewConsumer()
				.Topic("persistent://public/default/origin-RETRY")
				.SubscriptionName("test")
				.SubscriptionType(SubscriptionType.Exclusive)
				.Create();
			await using var dlqConsumer = client.NewConsumer()
				.Topic("persistent://public/default/origin-DLQ")
				.SubscriptionName("test")
				.SubscriptionType(SubscriptionType.Exclusive)
				.Create();
			await using var dlq = new DeadLetterPolicy(
				client.NewProducer().Topic("persistent://public/default/origin-DLQ"),
				client.NewProducer().Topic("persistent://public/default/origin-RETRY"),
				maxRedeliveryCount: 1);

			var retries = new ConcurrentQueue<IMessage>();
			var dead = new ConcurrentQueue<IMessage>();
			_ = Task.Run(() => ProcessToQueue(retryConsumer, retries));
			_ = Task.Run(() => ProcessToQueue(dlqConsumer, dead));

			await originProducer.Send(StringSchema.UTF8.Encode("test"));
			var testMessage = await originConsumer.Receive();
			Assert.Equal("test", StringSchema.UTF8.Decode(testMessage.Value(), null));

			await dlq.ReconsumeLater(testMessage);
			var retryMessage = await AwaitForMessage(retries);
			Assert.NotNull(retryMessage);
			Assert.Equal("test", StringSchema.UTF8.Decode(Assert.IsAssignableFrom<IMessage<ReadOnlySequence<byte>>>(retryMessage).Value(), null));
			Assert.Empty(dead);

			await dlq.ReconsumeLater(retryMessage);
			var deadMessage = await AwaitForMessage(dead);
			Assert.NotNull(deadMessage);
			Assert.Equal("test", StringSchema.UTF8.Decode(Assert.IsAssignableFrom<IMessage<ReadOnlySequence<byte>>>(deadMessage).Value(), null));
			Assert.Empty(retries);
		} finally {
			await pulsarContainer.StopAsync();
		}

		static async ValueTask<IMessage?> AwaitForMessage(ConcurrentQueue<IMessage> queue) {
			const int maxAttempts = 50;
			var attempt = 0;
			while (attempt < maxAttempts) {
				attempt++;
				if (queue.TryDequeue(out var message)) {
					return message;
				}
				await Task.Delay(100);
			}

			return null;
		}

		static async Task ProcessToQueue(IConsumer<ReadOnlySequence<byte>> consumer, ConcurrentQueue<IMessage> queue) {
			try {
				while (true) {
					var message = await consumer.Receive();
					queue.Enqueue(message);
				}
#pragma warning disable CA1031
			} catch (Exception e) {
				Debug.WriteLine(e);
			}
#pragma warning restore CA1031
		}
	}
}
