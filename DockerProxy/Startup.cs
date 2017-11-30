using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Events;

namespace DockerProxy
{
	public class Startup
	{
		public Startup(string env)
		{
			var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{env}.json", true);
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		public Task Start(CancellationToken token)
		{
			ConfigureLogging();

			var proxyManager = new ProxyManager(token);
			return proxyManager.StartAsync();
		}

		private void ConfigureLogging()
		{
			var config = new LoggerConfiguration().MinimumLevel.Debug().Enrich.FromLogContext().WriteTo.LiterateConsole(
				LogEventLevel.Debug,
				"{Timestamp:HH:mm:ss.fff} [{ThreadId}] [{Level}] {Message}{NewLine}{Exception}");

			Log.Logger = config.CreateLogger();
		}
	}
}