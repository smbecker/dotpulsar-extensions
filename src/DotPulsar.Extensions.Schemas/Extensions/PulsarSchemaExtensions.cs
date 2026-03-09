using System.Text.Json;
using DotPulsar.Abstractions;
using DotPulsar.Schemas;
using Google.Protobuf;

namespace DotPulsar.Extensions;

public static class PulsarSchemaExtensions
{
#if NET8_0_OR_GREATER
	public static IProducerBuilder<TMessage> NewProtobufProducer<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewProducer(ProtobufByteSchema.Get<TMessage>());
	}
#endif

	public static IProducerBuilder<TMessage> NewProtobufProducer<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewProducer(new ProtobufByteSchema<TMessage>(parser));
	}

#if NET8_0_OR_GREATER
	public static IConsumerBuilder<TMessage> NewProtobufConsumer<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewConsumer(ProtobufByteSchema.Get<TMessage>());
	}
#endif

	public static IConsumerBuilder<TMessage> NewProtobufConsumer<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewConsumer(new ProtobufByteSchema<TMessage>(parser));
	}

#if NET8_0_OR_GREATER
	public static IReaderBuilder<TMessage> NewProtobufReader<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewReader(ProtobufByteSchema.Get<TMessage>());
	}
#endif

	public static IReaderBuilder<TMessage> NewProtobufReader<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewReader(new ProtobufByteSchema<TMessage>(parser));
	}

	public static IProducerBuilder<TMessage> NewJsonProducer<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewProducer(JsonByteSchema.Get<TMessage>(serializerOptions));
	}

	public static IConsumerBuilder<TMessage> NewJsonConsumer<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewConsumer(JsonByteSchema.Get<TMessage>(serializerOptions));
	}

	public static IReaderBuilder<TMessage> NewJsonReader<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewReader(JsonByteSchema.Get<TMessage>(serializerOptions));
	}
}
