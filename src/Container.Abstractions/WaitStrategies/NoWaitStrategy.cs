using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    public class NoWaitStrategy : IWaitStrategy
    {
        public Task WaitUntil(IContainer container)
        {
            return Task.CompletedTask;
        }
    }
}