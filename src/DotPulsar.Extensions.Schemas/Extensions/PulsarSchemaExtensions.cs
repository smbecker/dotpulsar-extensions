using System.Text.Json;
using DotPulsar.Abstractions;
using DotPulsar.Schemas;
using Google.Protobuf;

namespace DotPulsar.Extensions;

public static class PulsarSchemaExtensions
{
#if NET8_0_OR_GREATER
	public static IProducerBuilder<TMessage> NewProtoBufProducer<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewProducer(ProtoBufSchema.Get<TMessage>());
	}
#endif

	public static IProducerBuilder<TMessage> NewProtoBufProducer<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewProducer(new ProtoBufSchema<TMessage>(parser));
	}

#if NET8_0_OR_GREATER
	public static IConsumerBuilder<TMessage> NewProtoBufConsumer<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewConsumer(ProtoBufSchema.Get<TMessage>());
	}
#endif

	public static IConsumerBuilder<TMessage> NewProtoBufConsumer<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewConsumer(new ProtoBufSchema<TMessage>(parser));
	}

#if NET8_0_OR_GREATER
	public static IReaderBuilder<TMessage> NewProtoBufReader<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewReader(ProtoBufSchema.Get<TMessage>());
	}
#endif

	public static IReaderBuilder<TMessage> NewProtoBufReader<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewReader(new ProtoBufSchema<TMessage>(parser));
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
