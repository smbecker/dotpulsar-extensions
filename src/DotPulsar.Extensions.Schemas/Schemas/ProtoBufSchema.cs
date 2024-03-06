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
public sealed class ProtoBufSchema<TMessage> : ISchema<TMessage>
{
	internal static ProtoBufSchema<TMessage> Instance {
		get;
	} = new();

	public SchemaInfo SchemaInfo => Schema.ByteSequence.SchemaInfo;

	public TMessage Decode(ReadOnlySequence<byte> bytes, byte[]? schemaVersion = null) {
		return ProtoBuf.Serializer.Deserialize<TMessage>(bytes);
	}

	public ReadOnlySequence<byte> Encode(TMessage message) {
		using var measure = ProtoBuf.Serializer.Measure(message);
		var writer = new ArrayBufferWriter<byte>((int)measure.Length);
		measure.Serialize(writer);
		return new ReadOnlySequence<byte>(writer.WrittenMemory);
	}
}
