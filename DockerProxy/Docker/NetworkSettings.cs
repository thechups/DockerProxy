using System.Collections.Generic;

namespace DockerProxy.Docker
{
	public class NetworkSettings
	{
		public Dictionary<string, Network> Networks { get; set; }
	}
}