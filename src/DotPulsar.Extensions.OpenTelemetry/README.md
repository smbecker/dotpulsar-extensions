# DotPulsar OpenTelemetry Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

Integrates [DotPulsar](https://github.com/apache/pulsar-dotpulsar) with the [OpenTelemetry SDK](https://github.com/open-telemetry/opentelemetry-dotnet) APIs.

## Installation

```sh
dotnet add package DotPulsar.Extensions.OpenTelemetry
```

## Usage

```c#
services.AddOpenTelemetry()
	.WithTracing(static tracing => tracing.AddPulsarInstrumentation())
	.WithMetrics(static metrics => metrics.AddPulsarInstrumentation());
```

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
