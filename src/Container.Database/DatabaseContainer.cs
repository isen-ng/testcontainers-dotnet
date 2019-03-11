using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Database.Hosting;

namespace TestContainers.Container.Database
{
    /// <summary>
    /// Convenience container for db type containers
    /// </summary>
    public class DatabaseContainer : GenericContainer
    {
        /// <summary>
        /// Database name
        /// </summary>
        public virtual string DatabaseName { get; set; }
        
        /// <summary>
        /// Database username
        /// </summary>
        public virtual string Username { get; set; }
        
        /// <summary>
        /// Database password
        /// </summary>
        public virtual string Password { get; set; }

        /// <inheritdoc />
        public DatabaseContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }
    }
}