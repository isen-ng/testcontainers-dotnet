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
                .ConfigureDockerImageName("mcr.microsoft.com/mssql/server:2017-latest-ubuntu")
                .ConfigureDatabaseConfiguration("not-used", Password, "not-used")
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    // minimum level set to trace to produce enough logs to make travis happy
                    // because this image is big and takes a long time to download
                    builder.SetMinimumLevel(LogLevel.Trace);
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
