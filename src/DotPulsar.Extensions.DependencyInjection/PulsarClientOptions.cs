using DotPulsar.Abstractions;
using DotPulsar.Internal;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DotPulsar;

public class PulsarClientOptions
{
	internal const string SectionName = "Pulsar";

	/// <summary>
	/// The service URL for the Pulsar cluster. The default is "pulsar://localhost:6650".
	/// </summary>
	public Uri? ServiceUrl {
		get;
		set;
	}

	/// <summary>
	/// Authenticate using a client certificate. This is optional.
	/// </summary>
	public PulsarClientCertificate? AuthenticateUsingClientCertificate {
		get;
		set;
	}

	/// <summary>
	/// Authenticate using a token. This is optional.
	/// </summary>
	public string? AuthenticationToken {
		get;
		set;
	}

	/// <summary>
	/// Specifies whether the certificate revocation list is checked during authentication. The default is 'true'.
	/// </summary>
	public bool? CheckCertificateRevocation {
		get;
		set;
	}

	/// <summary>
	/// Set connection encryption policy. The default is 'EnforceUnencrypted' if the ServiceUrl scheme is 'pulsar' and 'EnforceEncrypted' if it's 'pulsar+ssl'.
	/// </summary>
	public EncryptionPolicy? ConnectionSecurity {
		get;
		set;
	}

	/// <summary>
	/// The time to wait before sending a 'ping' if there has been no activity on the connection. The default is 30 seconds.
	/// </summary>
	public TimeSpan? KeepAliveInterval {
		get;
		set;
	}

	/// <summary>
	/// Set the listener name. This is optional.
	/// </summary>
	public string? ListenerName {
		get;
		set;
	}

	/// <summary>
	/// The time to wait before retrying an operation or a reconnect. The default is 3 seconds.
	/// </summary>
	public TimeSpan? RetryInterval {
		get;
		set;
	}

	/// <summary>
	/// Add a trusted certificate authority. This is optional.
	/// </summary>
	public PulsarClientCertificate? TrustedCertificateAuthority {
		get;
		set;
	}

	/// <summary>
	/// Verify the certificate authority. The default is 'true'.
	/// </summary>
	public bool? VerifyCertificateAuthority {
		get;
		set;
	}

	/// <summary>
	/// Verify the certificate name with the hostname. The default is 'false'.
	/// </summary>
	public bool? VerifyCertificateName {
		get;
		set;
	}

	/// <summary>
	/// The time to wait before closing inactive connections. The default is 60 seconds.
	/// </summary>
	public TimeSpan? CloseInactiveConnectionsInterval {
		get;
		set;
	}

	public void Apply(IPulsarClientBuilder builder) {
		ArgumentNullException.ThrowIfNull(builder);

		if (ServiceUrl != null) {
			builder.ServiceUrl(ServiceUrl);
		}

#pragma warning disable CA2000
		var authenticateUsingClientCertificate = AuthenticateUsingClientCertificate?.Load();
#pragma warning restore CA2000
		if (authenticateUsingClientCertificate != null) {
			builder.AuthenticateUsingClientCertificate(authenticateUsingClientCertificate);
			builder.ConnectionSecurity(EncryptionPolicy.PreferEncrypted);
		}

		if (AuthenticationToken != null) {
			builder.Authentication(AuthenticationFactory.Token(AuthenticationToken));
			builder.ConnectionSecurity(EncryptionPolicy.PreferEncrypted);
		}

		if (CheckCertificateRevocation != null) {
			builder.CheckCertificateRevocation(CheckCertificateRevocation.Value);
		}

		if (ConnectionSecurity != null) {
			builder.ConnectionSecurity(ConnectionSecurity.Value);
		}

		if (KeepAliveInterval != null) {
			builder.KeepAliveInterval(KeepAliveInterval.Value);
		}

		if (ListenerName != null) {
			builder.ListenerName(ListenerName);
		}

		if (RetryInterval != null) {
			builder.RetryInterval(RetryInterval.Value);
		}

#pragma warning disable CA2000
		var trustedCertificateAuthority = TrustedCertificateAuthority?.Load();
#pragma warning restore CA2000
		if (trustedCertificateAuthority != null) {
			builder.TrustedCertificateAuthority(trustedCertificateAuthority);
		}

		if (VerifyCertificateAuthority != null) {
			builder.VerifyCertificateAuthority(VerifyCertificateAuthority.Value);
		}

		if (VerifyCertificateName != null) {
			builder.VerifyCertificateName(VerifyCertificateName.Value);
		}

		if (CloseInactiveConnectionsInterval != null) {
			builder.CloseInactiveConnectionsInterval(CloseInactiveConnectionsInterval.Value);
		}
	}
}
