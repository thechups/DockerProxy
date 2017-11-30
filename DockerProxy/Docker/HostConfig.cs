using System.Collections.Generic;

namespace DockerProxy.Docker
{
	public class HostConfig
	{
		public Dictionary<string, PortBinding[]> PortBindings { get; set; }
	}
}