using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers.Containers
{
    public class DockerClientFactory
    {
        private readonly DockerClientConfiguration _configuration;

        public DockerClientFactory()
        {
            _configuration = BuildDockerConfigBasedOnOs();
        }

        public IDockerClient Create()
        {
            return _configuration.CreateClient();
        }

        public static string GetDockerSocket()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "./pipe/docker_engine";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "/var/run/docker.sock";
            }

            throw new InvalidOperationException("OS is not supported for testcontainers-dotnet");
        }

        private static DockerClientConfiguration BuildDockerConfigBasedOnOs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));
            }

            throw new InvalidOperationException("OS is not supported for testcontainers-dotnet");
        }
    }
}