using System.Threading.Tasks;
using Container.Abstractions.Integration.Tests.Images.Fixtures;
using Docker.DotNet;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Images;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Images
{
    [Collection(GenericImageTestCollection.CollectionName)]
    public class GenericImageTests
    {
        private readonly GenericImageFixture _fixture;

        private IImage Image => _fixture.Image;

        private IDockerClient DockerClient => _fixture.DockerClient;

        public GenericImageTests(GenericImageFixture fixture)
        {
            _fixture = fixture;
        }

        public class ResolveTests : GenericImageTests
        {
            public ResolveTests(GenericImageFixture fixture) : base(fixture)
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
                .Build();

            // act
            await container.StartAsync();

            // assert
            Assert.Equal(Image.ImageName, container.DockerImageName);
            await container.StopAsync();
        }
    }
}
