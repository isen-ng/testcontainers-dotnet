using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Models;
using TestContainers.Container.Abstractions.Reaper.Filters;
using TestContainers.Container.Abstractions.Utilities;
using TestContainers.Container.Abstractions.WaitStrategies;

namespace TestContainers.Container.Abstractions.Reaper
{
    /// <summary>
    /// Container to start ryuk
    /// </summary>
    public class RyukContainer : AbstractContainer
    {
        private const string RyukAck = "ACK";

        internal const int RyukPort = 8080;

        private readonly ILogger<RyukContainer> _logger;

        private readonly BatchWorker _sendToRyukWorker;

        private readonly BatchWorker _connectToRyukWorker;

        private readonly List<string> _deathNote = new List<string>();

        private string _ryukHost;

        private int _ryukPort;

        private TcpClient _tcpClient;

        private Stream _tcpWriter;

        private StreamReader _tcpReader;

        /// <inheritdoc />
        public RyukContainer(string ryukImage, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(ryukImage, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RyukContainer>();
            _sendToRyukWorker = new BatchWorkerFromDelegate(SendToRyuk);
            _connectToRyukWorker = new BatchWorkerFromDelegate(ConnectToRyuk);
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            WaitStrategy = new ExposedPortsWaitStrategy(new List<int>
            {
                RyukPort
            });
            ExposedPorts.Add(RyukPort);

            var dockerHostPath = (Environment.GetEnvironmentVariable("DOCKER_HOST"), Environment.GetEnvironmentVariable("TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE")) switch
            {
                ({ } s, _) when !string.IsNullOrEmpty(s) => new Uri(s).PathAndQuery,
                (null, { } s) when !string.IsNullOrEmpty(s) => s,
                _ => "/var/run/docker.sock"
            };

            BindMounts.Add(new Bind
            {
                // apparently this is the correct way to mount the docker socket on both windows and linux
                // mounting the npipe will not work
                HostPath = dockerHostPath,
                // ryuk is a linux container, so we have to mount onto the linux socket
                ContainerPath = "/var/run/docker.sock",
                AccessMode = AccessMode.ReadOnly
            });

            AutoRemove = true;
        }

        /// <inheritdoc />
        protected override Task ServiceStarted()
        {
            _ryukHost = GetDockerHostIpAddress();
            _ryukPort = GetMappedPort(RyukPort);

            _connectToRyukWorker.Notify();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task ContainerStopping()
        {
            _tcpClient?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a filter for ryuk to kill
        /// </summary>
        /// <param name="filter">filter to add</param>
        public void AddToDeathNote(IFilter filter)
        {
            _deathNote.Add(filter.ToFilterString());
            _sendToRyukWorker.Notify();
        }

        internal void KillTcpConnection()
        {
            _tcpClient?.Dispose();
        }

        internal void Dispose()
        {
            _tcpClient?.Dispose();
            _connectToRyukWorker.Dispose();
        }

        internal async Task<bool?> IsConnected()
        {
            await _connectToRyukWorker.WaitForCurrentWorkToBeServiced();
            await _sendToRyukWorker.WaitForCurrentWorkToBeServiced();
            return _tcpClient?.Connected;
        }

        private Task ConnectToRyuk()
        {
            try
            {
                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    _tcpClient = new TcpClient(_ryukHost, _ryukPort);
                    _tcpWriter = _tcpClient.GetStream();
                    _tcpReader = new StreamReader(new BufferedStream(_tcpClient.GetStream()), Encoding.UTF8);
                    _sendToRyukWorker.Notify();
                }

                _connectToRyukWorker.Notify(DateTime.UtcNow + TimeSpan.FromSeconds(4));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Disconnected from ryuk. Reconnecting now.");
                _tcpClient = null;
                _connectToRyukWorker.Notify();
            }

            return Task.CompletedTask;
        }

        private async Task SendToRyuk()
        {
            if (_deathNote.Count <= 0)
            {
                return;
            }

            var clone = _deathNote.ToList();

            try
            {
                foreach (var filter in clone)
                {
                    var bodyBytes = Encoding.UTF8.GetBytes(filter + "\n");

                    await _tcpWriter.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                    await _tcpWriter.FlushAsync();

                    var response = await _tcpReader.ReadLineAsync();
                    while (response != null && !RyukAck.Equals(response, StringComparison.InvariantCultureIgnoreCase))
                    {
                        response = await _tcpReader.ReadLineAsync();
                    }

                    _deathNote.Remove(filter);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Disconnected from ryuk while sending. Reconnecting now.");
                _connectToRyukWorker.Notify();
            }
        }
    }
}
