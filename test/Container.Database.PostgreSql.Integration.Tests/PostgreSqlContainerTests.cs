using System;
using System.Threading.Tasks;
using Container.Database.PostgreSql.Integration.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Npgsql;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.PostgreSql;
using Xunit;

namespace Container.Database.PostgreSql.Integration.Tests
{
    public class PostgreSqlContainerTests
    {
        public class DefaultImageTests
        {
            [Fact]
            public void ShouldUseDefaultImageWhenImageIsNotSpecified()
            {
                // arrange
                var container = new ContainerBuilder<PostgreSqlContainer>()
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
                Assert.Equal($"{PostgreSqlContainer.DefaultImage}:{PostgreSqlContainer.DefaultTag}", actual);
            }

            [Fact]
            public void ShouldUseConfiguredUsernamePasswordAndDatabase()
            {
                // arrange
                const string username = "user";
                const string password = "my pwd";
                const string database = "my db 1234";
                var container = new ContainerBuilder<PostgreSqlContainer>()
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

        public class ExposedPortTests : IClassFixture<ExposedPortContainerFixture>
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
                    $"Port={_fixture.Container.GetMappedPort(PostgreSqlContainer.DefaultPort)};" +
                    $"Database={_fixture.DatabaseName};Username={_fixture.Username};Password={_fixture.Password}";

                // act
                var ex = await ProbeForException(connectionString);

                // assert
                Assert.Null(ex);
            }
        }

        public class PortBindingTests : IClassFixture<PortBindingContainerFixture>
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

        private static async Task<Exception> ProbeForException(string connectionString)
        {
            using (var connection = new NpgsqlConnection(connectionString))
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