namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    public interface IPlatformSpecific
    {
        string TinyDockerImage { get; }

        string RyukImage { get; }
    }
}