using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Containers.Integration.Tests.Fixtures;
using Containers.Integration.Tests.Platforms;
using Docker.DotNet;
using TestContainers.Containers;
using TestContainers.Containers.Reaper;
using Xunit;

namespace Containers.Integration.Tests
{
    [Collection(GenericContainerTestCollection.CollectionName)]
    public class GenericContainerTests
    {
        private readonly GenericContainerFixture _fixture;

        protected IContainer Container => _fixture.Container;

        protected IDockerClient DockerClient => _fixture.DockerClient;

        protected IPlatformSpecific PlatformSpecific => _fixture.PlatformSpecific;
        
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
                var (stdout, stderr) = await Container.ExecuteCommand(PlatformSpecific.EchoCommand, hello);

                // assert
                Assert.Equal(hello, stdout.TrimEndNewLine());
                Assert.True(string.IsNullOrEmpty(stderr));
            }

            [Fact]
            public async Task ShouldReturnFailureResponseInStdErr()
            {
                // act
                var (stdout, stderr) = await Container.ExecuteCommand(PlatformSpecific.ShellCommand, PlatformSpecific.EchoCommand);

                // assert
                Assert.True(string.IsNullOrEmpty(stdout));
                Assert.False(string.IsNullOrEmpty(stderr));
            }
        }

        public class EnvironmentVariablesTests : GenericContainerTests
        {
            private readonly KeyValuePair<string, string> _injectedEnvVar;

            public EnvironmentVariablesTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _injectedEnvVar = fixture.InjectedEnvVar;
            }

            [Fact]
            public async Task ShouldBeAvailableWhenTheyAreSet()
            {
                // act
                var (stdout, _) = await Container.ExecuteCommand(
                    PlatformSpecific.ShellCommandFormat(
                        $"{PlatformSpecific.EchoCommand} {PlatformSpecific.EnvVarFormat(_injectedEnvVar.Key)}"));

                // assert
                Assert.Equal(_injectedEnvVar.Value, stdout.TrimEndNewLine());
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

        public class CommandTests : GenericContainerTests
        {
            private readonly string _fileTouchedByCommand;

            public CommandTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _fileTouchedByCommand = fixture.FileTouchedByCommand;
            }

            [Fact]
            public async Task ShouldRunCommandWhenContainerStarts()
            {
                // act
                var (stdout, _) = await Container.ExecuteCommand(
                    PlatformSpecific.ShellCommandFormat(
                        PlatformSpecific.IfExistsThenFormat($"{_fileTouchedByCommand}",
                            $"{PlatformSpecific.EchoCommand} 1")));

                // assert
                Assert.Equal("1", stdout.TrimEndNewLine());
            }
        }

        public class WorkingDirectoryTests : GenericContainerTests
        {
            private readonly string _workingDirectory;

            public WorkingDirectoryTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _workingDirectory = fixture.WorkingDirectory;
            }

            [Fact]
            public async Task ShouldSetWorkingDirectoryWhenContainerStarts()
            {
                // act
                var (stdout, _) = await Container.ExecuteCommand(PlatformSpecific.CurrentPathCommand);

                // assert
                Assert.Equal(_workingDirectory, stdout.TrimEndNewLine());
            }
        }

        public class PrivilegedModeTests : GenericContainerTests
        {
            public PrivilegedModeTests(GenericContainerFixture fixture)
                : base(fixture)
            {
            }

            // todo: test for privileged mode with privileged container
            [Fact]
            public async Task ShouldFailToRunPrivilegedOperations()
            {
                // act
                var (stdout, stderr) = await Container.ExecuteCommand(PlatformSpecific.PrivilegedCommand);

                // assert
                Assert.NotEmpty(stderr);
                Assert.Empty(stdout);
            }
        }

        public class BindMountTests : GenericContainerTests
        {
            private readonly KeyValuePair<string, string> _hostPathBinding;

            public BindMountTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _hostPathBinding = fixture.HostPathBinding;
            }

            [Fact]
            public async Task ShouldContainFileCreatedInHostBindingDirectory()
            {
                // arrange
                var content = Guid.NewGuid().ToString();
                const string filename = "my_file";
                var filepath = Path.Combine(_hostPathBinding.Key, filename);
                File.WriteAllText(filepath, content);

                // act
                var (stdout, stderr) =
                    await Container.ExecuteCommand(PlatformSpecific.CatCommand, Path.Combine(_hostPathBinding.Value, filename));

                // assert
                Assert.Equal(content, stdout.TrimEndNewLine());
            }
        }
        
        public class LabelTests : GenericContainerTests
        {
            private readonly KeyValuePair<string, string> _customLabel;

            public LabelTests(GenericContainerFixture fixture)
                : base(fixture)
            {
                _customLabel = fixture.CustomLabel;
            }

            [Fact]
            public async Task ShouldContainConfiguredCustomLabels()
            {
                // act
                var response = await DockerClient.Containers.InspectContainerAsync(Container.ContainerId);

                // assert
                Assert.Equal(response.Config.Labels[_customLabel.Key], _customLabel.Value);
            }
            
            [Fact]
            public async Task ShouldContainReaperLabels()
            {
                // act
                var response = await DockerClient.Containers.InspectContainerAsync(Container.ContainerId);

                // assert
                foreach (var label in ResourceReaper.Labels)
                {
                    Assert.Equal(response.Config.Labels[label.Key], label.Value);    
                }
            }
        }
    }
}