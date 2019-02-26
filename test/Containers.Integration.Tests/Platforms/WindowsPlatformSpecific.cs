namespace Containers.Integration.Tests.Platforms
{
    public class WindowsPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new WindowsPlatformSpecific();
        
        public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:1809";
        
        public string ShellCommand { get; } = "cmd";
        
        public string EchoCommand { get; } = "echo";
        
        public string CurrentPathCommand { get; } = "echo %cd%";
        
        public string CatCommand { get; } = "type";
        
        public string TouchCommand { get; } = "type NUL >";
        
        public string[] PrivilegedCommand { get; } = {"ip", "link", "add", "dummy0", "type", "dummy"};
        
        public string BindPath { get; } = "C:\\host";

        public string TouchedFilePath { get; } = "C:\\%TEMP%\\touched";

        public string WorkingDirectory { get; } = "C:\\Windows";
        
        public string EnvVarFormat(string var)
        {
            return $"%{var}%";
        }
        
        public string[] ShellCommandFormat(string command)
        {
            return new[] {$"{ShellCommand}", "-c", command};
        }
        
        public string IfExistsThenFormat(string @if, string then)
        {
            return $"if exist {@if} {then}";
        }
    }
}