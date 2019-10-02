using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Reaper;
using TestContainers.Container.Abstractions.Transferables;
using TestContainers.Container.Abstractions.Utilities;

namespace TestContainers.Container.Abstractions.Images
{
    /// <summary>
    /// Represents a docker image that will be built from a Dockerfile
    /// </summary>
    public class DockerfileImage : AbstractImage
    {
        /// <summary>
        /// Default Dockerfile path to be passed into the image build docker command
        /// </summary>
        public const string DefaultDockerfilePath = "Dockerfile";

        private static readonly Random SRandom = new Random();

        /// <summary>
        /// Gets or sets the path to the Dockerfile in the tar archive to be passed into the image build command
        /// </summary>
        public string DockerfilePath { get; set; } = DefaultDockerfilePath;

        /// <summary>
        /// Indicates whether this image should be deleted after the process ends
        /// </summary>
        public bool DeleteOnExit { get; set; } = true;

        /// <summary>
        /// Transferables that will be passed as build context to the image build command
        /// </summary>
        public Dictionary<string, ITransferable> Transferables { get; } = new Dictionary<string, ITransferable>();

        private readonly ILogger _logger;

        /// <inheritdoc />
        public DockerfileImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());

            if (string.IsNullOrWhiteSpace(ImageName))
            {
                ImageName = "testcontainers/" + SRandom.NextAlphaNumeric(16).ToLower();
            }
        }

        /// <summary>
        /// Runs the docker image build command to build this image
        /// </summary>
        /// <inheritdoc />
        public override async Task<string> Resolve(CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (DeleteOnExit)
            {
                ResourceReaper.RegisterImageForCleanup(ImageName, DockerClient);
            }

            _logger.LogDebug("Begin building image: {}", ImageName);

            var buildImageParameters = new ImageBuildParameters
            {
                Dockerfile = DockerfilePath, Tags = new List<string> {ImageName}
            };

            var tempTarPath = Path.Combine(Path.GetTempPath(), ImageName.Replace('/', '_') + ".tar");

            try
            {
                using (var tempFile = new FileStream(tempTarPath, FileMode.Create))
                using (var tarArchive = TarArchive.CreateOutputTarArchive(tempFile))
                {
                    foreach (var entry in Transferables)
                    {
                        var destinationPath = entry.Key;
                        var transferable = entry.Value;
                        await transferable.TransferTo(tarArchive, destinationPath, ct);

                        _logger.LogDebug("Transferred [{}] into tar archive", destinationPath);
                    }

                    tarArchive.Close();
                }

                if (ct.IsCancellationRequested)
                {
                    return null;
                }

                using (var tempFile = new FileStream(tempTarPath, FileMode.Open))
                {
                    var output =
                        await DockerClient.Images.BuildImageFromDockerfileAsync(tempFile, buildImageParameters, ct);

                    using (var reader = new StreamReader(output))
                    {
                        while (!reader.EndOfStream)
                        {
                            _logger.LogTrace(reader.ReadLine());
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempTarPath);
            }

            _logger.LogInformation("Dockerfile image built: {}", ImageName);

            // we should not catch exceptions thrown by inspect because the image is
            // expected to be available since we've just built it
            var image = await DockerClient.Images.InspectImageAsync(ImageName, ct);
            ImageId = image.ID;

            return ImageId;
        }
    }
}
