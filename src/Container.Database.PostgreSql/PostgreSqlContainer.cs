using System;
using System.Data.Common;
using System.Threading.Tasks;
using Container.Database.AdoNet;
using Container.Database.Hosting;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Container.Database.PostgreSql
{
    public class PostgreSqlContainer : AdoNetContainer
    {
        public const string Image = "postgres";
        public const string DefaultTag = "11-alpine";
        public const int PostgreSqlPort = 5432;

        private string _connectionString;
        
        protected override DbProviderFactory DbProviderFactory { get; } = NpgsqlFactory.Instance;

        public PostgreSqlContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory, 
            IDatabaseContext context) 
            : base(dockerImageName, dockerClient, loggerFactory, context)
        {
        }
        
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();
            
            ExposedPorts.Add(PostgreSqlPort);
            Env.Add("POSTGRES_DB", DatabaseName);
            Env.Add("POSTGRES_USER", Username);
            Env.Add("POSTGRES_PASSWORD", Password);
        }

        protected override Task ContainerStarted()
        {
            var builder = NpgsqlFactory.Instance.CreateConnectionStringBuilder();
            builder["server"] = GetDockerHostIpAddress();
            builder["port"] = GetMappedPort(PostgreSqlPort);
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