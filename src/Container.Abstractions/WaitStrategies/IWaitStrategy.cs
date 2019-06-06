using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    /// <summary>
    /// Strategy for waiting for services in the container to start
    /// </summary>
    public interface IWaitStrategy
    {
        /// <summary>
        /// Wait for the services to start
        /// </summary>
        /// <param name="container">Container to wait for</param>
        /// <returns>Task that completes when the services started successfully</returns>
        Task WaitUntil(IContainer container);
    }
}
