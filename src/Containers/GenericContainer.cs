using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Exceptions;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;

namespace TestContainers.Containers
{
    public class GenericContainer : IContainer
    {
        public const string HostMachineHostname = "host.docker.internal";

        public const string HostMachineUrl = "http://" + HostMachineHostname;

        private const string TcpExposedPortFormat = "{0}/tcp";

        private readonly ILogger _logger;

        protected IDockerClient DockerClient { get; }

        protected string ContainerId { get; private set; }

        protected string ContainerName { get; private set; }

        protected IWaitStrategy WaitStrategy { get; [NotNull] set; } = new NoWaitStrategy();

        protected IStartupStrategy StartupStrategy { get; [NotNull] set; } = new IsRunningStartupCheckStrategy();

        private ContainerInspectResponse ContainerInfo { get; set; }

        public string DockerImageName { get; }

        public IList<int> ExposedPorts { get; } = new List<int>();

        public Dictionary<string, string> Env { get; } = new Dictionary<string, string>();

        public GenericContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            DockerImageName = dockerImageName;
            DockerClient = dockerClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task StartAsync(CancellationToken ct = default(CancellationToken))
        {
            if (ContainerId != null)
            {
                return;
            }

            await ConfigureAsync();

            await PullImage(ct);

            ContainerId = await CreateContainer(ct);

            await StartContainer(ct);
        }

        public async Task StopAsync(CancellationToken ct = default(CancellationToken))
        {
            if (ContainerId == null)
            {
                return;
            }

            await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters(), ct);
            await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters(), ct);
        }

        public string GetDockerHostIpAddress()
        {
            var dockerHostUri = DockerClient.Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe":
                case "unix":
                    return File.Exists("/.dockerenv") && ContainerInfo != null
                        ? ContainerInfo.NetworkSettings.Gateway
                        : "localhost";
                default:
                    throw new InvalidOperationException("Docker client is using a unsupported transport: " +
                                                        dockerHostUri);
            }
        }

        public int GetMappedPort(int exposedPort)
        {
            if (ContainerInfo == null)
            {
                throw new InvalidOperationException($"Container must be started before mapped ports can be retrieved");
            }

            var tcpExposedPort = string.Format(TcpExposedPortFormat, exposedPort);

            if (ContainerInfo.NetworkSettings.Ports.TryGetValue(tcpExposedPort, out var binding) &&
                binding.Count > 0 &&
                int.TryParse(binding[0].HostPort, out var mappedPort))
            {
                return mappedPort;
            }

            throw new InvalidOperationException($"ExposedPort[{exposedPort}] is not mapped");
        }

        public async Task<(string stdout, string stderr)> ExecuteCommand(params string[] command)
        {
            var parameters = new ContainerExecCreateParameters
            {
                AttachStderr = true,
                AttachStdout = true,
                Cmd = command
            };

            var response = await DockerClient.Containers.ExecCreateContainerAsync(ContainerId, parameters);

            var stream = await DockerClient.Containers.StartAndAttachContainerExecAsync(response.ID, false);
            return await stream.ReadOutputToEndAsync(default(CancellationToken));
        }

        protected virtual Task ConfigureAsync()
        {
            return Task.CompletedTask;
        }

        private async Task PullImage(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation("Pulling container image: {}", DockerImageName);
            var createParameters = new ImagesCreateParameters
            {
                FromImage = DockerImageName,
                Tag = DockerImageName.Split(':').Last(),
            };

            await DockerClient.Images.CreateImageAsync(
                createParameters,
                new AuthConfig(),
                new Progress<JSONMessage>(),
                ct);
        }

        private async Task<string> CreateContainer(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            _logger.LogInformation("Creating container for image: {}", DockerImageName);
            var createParameters = ApplyConfiguration();
            var containerCreated = await DockerClient.Containers.CreateContainerAsync(createParameters, ct);

            return containerCreated.ID;
        }

        private async Task StartContainer(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _logger.LogInformation("Starting container with id: {}", ContainerId);
                var started =
                    await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters(), ct);
                if (!started)
                {
                    throw new ContainerLaunchException("Unable to start container: " + ContainerId);
                }

                await StartupStrategy.WaitUntilSuccess(DockerClient, ContainerId);

                ContainerInfo = await DockerClient.Containers.InspectContainerAsync(ContainerId, ct);
                ContainerName = ContainerInfo.Name;

                await WaitStrategy.WaitUntil(this);

                _logger.LogInformation("Container {} started!", DockerImageName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to start container: {}", DockerImageName);

                if (ContainerId != null && _logger.IsEnabled(LogLevel.Error))
                {
                    using (var logStream = await DockerClient.Containers.GetContainerLogsAsync(ContainerId,
                        new ContainerLogsParameters
                        {
                            ShowStderr = true,
                            ShowStdout = true,
                        },
                        ct))
                    {
                        using (var reader = new StreamReader(logStream))
                        {
                            _logger.LogError(reader.ReadToEnd());
                        }
                    }
                }

                throw;
            }
        }

        private CreateContainerParameters ApplyConfiguration()
        {
            var config = new Config
            {
                Image = DockerImageName,
                Env = Env.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList(),
                ExposedPorts = ExposedPorts.ToDictionary(e => string.Format(TcpExposedPortFormat, e),
                    e => default(EmptyStruct)),
                Tty = true,
                AttachStderr = true,
                AttachStdout = true,
            };

            //var portBindings = new Dictionary<string, IList<PortBinding>>();
//            foreach (var exposed in ExposedPorts)
//            {
//                portBindings.Add($"{exposed}/tcp", new[] {new PortBinding {HostPort = exposed.ToString()}});
//            }

            return new CreateContainerParameters(config)
            {
                HostConfig = new HostConfig
                {
                    //PortBindings = portBindings,
                    PublishAllPorts = true
                }
            };
        }
    }
}