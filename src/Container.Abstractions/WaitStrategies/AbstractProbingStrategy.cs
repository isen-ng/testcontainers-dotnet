using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using TestContainers.Container.Abstractions.Exceptions;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    /// <summary>
    /// Probes the container regularly to test if services has started
    /// </summary>
    /// <inheritdoc />
    public abstract class AbstractProbingStrategy : IWaitStrategy
    {
        /// <summary>
        /// Timeout before the strategy fails
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Interval between each retry
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Exceptions that are considered acceptable in the probe to continue probing
        /// </summary>
        protected abstract IEnumerable<Type> ExceptionTypes { get; }

        /// <inheritdoc />
        protected AbstractProbingStrategy()
        {
        }

        /// <inheritdoc />
        public async Task WaitUntil(IContainer container)
        {
            var exceptionPolicy = Policy
                .Handle<Exception>(e =>
                {
                    if (e is AggregateException ae)
                    {
                        return ae.InnerExceptions.Any(ie => ExceptionTypes.Any(t => t.IsInstanceOfType(ie)));
                    }

                    return ExceptionTypes.Any(t => t.IsInstanceOfType(e));
                })
                .WaitAndRetryForeverAsync(_ => RetryInterval);

            var result = await Policy
                .TimeoutAsync(Timeout)
                .WrapAsync(exceptionPolicy)
                .ExecuteAndCaptureAsync(async () => { await Probe(container); });

            if (result.Outcome == OutcomeType.Failure)
            {
                throw new ContainerLaunchException(result.FinalException.Message, result.FinalException);
            }
        }

        /// <summary>
        /// The action to probe
        /// </summary>
        /// <param name="container">Container to probe</param>
        /// <returns>A task that completes when the probe completes</returns>
        protected abstract Task Probe(IContainer container);
    }
}
