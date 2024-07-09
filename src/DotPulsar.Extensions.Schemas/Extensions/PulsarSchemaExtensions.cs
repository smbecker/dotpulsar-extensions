using System.Text.Json;
using DotPulsar.Abstractions;
using DotPulsar.Schemas;

namespace DotPulsar.Extensions;

public static class PulsarSchemaExtensions
{
	public static IProducerBuilder<TMessage> NewProtoBufProducer<TMessage>(this IPulsarClient client) {
		return client.NewProducer(ProtoBufSchema.Get<TMessage>());
	}

	public static IConsumerBuilder<TMessage> NewProtoBufConsumer<TMessage>(this IPulsarClient client) {
		return client.NewConsumer(ProtoBufSchema.Get<TMessage>());
	}

	public static IReaderBuilder<TMessage> NewProtoBufReader<TMessage>(this IPulsarClient client) {
		return client.NewReader(ProtoBufSchema.Get<TMessage>());
	}

	public static IProducerBuilder<TMessage> NewJsonProducer<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewProducer(JsonSchema.Get<TMessage>(serializerOptions));
	}

	public static IConsumerBuilder<TMessage> NewJsonConsumer<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewConsumer(JsonSchema.Get<TMessage>(serializerOptions));
	}

	public static IReaderBuilder<TMessage> NewJsonReader<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewReader(JsonSchema.Get<TMessage>(serializerOptions));
	}
}
