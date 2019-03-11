using System;
using System.Runtime.InteropServices;

namespace TestContainers.Container.Abstractions.Utilities.Platform
{
    /// <summary>
    /// Factory to create platform specific stuff with
    /// </summary>
    public class PlatformSpecificFactory
    {
        /// <summary>
        /// Create platform specific stuff
        /// </summary>
        /// <returns>platform specific stuff</returns>
        /// <exception cref="InvalidOperationException">when os is not supported (linux and windows supported)</exception>
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

        /// <summary>
        /// True if os is windows
        /// </summary>
        /// <returns>True if os is windows</returns>
        public bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}