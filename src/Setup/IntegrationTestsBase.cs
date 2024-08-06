using System.Reflection;
using Ardalis.GuardClauses;
using DbUp;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;

namespace IntegrationTesting.Setup;

public abstract class IntegrationTestsBase : IAsyncLifetime
{
    private DatabaseConfiguration config;
    private MsSqlContainer? container;

    public string? ConnectionString { get; private set; }

    public IntegrationTestsBase(DatabaseConfiguration config)
    {
        Guard.Against.Null(config);
        Guard.Against.NullOrEmpty(config.DbName);
        Guard.Against.NullOrEmpty(config.UserPassword);
        Guard.Against.Null(config.MigrationsAssembly);

        this.config = config;
    }

    public async Task DisposeAsync()
    {
        await container?.StopAsync();
    }

    public async Task InitializeAsync()
    {
        container = CreateContainer();

        await container.StartAsync();

        Setup(container);
    }

    private void Setup(MsSqlContainer container)
    {
        ConnectionString = GetTargetConnectionString(container);

        EnsureDatabase.For.SqlDatabase(ConnectionString);
        
        var upgrader = DeployChanges.To
                        .SqlDatabase(ConnectionString)
                        .WithScriptsEmbeddedInAssembly(config.MigrationsAssembly)
                        .LogToTrace()
                        .Build();

        var result = upgrader.PerformUpgrade();

        if (result.Error != null)
        {
            throw result.Error;
        }
    }

    private MsSqlContainer CreateContainer()
    {
        var builder = new MsSqlBuilder();

        return builder
                    .WithPassword(config.UserPassword)
                    .Build();
    }

    private string GetTargetConnectionString(MsSqlContainer container)
    {
        var server = $"{container.Hostname},{container.GetMappedPublicPort(MsSqlBuilder.MsSqlPort)}";

        var dictionary = new Dictionary<string, string>
        {
            { "Server", server },
            { "Database", config.DbName },
            { "User Id", MsSqlBuilder.DefaultUsername },
            { "Password", config.UserPassword },
            { "TrustServerCertificate", bool.TrueString }
        };

        return string.Join(";", dictionary.Select((KeyValuePair<string, string> property) => string.Join("=", property.Key, property.Value)));
    }
}
