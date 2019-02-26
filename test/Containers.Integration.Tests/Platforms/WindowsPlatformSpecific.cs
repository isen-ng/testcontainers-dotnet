namespace Containers.Integration.Tests.Platforms
{
    public class WindowsPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new WindowsPlatformSpecific();
        
        // https://github.com/appveyor/ci/issues/2466
        // https://github.com/appveyor/ci/issues/1885
        //public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:1809";
        public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:sac2016";
        
        public string Shell { get; } = "powershell";
        
        public string Echo { get; } = "echo";
        
        public string Touch { get; } = "fc >";
        
        public string BindPath { get; } = "C:\\host";

        public string TouchedFilePath { get; } = "C:\\touched";

        public string WorkingDirectory { get; } = "C:\\Windows";

        public string[] PwdCommand()
        {
            return ShellCommand("echo \"$pwd\"");
        }

        public string[] CatCommand(string file)
        {
            return ShellCommand("type " + file);
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
            return new[] {$"{Shell}", "-command", command};
        }
        
        public string EnvVarFormat(string var)
        {
            return $"$env:{var}";
        }
        
        public string IfExistsThenFormat(string @if, string then)
        {
            return $"if (\"{@if}\" | Test-Path) {{ {then} }}";
        }
    }
}