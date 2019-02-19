using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Containers.Hosting;
using Xunit;

namespace Containers.Integration.Tests
{
    public class GenericContainerTests : IAsyncLifetime
    {
        private readonly IContainer _container;

        public GenericContainerTests()
        {
            _container = new ContainerBuilder<GenericContainer>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureDockerImageName("alpine:3.5")
                .ConfigureLogging(builder => builder.AddConsole())
                .Build();
        }

        public Task InitializeAsync()
        {
            return _container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return _container.StopAsync();
        }

        [Fact]
        public async Task ShouldReturnSuccessfulResponseInStdOut()
        {
            // arrange
            const string hello = "hello-world";
            
            // act
            var (stdout, stderr) = await _container.ExecuteCommand("echo", hello);
            
            // assert
            Assert.Equal(hello, stdout.TrimEnd(Environment.NewLine.ToCharArray()));
            Assert.True(string.IsNullOrEmpty(stderr));
        }
        
        [Fact]
        public async Task ShouldReturnFailureResponseInStdErr()
        {
            // act
            var (stdout, stderr) = await _container.ExecuteCommand("sh", "echo");
            
            // assert
            Assert.True(string.IsNullOrEmpty(stdout));
            Assert.False(string.IsNullOrEmpty(stderr));
        }
        
        // todo: environment variables tests
        // todo: exposed ports tests
    }
}