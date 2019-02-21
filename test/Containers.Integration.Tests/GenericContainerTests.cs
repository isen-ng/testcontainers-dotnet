using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Containers.Integration.Tests.Fixtures;
using TestContainers.Containers;
using Xunit;

namespace Containers.Integration.Tests
{
    [Collection(GenericContainerTestCollection.CollectionName)]
    public class GenericContainerTests
    {
        private readonly GenericContainerFixture _fixture;

        protected IContainer Container => _fixture.Container;

        public GenericContainerTests(GenericContainerFixture fixture)
        {
            _fixture = fixture;
        }

        public class ExecuteCommandTests : GenericContainerTests
        {
            public ExecuteCommandTests(GenericContainerFixture fixture)
                : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldReturnSuccessfulResponseInStdOut()
            {
                // arrange
                const string hello = "hello-world";

                // act
                var (stdout, stderr) = await Container.ExecuteCommand("echo", hello);

                // assert
                Assert.Equal(hello, stdout.TrimEnd(Environment.NewLine.ToCharArray()));
                Assert.True(string.IsNullOrEmpty(stderr));
            }

            [Fact]
            public async Task ShouldReturnFailureResponseInStdErr()
            {
                // act
                var (stdout, stderr) = await Container.ExecuteCommand("sh", "echo");

                // assert
                Assert.True(string.IsNullOrEmpty(stdout));
                Assert.False(string.IsNullOrEmpty(stderr));
            }
        }

        public class EnvironmentVariablesTests : GenericContainerTests
        {
            private readonly KeyValuePair<string, string> _injectedEnvironmentVariable;

            public EnvironmentVariablesTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _injectedEnvironmentVariable = fixture.InjectedEnvVar;
            }

            [Fact]
            public async Task ShouldBeAvailableWhenTheyAreSet()
            {
                // act
                var (stdout, _) = await Container.ExecuteCommand("sh", "-c", "echo $MY_KEY");

                // assert
                Assert.Equal(_injectedEnvironmentVariable.Value, stdout.TrimEnd(Environment.NewLine.ToCharArray()));
            }
        }

        public class ExposedPortsTests : GenericContainerTests
        {
            private readonly int _exposedPort;

            public ExposedPortsTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _exposedPort = fixture.ExposedPort;
            }

            [Fact]
            public void ShouldBeAvailableWhenTheyAreSet()
            {
                // arrange
                var mappedPort = Container.GetMappedPort(_exposedPort);

                // act
                var tcpClient = new TcpClient("localhost", mappedPort);

                // assert
                Assert.True(tcpClient.Connected);
            }

            [Fact]
            public void ShouldNotBeAbleToConnectToUnexposedPort()
            {
                // act
                var ex = Record.Exception(() => new TcpClient("localhost", _exposedPort));

                // assert
                Assert.IsAssignableFrom<SocketException>(ex);
            }
        }
        
        public class PortBindingTests : GenericContainerTests
        {
            private readonly KeyValuePair<int, int> _portBinding;

            public PortBindingTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _portBinding = fixture.PortBinding;
            }

            [Fact]
            public void ShouldBeAvailableWhenTheyAreSet()
            {
                // act
                var tcpClient = new TcpClient("localhost", _portBinding.Value);

                // assert
                Assert.True(tcpClient.Connected);
            }

            [Fact]
            public void ShouldReturnBoundPortWhenGetMappedPortIsCalled()
            {
                // act
                var result = Container.GetMappedPort(_portBinding.Key);

                // assert
                Assert.Equal(_portBinding.Value, result);
            }
        }
    }
}