using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Images;
using TestContainers.Container.Abstractions.Reaper;

namespace TestContainers.Container.Abstractions
{
    /// <summary>
    /// Generic implementation for a container. This can be used to start any container.
    /// </summary>
    /// <inheritdoc />
    public class GenericContainer : AbstractContainer
    {
        /// <summary>
        /// Default container image name to use if none is supplied
        /// </summary>
        public const string DefaultImage = "alpine";

        /// <summary>
        /// Default image tag to use if none is supplied
        /// </summary>
        public const string DefaultTag = "3.5";

        private static IImage CreateDefaultImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            return new GenericImage(dockerClient, loggerFactory) {ImageName = $"{DefaultImage}:{DefaultTag}"};
        }

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <inheritdoc />
        public GenericContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : this($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public GenericContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        [ActivatorUtilitiesConstructor]
        public GenericContainer(IImage dockerImage, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(NullImage.IsNullImage(dockerImage) ? CreateDefaultImage(dockerClient, loggerFactory) : dockerImage,
                dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        protected override async Task ContainerStarting()
        {
            await base.ContainerStarting();

            _logger.LogDebug("Starting reaper ...");
            await ResourceReaper.StartAsync(DockerClient, _loggerFactory);
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            _logger.LogDebug("Adding session labels to generic container: {}", ResourceReaper.SessionId);

            foreach (var label in ResourceReaper.Labels)
            {
                Labels.Add(label.Key, label.Value);
            }
        }
    }
}
