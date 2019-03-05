using System;
using System.Runtime.InteropServices;

namespace Container.Abstractions.Integration.Tests.Platforms
{
    public static class PlatformHelper
    {
        public static IPlatformSpecific GetPlatform()
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
    }
}