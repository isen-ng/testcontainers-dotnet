using System;
using System.Runtime.InteropServices;

namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    public class PlatformSpecificFactory
    {
        public IPlatformSpecific Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsPlatformSpecific.Instance;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return LinuxPlatformSpecific.Instance;
            }

            throw new InvalidOperationException("OS is not supported for testcontainers-dotnet");
        }

        public bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}