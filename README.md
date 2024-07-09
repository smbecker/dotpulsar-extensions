# DotPulsar Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

Provides extension packages to [DotPulsar](https://github.com/apache/pulsar-dotpulsar), the official .NET client library [Apache Pulsar](https://pulsar.apache.org/).


| Package                                                                                                                                                                                                                                                                        | Downloads                                                                                                                                                     | NuGet Latest |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------| ------------ |
| `DotPulsar.Extensions.DependencyInjection`<br />Enables easy integration with [.Net Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder) applications                                                                          | [![Nuget](https://img.shields.io/nuget/dt/DotPulsar.Extensions.DependencyInjection)](https://www.nuget.org/packages/DotPulsar.Extensions.DependencyInjection) | [![Nuget](https://img.shields.io/nuget/v/DotPulsar.Extensions.DependencyInjection)](https://www.nuget.org/packages/DotPulsar.Extensions.DependencyInjection) |
| `DotPulsar.Extensions.OpenTelemetry`<br />Adds extension methods to facilitate easy integration with the [OpenTelemetry SDK](https://github.com/open-telemetry/opentelemetry-dotnet).                                                                                          | [![Nuget](https://img.shields.io/nuget/dt/DotPulsar.Extensions.OpenTelemetry)](https://www.nuget.org/packages/DotPulsar.Extensions.OpenTelemetry)             | [![Nuget](https://img.shields.io/nuget/v/DotPulsar.Extensions.OpenTelemetry)](https://www.nuget.org/packages/DotPulsar.Extensions.OpenTelemetry) |
| `DotPulsar.Extensions.Resiliency`<br />Adds resiliency extensions for working with [DotPulsar](https://github.com/apache/pulsar-dotpulsar).                                                                                                                                                                                           | [![Nuget](https://img.shields.io/nuget/dt/DotPulsar.Extensions.Resiliency)](https://www.nuget.org/packages/DotPulsar.Extensions.Resiliency)             | [![Nuget](https://img.shields.io/nuget/v/DotPulsar.Extensions.Resiliency)](https://www.nuget.org/packages/DotPulsar.Extensions.Resiliency) |
| `DotPulsar.Extensions.Schemas`<br />Enables using strongly-typed messages using JSON or [ProtoBuf](https://protobuf.dev/) contracts. This **does not** use the official [Pulsar Schema Registry](https://pulsar.apache.org/docs/3.1.x/schema-overview/#what-is-pulsar-schema). | [![Nuget](https://img.shields.io/nuget/dt/DotPulsar.Extensions.Schemas)](https://www.nuget.org/packages/DotPulsar.Extensions.Schemas)                         | [![Nuget](https://img.shields.io/nuget/v/DotPulsar.Extensions.Schemas)](https://www.nuget.org/packages/DotPulsar.Extensions.Schemas) |
| `Ulid.protobuf-net`<br />Add support for [Ulid](https://www.nuget.org/packages/Ulid) types within ProtoBuf schemas. While not specifically related to [DotPulsar](https://github.com/apache/pulsar-dotpulsar), it compliments `DotPulsar.Extensions.Schemas` quite nicely.     | [![Nuget](https://img.shields.io/nuget/dt/Ulid.protobuf-net)](https://www.nuget.org/packages/Ulid.protobuf-net)                              | [![Nuget](https://img.shields.io/nuget/v/Ulid.protobuf-net)](https://www.nuget.org/packages/Ulid.protobuf-net) |

## Versioning

We use [SemVer](http://semver.org/) along with [MinVer](https://github.com/adamralph/minver) for versioning. For the versions available, see the [tags on this repository](https://github.com/smbecker/dotpulsar-extensions/tags).

## Contributing

Contributions are welcomed and greatly appreciated. See also the list of [contributors](https://github.com/smbecker/dotpulsar-extensions/contributors) who participated in this project. Read the [CONTRIBUTING](CONTRIBUTING.md) guide for how to participate.

Git Hooks are enabled on this repository. You will need to run `git config --local core.hooksPath .githooks/` to enable them in your environment.

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
