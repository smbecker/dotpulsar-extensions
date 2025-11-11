using DotPulsar.Internal;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

public static class PulsarTracerProviderBuilderExtensions
{
	public static TracerProviderBuilder AddPulsarInstrumentation(this TracerProviderBuilder builder) {
#if NET7_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
#else
		if (builder is null) {
			throw new ArgumentNullException(nameof(builder));
		}
#endif
		builder.AddSource(Constants.ClientName);
		return builder;
	}
}
