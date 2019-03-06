using System;
using System.Threading.Tasks;
using Container.Database.PostgreSql.Integration.Tests.Fixtures;
using Npgsql;
using TestContainers.Container.Database.PostgreSql;
using Xunit;

namespace Container.Database.PostgreSql.Integration.Tests
{
    public class PostgreSqlContainerTests
    {
        // todo: tests default image name

        public class ExposedPortTests : IClassFixture<ExposedPortContainerFixture>
        {
            private readonly ExposedPortContainerFixture _fixture;

            public ExposedPortTests(ExposedPortContainerFixture fixture)
            {
                _fixture = fixture;
            }

            [Fact]
            public async Task CanQueryPostgresContainerUsingProvidedConnectionString()
            {
                // act
                Exception ex;
                using (var connection = new NpgsqlConnection(_fixture.Container.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    ex = await Record.ExceptionAsync(async () =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT 1";
                            await command.ExecuteScalarAsync();
                        }
                    });
                }

                // assert
                Assert.Null(ex);
            }

            [Fact]
            public async Task CanQueryPostgresContainerUsingConstructedConnectionString()
            {
                // arrange
                var connectionString =
                    $"Server={_fixture.Container.GetDockerHostIpAddress()};" +
                    $"Port={_fixture.Container.GetMappedPort(PostgreSqlContainer.PostgreSqlPort)};" +
                    $"Database={_fixture.DatabaseName};Username={_fixture.Username};Password={_fixture.Password}";

                // act
                Exception ex;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    ex = await Record.ExceptionAsync(async () =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT 1";
                            await command.ExecuteScalarAsync();
                        }
                    });
                }

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
            public async Task CanQueryPostgresContainerUsingProvidedConnectionString()
            {
                // act
                Exception ex;
                using (var connection = new NpgsqlConnection(_fixture.Container.GetConnectionString()))
                {
                    await connection.OpenAsync();

                    ex = await Record.ExceptionAsync(async () =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT 1";
                            await command.ExecuteScalarAsync();
                        }
                    });
                }

                // assert
                Assert.Null(ex);
            }

            [Fact]
            public async Task CanQueryPostgresContainerUsingConstructedConnectionString()
            {
                // arrange
                var connectionString =
                    $"Server={_fixture.Container.GetDockerHostIpAddress()};" +
                    $"Port={_fixture.MyPort};Database={_fixture.DatabaseName};" +
                    $"Username={_fixture.Username};Password={_fixture.Password}";

                // act
                Exception ex;
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    ex = await Record.ExceptionAsync(async () =>
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT 1";
                            await command.ExecuteScalarAsync();
                        }
                    });
                }

                // assert
                Assert.Null(ex);
            }
        }
    }
}