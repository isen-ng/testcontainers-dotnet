namespace Container.Abstractions.Integration.Tests.Platforms
{
    public class LinuxPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new LinuxPlatformSpecific();

        public string TinyDockerImage { get; } = "alpine:3.5";

        public string Shell { get; } = "/bin/sh";

        public string Echo { get; } = "echo";

        public string Touch { get; } = "touch";

        public string BindPath { get; } = "/host";

        public string TouchedFilePath { get; } = "/tmp/touched";

        public string WorkingDirectory { get; } = "/etc";

        public string[] PwdCommand()
        {
            return new[] {"pwd"};
        }

        public string[] CatCommand(string file)
        {
            return ShellCommand("cat " + file);
        }

        public string[] EchoCommand(string message)
        {
            return ShellCommand($"{Echo} {message}");
        }

        public string[] PrivilegedCommand()
        {
            return ShellCommand("ip link add dummy0 type dummy");
        }

        public string[] ShellCommand(string command)
        {
            return new[] {$"{Shell}", "-c", command};
        }

        public string EnvVarFormat(string var)
        {
            return $"${var}";
        }

        public string IfExistsThenFormat(string @if, string then)
        {
            return $"if [ -e {@if} ]; then {then}; fi";
        }
    }
}