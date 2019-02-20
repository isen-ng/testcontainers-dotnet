using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Containers.Hosting;
using Xunit;

namespace Containers.Integration.Tests
{
    public class GenericContainerFixture : IAsyncLifetime
    {
        public IContainer Container { get; }

        public KeyValuePair<string, string> InjectedEnvironmentVariable { get; } =
            new KeyValuePair<string, string>("MY_KEY", "my value");

        public GenericContainerFixture()
        {
            Container = new ContainerBuilder<GenericContainer>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureDockerImageName("alpine:3.5")
                .ConfigureLogging(builder => builder.AddConsole())
                .ConfigureContainer((context, container) =>
                {
                    container.Env[InjectedEnvironmentVariable.Key] = InjectedEnvironmentVariable.Value;
                })
                .Build();
        }

        public Task InitializeAsync()
        {
            return Container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return Container.StopAsync();
        }
    }
}