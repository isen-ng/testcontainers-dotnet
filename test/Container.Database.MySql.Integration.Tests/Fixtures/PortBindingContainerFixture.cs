using System.Threading.Tasks;
using Container.Test.Utility;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.MySql;
using Xunit;

namespace Container.Database.MySql.Integration.Tests.Fixtures
{
    public abstract class PortBindingContainerFixture : IAsyncLifetime
    {
        public MySqlContainer Container { get; }

        public string DatabaseName { get; } = "my_db";

        public string Username { get; } = "my_user";

        public string Password { get; } = "my_password";

        public int MyPort { get; } = FreePortHelper.GetFreePort();

        protected PortBindingContainerFixture(string dockerImageName)
        {
            Container = new ContainerBuilder<MySqlContainer>()
                .ConfigureDockerImageName(dockerImageName)
                .ConfigureDatabaseConfiguration(Username, Password, DatabaseName)
                .ConfigureContainer((h, c) => { c.PortBindings.Add(MySqlContainer.DefaultPort, MyPort); })
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

    public class MySqlPortBindingContainerFixture : PortBindingContainerFixture
    {
        public MySqlPortBindingContainerFixture() : base("mysql:8")
        {
        }
    }

    public class MariaDbPortBindingContainerFixture : PortBindingContainerFixture
    {
        public MariaDbPortBindingContainerFixture() : base("mariadb:10")
        {
        }
    }
}
