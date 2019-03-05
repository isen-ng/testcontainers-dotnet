using Microsoft.Extensions.Configuration;

namespace TestContainers.Container.Abstractions.Hosting
{
    public class HostContext
    {
        public IConfiguration Configuration { get; set; }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; }
    }
}