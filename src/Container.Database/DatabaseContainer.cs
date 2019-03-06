using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Database.Hosting;

namespace TestContainers.Container.Database
{
    public class DatabaseContainer : GenericContainer
    {
        protected IDatabaseContext Context { get; }

        public virtual string DatabaseName => Context.DatabaseName;

        public virtual string Username => Context.Username;

        public virtual string Password => Context.Password;

        public DatabaseContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory,
            IDatabaseContext context)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
            Context = context;
        }
    }
}