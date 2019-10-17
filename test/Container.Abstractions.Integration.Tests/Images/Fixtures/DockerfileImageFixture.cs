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
        public const string DockerfileImageContext = "Images/Fixtures/Dockerfiles/Context";
        public const string DockerfileImageTransferableFile = "Images/Fixtures/Dockerfiles/Transferables/file1.txt";
        public const string DockerfileImageTransferableFolder = "Images/Fixtures/Dockerfiles/Transferables/folder1";

        public IPlatformSpecific PlatformSpecific { get; } = PlatformHelper.GetPlatform();

        public IImage Image { get; }

        public IDockerClient DockerClient { get; }

        public DockerfileImageFixture()
        {
            Image = new ImageBuilder<DockerfileImage>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging((hostContext, builder) =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureImage((context, image) =>
                {
                    image.DockerfilePath = "MyDockerfile";
                    image.DeleteOnExit = false;
                    image.BasePath = DockerfileImageContext;
                    image.Transferables.Add("MyDockerfile", new MountableFile(PlatformSpecific.DockerfileImagePath));
                    image.Transferables.Add("file1.txt", new MountableFile(DockerfileImageTransferableFile));
                    image.Transferables.Add("folder1", new MountableFile(DockerfileImageTransferableFolder));
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
