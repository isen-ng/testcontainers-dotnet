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
using TestContainers.Container.Abstractions.Utilities;
using TestContainers.Container.Abstractions.Utilities.Platform;
using TestContainers.Container.Abstractions.WaitStrategies;

namespace TestContainers.Container.Abstractions.Reaper
{
    /// <summary>
    /// Container to start ryuk
    /// </summary>
    public class RyukContainer : AbstractContainer
    {
        private const string RyukAck = "ACK";

        private const int RyukPort = 8080;

        private readonly ILogger<RyukContainer> _logger;
        
        private readonly BatchWorker _sendToRyukWorker;
        
        private readonly BatchWorker _connectToRyukWorker;

        private readonly Dictionary<string, string> _deathNote = new Dictionary<string, string>();

        private string _ryukHost;
        
        private int _ryukPort;
        
        private TcpClient _tcpClient;
        
        private Stream _tcpWriter;
            
        private StreamReader _tcpReader;

        /// <inheritdoc />
        public RyukContainer(IDockerClient dockerClient, IPlatformSpecific platformSpecific, ILoggerFactory loggerFactory)
            : base(platformSpecific.RyukImage, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RyukContainer>();
            _sendToRyukWorker = new BatchWorkerFromDelegate(SendToRyuk);
            _connectToRyukWorker = new BatchWorkerFromDelegate(ConnectToRyuk);
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            WaitStrategy = new ExposedPortsWaitStrategy(new List<int> {RyukPort});
            ExposedPorts.Add(RyukPort);

            BindMounts.Add(new Bind
            {
                HostPath = DockerClient.Configuration.EndpointBaseUri.AbsolutePath,
                ContainerPath = DockerClient.Configuration.EndpointBaseUri.AbsolutePath,
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
        /// Adds labels for ryuk to kill
        /// </summary>
        /// <param name="dictionary">dictionary of labels</param>
        public void AddToDeathNote(Dictionary<string, string> dictionary)
        {
            foreach (var entry in dictionary)
            {
                _deathNote.Add(entry.Key, entry.Value);
            }

            _sendToRyukWorker.Notify();
        }

        /// <summary>
        /// Add label for ryuk to kill
        /// </summary>
        /// <param name="label">label name</param>
        /// <param name="value">label value</param>
        public void AddToDeathNote(string label, string value)
        {
            _deathNote.Add(label, value);
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
            
            var clone = _deathNote.ToDictionary(e => e.Key, e => e.Value);

            var labels = clone
                .Select(note => $"label={note.Key}={note.Value}")
                .Aggregate((current, next) => current + "&" + next);

            var bodyBytes = Encoding.UTF8.GetBytes(labels + "\n");

            try
            {
                await _tcpWriter.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                await _tcpWriter.FlushAsync();
                    
                var response = await _tcpReader.ReadLineAsync();
                while (response != null && !RyukAck.Equals(response, StringComparison.InvariantCultureIgnoreCase))
                {
                    response = await _tcpReader.ReadLineAsync();
                }
                    
                foreach (var note in clone)
                {
                    _deathNote.Remove(note.Key);
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