using System;

namespace DockerProxy.Docker
{
	public class Container
	{
		public ContainerConfig Config { get; set; }

		public DateTime Created { get; set; }

		public HostConfig HostConfig { get; set; }

		public string Id { get; set; }

		public string Image { get; set; }

		public string Name { get; set; }

		public NetworkSettings NetworkSettings { get; set; }

		public override string ToString()
		{
			return $"{Name} ({string.Join(", ", HostConfig.PortBindings.Keys)})";
		}
	}
}