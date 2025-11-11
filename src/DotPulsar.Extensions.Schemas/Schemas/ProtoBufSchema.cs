using System.Buffers;
using DotPulsar.Abstractions;
using Google.Protobuf;

namespace DotPulsar.Schemas;

#if NET8_0_OR_GREATER
public static class ProtoBufSchema
{
	public static ISchema<TMessage> Get<TMessage>()
		where TMessage : IMessageWithParser<TMessage> {
		return ProtoBufSchemaCache<TMessage>.Instance;
	}

	private static class ProtoBufSchemaCache<TMessage>
		where TMessage : IMessageWithParser<TMessage>
	{
		internal static ProtoBufSchema<TMessage> Instance {
			get;
		} = new(TMessage.Parser);
	}
}
#endif

/// <summary>
/// A schema that uses ProtoBuf to serialize and deserialize messages.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <remarks>This still relies on the native Bytes schema rather than leveraging the official Pulsar schema registry.</remarks>
public sealed class ProtoBufSchema<TMessage> : TypedSchema<TMessage> where TMessage : Google.Protobuf.IMessage<TMessage>
{
	private readonly MessageParser<TMessage> parser;

	public ProtoBufSchema(MessageParser<TMessage> parser) {
		this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
	}

	public ProtoBufSchema(MessageParser<TMessage> parser, ISchema<ReadOnlySequence<byte>> byteSchema)
		: base(byteSchema) {
		this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
	}

	protected override TMessage DecodeBytes(ReadOnlySequence<byte> bytes) {
		return parser.ParseFrom(bytes);
	}

	protected override ReadOnlySequence<byte> EncodeBytes(TMessage message) {
		var length = message.CalculateSize();
		if (length <= 0) {
			return ReadOnlySequence<byte>.Empty;
		}

		var writer = new ArrayBufferWriter<byte>(length);
		message.WriteTo(writer);
		return new ReadOnlySequence<byte>(writer.WrittenMemory);
	}
}
