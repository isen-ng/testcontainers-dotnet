using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestContainers.Containers;
using Xunit;

namespace Containers.Integration.Tests
{
    public class GenericContainerTests : IClassFixture<GenericContainerFixture>
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
                _injectedEnvironmentVariable = fixture.InjectedEnvironmentVariable;
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

        // todo: exposed ports tests
    }
}