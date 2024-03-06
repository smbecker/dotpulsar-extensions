using DotPulsar.Abstractions;

namespace DotPulsar;

public static class PulsarClientBuilderExtensions
{
	public static IPulsarClientBuilder UseOptions(this IPulsarClientBuilder builder, PulsarClientOptions options) {
		options.Apply(builder);
		return builder;
	}
}
