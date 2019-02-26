using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Containers.Integration.Tests.Platforms;
using Docker.DotNet.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Containers.Hosting;
using TestContainers.Containers.Models;
using Xunit;

namespace Containers.Integration.Tests
{
    public class WindowsTest : IAsyncLifetime
    {
        public IPlatformSpecific PlatformSpecific { get; } = PlatformHelper.GetPlatform();
        
        public IContainer Container { get; }
        
        public WindowsTest()
        {   
            Container = new ContainerBuilder<GenericContainer>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureDockerImageName(PlatformSpecific.TinyDockerImage)
                .ConfigureLogging(builder => builder.AddConsole())
                .ConfigureContainer((context, container) => { container.Command = new List<string> {"pwsh"}; })
                .Build();
            
            //DockerClient = new DockerClientFactory().Create();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
        }

        [Fact]
        public void ShouldRun()
        {
            Assert.True(true);
        }
    }
}