using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Database.Hosting;

namespace TestContainers.Container.Database
{
    public class DatabaseContainer : GenericContainer
    {
        public virtual string DatabaseName { get; set; }
        
        public virtual string Username { get; set; }
        
        public virtual string Password { get; set; }

        public DatabaseContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }
    }
}