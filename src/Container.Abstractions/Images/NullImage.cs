using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Container.Abstractions.Images
{
    /// <summary>
    /// Represents a null image for ease of dependency injection
    /// </summary>
    public sealed class NullImage : IImage
    {
        /// <summary>
        /// Instance of this null image
        /// </summary>
        public static readonly IImage Instance = new NullImage();

        /// <summary>
        /// Checks if image is a null image
        /// </summary>
        /// <param name="image">image to check</param>
        /// <returns>if image is null or a null image</returns>
        public static bool IsNullImage(IImage image)
        {
            return image == null || image is NullImage;
        }

        /// <inheritdoc />
        public string ImageName { get; }

        /// <inheritdoc />
        public string ImageId { get; }

        /// <inheritdoc />
        private NullImage()
        {
            ImageName = null;
            ImageId = null;
        }

        /// <inheritdoc />
        public Task<string> Resolve(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
