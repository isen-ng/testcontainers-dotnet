using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ArangoDB.Client;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Images;
using TestContainers.Container.Abstractions.WaitStrategies;

namespace TestContainers.Container.Database.ArangoDb
{
    /// <summary>
    /// ArangoDb container
    /// </summary>
    /// <inheritdoc />
    public class ArangoDbContainer : DatabaseContainer
    {
        /// <summary>
        /// Default image name
        /// </summary>
        public new const string DefaultImage = "arangodb";

        /// <summary>
        /// Default image tag
        /// </summary>
        public new const string DefaultTag = "3.4";

        /// <summary>
        /// Default port db is going to start in
        /// </summary>
        public const int DefaultPort = 8529;

        private const string TestQueryString = "RETURN 1";

        private static IImage CreateDefaultImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            return new GenericImage(dockerClient, loggerFactory) {ImageName = $"{DefaultImage}:{DefaultTag}"};
        }

        /// <inheritdoc />
        public override string Username => "root";

        /// <inheritdoc />
        public override string DatabaseName => "_system";

        /// <inheritdoc />
        public ArangoDbContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        public ArangoDbContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        [ActivatorUtilitiesConstructor]
        public ArangoDbContainer(IImage dockerImage, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(NullImage.IsNullImage(dockerImage) ? CreateDefaultImage(dockerClient, loggerFactory) : dockerImage,
                dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Gets the arango url required to make commands and queries
        /// </summary>
        /// <returns></returns>
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
