using System.Threading.Tasks;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.ArangoDb;
using TestContainers.Container.Database.Hosting;
using Xunit;

namespace Containers.Database.ArangoDb.Integration.Tests.Fixtures
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
                .ConfigureDatabaseConfiguration("", Password, "")
                .Build();
        }

        public Task InitializeAsync()
        {
            return Container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return Container.StopAsync();
        }
    }
}