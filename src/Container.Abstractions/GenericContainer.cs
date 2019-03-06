using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Reaper;

namespace TestContainers.Container.Abstractions
{
    public class GenericContainer : AbstractContainer
    {
        public const string DefaultImage = "alpine";
        public const string DefaultTag = "3.5";
        
        private readonly ILogger _logger;

        public GenericContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : this($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }
        
        public GenericContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        protected override async Task ContainerStarting()
        {
            await base.ContainerStarting();

            _logger.LogDebug("Starting reaper ...");
            await ResourceReaper.StartAsync(DockerClient);
        }

        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();
            
            _logger.LogDebug("Adding session labels to generic container: " + ResourceReaper.SessionId);

            foreach (var label in ResourceReaper.Labels)
            {
                Labels.Add(label.Key, label.Value);
            }
        }
    }
}