using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers.Container.Abstractions.DockerClient
{
    /// <summary>
    /// Npipe socket docker client provider
    /// </summary>
    public class NpipeDockerClientProvider : AbstractDockerClientProvider
    {
        private const string Npipe = "npipe://./pipe/docker_engine";
        
        /// <summary>
        /// Default provider; default priority
        /// </summary>
        public const int Priority = DefaultPriority;

        /// <inheritdoc />
        public override string Description => $"local npipe: [{Npipe}]";

        /// <summary>
        /// Applicable if os is windows based
        /// </summary>
        public override bool IsApplicable =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private readonly DockerClientConfiguration _dockerConfiguration;

        /// <inheritdoc />
        public NpipeDockerClientProvider()
        {
            _dockerConfiguration =
                new DockerClientConfiguration(new Uri(Npipe));
        }

        /// <inheritdoc />
        protected override IDockerClient CreateDockerClient()
        {
            return _dockerConfiguration.CreateClient();
        }
        
        /// <inheritdoc />
        public override int GetPriority()
        {
            return Priority;
        }
        
        /// <inheritdoc />
        public override DockerClientConfiguration GetConfiguration()
        {
            return _dockerConfiguration;
        }
    }
}