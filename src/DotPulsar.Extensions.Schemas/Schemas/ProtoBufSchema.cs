using System.Buffers;
using DotPulsar.Abstractions;

namespace DotPulsar.Schemas;

public static class ProtoBufSchema
{
	public static ISchema<TMessage> Get<TMessage>() {
		return ProtoBufSchema<TMessage>.Instance;
	}
}

/// <summary>
/// A schema that uses ProtoBuf to serialize and deserialize messages.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <remarks>This still relies on the native Bytes schema rather than leveraging the official Pulsar schema registry.</remarks>
public sealed class ProtoBufSchema<TMessage> : TypedSchema<TMessage>
{
	public ProtoBufSchema() {
	}

	public ProtoBufSchema(ISchema<ReadOnlySequence<byte>> byteSchema)
		: base(byteSchema) {
	}

	internal static ProtoBufSchema<TMessage> Instance {
		get;
	} = new();

	protected override TMessage DecodeBytes(ReadOnlySequence<byte> bytes) {
		return ProtoBuf.Serializer.Deserialize<TMessage>(bytes);
	}

	protected override ReadOnlySequence<byte> EncodeBytes(TMessage message) {
		using var measure = ProtoBuf.Serializer.Measure(message);
		var writer = new ArrayBufferWriter<byte>((int)measure.Length);
		measure.Serialize(writer);
		return new ReadOnlySequence<byte>(writer.WrittenMemory);
	}
}
