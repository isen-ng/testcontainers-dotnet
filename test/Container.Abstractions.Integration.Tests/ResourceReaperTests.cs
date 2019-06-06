using System.Threading.Tasks;
using Container.Abstractions.Integration.Tests.Platforms;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Reaper;
using TestContainers.Container.Abstractions.Reaper.Filters;
using Xunit;
using Xunit.Extensions.Ordering;

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

            _dockerClient = ((GenericContainer) _container).DockerClient;
        }

        public Task InitializeAsync()
        {
            return _container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Must always be run last because it destroys the reaper worker
        /// </summary>
        [Fact, Order(int.MaxValue)]
        public async Task ShouldReapContainersWhenReaperStops()
        {
            // act
            ResourceReaper.Dispose();

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

        [Fact]
        public async Task ShouldReconnectIfConnectionDrops()
        {
            // arrange
            ResourceReaper.KillTcpConnection();

            // act
            ResourceReaper.RegisterFilterForCleanup(new LabelsFilter("key", "value"));

            // assert
            Assert.True(await ResourceReaper.IsConnected());
        }
    }
}
