using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using TestContainers.Container.Abstractions.Exceptions;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    public abstract class AbstractProbingStrategy : IWaitStrategy
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);

        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(3);

        protected abstract IEnumerable<Type> ExceptionTypes { get; }

        public AbstractProbingStrategy()
        {
        }

        public async Task WaitUntil(IContainer container)
        {
            var exceptionPolicy = Policy
                .Handle<Exception>(e => ExceptionTypes.Any(t => t.IsInstanceOfType(e)))
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

        protected abstract Task Probe(IContainer container);
    }
}