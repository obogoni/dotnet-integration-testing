using System.Reflection;

namespace IntegrationTesting.Setup;

public class SetupConfiguration
{
	public string DatabaseName { get; set; } = null!;

	public string DatabasePassword { get; set; } = null!;

	public string? DockerEndpoint { get; set; }

	public Assembly MigrationsAssembly { get; set; } = null!;
}