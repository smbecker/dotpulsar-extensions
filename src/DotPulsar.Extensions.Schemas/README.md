# DotPulsar Schema Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

Provides support for using Json and ProtoBuf producers, consumers, and receivers.

It is important to note that this library **does not** use the official [Pulsar Schema Registry](https://pulsar.apache.org/docs/3.1.x/schema-overview/#what-is-pulsar-schema). The types provided are merely for convenience on top of the existing `Schema.ByteSequence` schema.

[DotPulsar](https://github.com/apache/pulsar-dotpulsar) library recently released official support for Json and Protobuf messages that use the official [Pulsar Schema Registry](https://pulsar.apache.org/docs/3.1.x/schema-overview/#what-is-pulsar-schema). Consider migrating to the latest version.

## Installation

```sh
dotnet add package DotPulsar.Extensions.Schemas
```

## Usage

A JSON schema can be created using `JsonByteSchema.Get<T>()`. This schema can be used to create a producer, consumer, or receiver.

```c#
await using var producer = client.NewProducer(JsonByteSchema.Get<YourType>())
	.Topic("persistent://public/default/my-topic")
	.Create();
```

Custom serialization options can be passed into the constructor `new JsonByteSchema<T>(options)` or by overriding the default options using `JsonByteSchema.DefaultSerializerOptions`.

### Native AOT and trimming

The `JsonSerializerOptions`-based APIs rely on reflection and are annotated with `RequiresUnreferencedCode`/`RequiresDynamicCode`. For trimmed or native AOT applications, use the overloads that accept a source-generated `JsonTypeInfo<T>` instead:

```c#
[JsonSerializable(typeof(YourType))]
internal partial class MyJsonContext : JsonSerializerContext;

await using var producer = client.NewProducer(JsonByteSchema.Get(MyJsonContext.Default.YourType))
	.Topic("persistent://public/default/my-topic")
	.Create();

// or
await using var consumer = client.NewJsonConsumer(MyJsonContext.Default.YourType)
	.SubscriptionName("my-subscription")
	.Topic("persistent://public/default/my-topic")
	.Create();
```

The same can be done for Protobuf schemas using `ProtobufByteSchema.Get<T>()`.

```c#
await using var producer = client.NewProducer(ProtobufByteSchema.Get<YourType>())
	.Topic("persistent://public/default/my-topic")
	.Create();
```

Both `JsonByteSchema` and `ProtobufByteSchema` support passing a custom `ISchema<ReadOnlySequence<byte>>` schema. This allows for custom handling of the bytes that are sent and received. One potential use case would be to implement custom encryption or compression of the message bytes.

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
