using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArangoDB.Client;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.WaitStrategies;
using TestContainers.Container.Database.Hosting;

namespace TestContainers.Container.Database.ArangoDb
{
    public class ArangoDbContainer : DatabaseContainer
    {
        public new const string DefaultImage = "arangodb";
        public new const string DefaultTag = "3.4";
        public const int DefaultPort = 8529;
        
        private const string TestQueryString = "RETURN 1";
        
        public override string Username => "root";

        public override string DatabaseName => "_system";

        public ArangoDbContainer(IDockerClient dockerClient, 
            ILoggerFactory loggerFactory, IDatabaseContext context)
            : base($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory, context)
        {
        }
        
        public ArangoDbContainer(string dockerImageName, IDockerClient dockerClient, 
            ILoggerFactory loggerFactory, IDatabaseContext context)
            : base(dockerImageName, dockerClient, loggerFactory, context)
        {
        }

        protected override async Task ConfigureAsync()
        {
            if (string.IsNullOrEmpty(Password))
            {
                throw new InvalidOperationException("Root password cannot null or empty");
            }
            
            await base.ConfigureAsync();

            ExposedPorts.Add(DefaultPort);
            Env.Add("ARANGO_ROOT_PASSWORD", Password);
            
            WaitStrategy = new ProbingStrategy(Probe, 
                typeof(HttpRequestException), // when service isn't up yet
                typeof(InvalidOperationException)); // sometimes http server up but response still empty/null
        }

        public string GetArangoUrl()
        {
            return $"http://{GetDockerHostIpAddress()}:{GetMappedPort(DefaultPort)}";
        }
        
        private async Task Probe(IContainer container)
        {
            var settings = new DatabaseSharedSetting
            {
                Url = GetArangoUrl(),
                Database = DatabaseName,
                Credential = new NetworkCredential(Username, Password)
            };

            using (var db = new ArangoDatabase(settings))
            {
                await db.CreateStatement<int>(TestQueryString).ToListAsync();
            }
        }
    }
}