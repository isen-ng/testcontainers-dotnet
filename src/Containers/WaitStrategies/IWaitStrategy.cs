using System.Threading.Tasks;

namespace TestContainers.Containers.WaitStrategies
{
    public interface IWaitStrategy
    {
        Task WaitUntil(IContainer container);
    }
}