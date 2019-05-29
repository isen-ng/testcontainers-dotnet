using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Container.Abstractions.DockerClient
{
    public interface IDockerClientProvider
    {
        string Description { get; }
        
        bool IsApplicable { get; }

        int GetPriority();

        DockerClientConfiguration GetConfiguration();
        
        Task<bool> TryTest(CancellationToken ct = default(CancellationToken));
    }
}