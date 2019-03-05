using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging.Abstractions;
using TestContainers.Container.Abstractions.Models;
using TestContainers.Container.Abstractions.Utilities;
using TestContainers.Container.Abstractions.Utilities.Platform;
using TestContainers.Container.Abstractions.WaitStrategies;

namespace TestContainers.Container.Abstractions.Reaper
{
    public class RyukContainer : AbstractContainer
    {
        private const string RyukAck = "ACK";

        private const int RyukPort = 8080;

        private readonly BatchWorker _sendToRyukWorker;

        private readonly Dictionary<string, string> _deathNote = new Dictionary<string, string>();

        private TcpClient _tcpClient;

        private NetworkStream _tcpWriter;

        private TextReader _tcpReader;

        public RyukContainer(IDockerClient dockerClient, IPlatformSpecific platformSpecific)
            : base(platformSpecific.RyukImage, dockerClient, NullLoggerFactory.Instance)
        {
            _sendToRyukWorker = new BatchWorkerFromDelegate(SendToRyuk);
        }

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

        protected override Task ContainerStarted()
        {
            // persistent tcp connection to container
            // only closed when process exits
            _tcpClient = new TcpClient(GetDockerHostIpAddress(), GetMappedPort(RyukPort));
            _tcpWriter = _tcpClient.GetStream();
            _tcpReader = new StreamReader(new BufferedStream(_tcpClient.GetStream()), Encoding.UTF8);

            return Task.CompletedTask;
        }

        protected override Task ContainerStopping()
        {
            _tcpClient?.Dispose();
            return Task.CompletedTask;
        }

        public void AddToDeathNote(Dictionary<string, string> dictionary)
        {
            foreach (var entry in dictionary)
            {
                _deathNote.Add(entry.Key, entry.Value);
            }

            _sendToRyukWorker.Notify();
        }

        public void AddToDeathNote(string label, string value)
        {
            _deathNote.Add(label, value);
            _sendToRyukWorker.Notify();
        }

        internal void KillTcpConnection()
        {
            _tcpClient?.Dispose();
        }

        private async Task SendToRyuk()
        {
            var clone = _deathNote.ToDictionary(e => e.Key, e => e.Value);

            var labels = clone
                .Select(note => $"label={note.Key}={note.Value}")
                .Aggregate((current, next) => current + "&" + next);

            var bodyBytes = Encoding.UTF8.GetBytes(labels);

            // write to ryuk
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
    }
}