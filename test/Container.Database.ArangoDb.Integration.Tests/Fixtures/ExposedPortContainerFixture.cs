using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.ArangoDb;
using TestContainers.Container.Database.Hosting;
using Xunit;

namespace Container.Database.ArangoDb.Integration.Tests.Fixtures
{
    public class ExposedPortContainerFixture : IAsyncLifetime
    {
        public ArangoDbContainer Container { get; }

        public string Username { get; } = "root";

        public string Password { get; } = "Acbd1234";

        public ExposedPortContainerFixture()
        {
            Container = new ContainerBuilder<ArangoDbContainer>()
                .ConfigureDockerImageName("arangodb:3.4.3")
                .ConfigureDatabaseConfiguration("not-used", Password, "not-used")
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
        }
    }
}
