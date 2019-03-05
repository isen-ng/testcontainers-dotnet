using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers.Container.Abstractions
{
    public class DockerClientFactory
    {
        private static readonly DockerClientConfiguration WindowsDockerConfiguration =
            new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));

        private static readonly DockerClientConfiguration UnixDockerConfiguration =
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));

        private readonly DockerClientConfiguration _configuration;

        public DockerClientFactory()
        {
            _configuration = BuildDockerConfigBasedOnOs();
        }

        public IDockerClient Create()
        {
            return _configuration.CreateClient();
        }

        private static DockerClientConfiguration BuildDockerConfigBasedOnOs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsDockerConfiguration;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return UnixDockerConfiguration;
            }

            throw new InvalidOperationException("OS is not supported for testcontainers-dotnet");
        }
    }
}