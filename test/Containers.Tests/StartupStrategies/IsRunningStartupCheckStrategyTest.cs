using System;
using System.Threading;
using System.Threading.Tasks;
using Containers.Tests.DockerClientMocks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Moq;
using TestContainers.Containers.Exceptions;
using TestContainers.Containers.StartupStrategies;
using Xunit;

namespace Containers.Tests.StartupStrategies
{
    public class IsRunningStartupCheckStrategyTest
    {
        private readonly IStartupStrategy _strategy;
        private readonly string _mockContainerId;
        private readonly Mock<IDockerClient> _dockerClientMock;
        private readonly ContainerState _containerStateMock;

        public IsRunningStartupCheckStrategyTest()
        {
            _mockContainerId = "my_container_id";
            
            var dockerClientMock = new DockerClientMock();
            _dockerClientMock = dockerClientMock.MockDockerClient;

            var mockInspectResponse = new ContainerInspectResponse();
            dockerClientMock.MockContainerOperations.Setup(e => e.InspectContainerAsync(
                    _mockContainerId, 
                    default(CancellationToken)))
                .Returns(Task.FromResult(mockInspectResponse));
            
            _containerStateMock = new ContainerState();
            mockInspectResponse.State = _containerStateMock;
            
            _strategy = new IsRunningStartupCheckStrategy();
        }

        [Fact]
        public async Task ShouldCompleteSuccessfullyIfStateIsRunning()
        {
            // arrange
            _containerStateMock.Running = true;
            
            // act
            var result = _strategy.WaitUntilSuccess(_dockerClientMock.Object, _mockContainerId);
            await result;
            
            // assert
            Assert.True(result.IsCompletedSuccessfully);
        }
        
        [Fact]
        public async Task ShouldThrowContainerLaunchExceptionIfContainerStoppedAfterStarting()
        {
            // arrange
            _containerStateMock.FinishedAt = "some finish time";
            
            // act
            var ex = await Record.ExceptionAsync(async () =>
                await _strategy.WaitUntilSuccess(_dockerClientMock.Object, _mockContainerId));
            
            // assert
            Assert.IsType<ContainerLaunchException>(ex);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
        
        [Fact]
        public async Task ShouldWaitUntilContainerIsInTheRunningState()
        {
            // arrange
            _containerStateMock.Running = false;
            var task = _strategy.WaitUntilSuccess(_dockerClientMock.Object, _mockContainerId);
            Assert.False(task.IsCompleted);
            
            // act
            _containerStateMock.Running = true;
            await task;
            
            // assert
            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}