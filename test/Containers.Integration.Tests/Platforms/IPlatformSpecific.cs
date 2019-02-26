namespace Containers.Integration.Tests.Platforms
{
    public interface IPlatformSpecific
    {
        string TinyDockerImage { get; }
        
        string ShellCommand { get; }
        
        string EchoCommand { get; }

        string CurrentPathCommand { get; }
 
        string CatCommand { get; }
        
        string[] PrivilegedCommand { get; }
        
        string EnvVarFormat(string var);

        string[] ShellCommandFormat(string command);
        
        string IfExistsThenFormat(string @if, string then);
    }
}