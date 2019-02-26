namespace Containers.Integration.Tests.Platforms
{
    public interface IPlatformSpecific
    {
        string TinyDockerImage { get; }
        
        string ShellCommand { get; }
        
        string EchoCommand { get; }

        string CurrentPathCommand { get; }
 
        string CatCommand { get; }
        
        string TouchCommand { get; }
        
        string[] PrivilegedCommand { get; }
        
        string BindPath { get; }
        
        string TouchedFilePath { get; }
        
        string WorkingDirectory { get; }
        
        string EnvVarFormat(string var);

        string[] ShellCommandFormat(string command);
        
        string IfExistsThenFormat(string @if, string then);
    }
}