namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    public class LinuxPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new LinuxPlatformSpecific();

        public string TinyDockerImage { get; } = "alpine:3.5";

        public string RyukImage { get; } = "quay.io/testcontainers/ryuk:0.2.3";
    }
}