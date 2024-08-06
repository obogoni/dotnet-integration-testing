using System.Reflection;

namespace IntegrationTesting.Setup;

public class DatabaseConfiguration
{
    public string DbName { get; set; } = null!;

    public string UserPassword { get; set; } = null!; 

    public Assembly MigrationsAssembly { get; set; } = null!;
}
