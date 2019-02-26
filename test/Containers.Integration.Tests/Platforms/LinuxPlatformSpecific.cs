namespace Containers.Integration.Tests.Platforms
{
    public class LinuxPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new LinuxPlatformSpecific();

        public string TinyDockerImage { get; } = "alpine:3.5";

        public string ShellCommand { get; } = "/bin/sh";
        
        public string EchoCommand { get; } = "echo";

        public string CurrentPathCommand { get; } = "pwd";
        
        public string CatCommand { get; } = "cat";

        public string TouchCommand { get; } = "touch";

        public string[] PrivilegedCommand { get; } = {"ip", "link", "add", "dummy0", "type", "dummy"};

        public string BindPath { get; } = "/host";

        public string TouchedFilePath { get; } = "/tmp/touched";

        public string WorkingDirectory { get; } = "/etc";

        public string EnvVarFormat(string var)
        {
            return $"${var}";
        }

        public string[] ShellCommandFormat(string command)
        {
            return new[] {$"{ShellCommand}", "-c", command};
        }

        public string IfExistsThenFormat(string @if, string then)
        {
            return $"if [ -e {@if} ]; then {then}; fi";
        }
    }
}