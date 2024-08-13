using Microsoft.Extensions.Configuration;

namespace IntegrationTesting.App.IntegrationTests
{
	internal static class Startup
	{
		static Startup()
		{
			Configuration = new ConfigurationBuilder()
					.AddJsonFile("appSettings.json", optional: false)
					.Build();
		}

		public static IConfiguration Configuration { get; private set; }
	}
}