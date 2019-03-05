using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Container.Abstractions.StartupStrategies
{
    public interface IStartupStrategy
    {
        Task WaitUntilSuccess(IDockerClient dockerClient, string containerId);
    }
}