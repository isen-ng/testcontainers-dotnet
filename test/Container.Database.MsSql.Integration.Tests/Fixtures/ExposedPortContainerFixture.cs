using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.MsSql;
using Xunit;

namespace Container.Database.MsSql.Integration.Tests.Fixtures
{
    public class ExposedPortContainerFixture : IAsyncLifetime
    {
        public MsSqlContainer Container { get; }

        public string Username { get; } = "sa";

        public string Password { get; } = "Abcd1234!";

        public ExposedPortContainerFixture()
        {
            Container = new ContainerBuilder<MsSqlContainer>()
                //.ConfigureDockerImageName("mcr.microsoft.com/mssql/server:2017-latest-ubuntu")
                .ConfigureDockerImageName("justin2004/mssql_server_tiny")
                .ConfigureDatabaseConfiguration("not-used", Password, "not-used")
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
