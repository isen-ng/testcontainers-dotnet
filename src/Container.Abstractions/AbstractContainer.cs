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
using TestContainers.Container.Abstractions.Exceptions;
using TestContainers.Container.Abstractions.Models;
using TestContainers.Container.Abstractions.StartupStrategies;
using TestContainers.Container.Abstractions.WaitStrategies;

namespace TestContainers.Container.Abstractions
{
    /// <inheritdoc />
    public abstract class AbstractContainer : IContainer
    {
        /// <summary>
        /// Internal hostname to reach a host from inside a container
        /// </summary>
        public const string HostMachineHostname = "host.docker.internal";

        /// <summary>
        /// Http url version of the HostMachineHostName
        /// </summary>
        public const string HostMachineUrl = "http://" + HostMachineHostname;

        private const string TcpExposedPortFormat = "{0}/tcp";

        private readonly ILogger _logger;

        /// <summary>
        /// DockerClient used to perform docker operations
        /// Exposed for unit testing
        /// </summary>
        internal IDockerClient DockerClient { get; }

        /// <summary>
        /// Name of the container after it has started
        /// </summary>
        protected string ContainerName { get; private set; }

        /// <summary>
        /// Strategy to use to wait for services in the container to successfully start
        /// </summary>
        protected IWaitStrategy WaitStrategy { get; [NotNull] set; } = new NoWaitStrategy();

        /// <summary>
        /// Strategy to use to wait for the container to start
        /// </summary>
        protected IStartupStrategy StartupStrategy { get; [NotNull] set; } = new IsRunningStartupCheckStrategy();

        private ContainerInspectResponse ContainerInfo { get; set; }

        /// <inheritdoc />
        public string DockerImageName { get; }

        /// <inheritdoc />
        public string ContainerId { get; private set; }

        /// <inheritdoc />
        public IList<int> ExposedPorts { get; } = new List<int>();

        /// <inheritdoc />
        public Dictionary<int, int> PortBindings { get; } = new Dictionary<int, int>();

        /// <inheritdoc />
        public Dictionary<string, string> Env { get; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public Dictionary<string, string> Labels { get; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public IList<Bind> BindMounts { get; } = new List<Bind>();

        /// <inheritdoc />
        public bool IsPrivileged { get; set; }

        /// <inheritdoc />
        public string WorkingDirectory { get; set; }

        /// <inheritdoc />
        public List<string> Command { get; set; }

        /// <inheritdoc />
        public bool AutoRemove { get; set; }

        /// <inheritdoc />
        protected AbstractContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            DockerImageName = dockerImageName;
            DockerClient = dockerClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken ct = default(CancellationToken))
        {
            if (ContainerId != null)
            {
                return;
            }

            await ConfigureAsync();

            await ContainerStarting();

            await PullImage(ct);

            ContainerId = await CreateContainer(ct);

            await StartContainer(ct);

            await ContainerStarted();

            await StartServices(ct);

            await ServiceStarted();
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken ct = default(CancellationToken))
        {
            if (ContainerId == null)
            {
                return;
            }

            await ContainerStopping();

            await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters(), ct);

            if (!AutoRemove)
            {
                await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters(), ct);
            }

            await ContainerStopped();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int GetMappedPort(int exposedPort)
        {
            if (ContainerInfo == null)
            {
                throw new InvalidOperationException(
                    "Container must be started before mapped ports can be retrieved");
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

        /// <inheritdoc />
        public async Task<(string stdout, string stderr)> ExecuteCommand(params string[] command)
        {
            if (ContainerInfo == null)
            {
                throw new InvalidOperationException(
                    "Container must be started before mapped ports can be retrieved");
            }

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

        /// <summary>
        /// Configuration hook for inherited containers to implement
        /// </summary>
        protected virtual Task ConfigureAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook before starting the container
        /// </summary>
        protected virtual Task ContainerStarting()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook after starting the container
        /// </summary>
        protected virtual Task ContainerStarted()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook after service in container started
        /// </summary>
        protected virtual Task ServiceStarted()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook before stopping the container
        /// </summary>
        protected virtual Task ContainerStopping()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook after stopping the container
        /// </summary>
        protected virtual Task ContainerStopped()
        {
            return Task.CompletedTask;
        }

        private async Task PullImage(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            // todo: write a test for this
            var images = await DockerClient.Images.ListImagesAsync(new ImagesListParameters(), ct);
            if (images.Any(image => image.RepoTags != null && image.RepoTags.Contains(DockerImageName)))
            {
                _logger.LogDebug("Image already exists, not pulling: {}", DockerImageName);
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

            _logger.LogDebug("Creating container for image: {}", DockerImageName);
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
                _logger.LogDebug("Starting container with id: {}", ContainerId);

                var started =
                    await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters(), ct);
                if (!started)
                {
                    throw new ContainerLaunchException("Unable to start container: " + ContainerId);
                }

                await StartupStrategy.WaitUntilSuccess(DockerClient, ContainerId);

                ContainerInfo = await DockerClient.Containers.InspectContainerAsync(ContainerId, ct);
                ContainerName = ContainerInfo.Name;

                _logger.LogDebug("Container {} startup complete", DockerImageName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to start container: {}", DockerImageName);

                await PrintContainerLogs(ct);

                throw;
            }
        }

        private async Task StartServices(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            try
            {
                await WaitStrategy.WaitUntil(this);

                _logger.LogInformation("Container {} started!", DockerImageName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to start container services: {}", DockerImageName);

                await PrintContainerLogs(ct);

                throw;
            }
        }

        private CreateContainerParameters ApplyConfiguration()
        {
            var config = new Config
            {
                Image = DockerImageName,
                Env = Env.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList(),
                ExposedPorts = ExposedPorts.ToDictionary(
                    e => string.Format(TcpExposedPortFormat, e),
                    e => default(EmptyStruct)),
                Labels = Labels,
                WorkingDir = WorkingDirectory,
                Cmd = Command,
                Tty = true,
                AttachStderr = true,
                AttachStdout = true,
            };

            return new CreateContainerParameters(config)
            {
                HostConfig = new HostConfig
                {
                    AutoRemove = AutoRemove,
                    PortBindings = PortBindings.ToDictionary(
                        e => string.Format(TcpExposedPortFormat, e.Key),
                        e => (IList<PortBinding>) new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = e.Value.ToString()
                            }
                        }),
                    Mounts = BindMounts.Select(m => new Mount
                    {
                        Source = m.HostPath,
                        Target = m.ContainerPath,
                        ReadOnly = m.AccessMode == AccessMode.ReadOnly,
                        Type = "bind"
                    })
                        .ToList(),
                    PublishAllPorts = true,
                    Privileged = IsPrivileged
                }
            };
        }

        private async Task PrintContainerLogs(CancellationToken ct)
        {
            if (ContainerId != null && _logger.IsEnabled(LogLevel.Error))
            {
                using (var logStream = await DockerClient.Containers.GetContainerLogsAsync(ContainerId,
                    new ContainerLogsParameters
                    {
                        ShowStderr = true,
                        ShowStdout = true
                    },
                    ct))
                {
                    using (var reader = new StreamReader(logStream))
                    {
                        _logger.LogError(reader.ReadToEnd());
                    }
                }
            }
        }
    }
}
