using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    public class ProbingStrategy : AbstractProbingStrategy
    {
        private readonly Func<IContainer, Task> _probe;

        protected override IEnumerable<Type> ExceptionTypes { get; }

        public ProbingStrategy(Func<IContainer, Task> probe,
            params Type[] exceptionTypes)
        {
            _probe = probe;
            ExceptionTypes = exceptionTypes;
        }

        protected override Task Probe(IContainer container)
        {
            return _probe.Invoke(container);
        }
    }
}