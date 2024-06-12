using DotPulsar.Internal;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

public static class PulsarTracerProviderBuilderExtensions
{
	public static TracerProviderBuilder AddPulsarInstrumentation(this TracerProviderBuilder builder) {
		ArgumentNullException.ThrowIfNull(builder);
		builder.AddSource(Constants.ClientName);
		return builder;
	}
}
