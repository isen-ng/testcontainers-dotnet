using System.Threading.Tasks;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.MySql;
using Xunit;

namespace Container.Database.MySql.Integration.Tests.Fixtures
{
    public abstract class ExposedPortContainerFixture : IAsyncLifetime
    {
        public MySqlContainer Container { get; }

        public string DatabaseName { get; } = "my_db";

        public string Username { get; } = "root";

        public string Password { get; } = "my_password";

        public ExposedPortContainerFixture(string dockerImageName)
        {
            Container = new ContainerBuilder<MySqlContainer>()
                .ConfigureDockerImageName(dockerImageName)
                .ConfigureDatabaseConfiguration(Username, Password, DatabaseName)
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

    public class MySqlExposedPortContainerFixture : ExposedPortContainerFixture
    {
        public MySqlExposedPortContainerFixture() : base("mysql:8")
        {
        }
    }

    public class MariaDbExposedPortContainerFixture : ExposedPortContainerFixture
    {
        public MariaDbExposedPortContainerFixture() : base("mariadb:10")
        {
        }
    }
}
