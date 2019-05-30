using System;
using System.Runtime.InteropServices;
using TestContainers.Container.Abstractions.DockerClient;
using Xunit;

namespace Container.Abstractions.Tests.DockerClient
{
    public class NpipeDockerClientProviderTests
    {
        public class GetPriority : NpipeDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturn100ForPriority()
            {
                // arrange
                var provider = new NpipeDockerClientProvider();

                // act
                var result = provider.GetPriority();

                // assert
                Assert.Equal(100, result);
            }
        }

        public class GetConfiguration : NpipeDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnConfigurationWithPresetNpipeDockerHost()
            {
                // arrange
                var provider = new NpipeDockerClientProvider();

                // act
                var result = provider.GetConfiguration();

                // assert
                Assert.Equal(new Uri(NpipeDockerClientProvider.Npipe), result.EndpointBaseUri);
            }
        }

        public class IsApplicable : NpipeDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnTrueIfEnvironmentIsWindows()
            {
                // arrange
                var provider = new NpipeDockerClientProvider();

                // act
                var result = provider.IsApplicable;

                // assert
                Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), result);
            }
        }
    }
}