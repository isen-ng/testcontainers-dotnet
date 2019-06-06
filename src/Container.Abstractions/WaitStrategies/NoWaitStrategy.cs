using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    /// <summary>
    /// Don't wait
    /// </summary>
    /// <inheritdoc />
    public class NoWaitStrategy : IWaitStrategy
    {
        /// <inheritdoc />
        public Task WaitUntil(IContainer container)
        {
            return Task.CompletedTask;
        }
    }
}
