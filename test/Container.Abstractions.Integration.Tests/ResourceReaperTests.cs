using System.Threading.Tasks;
using Container.Abstractions.Integration.Tests.Platforms;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Reaper;
using Xunit;

namespace Container.Abstractions.Integration.Tests
{
    public class ResourceReaperTests : IAsyncLifetime
    {
        private readonly IContainer _container;
        private readonly IDockerClient _dockerClient;

        public ResourceReaperTests()
        {
            _container = new ContainerBuilder<GenericContainer>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureDockerImageName(PlatformHelper.GetPlatform().TinyDockerImage)
                .Build();

            _dockerClient = new DockerClientFactory().Create();
        }

        public Task InitializeAsync()
        {
            return _container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ShouldReapContainersWhenReaperStops()
        {
            // act
            ResourceReaper.KillTcpConnectionAsync();

            // assert
            var ryukStopped = false;
            while (!ryukStopped)
            {
                try
                {
                    await _dockerClient.Containers.InspectContainerAsync(ResourceReaper.GetRyukContainerId());
                }
                catch (DockerContainerNotFoundException)
                {
                    ryukStopped = true;
                }
            }

            var exception = await Record.ExceptionAsync(async () =>
                await _dockerClient.Containers.InspectContainerAsync(_container.ContainerId));

            Assert.IsType<DockerContainerNotFoundException>(exception);
        }
    }
}