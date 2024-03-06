using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using DotPulsar.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotPulsar;

public class PulsarServiceCollectionExtensionsTests
{
	[Fact]
	public async Task can_configure_pulsar_client() {
		var serviceUrl = new Uri("pulsar://localhost:1234");
		await using var services = new ServiceCollection()
			.AddSingleton<IConfiguration>(new ConfigurationBuilder()
				.AddInMemoryCollection(new[] {
					new KeyValuePair<string, string?>("Pulsar:ServiceUrl", serviceUrl.ToString())
				}).Build())
			.AddPulsarClient()
			.BuildServiceProvider();
		var client = services.GetRequiredService<IPulsarClient>();
		Assert.Equal(serviceUrl, client.ServiceUrl);
	}
}
