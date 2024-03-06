using System.Buffers;
using System.Text.Json;
using DotPulsar.Abstractions;

namespace DotPulsar.Schemas;

public static class JsonSchema
{
	public static JsonSerializerOptions? DefaultSerializerOptions {
		internal get;
		set;
	} = new(JsonSerializerDefaults.Web);

	public static ISchema<TMessage> Get<TMessage>() {
		return JsonSchema<TMessage>.Instance;
	}
}

/// <summary>
/// A schema that uses Json to serialize and deserialize messages.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <remarks>This still relies on the native String schema rather than leveraging the official Pulsar schema registry.</remarks>
public sealed class JsonSchema<TMessage> : ISchema<TMessage>
{

	private readonly JsonSerializerOptions? serializerOptions;

	public JsonSchema(JsonSerializerOptions? serializerOptions = null) {
		this.serializerOptions = serializerOptions;
	}

	internal static JsonSchema<TMessage> Instance {
		get;
	} = new();

	public SchemaInfo SchemaInfo => StringSchema.UTF8.SchemaInfo;

	public TMessage Decode(ReadOnlySequence<byte> bytes, byte[]? schemaVersion = null) {
		return JsonSerializer.Deserialize<TMessage>(bytes.ToArray(), serializerOptions ?? JsonSchema.DefaultSerializerOptions)!;
	}

	public ReadOnlySequence<byte> Encode(TMessage message) {
		return new ReadOnlySequence<byte>(JsonSerializer.SerializeToUtf8Bytes(message, serializerOptions ?? JsonSchema.DefaultSerializerOptions));
	}
}
