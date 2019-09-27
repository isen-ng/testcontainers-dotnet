using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace TestContainers.Container.Abstractions.Images
{
    /// <summary>
    /// Represents a generic docker image that can be pulled from a docker repository
    /// </summary>
    public class GenericImage : AbstractImage
    {
        private readonly ILogger _logger;

        /// <inheritdoc />
        public GenericImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Pulls the image from the remote repository if it does not exist locally
        /// </summary>
        /// <inheritdoc />
        public override async Task<string> Resolve(CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            var images = await DockerClient.Images.ListImagesAsync(new ImagesListParameters(), ct);
            var existingImage = images.FirstOrDefault(i => i.RepoTags != null && i.RepoTags.Contains(ImageName));
            if (existingImage != null)
            {
                _logger.LogDebug("Image already exists, not pulling: {}", ImageName);
                ImageId = existingImage.ID;
                return ImageId;
            }

            _logger.LogInformation("Pulling container image: {}", ImageName);
            var createParameters = new ImagesCreateParameters
            {
                FromImage = ImageName, Tag = ImageName.Split(':').Last(),
            };

            await DockerClient.Images.CreateImageAsync(
                createParameters,
                new AuthConfig(),
                new Progress<JSONMessage>(),
                ct);

            // we should not catch exceptions thrown by inspect because the image is
            // expected to be available since we've just pulled it
            var image = await DockerClient.Images.InspectImageAsync(ImageName, ct);
            ImageId = image.ID;

            return ImageId;
        }
    }
}
