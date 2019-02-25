using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Containers.Hosting;
using TestContainers.Containers.Models;
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

        public KeyValuePair<string, string> HostPathBinding =
            new KeyValuePair<string, string>(Directory.GetCurrentDirectory(), "/host");

        public string FileTouchedByCommand { get; } = "/tmp/touched";

        public string WorkingDirectory { get; } = "/etc";

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
                    container.BindMounts.Add(new Bind
                    {
                        HostPath = HostPathBinding.Key,
                        ContainerPath = HostPathBinding.Value,
                        AccessMode = AccessMode.ReadOnly
                    });
                    container.WorkingDirectory = WorkingDirectory;
                    container.Command = new List<string>
                    {
                        "/bin/sh", "-c", $"touch {FileTouchedByCommand}; /bin/sh"
                    };
                })
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await Container.StopAsync();
        }
    }
}