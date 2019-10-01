using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Container.Test.Utility.Platforms;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Models;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Fixtures
{
    public class GenericContainerFixture : IAsyncLifetime
    {
        public IPlatformSpecific PlatformSpecific { get; } = PlatformHelper.GetPlatform();

        public IContainer Container { get; }

        public IDockerClient DockerClient { get; }

        public KeyValuePair<string, string> CustomLabel { get; } =
            new KeyValuePair<string, string>("custom label", "custom value");

        public KeyValuePair<string, string> InjectedEnvVar { get; } =
            new KeyValuePair<string, string>("MY_KEY", "my value");

        public int ExposedPort { get; } = 1234;

        public KeyValuePair<int, int> PortBinding { get; } = new KeyValuePair<int, int>(2345, 34567);

        public KeyValuePair<string, string> HostPathBinding { get; }

        public string FileTouchedByCommand { get; }

        public string WorkingDirectory { get; }

        public GenericContainerFixture()
        {
            HostPathBinding =
                new KeyValuePair<string, string>(Directory.GetCurrentDirectory(), PlatformSpecific.BindPath);
            FileTouchedByCommand = PlatformSpecific.TouchedFilePath;
            WorkingDirectory = PlatformSpecific.WorkingDirectory;

            Container = new ContainerBuilder<GenericContainer>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureDockerImageName(PlatformSpecific.TinyDockerImage)
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureContainer((context, container) =>
                {
                    container.Labels.Add(CustomLabel.Key, CustomLabel.Value);
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
                    container.Command = PlatformSpecific.ShellCommand(
                            $"{PlatformSpecific.Touch} {FileTouchedByCommand}; {PlatformSpecific.Shell}")
                        .ToList();
                })
                .Build();

            DockerClient = ((GenericContainer) Container).DockerClient;
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
