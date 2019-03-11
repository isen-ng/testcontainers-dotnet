using System;
using System.Data.Common;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Database.AdoNet.WaitStrategies;

namespace TestContainers.Container.Database.AdoNet
{
    /// <summary>
    /// Base class ADO.NET type db containers
    /// </summary>
    /// <inheritdoc />
    public abstract class AdoNetContainer : DatabaseContainer
    {
        /// <summary>
        /// Factory to create db connection instances
        /// </summary>
        protected abstract DbProviderFactory DbProviderFactory { get; }

        /// <inheritdoc />
        protected AdoNetContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            WaitStrategy = new AdoNetSqlProbeStrategy(DbProviderFactory);
        }

        /// <summary>
        /// Gets the connection string for this ADO.Net container after the container has started
        /// </summary>
        /// <returns>a connection string</returns>
        /// <exception cref="InvalidOperationException">when the container has yet to start</exception>
        public abstract string GetConnectionString();
    }
}