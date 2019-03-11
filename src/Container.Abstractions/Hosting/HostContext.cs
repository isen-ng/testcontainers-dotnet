using Microsoft.Extensions.Configuration;

namespace TestContainers.Container.Abstractions.Hosting
{
    /// <summary>
    /// Context to hold host variables
    /// </summary>
    public class HostContext
    {
        /// <summary>
        /// Configuration by host settings
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Environment name in host settings
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Application name in host settings
        /// </summary>
        public string ApplicationName { get; set; }
    }
}