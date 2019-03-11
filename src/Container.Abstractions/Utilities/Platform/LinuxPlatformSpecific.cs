namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    /// <inheritdoc />
    public class LinuxPlatformSpecific : IPlatformSpecific
    {
        /// <summary>
        /// Linux platform specific stuff
        /// </summary>
        public static IPlatformSpecific Instance { get; } = new LinuxPlatformSpecific();

        /// <inheritdoc />
        public string TinyDockerImage { get; } = "alpine:3.5";

        /// <inheritdoc />
        public string RyukImage { get; } = "quay.io/testcontainers/ryuk:0.2.3";

        private LinuxPlatformSpecific()
        {
        }
    }
}