using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    /// <inheritdoc />
    public class ProbingStrategy : AbstractProbingStrategy
    {
        private readonly Func<IContainer, Task> _probe;

        /// <inheritdoc />
        protected override IEnumerable<Type> ExceptionTypes { get; }

        /// <inheritdoc />
        public ProbingStrategy(Func<IContainer, Task> probe,
            params Type[] exceptionTypes)
        {
            _probe = probe;
            ExceptionTypes = exceptionTypes;
        }

        /// <inheritdoc />
        protected override Task Probe(IContainer container)
        {
            return _probe.Invoke(container);
        }
    }
}
