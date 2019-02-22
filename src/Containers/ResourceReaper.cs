using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace TestContainers.Containers
{
    public class ResourceReaper
    {
        public static readonly string TestContainerLabelName = typeof(IContainer).FullName;
        public static readonly string TestContainerSessionLabelName = TestContainerLabelName + ".SessionId";
        public static readonly string TestContainerAssemblyLabelName = TestContainerLabelName + ".EntryAssembly";

        public static readonly string SessionId = Guid.NewGuid().ToString();
        public static readonly string EntryAssemblyName = Assembly.GetEntryAssembly().GetName().Name;

        public static readonly ResourceReaper Instance;

        static ResourceReaper()
        {
            Instance = new ResourceReaper();
            AppDomain.CurrentDomain.ProcessExit += async (s, e) => await Instance.ReapCurrentSessionContainers();
            Console.CancelKeyPress += async (s, e) => await Instance.ReapCurrentSessionContainers();
        }
        
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            {TestContainerLabelName, "true"},
            {TestContainerSessionLabelName, SessionId},
            {TestContainerAssemblyLabelName, EntryAssemblyName}
        };

        private readonly IDockerClient _dockerClient;

        public ResourceReaper(DockerClientFactory factory = null)
        {
            _dockerClient = (factory ?? new DockerClientFactory()).Create();
        }
        
        public async Task ReapCurrentSessionContainers()
        {
            var result = await GetContainersByReaperLabels();
            
            await KillContainers(result);
            await RemoveContainers(result);
        }

        public async Task ReapPreviousSessionContainers()
        {
            var result = await GetContainersByReaperLabels();

            result = result.Where(c =>
                {
                    if (c.Labels.TryGetValue(TestContainerSessionLabelName, out var label))
                    {
                        return label != SessionId;
                    }

                    return true;
                })
                .ToList();

            await KillContainers(result);
            await RemoveContainers(result);
        }

        private async Task<IList<ContainerListResponse>> GetContainersByReaperLabels()
        {
            var parameters = new ContainersListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "label", new Dictionary<string, bool>
                        {
                            {TestContainerLabelName, true}
                        }
                    }
                }
            };
            
            
            var response = await _dockerClient.Containers.ListContainersAsync(parameters);
            return response.Where(c =>
                {
                    if (c.Labels.TryGetValue(TestContainerAssemblyLabelName, out var label))
                    {
                        return label == EntryAssemblyName;
                    }

                    return false;
                })
                .ToList();
        }

        private async Task KillContainers(IEnumerable<ContainerListResponse> containers)
        {
            await Task.WhenAll(containers
                .Select(c => _dockerClient.Containers.KillContainerAsync(c.ID, new ContainerKillParameters()))
                .ToList());
        }
        
        private async Task RemoveContainers(IEnumerable<ContainerListResponse> containers)
        {
            await Task.WhenAll(containers
                .Select(c =>
                {
                    try
                    {
                        return _dockerClient.Containers.RemoveContainerAsync(c.ID, new ContainerRemoveParameters());
                    }
                    catch (DockerContainerNotFoundException)
                    {
                        return Task.CompletedTask;
                    }
                })
                .ToList());
        }
    }
}