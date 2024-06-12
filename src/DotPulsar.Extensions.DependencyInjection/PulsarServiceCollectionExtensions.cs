using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class PulsarServiceCollectionExtensions
{
	public static IServiceCollection AddPulsarClient(this IServiceCollection services, IPulsarClient pulsarClient) {
		services.TryAddSingleton(pulsarClient);
		return services;
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, Func<IServiceProvider, IPulsarClient> pulsarClientFactory) {
		services.TryAddSingleton(pulsarClientFactory);
		return services;
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, IPulsarClientBuilder pulsarClientBuilder) {
		ArgumentNullException.ThrowIfNull(pulsarClientBuilder);
		return AddPulsarClient(services, pulsarClientBuilder.Build());
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services) {
		services.AddOptions<PulsarClientOptions>().Configure<IConfiguration>(static (opts, config) => BindOptions(opts, config.GetSection("Pulsar")));
		return AddPulsarClient(services, static (sp, builder) => {
			var options = sp.GetRequiredService<IOptions<PulsarClientOptions>>().Value;
			options.Apply(builder);
		});
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, IConfigurationSection pulsarSection) {
		return AddPulsarClient(services, opts => BindOptions(opts, pulsarSection));
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, Action<PulsarClientOptions> configure) {
		services.AddOptions<PulsarClientOptions>().Configure(configure);
		return AddPulsarClient(services, static (sp, builder) => {
			var options = sp.GetRequiredService<IOptions<PulsarClientOptions>>().Value;
			options.Apply(builder);
		});
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, Action<IPulsarClientBuilder> configure) {
		return AddPulsarClient(services, (_, builder) => configure.Invoke(builder));
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, Action<IServiceProvider, IPulsarClientBuilder> configure) {
		ArgumentNullException.ThrowIfNull(configure);
		return services.AddPulsarClient(sp => {
			var builder = PulsarClient.Builder();
			configure(sp, builder);
			return builder.Build();
		});
	}

	private static void BindOptions(this PulsarClientOptions options, IConfiguration configuration) {
		configuration.Bind(options);

		options.AuthenticateUsingClientCertificate = BindCertificate(configuration.GetSection(nameof(PulsarClientOptions.AuthenticateUsingClientCertificate)));
		options.TrustedCertificateAuthority = BindCertificate(configuration.GetSection(nameof(PulsarClientOptions.TrustedCertificateAuthority)));

		static X509Certificate2? BindCertificate(IConfigurationSection configSection) => new CertificateConfig(configSection).LoadCertificate();
	}

	// Inspired from https://github.com/dotnet/aspnetcore/blob/449abac6f1ca12fa0ad557a872c219fcdfae09f3/src/Servers/Kestrel/Core/src/Internal/Certificates/CertificateConfigLoader.cs#L13
	private sealed class CertificateConfig
	{
		public CertificateConfig(IConfiguration configSection) {
			Path = configSection[nameof(Path)];
			KeyPath = configSection[nameof(KeyPath)];
			Password = configSection[nameof(Password)];
		}

		public string? Path {
			get;
			init;
		}

		public string? KeyPath {
			get;
			init;
		}

		public string? Password {
			get;
			init;
		}

		public X509Certificate2? LoadCertificate() {
			if (!string.IsNullOrEmpty(Path)) {
				var fullChain = new X509Certificate2Collection();
				fullChain.ImportFromPemFile(Path);

				if (KeyPath != null) {
#pragma warning disable CA2000
					var certificate = new X509Certificate2(Path);
#pragma warning restore CA2000
					try {
						certificate = LoadCertificateKey(certificate, KeyPath, Password);
					} catch {
						certificate.Dispose();
						throw;
					}

					if (OperatingSystem.IsWindows()) {
						try {
							certificate = PersistKey(certificate);
						} catch {
							certificate.Dispose();
							throw;
						}
					}
					return certificate;
				}

				return new X509Certificate2(Path, Password);
			}

			return null;

			static X509Certificate2 LoadCertificateKey(X509Certificate2 certificate, string keyPath, string? password) {
				const string RSAOid = "1.2.840.113549.1.1.1";
				const string DSAOid = "1.2.840.10040.4.1";
				const string ECDsaOid = "1.2.840.10045.2.1";

				var keyText = File.ReadAllText(keyPath);
				switch (certificate.PublicKey.Oid.Value) {
					case RSAOid: {
						using var rsa = RSA.Create();
						ImportKeyFromFile(rsa, keyText, password);
						return certificate.CopyWithPrivateKey(rsa);
					}
					case ECDsaOid: {
						using var ecdsa = ECDsa.Create();
						ImportKeyFromFile(ecdsa, keyText, password);
						return certificate.CopyWithPrivateKey(ecdsa);
					}
					case DSAOid: {
						using var dsa = DSA.Create();
						ImportKeyFromFile(dsa, keyText, password);
						return certificate.CopyWithPrivateKey(dsa);
					}
					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unknown algorithm for certificate with public key type '{0}'", certificate.PublicKey.Oid.Value));
				}
			}

			static void ImportKeyFromFile(AsymmetricAlgorithm asymmetricAlgorithm, string keyText, string? password) {
				if (password == null) {
					asymmetricAlgorithm.ImportFromPem(keyText);
				} else {
					asymmetricAlgorithm.ImportFromEncryptedPem(keyText, password);
				}
			}

			static X509Certificate2 PersistKey(X509Certificate2 fullCertificate) {
				// We need to force the key to be persisted.
				// See https://github.com/dotnet/runtime/issues/23749
				var certificateBytes = fullCertificate.Export(X509ContentType.Pkcs12, "");
				return new X509Certificate2(certificateBytes, "", X509KeyStorageFlags.DefaultKeySet);
			}
		}
	}
}
