using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class PulsarServiceCollectionExtensions
{
	public static IServiceCollection AddPulsarOptions(this IServiceCollection services, string sectionName = PulsarClientOptions.SectionName) {
		services.AddOptions<PulsarClientOptions>().BindConfiguration(sectionName);
		return services;
	}

	public static IServiceCollection AddPulsarOptions(this IServiceCollection services, IConfigurationSection pulsarSection) {
		services.AddOptions<PulsarClientOptions>().Bind(pulsarSection);
		return services;
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, IPulsarClient pulsarClient) {
		services.TryAddSingleton(pulsarClient);
		return services;
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, Func<IServiceProvider, IPulsarClient> pulsarClientFactory) {
		services.TryAddSingleton(pulsarClientFactory);
		return services;
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, PulsarClientOptions? options = null) {
		return AddPulsarClient(services, (sp, builder) => {
			if (options == null) {
				options = sp.GetService<IOptions<PulsarClientOptions>>()?.Value;
			}
			builder.Configure(options);
		});
	}

	public static IServiceCollection AddPulsarClient(this IServiceCollection services, Action<PulsarClientOptions> configure) {
		services.AddOptions<PulsarClientOptions>().Configure(configure);
		return AddPulsarClient(services);
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

	public static IPulsarClientBuilder Configure(this IPulsarClientBuilder pulsarClientBuilder, PulsarClientOptions? options) {
		ArgumentNullException.ThrowIfNull(pulsarClientBuilder);
		options?.Apply(pulsarClientBuilder);
		return pulsarClientBuilder;
	}
}
