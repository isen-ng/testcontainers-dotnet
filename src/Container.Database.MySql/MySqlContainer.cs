using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Abstractions.Images;
using TestContainers.Container.Database.AdoNet;

namespace TestContainers.Container.Database.MySql
{
    /// <summary>
    /// MySql db container
    /// </summary>
    /// <inheritdoc />
    public class MySqlContainer : AdoNetContainer
    {
        /// <summary>
        /// Default image name
        /// </summary>
        public new const string DefaultImage = "mysql";

        /// <summary>
        /// Default image tag
        /// </summary>

        public new const string DefaultTag = "8";

        /// <summary>
        /// Default db port
        /// </summary>
        public const int DefaultPort = 3306;

        private static IImage CreateDefaultImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            return new GenericImage(dockerClient, loggerFactory) {ImageName = $"{DefaultImage}:{DefaultTag}"};
        }

        private string _connectionString;

        /*
         * NOTE: MySqlConnector made a breaking change in v1.0 by changing the namespace.
         * We have to try loading multiple types of DbProviderFactory depending on the installed version.
         * Once found, the DbProviderFactory is cached in this Lazy instance.
         */
        private static readonly Lazy<DbProviderFactory> s_lazyDbProviderFactory = new Lazy<DbProviderFactory>(() =>
        {
            var providerFactoryTypes = new[]
            {
                ("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory"),
                ("MySqlConnector", "MySql.Data.MySqlClient.MySqlClientFactory"),
                ("MySqlConnector", "MySqlConnector.MySqlConnectorFactory"),
            };
            foreach ((string assemblyName, string typeName) in providerFactoryTypes)
            {
                try
                {
                    var asmName = new AssemblyName(assemblyName);
                    var asm = Assembly.Load(asmName);
                    var providerFactoryType = asm.GetType(typeName);
                    var prop = providerFactoryType.GetFields().FirstOrDefault(p =>
                        string.Equals(p.Name, "Instance", StringComparison.OrdinalIgnoreCase) && p.IsStatic);
                    if (prop is null)
                    {
                        continue;
                    }
                    return (DbProviderFactory)prop.GetValue(null);
                }
                catch (Exception)
                {
                    // Could not load this factory, try with next one
                }
            }
            throw new TypeLoadException("Could not load any DbProviderFactory type. " +
                                        "Ensure that a suitable MySQL data provider is installed."
            );
        });

        /// <inheritdoc />
        protected override DbProviderFactory DbProviderFactory => s_lazyDbProviderFactory.Value;

        /// <inheritdoc />
        public MySqlContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        public MySqlContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        [ActivatorUtilitiesConstructor]
        public MySqlContainer(IImage dockerImage, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(NullImage.IsNullImage(dockerImage) ? CreateDefaultImage(dockerClient, loggerFactory) : dockerImage,
                dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            await base.ConfigureAsync();

            ExposedPorts.Add(DefaultPort);
            Env.Add("MYSQL_DATABASE", DatabaseName);
            Env.Add("MYSQL_ALLOW_EMPTY_PASSWORD", "yes");

            if (Username == "root")
            {
                Env.Add("MYSQL_ROOT_PASSWORD", Password);
            }
            else
            {
                Env.Add("MYSQL_USER", Username);
                Env.Add("MYSQL_PASSWORD", Password);
            }
        }

        /// <inheritdoc />
        protected override Task ContainerStarted()
        {
            var builder = DbProviderFactory.CreateConnectionStringBuilder();
            builder["server"] = GetDockerHostIpAddress();
            builder["port"] = GetMappedPort(DefaultPort);
            builder["database"] = DatabaseName;
            builder["username"] = Username;
            builder["password"] = Password;

            _connectionString = builder.ConnectionString;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public override string GetConnectionString()
        {
            if (_connectionString == null)
            {
                throw new InvalidOperationException("Container must be started before the connection string is ready");
            }

            return _connectionString;
        }
    }
}
