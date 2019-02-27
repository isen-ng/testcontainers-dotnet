namespace TestContainers.Containers.Utilities.Platform
{
    public class LinuxPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new LinuxPlatformSpecific();

        public string TinyDockerImage { get; } = "alpine:3.5";

        public string RyukImage { get; } = "LinuxRyukContainerName";
    }
}