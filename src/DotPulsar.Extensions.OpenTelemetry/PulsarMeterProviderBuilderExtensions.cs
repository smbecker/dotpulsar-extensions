using DotPulsar.Internal;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Metrics;

public static class PulsarMeterProviderBuilderExtensions
{
	public static MeterProviderBuilder AddPulsarInstrumentation(this MeterProviderBuilder builder) {
#if NET7_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(builder);
#else
		if (builder is null) {
			throw new ArgumentNullException(nameof(builder));
		}
#endif
		builder.AddMeter(Constants.ClientName);
		return builder;
	}
}
