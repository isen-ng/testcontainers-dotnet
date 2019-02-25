using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Reaper;

namespace TestContainers.Containers
{
    public class GenericContainer : AbstractContainer
    {
        private readonly ILogger _logger;

        public GenericContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        protected override async Task ContainerStarting()
        {
            await base.ContainerStarting();

            _logger.LogDebug("Starting reaper ...");
            await ResourceReaper.Start(DockerClient);
        }

        protected override Task ConfigureAsync()
        {
            _logger.LogDebug("Adding session labels to generic container: " + ResourceReaper.SessionId);

            foreach (var label in ResourceReaper.Labels)
            {
                Labels.Add(label.Key, label.Value);
            }

            return Task.CompletedTask;
        }
    }
}