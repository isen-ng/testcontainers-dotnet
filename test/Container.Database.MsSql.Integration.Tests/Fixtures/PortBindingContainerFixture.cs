using System.Threading.Tasks;
using Container.Test.Utility;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.MsSql;
using Xunit;

namespace Container.Database.MsSql.Integration.Tests.Fixtures
{
    public class PortBindingContainerFixture : IAsyncLifetime
    {
        public MsSqlContainer Container { get; }

        public string Username { get; } = "sa";

        public string Password { get; } = "!abcD1234";

        public int MyPort { get; } = FreePortHelper.GetFreePort();

        public PortBindingContainerFixture()
        {
            Container = new ContainerBuilder<MsSqlContainer>()
                //.ConfigureDockerImageName("mcr.microsoft.com/mssql/server:2017-latest-ubuntu")
                .ConfigureDockerImageName("postgres:11-alpine")
                .ConfigureDatabaseConfiguration("not-used", Password, "not-used")
                .ConfigureContainer((h, c) => { c.PortBindings.Add(MsSqlContainer.DefaultPort, MyPort); })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
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
