using System;
using System.Threading.Tasks;

using Docker.DotNet;
using Docker.DotNet.Models;

using Xunit;

namespace DockerProxy.Tests
{
	public class DockerClientExperiments
	{
		[Fact]
		public async Task Inspect()
		{
			var client = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

			var containers = await client.Containers.ListContainersAsync(new ContainersListParameters());

			Assert.Empty(containers);
		}
	}
}