using System;
using System.Threading.Tasks;
using Container.Database.MySql.Integration.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.MySql;
using Xunit;

namespace Container.Database.MySql.Integration.Tests
{
    public class MySqlContainerTests
    {
        public class DefaultImageTests
        {
            [Fact]
            public void ShouldUseDefaultImageWhenImageIsNotSpecified()
            {
                // arrange
                var container = new ContainerBuilder<MySqlContainer>()
                    .ConfigureDatabaseConfiguration("not-important", "not-important", "not-important")
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Debug);
                    })
                    .Build();

                // act
                var actual = container.DockerImageName;

                // assert
                Assert.Equal($"{MySqlContainer.DefaultImage}:{MySqlContainer.DefaultTag}", actual);
            }

            [Fact]
            public void ShouldUseConfiguredUsernamePasswordAndDatabase()
            {
                // arrange
                const string username = "user";
                const string password = "my pwd";
                const string database = "my db 1234";
                var container = new ContainerBuilder<MySqlContainer>()
                    .ConfigureDatabaseConfiguration(username, password, database)
                    .ConfigureLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Debug);
                    })
                    .Build();

                // act
                var actualUsername = container.Username;
                var actualPassword = container.Password;
                var actualDatabase = container.DatabaseName;

                // assert
                Assert.Equal(username, actualUsername);
                Assert.Equal(password, actualPassword);
                Assert.Equal(database, actualDatabase);
            }
        }

        public abstract class ExposedPortTests
        {
            private readonly ExposedPortContainerFixture _fixture;

            public ExposedPortTests(ExposedPortContainerFixture fixture)
            {
                _fixture = fixture;
            }

            [Fact]
            public async Task CanQueryContainerUsingProvidedConnectionString()
            {
                // act
                var ex = await ProbeForException(_fixture.Container.GetConnectionString());

                // assert
                Assert.Null(ex);
            }

            [Fact]
            public async Task CanQueryContainerUsingConstructedConnectionString()
            {
                // arrange
                var connectionString =
                    $"Server={_fixture.Container.GetDockerHostIpAddress()};" +
                    $"Port={_fixture.Container.GetMappedPort(MySqlContainer.DefaultPort)};" +
                    $"Database={_fixture.DatabaseName};Username={_fixture.Username};Password={_fixture.Password}";

                // act
                var ex = await ProbeForException(connectionString);

                // assert
                Assert.Null(ex);
            }
        }

        public class MySqlExposedPortTests : ExposedPortTests, IClassFixture<MySqlExposedPortContainerFixture>
        {
            public MySqlExposedPortTests(MySqlExposedPortContainerFixture fixture) : base(fixture)
            {
            }
        }

        public class MariaDbExposedPortTests : ExposedPortTests, IClassFixture<MariaDbExposedPortContainerFixture>
        {
            public MariaDbExposedPortTests(MariaDbExposedPortContainerFixture fixture) : base(fixture)
            {
            }
        }

        public abstract class PortBindingTests
        {
            private readonly PortBindingContainerFixture _fixture;

            public PortBindingTests(PortBindingContainerFixture fixture)
            {
                _fixture = fixture;
            }

            [Fact]
            public async Task CanQueryContainerUsingProvidedConnectionString()
            {
                // act
                var ex = await ProbeForException(_fixture.Container.GetConnectionString());

                // assert
                Assert.Null(ex);
            }

            [Fact]
            public async Task CanQueryContainerUsingConstructedConnectionString()
            {
                // arrange
                var connectionString =
                    $"Server={_fixture.Container.GetDockerHostIpAddress()};" +
                    $"Port={_fixture.MyPort};Database={_fixture.DatabaseName};" +
                    $"Username={_fixture.Username};Password={_fixture.Password}";

                // act
                var ex = await ProbeForException(connectionString);

                // assert
                Assert.Null(ex);
            }
        }

        public class MySqlPortBindingTests : PortBindingTests, IClassFixture<MySqlPortBindingContainerFixture>
        {
            public MySqlPortBindingTests(MySqlPortBindingContainerFixture fixture) : base(fixture)
            {
            }
        }

        public class MariaDbPortBindingTests : PortBindingTests, IClassFixture<MariaDbPortBindingContainerFixture>
        {
            public MariaDbPortBindingTests(MariaDbPortBindingContainerFixture fixture) : base(fixture)
            {
            }
        }

        private static async Task<Exception> ProbeForException(string connectionString)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                return await Record.ExceptionAsync(async () =>
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT 1";
                        await command.ExecuteScalarAsync();
                    }
                });
            }
        }
    }
}
