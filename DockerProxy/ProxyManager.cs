using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DockerProxy.Docker;

using Serilog;

namespace DockerProxy
{
	public class ProxyManager
	{
		private static readonly ILogger Logger = Log.ForContext<ProxyManager>();

		private readonly ConcurrentDictionary<string, Proxy> _proxies = new ConcurrentDictionary<string, Proxy>();

		private readonly CancellationToken _shutdown;

		public ProxyManager(CancellationToken shutdown)
		{
			_shutdown = shutdown;
		}

		public async Task ProcessContainersAsync(Container[] containers)
		{
			var added = containers.Where(c => !_proxies.ContainsKey(c.Id)).ToList();
			var removed = _proxies.Where(p => containers.All(c => c.Id != p.Key)).ToList();

			foreach (var remove in removed)
			{
				Logger.Information(
					"Removing Container: {@Container}",
					new { remove.Value.Container.Name, remove.Value.Container.Config.Image });

				if (!_proxies.TryRemove(remove.Key, out var proxy))
				{
					Logger.Warning("Error removing proxy for: {0}", remove.Key);
				}

				await proxy.CloseAsync();
			}

			foreach (var add in added)
			{
				Logger.Information("Adding Container: {@Container}", new { add.Name, add.Config.Image });
				var task = Task.Run(
					async () =>
						{
							using (var proxy = new Proxy(_shutdown))
							{
								_proxies[add.Id] = proxy;
								await proxy.OpenAsync(add);
								await proxy.Closed;
							}
						},
					_shutdown);
			}
		}

		public async Task StartAsync()
		{
			while (!_shutdown.IsCancellationRequested)
			{
				try
				{
					var containers = await DockerClient.ContainerListAsync();
					await ProcessContainersAsync(containers);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "Something went wrong");
				}
				finally
				{
					await Task.Delay(5000, _shutdown);
				}
			}
		}
	}
}