using System;
using System.Data.Common;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Npgsql;
using TestContainers.Container.Database.AdoNet;

namespace TestContainers.Container.Database.PostgreSql
{
    public class PostgreSqlContainer : AdoNetContainer
    {
        public new const string DefaultImage = "postgres";
        public new const string DefaultTag = "11-alpine";
        public const int DefaultPort = 5432;

        private string _connectionString;

        protected override DbProviderFactory DbProviderFactory { get; } = NpgsqlFactory.Instance;

        public PostgreSqlContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
        }

        public PostgreSqlContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }

        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            ExposedPorts.Add(DefaultPort);
            Env.Add("POSTGRES_DB", DatabaseName);
            Env.Add("POSTGRES_USER", Username);
            Env.Add("POSTGRES_PASSWORD", Password);
        }

        protected override Task ContainerStarted()
        {
            var builder = NpgsqlFactory.Instance.CreateConnectionStringBuilder();
            builder["server"] = GetDockerHostIpAddress();
            builder["port"] = GetMappedPort(DefaultPort);
            builder["database"] = DatabaseName;
            builder["username"] = Username;
            builder["password"] = Password;

            _connectionString = builder.ConnectionString;

            return Task.CompletedTask;
        }

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