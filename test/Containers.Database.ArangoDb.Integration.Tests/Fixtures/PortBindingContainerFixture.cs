using System.Threading.Tasks;
using Container.Test.Utility;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.ArangoDb;
using TestContainers.Container.Database.Hosting;
using Xunit;

namespace Containers.Database.ArangoDb.Integration.Tests.Fixtures
{
    public class PortBindingContainerFixture : IAsyncLifetime
    {
        public ArangoDbContainer Container { get; }
        
        public string Username { get; } = "root";
        
        public string Password { get; } = "Acbd1234";
        
        public int MyPort { get; } = FreePortHelper.GetFreePort();
        
        public PortBindingContainerFixture()
        {
            Container = new ContainerBuilder<ArangoDbContainer>()
                .ConfigureDockerImageName("arangodb:3.4.3")
                .ConfigureDatabaseConfiguration("", Password, "")
                .ConfigureContainer((h, c) => { c.PortBindings.Add(ArangoDbContainer.DefaultPort, MyPort); })
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