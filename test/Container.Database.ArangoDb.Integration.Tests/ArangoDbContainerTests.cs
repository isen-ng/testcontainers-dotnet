using System;
using System.Net;
using System.Threading.Tasks;
using ArangoDB.Client;
using Container.Database.ArangoDb.Integration.Tests.Fixtures;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Database.ArangoDb;
using TestContainers.Container.Database.Hosting;
using Xunit;

namespace Container.Database.ArangoDb.Integration.Tests
{
    public class ArangoDbContainerTests
    {
        public class DefaultImageTests
        {
            [Fact]
            public void ShouldUseDefaultImageWhenImageIsNotSpecified()
            {
                // arrange
                var container = new ContainerBuilder<ArangoDbContainer>()
                    .ConfigureDatabaseConfiguration("not-important", "not-important", "not-important")
                    .Build();

                // act
                var actual = container.DockerImageName;

                // assert
                Assert.Equal($"{ArangoDbContainer.DefaultImage}:{ArangoDbContainer.DefaultTag}", actual);
            }

            [Fact]
            public void ShouldUseConfiguredPassword()
            {
                // arrange
                const string username = "user";
                const string password = "my pwd";
                const string database = "my db 1234";
                var container = new ContainerBuilder<ArangoDbContainer>()
                    .ConfigureDatabaseConfiguration(username, password, database)
                    .Build();

                // act
                var actualUsername = container.Username;
                var actualPassword = container.Password;

                // assert
                Assert.Equal("root", actualUsername);
                Assert.Equal(password, actualPassword);
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
                var ex = await ProbeForException(_fixture.Container.GetArangoUrl(), _fixture.Container.DatabaseName,
                    _fixture.Container.Username, _fixture.Container.Password);

                // assert
                Assert.Null(ex);
            }

            [Fact]
            public async Task CanQueryContainerUsingConstructedConnectionString()
            {
                // arrange
                var arangoUrl =
                    $"http://{_fixture.Container.GetDockerHostIpAddress()}:{_fixture.Container.GetMappedPort(ArangoDbContainer.DefaultPort)}";

                // act
                var ex = await ProbeForException(arangoUrl, _fixture.Container.DatabaseName,
                    _fixture.Container.Username, _fixture.Container.Password);

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
                var ex = await ProbeForException(_fixture.Container.GetArangoUrl(), _fixture.Container.DatabaseName,
                    _fixture.Container.Username, _fixture.Container.Password);

                // assert
                Assert.Null(ex);
            }

            [Fact]
            public async Task CanQueryContainerUsingConstructedConnectionString()
            {
                // arrange
                var arangoUrl =
                    $"http://{_fixture.Container.GetDockerHostIpAddress()}:{_fixture.MyPort}";

                // act
                var ex = await ProbeForException(arangoUrl, _fixture.Container.DatabaseName,
                    _fixture.Container.Username, _fixture.Container.Password);

                // assert
                Assert.Null(ex);
            }
        }

        private static async Task<Exception> ProbeForException(string arangoUrl, string database, string username,
            string password)
        {
            var settings = new DatabaseSharedSetting
            {
                Url = arangoUrl,
                Database = database,
                Credential = new NetworkCredential(username, password)
            };

            using (var db = new ArangoDatabase(settings))
            {
                return await Record.ExceptionAsync(async () =>
                    await db.CreateStatement<int>("RETURN 1").ToListAsync());
            }
        }
    }
}
