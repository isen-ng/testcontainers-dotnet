namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    /// <summary>
    /// Platform specific stugg
    /// </summary>
    public interface IPlatformSpecific
    {
        /// <summary>
        /// Platform specific base docker image
        /// </summary>
        string TinyDockerImage { get; }

        /// <summary>
        /// Platform specific ryuk image
        /// </summary>
        string RyukImage { get; }
    }
}