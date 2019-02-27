namespace TestContainers.Containers.Utilities.Platform
{
    public interface IPlatformSpecific
    {
        string TinyDockerImage { get; }
        
        string RyukImage { get; }
    }
}