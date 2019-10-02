using System.Threading.Tasks;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.PostgreSql;
using Xunit;

namespace Container.Database.PostgreSql.Integration.Tests.Fixtures
{
    public class ExposedPortContainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; }

        public string DatabaseName { get; } = "my_db";

        public string Username { get; } = "my_user";

        public string Password { get; } = "my_password";

        public ExposedPortContainerFixture()
        {
            Container = new ContainerBuilder<PostgreSqlContainer>()
                .ConfigureDockerImageName("postgres:11-alpine")
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
}
