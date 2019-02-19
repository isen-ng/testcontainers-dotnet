using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    public interface IStartupStrategy
    {
        Task WaitUntilSuccess(IDockerClient dockerClient, string containerId);
    }
}