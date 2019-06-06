using System;
using System.Runtime.InteropServices;
using TestContainers.Container.Abstractions.DockerClient;
using Xunit;

namespace Container.Abstractions.Tests.DockerClient
{
    public class UnixDockerClientProviderTests
    {
        public class GetPriority : UnixDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturn200ForPriority()
            {
                // arrange
                var provider = new UnixDockerClientProvider();

                // act
                var result = provider.GetPriority();

                // assert
                Assert.Equal(100, result);
            }
        }

        public class GetConfiguration : UnixDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnConfigurationWithPresetUnixDockerHost()
            {
                // arrange
                var provider = new UnixDockerClientProvider();

                // act
                var result = provider.GetConfiguration();

                // assert
                Assert.Equal(new Uri(UnixDockerClientProvider.UnixSocket), result.EndpointBaseUri);
            }
        }

        public class IsApplicable : UnixDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnTrueIfEnvironmentIsSetAndOsIsLinuxOrOsx()
            {
                // arrange
                var provider = new UnixDockerClientProvider();

                // act
                var result = provider.IsApplicable;

                // assert
                Assert.Equal(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) |
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                    result);
            }
        }
    }
}
