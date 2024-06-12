# DotPulsar Dependency Injection Extensions

_[![Build status](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/ci.yaml)_
_[![CodeQL analysis](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml/badge.svg?branch=main)](https://github.com/smbecker/dotpulsar-extensions/actions/workflows/codeql.yaml)_

Enables easy integration of [DotPulsar](https://github.com/apache/pulsar-dotpulsar) with the new [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder) project types.

## Installation

```sh
dotnet add package DotPulsar.Extensions.DependencyInjection
```

## Usage

```c#
services.AddPulsarClient();
```

This will register the `IPulsarClient` as a singleton in the service collection with the default settings. The settings can be configured using the standard `appSettings.json` file with the following configuration keys supported.

* `Pulsar:ServiceUrl` - The service URL for the Pulsar cluster.
* `Pulsar:AuthenticateUsingClientCertificate` - Authenticate using a client certificate.
  * `Pulsar:AuthenticateUsingClientCertificate:Path` - The path to the client certificate.
  * `Pulsar:AuthenticateUsingClientCertificate:KeyPath` - The path to the client certificate key.
  * `Pulsar:AuthenticateUsingClientCertificate:Password` - The client certificate password.
* `Pulsar:AuthenticationToken` - Authenticate using a token.
* `Pulsar:CheckCertificateRevocation` - Specifies whether the certificate revocation list is checked during authentication.
* `Pulsar:ConnectionSecurity` - Set connection encryption policy.
* `Pulsar:KeepAliveInterval` - The time to wait before sending a 'ping' if there has been no activity on the connection.
* `Pulsar:ListenerName` - Set the listener name.
* `Pulsar:RetryInterval` - The time to wait before retrying an operation or a reconnect.
* `Pulsar:TrustedCertificateAuthority` - Add a trusted certificate authority.
* `Pulsar:VerifyCertificateAuthority` - Verify the certificate authority.
* `Pulsar:VerifyCertificateName` - Verify the certificate name with the hostname.
* `Pulsar:CloseInactiveConnectionsInterval` - The time to wait before closing inactive connections.

There are also several overloads to `AddPulsarClient` that allow for more fine-grained control over the client settings.

## License

This project is licensed under [Apache License, Version 2.0](https://apache.org/licenses/LICENSE-2.0).
