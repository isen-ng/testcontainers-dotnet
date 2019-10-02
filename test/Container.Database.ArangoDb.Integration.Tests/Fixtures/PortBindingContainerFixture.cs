using System.Threading.Tasks;
using Container.Test.Utility;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.ArangoDb;
using TestContainers.Container.Database.Hosting;
using Xunit;

namespace Container.Database.ArangoDb.Integration.Tests.Fixtures
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
                .ConfigureDatabaseConfiguration("not-used", Password, "not-used")
                .ConfigureContainer((h, c) => { c.PortBindings.Add(ArangoDbContainer.DefaultPort, MyPort); })
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
