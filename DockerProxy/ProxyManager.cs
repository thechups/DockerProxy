using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using Serilog;

namespace DockerProxy
{
	public class ProxyManager : IDisposable
	{
		private static readonly ILogger Logger = Log.ForContext<ProxyManager>();

		private readonly DockerClient _dockerClient;

		private readonly ConcurrentDictionary<string, Proxy> _proxies = new ConcurrentDictionary<string, Proxy>();

		private readonly CancellationToken _shutdown;

		public ProxyManager(CancellationToken shutdown)
		{
			_shutdown = shutdown;
			_dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
		}

		public void Dispose()
		{
			_dockerClient?.Dispose();
		}

		public async Task ProcessContainersAsync(IList<ContainerListResponse> containers)
		{
			var added = containers.Where(c => !_proxies.ContainsKey(c.ID)).ToList();
			var removed = _proxies.Where(p => containers.All(c => c.ID != p.Key)).ToList();

			foreach (var remove in removed)
			{
				Logger.Information(
					"Removing Container: {@Container}",
					new { Name = remove.Value.Container.Name(), remove.Value.Container.Image });

				if (!_proxies.TryRemove(remove.Key, out var proxy))
				{
					Logger.Warning("Error removing proxy for: {0}", remove.Key);
				}

				await proxy.CloseAsync();
			}

			foreach (var add in added)
			{
				Logger.Information("Adding Container: {@Container}", new { Name = add.Name(), add.Image });
				var task = Task.Run(
					async () =>
						{
							using (var proxy = new Proxy(_shutdown))
							{
								_proxies[add.ID] = proxy;
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
					var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters(), _shutdown);
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