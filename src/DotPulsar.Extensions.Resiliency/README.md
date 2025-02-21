# DotPulsar Resiliency Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

Provides resiliency extensions for [DotPulsar](https://github.com/apache/pulsar-dotpulsar). This includes an implementation of [dead letter queue](https://pulsar.apache.org/docs/3.3.x/concepts-messaging/#retry-letter-topic) as well as integration with [Polly](https://www.pollydocs.org/).

## Installation

```sh
dotnet add package DotPulsar.Extensions.Resiliency
```

## Usage

**Using the DLQ implementation:**
```c#
await using var dlq = new DeadLetterPolicy(
	client.NewProducer().Topic("...-DLQ"),
	client.NewProducer().Topic("...-RETRY"));
...
await dlq.ReconsumeLater(message);
```
You will need to have a separate consumer set up to listen to the retry topic in order to process the retry messages. If those messages fail, then you will need to re-submit them to the `DeadLetterPolicy` to mark them as retry or dead.

**Using [Polly](https://www.pollydocs.org/) to create resilient producers:**
```c#
await using var producer = client.NewProducer(Schema.String)
	.Topic("...")
	.CreateResilient(static pipeline => {
		pipeline.AddResilientProducerDefaults(configureRetry: static options => {
			options.MaxRetryAttempts = 3;
		});
	});
```

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
