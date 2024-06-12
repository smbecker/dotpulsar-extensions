# Ulid ProtoBuf Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

The [Ulid](https://github.com/Cysharp/Ulid) type is not supported by default in [protobuf-net](https://github.com/protobuf-net/protobuf-net). This package provides the necessary extensions to support `Ulid` as a serializable type.

## Installation

```sh
dotnet add package Ulid.protobuf-net
```

## Usage

Simply add the `Ulid` type to the `RuntimeTypeModel` and define `Ulid` properties in your type.

```c#
RuntimeTypeModel.Default.AddUlid();
...
[ProtoContract]
public class YourType
{
	[ProtoMember(1)]
	public Ulid Id { get; set; }
}
```

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
