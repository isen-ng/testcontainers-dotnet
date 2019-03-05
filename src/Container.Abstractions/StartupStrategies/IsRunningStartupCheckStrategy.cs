using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Polly;
using TestContainers.Container.Abstractions.Exceptions;

namespace TestContainers.Container.Abstractions.StartupStrategies
{
    public class IsRunningStartupCheckStrategy : IStartupStrategy
    {
        public async Task WaitUntilSuccess(IDockerClient dockerClient, string containerId)
        {
            var retryPolicy = Policy
                .HandleResult<ContainerState>(s => !IsContainerRunning(s)).
                WaitAndRetryForeverAsync(retry => TimeSpan.FromSeconds(1));

            var outcome = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(1))
                .WrapAsync(retryPolicy)
                .ExecuteAndCaptureAsync(async () => await GetCurrentState(dockerClient, containerId));

            if (outcome.Outcome == OutcomeType.Failure)
            {
                throw new ContainerLaunchException("Container.Abstractions startup failed", outcome.FinalException);
            }
        }

        private static bool IsContainerRunning(ContainerState state)
        {
            if (state.Running)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(state.FinishedAt))
            {
                // container exited early?
                throw new InvalidOperationException("Container.Abstractions has exited with code: " + state.ExitCode);
            }

            // still starting, I guess...
            return false;
        }

        private static async Task<ContainerState> GetCurrentState(IDockerClient dockerClient, string containerId)
        {
            var response = await dockerClient.Containers.InspectContainerAsync(containerId);
            return response.State;
        }
    }
}