using DotPulsar.Internal;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Metrics;

public static class PulsarMeterProviderBuilderExtensions
{
	public static MeterProviderBuilder AddPulsarInstrumentation(this MeterProviderBuilder builder) {
		ArgumentNullException.ThrowIfNull(builder);
		builder.AddMeter(Constants.ClientName);
		return builder;
	}
}
