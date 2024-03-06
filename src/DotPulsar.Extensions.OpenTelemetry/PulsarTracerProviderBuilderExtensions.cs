using DotPulsar.Internal;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

public static class PulsarTracerProviderBuilderExtensions
{
	public static TracerProviderBuilder AddPulsarInstrumentation(this TracerProviderBuilder builder) {
		if (builder is null) {
			throw new ArgumentNullException(nameof(builder));
		}

		builder.AddSource(Constants.ClientName);
		return builder;
	}
}
