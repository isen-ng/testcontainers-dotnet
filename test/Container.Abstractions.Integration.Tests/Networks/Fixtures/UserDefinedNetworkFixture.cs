using System.Threading.Tasks;
using Container.Test.Utility;
using Container.Test.Utility.Platforms;
using Docker.DotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Networks;
using Xunit;

namespace Container.Abstractions.Integration.Tests.Networks.Fixtures
{
    public class UserDefinedNetworkFixture : IAsyncLifetime
    {
        public IPlatformSpecific PlatformSpecific { get; } = PlatformHelper.GetPlatform();

        public INetwork Network { get; }

        public IDockerClient DockerClient { get; }

        public UserDefinedNetworkFixture()
        {
            Network = new NetworkBuilder<UserDefinedNetwork>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureNetwork((context, network) =>
                {
                })
                .Build();

            DockerClient = ((UserDefinedNetwork) Network).DockerClient;
        }

        public async Task InitializeAsync()
        {
            await DockerClientHelper.DeleteNetwork(DockerClient, Network.NetworkName);
        }

        public async Task DisposeAsync()
        {
            await DockerClientHelper.DeleteNetwork(DockerClient, Network.NetworkName);
        }
    }
}
