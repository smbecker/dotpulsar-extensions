using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DotPulsar.Internal;

// Inspired from https://github.com/dotnet/aspnetcore/blob/449abac6f1ca12fa0ad557a872c219fcdfae09f3/src/Servers/Kestrel/Core/src/Internal/Certificates/CertificateConfigLoader.cs#L13

/// <summary>
/// Represents a client certificate.
/// </summary>
public sealed class PulsarClientCertificate
{
	/// <summary>
	/// The path to the certificate file.
	/// </summary>
	public string? Path {
		get;
		set;
	}

	/// <summary>
	/// The path to the certificate key file.
	/// </summary>
	public string? KeyPath {
		get;
		set;
	}

	/// <summary>
	/// The password for the certificate key file.
	/// </summary>
	public string? Password {
		get;
		init;
	}

	public X509Certificate2? Load() {
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
