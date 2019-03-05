using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.WaitStrategies
{
    public interface IWaitStrategy
    {
        Task WaitUntil(IContainer container);
    }
}