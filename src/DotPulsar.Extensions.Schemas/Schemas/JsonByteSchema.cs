using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotPulsar.Abstractions;

namespace DotPulsar.Schemas;

public static class JsonByteSchema
{
	internal const string RequiresUnreferencedCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo, or make sure all of the required types are preserved.";
	internal const string RequiresDynamicCodeMessage = "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use the overload that takes a JsonTypeInfo for native AOT applications.";

	public static JsonSerializerOptions? DefaultSerializerOptions {
		get;
		set;
	} = new(JsonSerializerDefaults.Web);

	[RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
	[RequiresDynamicCode(RequiresDynamicCodeMessage)]
	public static ISchema<TMessage> Get<TMessage>(JsonSerializerOptions? serializerOptions = null) {
		if (serializerOptions is not null) {
			return new JsonByteSchema<TMessage>(serializerOptions);
		}
		return JsonByteSchema<TMessage>.Instance;
	}

	public static ISchema<TMessage> Get<TMessage>(JsonTypeInfo<TMessage> typeInfo) {
		return new JsonByteSchema<TMessage>(typeInfo);
	}
}

/// <summary>
/// A schema that uses Json to serialize and deserialize messages.
/// </summary>
/// <typeparam name="TMessage"></typeparam>
/// <remarks>This still relies on the native String schema rather than leveraging the official Pulsar schema registry.</remarks>
public sealed class JsonByteSchema<TMessage> : WrappedByteSchema<TMessage>
{
	private static JsonByteSchema<TMessage>? instance;

	private readonly JsonSerializerOptions? serializerOptions;
	private readonly JsonTypeInfo<TMessage>? typeInfo;

	[RequiresUnreferencedCode(JsonByteSchema.RequiresUnreferencedCodeMessage)]
	[RequiresDynamicCode(JsonByteSchema.RequiresDynamicCodeMessage)]
	public JsonByteSchema(JsonSerializerOptions? serializerOptions = null) {
		this.serializerOptions = serializerOptions;
	}

	[RequiresUnreferencedCode(JsonByteSchema.RequiresUnreferencedCodeMessage)]
	[RequiresDynamicCode(JsonByteSchema.RequiresDynamicCodeMessage)]
	public JsonByteSchema(ISchema<ReadOnlySequence<byte>> byteSchema, JsonSerializerOptions? serializerOptions = null)
		: base(byteSchema) {
		this.serializerOptions = serializerOptions;
	}

	public JsonByteSchema(JsonTypeInfo<TMessage> typeInfo) {
		this.typeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
	}

	public JsonByteSchema(ISchema<ReadOnlySequence<byte>> byteSchema, JsonTypeInfo<TMessage> typeInfo)
		: base(byteSchema) {
		this.typeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
	}

	internal static JsonByteSchema<TMessage> Instance {
		[RequiresUnreferencedCode(JsonByteSchema.RequiresUnreferencedCodeMessage)]
		[RequiresDynamicCode(JsonByteSchema.RequiresDynamicCodeMessage)]
		get => instance ??= new();
	}

	protected override TMessage DecodeBytes(ReadOnlySequence<byte> bytes) {
		if (typeInfo is not null) {
			return JsonSerializer.Deserialize(bytes.ToArray(), typeInfo)!;
		}
		return DeserializeWithOptions(bytes);
	}

	protected override ReadOnlySequence<byte> EncodeBytes(TMessage message) {
		if (typeInfo is not null) {
			return new ReadOnlySequence<byte>(JsonSerializer.SerializeToUtf8Bytes(message, typeInfo));
		}
		return SerializeWithOptions(message);
	}

	// Only reachable when constructed via the JsonSerializerOptions constructors, which carry the trimming/AOT annotations.
	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only reachable through constructors annotated with RequiresUnreferencedCode.")]
	[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Only reachable through constructors annotated with RequiresDynamicCode.")]
	private TMessage DeserializeWithOptions(ReadOnlySequence<byte> bytes) {
		return JsonSerializer.Deserialize<TMessage>(bytes.ToArray(), serializerOptions ?? JsonByteSchema.DefaultSerializerOptions)!;
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only reachable through constructors annotated with RequiresUnreferencedCode.")]
	[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Only reachable through constructors annotated with RequiresDynamicCode.")]
	private ReadOnlySequence<byte> SerializeWithOptions(TMessage message) {
		return new ReadOnlySequence<byte>(JsonSerializer.SerializeToUtf8Bytes(message, serializerOptions ?? JsonByteSchema.DefaultSerializerOptions));
	}
}
