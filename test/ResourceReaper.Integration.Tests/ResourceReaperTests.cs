using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Reaper.Filters;
using Xunit;
using Xunit.Extensions.Ordering;

namespace ResourceReaper.Integration.Tests
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
                .ConfigureDockerImageName($"{GenericContainer.DefaultImage}:{GenericContainer.DefaultTag}")
                .Build();

            _dockerClient = ((GenericContainer) _container).DockerClient;
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
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
            TestContainers.Container.Abstractions.Reaper.ResourceReaper.Dispose();

            // assert
            var ryukStopped = false;
            while (!ryukStopped)
            {
                try
                {
                    await _dockerClient.Containers.InspectContainerAsync(TestContainers.Container.Abstractions.Reaper
                        .ResourceReaper.GetRyukContainerId());
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
            TestContainers.Container.Abstractions.Reaper.ResourceReaper.KillTcpConnection();

            // act
            TestContainers.Container.Abstractions.Reaper.ResourceReaper.RegisterFilterForCleanup(
                new LabelsFilter("key", "value"));

            // assert
            Assert.True(await TestContainers.Container.Abstractions.Reaper.ResourceReaper.IsConnected());
        }
    }
}
