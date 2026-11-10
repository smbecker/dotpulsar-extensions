using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotPulsar.Abstractions;
using DotPulsar.Schemas;
using Google.Protobuf;

namespace DotPulsar.Extensions;

public static class PulsarSchemaExtensions
{
	public static IProducerBuilder<TMessage> NewProtobufProducer<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewProducer(ProtobufByteSchema.Get<TMessage>());
	}

	public static IProducerBuilder<TMessage> NewProtobufProducer<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewProducer(new ProtobufByteSchema<TMessage>(parser));
	}

	public static IConsumerBuilder<TMessage> NewProtobufConsumer<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewConsumer(ProtobufByteSchema.Get<TMessage>());
	}

	public static IConsumerBuilder<TMessage> NewProtobufConsumer<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewConsumer(new ProtobufByteSchema<TMessage>(parser));
	}

	public static IReaderBuilder<TMessage> NewProtobufReader<TMessage>(this IPulsarClient client)
		where TMessage : IMessageWithParser<TMessage> {
		return client.NewReader(ProtobufByteSchema.Get<TMessage>());
	}

	public static IReaderBuilder<TMessage> NewProtobufReader<TMessage>(this IPulsarClient client, MessageParser<TMessage> parser)
		where TMessage : Google.Protobuf.IMessage<TMessage> {
		return client.NewReader(new ProtobufByteSchema<TMessage>(parser));
	}

	[RequiresUnreferencedCode(JsonByteSchema.RequiresUnreferencedCodeMessage)]
	[RequiresDynamicCode(JsonByteSchema.RequiresDynamicCodeMessage)]
	public static IProducerBuilder<TMessage> NewJsonProducer<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewProducer(JsonByteSchema.Get<TMessage>(serializerOptions));
	}

	public static IProducerBuilder<TMessage> NewJsonProducer<TMessage>(this IPulsarClient client, JsonTypeInfo<TMessage> typeInfo) {
		return client.NewProducer(JsonByteSchema.Get(typeInfo));
	}

	[RequiresUnreferencedCode(JsonByteSchema.RequiresUnreferencedCodeMessage)]
	[RequiresDynamicCode(JsonByteSchema.RequiresDynamicCodeMessage)]
	public static IConsumerBuilder<TMessage> NewJsonConsumer<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewConsumer(JsonByteSchema.Get<TMessage>(serializerOptions));
	}

	public static IConsumerBuilder<TMessage> NewJsonConsumer<TMessage>(this IPulsarClient client, JsonTypeInfo<TMessage> typeInfo) {
		return client.NewConsumer(JsonByteSchema.Get(typeInfo));
	}

	[RequiresUnreferencedCode(JsonByteSchema.RequiresUnreferencedCodeMessage)]
	[RequiresDynamicCode(JsonByteSchema.RequiresDynamicCodeMessage)]
	public static IReaderBuilder<TMessage> NewJsonReader<TMessage>(this IPulsarClient client, JsonSerializerOptions? serializerOptions = null) {
		return client.NewReader(JsonByteSchema.Get<TMessage>(serializerOptions));
	}

	public static IReaderBuilder<TMessage> NewJsonReader<TMessage>(this IPulsarClient client, JsonTypeInfo<TMessage> typeInfo) {
		return client.NewReader(JsonByteSchema.Get(typeInfo));
	}
}
