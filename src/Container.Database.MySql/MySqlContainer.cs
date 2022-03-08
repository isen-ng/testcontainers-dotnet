using System;
using System.Data.Common;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Images;
using TestContainers.Container.Database.AdoNet;

namespace TestContainers.Container.Database.MySql
{
    /// <summary>
    /// MySql db container
    /// </summary>
    /// <inheritdoc />
    public class MySqlContainer : AdoNetContainer
    {
        /// <summary>
        /// Default image name
        /// </summary>
        public new const string DefaultImage = "docker.io/mysql";

        /// <summary>
        /// Default image tag
        /// </summary>

        public new const string DefaultTag = "8";

        /// <summary>
        /// Default db port
        /// </summary>
        public const int DefaultPort = 3306;

        private static IImage CreateDefaultImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            return new GenericImage(dockerClient, loggerFactory) {ImageName = $"{DefaultImage}:{DefaultTag}"};
        }

        private string _connectionString;

        /// <inheritdoc />
        protected override DbProviderFactory DbProviderFactory { get; } = ClientFactoryAccessor.ClientFactoryInstance;

        /// <inheritdoc />
        public MySqlContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        public MySqlContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        [ActivatorUtilitiesConstructor]
        public MySqlContainer(IImage dockerImage, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(NullImage.IsNullImage(dockerImage) ? CreateDefaultImage(dockerClient, loggerFactory) : dockerImage,
                dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            ExposedPorts.Add(DefaultPort);
            Env.Add("MYSQL_DATABASE", DatabaseName);
            Env.Add("MYSQL_ALLOW_EMPTY_PASSWORD", "yes");

            if (Username == "root")
            {
                Env.Add("MYSQL_ROOT_PASSWORD", Password);
            }
            else
            {
                Env.Add("MYSQL_USER", Username);
                Env.Add("MYSQL_PASSWORD", Password);
            }
        }

        /// <inheritdoc />
        protected override Task ContainerStarted()
        {
            var builder = DbProviderFactory.CreateConnectionStringBuilder();
            builder["server"] = GetDockerHostIpAddress();
            builder["port"] = GetMappedPort(DefaultPort);
            builder["database"] = DatabaseName;
            builder["username"] = Username;
            builder["password"] = Password;

            _connectionString = builder.ConnectionString;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override string GetConnectionString()
        {
            if (_connectionString == null)
            {
                throw new InvalidOperationException("Container must be started before the connection string is ready");
            }

            return _connectionString;
        }
    }
}
