using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public class NoWaitStrategy : IWaitStrategy
    {
        public Task WaitUntil(IContainer container)
        {
            return Task.CompletedTask;
        }
    }
}