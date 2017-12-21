using System.Linq;

using Docker.DotNet.Models;

namespace DockerProxy
{
	public static class DockerDotNetExtensions
	{
		public static string Name(this ContainerListResponse response)
		{
			return response.Names.FirstOrDefault();
		}
	}
}