using System;
using System.IO;

namespace TestContainers.Container.Abstractions.Utilities
{
    /// <summary>
    /// Static helper class for OS specific actions
    /// </summary>
    public static class OS
    {
        private const char WindowsDirectorySeparator = '\\';
        private const char LinuxDirectorySeparator = '/';

        /// <summary>
        /// Normalizes a path into its OS specific form.
        ///
        /// It currently replaces directory separators.
        /// </summary>
        /// <param name="path">path to normalize</param>
        /// <returns>normalized path</returns>
        /// <exception cref="NotSupportedException">when OS's directory separator is not supported</exception>
        public static string NormalizePath(string path)
        {
            char toReplace;
            char replacedTo;

            switch (Path.DirectorySeparatorChar)
            {
                case WindowsDirectorySeparator:
                    toReplace = LinuxDirectorySeparator;
                    replacedTo = WindowsDirectorySeparator;
                    break;
                case LinuxDirectorySeparator:
                    toReplace = WindowsDirectorySeparator;
                    replacedTo = LinuxDirectorySeparator;
                    break;
                default:
                    throw new NotSupportedException(
                        $"Directory separator[{Path.DirectorySeparatorChar}] is not a supported type");
            }

            return path.Replace(toReplace, replacedTo);
        }
    }
}
