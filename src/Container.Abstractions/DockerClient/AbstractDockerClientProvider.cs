using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Polly;

namespace TestContainers.Container.Abstractions.DockerClient
{
    /// <summary>
    /// Base class for docker client providers
    /// </summary>
    public abstract class AbstractDockerClientProvider : IDockerClientProvider
    {
        private static readonly TimeSpan TestRetryInterval = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The default priority to start with
        /// </summary>
        protected const int DefaultPriority = 100;

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract bool IsApplicable { get; }

        /// <summary>
        /// Returns a created docker client
        /// </summary>
        /// <returns></returns>
        protected abstract IDockerClient CreateDockerClient();

        /// <inheritdoc />
        public abstract int GetPriority();

        /// <inheritdoc />
        public abstract DockerClientConfiguration GetConfiguration();

        /// <inheritdoc />
        public async Task<bool> TryTest(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                using (var client = CreateDockerClient())
                {
                    var exceptionPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryForeverAsync(_ => TestRetryInterval);

                    return await Policy
                        .TimeoutAsync(TestTimeout)
                        .WrapAsync(exceptionPolicy)
                        .ExecuteAsync(async () =>
                        {
                            await client.System.PingAsync(ct);
                            return true;
                        });
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
