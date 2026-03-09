using System.Buffers;
using DotPulsar.Abstractions;

namespace DotPulsar.Schemas;

public abstract class WrappedByteSchema<TMessage> : ISchema<TMessage>
{
	private readonly ISchema<ReadOnlySequence<byte>> byteSchema;

	protected WrappedByteSchema()
		: this(Schema.ByteSequence) {
	}

	protected WrappedByteSchema(SchemaInfo schemaInfo)
		: this(schemaInfo, Schema.ByteSequence) {
	}

	protected WrappedByteSchema(ISchema<ReadOnlySequence<byte>> byteSchema)
		: this((byteSchema ?? throw new ArgumentNullException(nameof(byteSchema))).SchemaInfo, byteSchema) {
	}

	protected WrappedByteSchema(SchemaInfo schemaInfo, ISchema<ReadOnlySequence<byte>> byteSchema) {
		this.byteSchema = byteSchema ?? throw new ArgumentNullException(nameof(byteSchema));
		SchemaInfo = schemaInfo ?? throw new ArgumentNullException(nameof(schemaInfo));
	}

	public SchemaInfo SchemaInfo {
		get;
	}

	protected abstract TMessage DecodeBytes(ReadOnlySequence<byte> bytes);

	public TMessage Decode(ReadOnlySequence<byte> bytes, byte[]? schemaVersion = null) {
		bytes = byteSchema.Decode(bytes, schemaVersion);
		return DecodeBytes(bytes);
	}

	protected abstract ReadOnlySequence<byte> EncodeBytes(TMessage message);

	public ReadOnlySequence<byte> Encode(TMessage message) {
		var bytes = EncodeBytes(message);
		return byteSchema.Encode(bytes);
	}
}
