namespace Containers.Integration.Tests.Platforms
{
    public class WindowsPlatformSpecific : IPlatformSpecific
    {
        public static IPlatformSpecific Instance { get; } = new WindowsPlatformSpecific();
        
        // https://github.com/appveyor/ci/issues/2466
        // https://github.com/appveyor/ci/issues/1885
        //public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:1809";
        public string TinyDockerImage { get; } = "mcr.microsoft.com/windows/nanoserver:sac2016";
        
        public string ShellCommand { get; } = "pwsh";
        
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
            return new[] {$"{ShellCommand}", "-command", command};
        }
        
        public string IfExistsThenFormat(string @if, string then)
        {
            return $"if exist {@if} {then}";
        }
    }
}