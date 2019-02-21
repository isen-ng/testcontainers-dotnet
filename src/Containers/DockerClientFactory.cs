using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers.Containers
{
    public class DockerClientFactory
    {
        public static readonly string TestContainerLabelName = typeof(IContainer).AssemblyQualifiedName;
        public static readonly string TestContainerSessionLabelName = TestContainerLabelName + ".SessionId";
        public static readonly string TestContainerAssemblyLabelName = TestContainerLabelName + ".EntryAssembly";

        public static readonly string SessionId = Guid.NewGuid().ToString();

        public static Dictionary<string, string> DefaultLabels = new Dictionary<string, string>
        {
            {TestContainerLabelName, "true"},
            {TestContainerSessionLabelName, SessionId},
            {TestContainerAssemblyLabelName, Assembly.GetEntryAssembly().FullName}
        };
        
        private static DockerClientFactory Instance { get; }

        static DockerClientFactory()
        {
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