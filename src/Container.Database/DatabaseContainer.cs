using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Database.Hosting;

namespace TestContainers.Container.Database
{
    public class DatabaseContainer : GenericContainer
    {
        protected IDatabaseContext Context { get; }

        public string DatabaseName => Context.DatabaseName;

        public string Username => Context.Username;

        public string Password => Context.Password;

        public DatabaseContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory,
            IDatabaseContext context)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
            Context = context;
        }
    }
}