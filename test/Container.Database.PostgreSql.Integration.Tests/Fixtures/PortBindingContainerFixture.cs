using System.Threading.Tasks;
using Container.Test.Utility;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.PostgreSql;
using Xunit;

namespace Container.Database.PostgreSql.Integration.Tests.Fixtures
{
    public class PortBindingContainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }

        public string DatabaseName { get; } = "my_db";

        public string Username { get; } = "my_user";

        public string Password { get; } = "my_password";

        public int MyPort { get; } = FreePortHelper.GetFreePort();

        public PortBindingContainerFixture()
        {
            Container = new ContainerBuilder<PostgreSqlContainer>()
                .ConfigureDockerImageName("postgres:11-alpine")
                .ConfigureDatabaseConfiguration(Username, Password, DatabaseName)
                .ConfigureContainer((h, c) => { c.PortBindings.Add(PostgreSqlContainer.DefaultPort, MyPort); })
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
