# DotPulsar Schema Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

Provides support for using Json and ProtoBuf producers, consumers, and receivers.

It is important to note that this library **does not** use the official [Pulsar Schema Registry](https://pulsar.apache.org/docs/3.1.x/schema-overview/#what-is-pulsar-schema). The types provided are merely for convenience on top of the existing `Schema.ByteSequence` schema.

## Installation

```sh
dotnet add package DotPulsar.Extensions.Schemas
```

## Usage

A JSON schema can be created using `JsonSchema.Get<T>()`. This schema can be used to create a producer, consumer, or receiver.

```c#
await using var producer = client.NewProducer(JsonSchema.Get<YourType>())
	.Topic("persistent://public/default/my-topic")
	.Create();
```

Custom serialization options can be passed into the constructor `new JsonSchema<T>(options)` or by overriding the default options using `JsonSchema.DefaultSerializerOptions`.

The same can be done for ProtoBuf schemas using `ProtoBufSchema.Get<T>()`. Currently, only [protbuf-net](https://github.com/protobuf-net/protobuf-net) serializable types are supported.

```c#
await using var producer = client.NewProducer(ProtoBufSchema.Get<YourType>())
	.Topic("persistent://public/default/my-topic")
	.Create();
```

Both `JsonSchema` and `ProtoBufSchema` support passing a custom `ISchema<ReadOnlySequence<byte>>` schema. This allows for custom handling of the bytes that are sent and received. One potential use case would be to implement custom encryption or compression of the message bytes.

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
