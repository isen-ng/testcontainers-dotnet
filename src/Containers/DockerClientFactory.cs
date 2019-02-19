using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers.Containers
{
    public class DockerClientFactory
    {
        private static DockerClientFactory Instance { get; }

        static DockerClientFactory() {
            Instance = new DockerClientFactory();
        }

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
                return new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));
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