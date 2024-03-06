using DotPulsar.Abstractions;
using DotPulsar.Schemas;

namespace DotPulsar.Extensions;

public static class PulsarClientExtensions
{
	public static IProducerBuilder<TMessage> NewProtoBufProducer<TMessage>(this IPulsarClient client)
		where TMessage : ProtoBuf.IExtensible {
		return client.NewProducer(ProtoBufSchema.Get<TMessage>());
	}

	public static IConsumerBuilder<TMessage> NewProtoBufConsumer<TMessage>(this IPulsarClient client)
		where TMessage : ProtoBuf.IExtensible {
		return client.NewConsumer(ProtoBufSchema.Get<TMessage>());
	}

	public static IReaderBuilder<TMessage> NewProtoBufReader<TMessage>(this IPulsarClient client)
		where TMessage : ProtoBuf.IExtensible {
		return client.NewReader(ProtoBufSchema.Get<TMessage>());
	}

	public static IProducerBuilder<TMessage> NewJsonProducer<TMessage>(this IPulsarClient client)
		where TMessage : ProtoBuf.IExtensible {
		return client.NewProducer(JsonSchema.Get<TMessage>());
	}

	public static IConsumerBuilder<TMessage> NewJsonConsumer<TMessage>(this IPulsarClient client)
		where TMessage : ProtoBuf.IExtensible {
		return client.NewConsumer(JsonSchema.Get<TMessage>());
	}

	public static IReaderBuilder<TMessage> NewJsonReader<TMessage>(this IPulsarClient client)
		where TMessage : ProtoBuf.IExtensible {
		return client.NewReader(JsonSchema.Get<TMessage>());
	}
}
