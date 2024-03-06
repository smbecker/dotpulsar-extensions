using DotPulsar.Internal;

// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Metrics;

public static class PulsarMeterProviderBuilderExtensions
{
	public static MeterProviderBuilder AddPulsarInstrumentation(this MeterProviderBuilder builder) {
		if (builder is null) {
			throw new ArgumentNullException(nameof(builder));
		}

		builder.AddMeter(Constants.ClientName);
		return builder;
	}
}
