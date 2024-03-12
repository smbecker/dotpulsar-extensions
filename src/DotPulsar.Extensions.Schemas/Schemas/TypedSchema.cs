using System.Buffers;
using DotPulsar.Abstractions;

namespace DotPulsar.Schemas;

public abstract class TypedSchema<TMessage> : ISchema<TMessage>
{
	private readonly ISchema<ReadOnlySequence<byte>> byteSchema;

	protected TypedSchema()
		: this(Schema.ByteSequence) {
	}

	protected TypedSchema(SchemaInfo schemaInfo)
		: this(schemaInfo, Schema.ByteSequence) {
	}

	protected TypedSchema(ISchema<ReadOnlySequence<byte>> byteSchema)
		: this(byteSchema.SchemaInfo, byteSchema) {
	}

	protected TypedSchema(SchemaInfo schemaInfo, ISchema<ReadOnlySequence<byte>> byteSchema) {
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
