using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;

namespace TestContainers.Container.Abstractions.DockerClient
{
    /// <summary>
    /// Factory to provide docker clients based on testing different providers
    /// </summary>
    public class DockerClientFactory2
    {
        private static readonly IReadOnlyList<IDockerClientProvider> OrderedDockerClientProviders;

        static DockerClientFactory2()
        {
            OrderedDockerClientProviders = new List<IDockerClientProvider>
            {
                new EnvironmentDockerClientProvider(),
                new NpipeDockerClientProvider(),
                new UnixDockerClientProvider()
            }
                .OrderByDescending(p => p.GetPriority())
                .ToList();
        }
        
        private readonly ILogger<DockerClientFactory2> _logger;
        private readonly Lazy<Task<DockerClientConfiguration>> _configuration;

        /// <inheritdoc />
        public DockerClientFactory2(ILogger<DockerClientFactory2> logger)
        {
            _logger = logger;
            _configuration = new Lazy<Task<DockerClientConfiguration>>(async () =>
            {
                foreach (var provider in OrderedDockerClientProviders)
                {
                    if (!provider.IsApplicable)
                    {
                        continue;
                    }
                    
                    var name = provider.GetType().Name;
                    var description = provider.Description;
                    
                    _logger.LogDebug("Testing provider: {}", name);
                    if (await provider.TryTest())
                    {
                        _logger.LogDebug("Provider[{}] found\n{}", name, description);
                        return provider.GetConfiguration();
                    }

                    _logger.LogDebug("Provider[{}] test failed\n{}", name, description);
                }
                
                throw new InvalidOperationException("There are no supported docker client providers!"); 
            });
        }
        
        /// <summary>
        /// Creates a new DockerClient
        /// </summary>
        /// <returns></returns>
        public async Task<IDockerClient> Create()
        {
            var configuration = await _configuration.Value;
            return configuration.CreateClient();
        }
    }
    
    /// <summary>
    /// Factory to provide docker clients based on the host operating system
    /// </summary>
    public class DockerClientFactory
    {
        private static readonly DockerClientConfiguration WindowsDockerConfiguration =
            new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));

        private static readonly DockerClientConfiguration UnixDockerConfiguration =
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));

        private readonly DockerClientConfiguration _configuration;

        /// <inheritdoc />
        public DockerClientFactory()
        {
            _configuration = BuildDockerConfigBasedOnOs();
        }

        /// <summary>
        /// Creates a new DockerClient
        /// </summary>
        /// <returns></returns>
        public IDockerClient Create()
        {
            return _configuration.CreateClient();
        }

        private static DockerClientConfiguration BuildDockerConfigBasedOnOs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsDockerConfiguration;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return UnixDockerConfiguration;
            }

            throw new InvalidOperationException("OS is not supported for testcontainers-dotnet");
        }
    }
}