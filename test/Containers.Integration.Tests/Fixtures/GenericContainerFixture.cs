using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Containers.Hosting;
using Xunit;

namespace Containers.Integration.Tests.Fixtures
{
    public class GenericContainerFixture : IAsyncLifetime
    {
        public IContainer Container { get; }

        public KeyValuePair<string, string> InjectedEnvVar { get; } =
            new KeyValuePair<string, string>("MY_KEY", "my value");

        public int ExposedPort { get; } = 1234;

        public KeyValuePair<int, int> PortBinding { get; } = new KeyValuePair<int, int>(2345, 34567);

        public GenericContainerFixture()
        {
            Container = new ContainerBuilder<GenericContainer>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureDockerImageName("alpine:3.5")
                .ConfigureLogging(builder => builder.AddConsole())
                .ConfigureContainer((context, container) =>
                {
                    container.Env[InjectedEnvVar.Key] = InjectedEnvVar.Value;
                    container.ExposedPorts.Add(ExposedPort);
                    
                    /*
                     to do something like `docker run -p 2345:34567 alpine:latest`,
                     both expose port and port binding must be set
                     */
                    container.ExposedPorts.Add(PortBinding.Key);
                    container.PortBindings.Add(PortBinding.Key, PortBinding.Value);
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