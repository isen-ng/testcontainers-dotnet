using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace TestContainers.Container.Abstractions.Images
{
    /// <inheritdoc />
    public abstract class AbstractImage : IImage
    {
        /// <inheritdoc />
        public string ImageName { get; set; }

        /// <inheritdoc />
        public string ImageId { get; protected set; }

        internal readonly IDockerClient DockerClient;

        private readonly ILogger _logger;

        /// <inheritdoc />
        protected AbstractImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            DockerClient = dockerClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public abstract Task<string> Resolve(CancellationToken ct = default);
    }
}
