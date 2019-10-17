using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using GlobExpressions;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Reaper;
using TestContainers.Container.Abstractions.Transferables;
using TestContainers.Container.Abstractions.Utilities;
using TestContainers.Container.Abstractions.Utilities.GoLang;

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

        /// <summary>
        /// Default .dockerignore path to be used to filter the context from the base path
        /// </summary>
        public const string DefaultDockerIgnorePath = ".dockerignore";

        private static readonly Random Random = new Random();

        /// <summary>
        /// Gets or sets the path to the Dockerfile in the tar archive to be passed into the image build command
        /// </summary>
        public string DockerfilePath { get; set; } = DefaultDockerfilePath;

        /// <summary>
        /// Gets or sets the path to set the base directory for the build context.
        ///
        /// Files ignored by .dockerignore will not be copied into the context.
        /// .dockerignore file must be in the root of the base path
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Indicates whether this image should be deleted after the process ends
        /// </summary>
        public bool DeleteOnExit { get; set; } = true;

        /// <summary>
        /// Transferables that will be passed as build context to the image build command.
        /// Files added by this method will not be filtered by .dockerignore
        /// </summary>
        public Dictionary<string, ITransferable> Transferables { get; } = new Dictionary<string, ITransferable>();

        private readonly ILogger _logger;

        /// <inheritdoc />
        public DockerfileImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            ImageName = "testcontainers/" + Random.NextAlphaNumeric(16).ToLower();
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
                    if (!string.IsNullOrWhiteSpace(BasePath))
                    {
                        var ignores = GetIgnores(BasePath);
                        var allFiles = GetAllFilesInDirectory(BasePath);

                        foreach (var file in allFiles)
                        {
                            if (IsFileIgnored(ignores, BasePath, file))
                            {
                                continue;
                            }

                            var relativePath = GetRelativePath(BasePath, file);
                            await new MountableFile(file).TransferTo(tarArchive, relativePath, ct);
                        }

                        _logger.LogDebug("Transferred base path [{}] into tar archive", BasePath);
                    }

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

        private static IList<string> GetIgnores(string basePath)
        {
            var dockerIgnorePath = Path.GetFullPath(Path.Combine(basePath, DefaultDockerIgnorePath));
            return File.Exists(dockerIgnorePath)
                ? File.ReadLines(dockerIgnorePath)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .ToList()
                : new List<string>();
        }

        private static bool IsFileIgnored(IEnumerable<string> ignores, string basePath, string filePath)
        {
            var relativePath = GetRelativePath(basePath, filePath);

            var matches = ignores
                .Select(i => i.StartsWith("!") ? i.Substring(1) : i)
                .Where(i => GoLangFileMatch.Match(i, relativePath))
                .ToList();

            if (matches.Count <= 0)
            {
                return false;
            }

            var lastMatchingPattern = matches[matches.Count - 1];
            return !lastMatchingPattern.StartsWith("!");
        }

        private static IList<string> GetIgnoredFilesInBasePath(string basePath)
        {
            var dockerIgnorePath = Path.GetFullPath(Path.Combine(basePath, DefaultDockerIgnorePath));
            var ignores = File.Exists(dockerIgnorePath)
                ? File.ReadLines(dockerIgnorePath).ToList()
                : new List<string>();

            var baseDirectoryInfo = new DirectoryInfo(basePath);
            return ignores
                .Select(i => baseDirectoryInfo.GlobFiles(i))
                .SelectMany(e => e)
                .Select(f => f.FullName)
                .Append(dockerIgnorePath)
                .ToList();
        }

        private static IList<string> GetAllFilesInDirectory(string directory)
        {
            var result = new List<string>();
            result.AddRange(Directory.GetFiles(directory).Select(Path.GetFullPath));

            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                result.AddRange(GetAllFilesInDirectory(subDirectory));
            }

            return result;
        }

        private static string GetRelativePath(string relativeTo, string path)
        {
            var fullRelativeTo = Path.GetFullPath(relativeTo);
            var fullPath = Path.GetFullPath(path);

            return fullPath.StartsWith(fullRelativeTo)
                ? fullPath.Substring(fullRelativeTo.Length).TrimStart('/')
                : path;
        }
    }
}
