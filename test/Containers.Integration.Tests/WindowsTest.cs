using System.Collections.Generic;
using System.Threading.Tasks;
using Containers.Integration.Tests.Platforms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Containers.Hosting;
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
                .ConfigureContainer((context, container) =>
                {
                    container.Env["my_key"] = "my_value";
                    container.Command = new List<string> {"powershell", "-command", "fc > C:\\touched; powershell"};
                })
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
        public async Task ShouldRunInWindows()
        {
            var (stdout, _) = await Container.ExecuteCommand("powershell", "-command", "echo $env:my_key");

            // assert
            Assert.Equal("my_value", stdout.TrimEndNewLine());
        }
        
        [Fact]
        public async Task ShouldRunInWindows2()
        {
            var (stdout, _) = await Container.ExecuteCommand("echo", "lala");

            // assert
            Assert.Equal("lala", stdout.TrimEndNewLine());
        }
    }
}