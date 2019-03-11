using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Polly;
using TestContainers.Container.Abstractions.Exceptions;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    /// <summary>
    /// Checks if exposed ports are available to indicate if services have started
    /// </summary>
    /// <inheritdoc />
    public class ExposedPortsWaitStrategy : IWaitStrategy
    {
        /// <summary>
        /// Ports to test
        /// </summary>
        public List<int> ExposedPorts { get; }

        /// <inheritdoc />
        public ExposedPortsWaitStrategy(List<int> exposedPorts)
        {
            ExposedPorts = exposedPorts;
        }

        /// <inheritdoc />
        public async Task WaitUntil(IContainer container)
        {
            var retryPolicy = Policy
                .Handle<SocketException>()
                .WaitAndRetryForeverAsync(retry => TimeSpan.FromSeconds(1));

            var outcome = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(1))
                .WrapAsync(retryPolicy)
                .ExecuteAndCaptureAsync(async () => await AllPortsExposed(container));

            if (outcome.Outcome == OutcomeType.Failure)
            {
                throw new ContainerLaunchException("Container.Abstractions startup failed", outcome.FinalException);
            }
        }

        private Task AllPortsExposed(IContainer container)
        {
            var ipAddress = container.GetDockerHostIpAddress();

            foreach (var exposedPort in ExposedPorts)
            {
                TcpClient client = null;
                try
                {
                    client = new TcpClient(ipAddress, container.GetMappedPort(exposedPort));
                }
                finally
                {
                    client?.Dispose();
                }
            }

            return Task.CompletedTask;
        }
    }
}