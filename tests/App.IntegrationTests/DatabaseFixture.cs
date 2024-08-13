using IntegrationTesting.Setup;
using System.Reflection;

namespace IntegrationTesting.App.IntegrationTests
{
	public class DatabaseFixture : DatabaseFixtureBase
	{
		protected override SetupConfiguration GetSetupConfiguration()
		{
			var dockerEndpoint = Startup.Configuration["dockerEndpoint"];

			return new SetupConfiguration
			{
				DatabaseName = "IntegrationTesting",
				DatabasePassword = "myStr0ngPassword!",
				MigrationsAssembly = Assembly.GetAssembly(typeof(Database.DbConfiguration)),
				DockerEndpoint = dockerEndpoint
			};
		}
	}
}