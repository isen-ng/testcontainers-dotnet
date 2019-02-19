using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TestContainers.Containers
{
    public interface IContainer
    {
        /// <summary>
        /// Get the image name.
        /// </summary>
        [NotNull] string DockerImageName { get; }
        
        // todo: implement port bindings
//        /// <summary>
//        /// Dictionary&lt;int ExposedPort, int PortBinding&gt;
//        /// </summary>
//        Dictionary<int, int> PortBindings { get; }
        
        // todo: implement extra hosts
//        /// <summary>
//        /// Dictionary&lt;int Hostname, int IpAddress&gt;
//        /// </summary>
//        Dictionary<int, int> ExtraHosts { get; }
        
        /// <summary>
        /// Dictionary&lt;int key, int value&gt;
        /// </summary>
        Dictionary<string, string> Env { get; }
        
        // todo: implement labels
        /// <summary>
        /// Dictionary&lt;int key, int value&gt;
        /// </summary>
//        Dictionary<string, string> Labels { get; }
        
        List<int> ExposedPorts { get; }
        
        // todo: implement mounts
        //List<Mount> Mounts { get; }
        
        // todo: implement commands
        //List<string> Commands { get; }

        Task StartAsync(CancellationToken ct = default(CancellationToken));

        Task StopAsync(CancellationToken ct = default(CancellationToken));

        string GetDockerHostIpAddress();

        Task<(string stdout, string stderr)> ExecuteCommand(params string[] command);
    }
}