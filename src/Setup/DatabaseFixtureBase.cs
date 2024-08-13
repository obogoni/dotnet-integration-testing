using Ardalis.GuardClauses;
using DbUp;
using Testcontainers.MsSql;

namespace IntegrationTesting.Setup
{
	public abstract class DatabaseFixtureBase : IDisposable, IAsyncDisposable
	{
		private MsSqlContainer? container;

		public SetupConfiguration Configuration { get; private set; }

		public string? ConnectionString { get; private set; }

		protected DatabaseFixtureBase()
		{
			Configuration = GetSetupConfiguration();

			Guard.Against.Null(Configuration);
			Guard.Against.NullOrEmpty(Configuration.DatabaseName);
			Guard.Against.NullOrEmpty(Configuration.DatabasePassword);
			Guard.Against.Null(Configuration.MigrationsAssembly);

			Initialize();
		}

		protected abstract SetupConfiguration GetSetupConfiguration();

		private void Initialize()
		{
			container = CreateContainer();

			container.StartAsync().GetAwaiter().GetResult();

			Setup(container);
		}

		private void Setup(MsSqlContainer container)
		{
			ConnectionString = GetTargetConnectionString(container);

			EnsureDatabase.For.SqlDatabase(ConnectionString);

			var upgrader = DeployChanges.To
								.SqlDatabase(ConnectionString)
												.WithScriptsEmbeddedInAssembly(Configuration.MigrationsAssembly)
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
			var builder = new MsSqlBuilder()
								.WithPassword(Configuration.DatabasePassword);

			if (string.IsNullOrEmpty(Configuration.DockerEndpoint))
			{
				return builder.Build();
			}
			else
			{
				return builder
						.WithDockerEndpoint(Configuration.DockerEndpoint)
						.Build();
			}
		}

		private string GetTargetConnectionString(MsSqlContainer container)
		{
			var server = $"{container.Hostname},{container.GetMappedPublicPort(MsSqlBuilder.MsSqlPort)}";

			var dictionary = new Dictionary<string, string>
			{
				{ "Server", server },
			{ "Database", Configuration.DatabaseName },
			{ "User Id", MsSqlBuilder.DefaultUsername },
			{ "Password", Configuration.DatabasePassword },
			{ "TrustServerCertificate", bool.TrueString }
		};

			return string.Join(";", dictionary.Select((KeyValuePair<string, string> property) => string.Join("=", property.Key, property.Value)));
		}

		public virtual void Dispose()
		{
		}

		public async ValueTask DisposeAsync()
		{
			await container?.StopAsync();
		}
	}
}