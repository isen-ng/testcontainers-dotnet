using System.Data.Common;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Database.AdoNet.WaitStrategies;
using TestContainers.Container.Database.Hosting;

namespace TestContainers.Container.Database.AdoNet
{
    public abstract class AdoNetContainer : DatabaseContainer
    {
        protected abstract DbProviderFactory DbProviderFactory { get; }

        public AdoNetContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory,
            IDatabaseContext databaseContext)
            : base(dockerImageName, dockerClient, loggerFactory, databaseContext)
        {
        }

        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            WaitStrategy = new AdoNetSqlProbeStrategy(DbProviderFactory);
        }

        public abstract string GetConnectionString();
    }
}