using System.Threading.Tasks;
using Container.Test.Utility;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Images;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Images.Fixtures
{
    public class GenericImageFixture : IAsyncLifetime
    {
        public IImage Image { get; }

        public IDockerClient DockerClient { get; }

        public GenericImageFixture()
        {
            Image = new ImageBuilder<GenericImage>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureImage((context, image) =>
                {
                    image.ImageName = $"{GenericContainer.DefaultImage}:{GenericContainer.DefaultTag}";
                })
                .Build();

            DockerClient = ((GenericImage) Image).DockerClient;
        }

        public async Task InitializeAsync()
        {
            await Image.Reap();
        }

        public async Task DisposeAsync()
        {
            await Image.Reap();
        }
    }
}
