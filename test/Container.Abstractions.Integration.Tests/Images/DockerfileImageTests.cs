using System.IO;
using System.Threading.Tasks;
using Container.Abstractions.Integration.Tests.Images.Fixtures;
using Container.Abstractions.Integration.Tests.Platforms;
using Container.Test.Utility;
using Docker.DotNet;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Images;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Images
{
    [Collection(DockerfileImageTestCollection.CollectionName)]
    public class DockerfileImageTests
    {
        private readonly DockerfileImageFixture _fixture;

        private IImage Image => _fixture.Image;

        private IDockerClient DockerClient => _fixture.DockerClient;

        private IPlatformSpecific PlatformSpecific => _fixture.PlatformSpecific;

        public DockerfileImageTests(DockerfileImageFixture fixture)
        {
            _fixture = fixture;
        }

        public class ResolveTests : DockerfileImageTests
        {
            public ResolveTests(DockerfileImageFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldResolveImageCorrectly()
            {
                // act
                var actualImageId = await Image.Resolve();

                // assert
                var actualImage = await DockerClient.Images.InspectImageAsync(Image.ImageName);

                Assert.Equal(actualImage.ID, actualImageId);
            }
        }

        [Fact]
        public async Task ShouldCreateAndStartContainerSuccessfully()
        {
            // arrange
            var container = new ContainerBuilder<GenericContainer>()
                .ConfigureDockerImage(Image)
                .ConfigureContainer((h, c) =>
                {
                    c.ExposedPorts.Add(80);
                })
                .Build();

            // act
            await container.StartAsync();

            // assert
            var mappedPort = container.GetMappedPort(80);
            var host = $"http://localhost:{mappedPort}";
            var actual = HttpClientHelper.MakeGetRequest($"{host}/dummy.txt");
            var expected = File.ReadAllText(PlatformSpecific.DockerfileImageContext + "/dummy.txt");

            Assert.Equal(expected, actual);
        }
    }
}
