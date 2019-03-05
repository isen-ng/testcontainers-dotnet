namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    public class WindowsPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new WindowsPlatformSpecific();

        // https://github.com/appveyor/ci/issues/2466
        // https://github.com/appveyor/ci/issues/1885
        //public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:1809";
        public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:sac2016";

        public string RyukImage { get; } = "ryuk:0.2.3-nanoserver-sac2016";
    }
}