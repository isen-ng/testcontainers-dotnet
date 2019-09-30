using System.Threading.Tasks;
using Container.Test.Utility;
using Container.Test.Utility.Platforms;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Images;
using TestContainers.Container.Abstractions.Transferables;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Images.Fixtures
{
    public class DockerfileImageFixture : IAsyncLifetime
    {
        public IPlatformSpecific PlatformSpecific { get; } = PlatformHelper.GetPlatform();

        public IImage Image { get; }

        public IDockerClient DockerClient { get; }

        public DockerfileImageFixture()
        {
            Image = new ImageBuilder<DockerfileImage>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureImage((context, image) =>
                {
                    image.DockerfilePath = "MyDockerfile";
                    image.DeleteOnExit = false;
                    image.Transferables.Add("MyDockerfile", new MountableFile(PlatformSpecific.DockerfileImagePath));
                    image.Transferables.Add(".", new MountableFile(PlatformSpecific.DockerfileImageContext));
                })
                .Build();

            DockerClient = ((DockerfileImage) Image).DockerClient;
        }

        public async Task InitializeAsync()
        {
            await DockerClientHelper.DeleteImage(DockerClient, Image.ImageName);
        }

        public async Task DisposeAsync()
        {
            await DockerClientHelper.DeleteImage(DockerClient, Image.ImageName);
        }
    }
}
