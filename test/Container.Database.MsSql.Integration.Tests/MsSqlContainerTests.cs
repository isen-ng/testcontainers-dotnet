using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Container.Database.MsSql.Integration.Tests.Fixtures;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.MsSql;
using Xunit;

namespace Container.Database.MsSql.Integration.Tests
{
    public class MsSqlContainerTests
    {
        public class DefaultImageTests
        {
            [Fact]
            public void ShouldUseDefaultImageWhenImageIsNotSpecified()
            {
                // arrange
                var container = new ContainerBuilder<MsSqlContainer>()
                    .ConfigureDatabaseConfiguration("not-important", "not-important", "not-important")
                    .Build();

                // act
                var actual = container.DockerImageName;

                // assert
                Assert.Equal($"{MsSqlContainer.DefaultImage}:{MsSqlContainer.DefaultTag}", actual);
            }

            [Fact]
            public void ShouldUseConfiguredPassword()
            {
                // arrange
                const string username = "user";
                const string password = "my pwd";
                const string database = "my db 1234";
                var container = new ContainerBuilder<MsSqlContainer>()
                    .ConfigureDatabaseConfiguration(username, password, database)
                    .Build();

                // act
                var actualUsername = container.Username;
                var actualPassword = container.Password;

                // assert
                Assert.Equal("sa", actualUsername);
                Assert.Equal(password, actualPassword);
            }
        }

        public class GetConnectionStringTests : IClassFixture<ExposedPortContainerFixture>
        {
            private readonly ExposedPortContainerFixture _fixture;

            public GetConnectionStringTests(ExposedPortContainerFixture fixture)
            {
                _fixture = fixture;
            }

            [Fact]
            public async Task ShouldReturnConnectionStringWithDatabaseDefined()
            {
                // arrange
                const string existingDatabaseName = "master";

                // act
                object result;
                using (var connection = new SqlConnection(_fixture.Container.GetConnectionString(existingDatabaseName)))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT db_name()";
                        result = await command.ExecuteScalarAsync();
                    }
                }

                // assert
                Assert.Equal(existingDatabaseName, result);
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
                    $"Server={_fixture.Container.GetDockerHostIpAddress()}," +
                    $"{_fixture.Container.GetMappedPort(MsSqlContainer.DefaultPort)};" +
                    $"Uid={_fixture.Username};Password={_fixture.Password}";

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
                    $"Server={_fixture.Container.GetDockerHostIpAddress()},{_fixture.MyPort};" +
                    $"Uid={_fixture.Username};Password={_fixture.Password}";

                // act
                var ex = await ProbeForException(connectionString);

                // assert
                Assert.Null(ex);
            }
        }

        private static async Task<Exception> ProbeForException(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
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
