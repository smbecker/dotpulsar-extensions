using System.Buffers;
using System.Text.Json;
using DotPulsar.Abstractions;

namespace DotPulsar.Schemas;

public static class JsonSchema
{
	public static JsonSerializerOptions? DefaultSerializerOptions {
		get;
		set;
	} = new(JsonSerializerDefaults.Web);

	public static ISchema<TMessage> Get<TMessage>(JsonSerializerOptions? serializerOptions = null) {
		if (serializerOptions is not null) {
			return new JsonSchema<TMessage>(serializerOptions);
		}
		return JsonSchema<TMessage>.Instance;
	}
}

/// <summary>
/// A schema that uses Json to serialize and deserialize messages.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <remarks>This still relies on the native String schema rather than leveraging the official Pulsar schema registry.</remarks>
public sealed class JsonSchema<TMessage> : TypedSchema<TMessage>
{

	private readonly JsonSerializerOptions? serializerOptions;

	public JsonSchema(JsonSerializerOptions? serializerOptions = null) {
		this.serializerOptions = serializerOptions;
	}

	public JsonSchema(ISchema<ReadOnlySequence<byte>> byteSchema, JsonSerializerOptions? serializerOptions = null)
		: base(byteSchema) {
		this.serializerOptions = serializerOptions;
	}

	internal static JsonSchema<TMessage> Instance {
		get;
	} = new();

	protected override TMessage DecodeBytes(ReadOnlySequence<byte> bytes) {
		return JsonSerializer.Deserialize<TMessage>(bytes.ToArray(), serializerOptions ?? JsonSchema.DefaultSerializerOptions)!;
	}

	protected override ReadOnlySequence<byte> EncodeBytes(TMessage message) {
		return new ReadOnlySequence<byte>(JsonSerializer.SerializeToUtf8Bytes(message, serializerOptions ?? JsonSchema.DefaultSerializerOptions));
	}
}
