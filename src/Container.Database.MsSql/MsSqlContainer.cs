using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Container.Database.AdoNet;

namespace TestContainers.Container.Database.MsSql
{
    /// <summary>
    /// MsSQL Server container
    /// Username is always "sa"
    /// Database parameter is ignored because container does not allow creating of database
    /// Password must be:
    ///  * at least 8 characters in length
    ///  * has at least 3 out of 4 categories of
    ///    * has upper case alphabet
    ///    * has lower case alphabet
    ///    * has digit
    ///    * has non-alphanumeric character
    /// </summary>
    /// <inheritdoc />
    public class MsSqlContainer : AdoNetContainer
    {
        /// <summary>
        /// Default image name
        /// </summary>
        public new const string DefaultImage = "mcr.microsoft.com/mssql/server";

        /// <summary>
        /// Default image tag
        /// </summary>
        public new const string DefaultTag = "2017-latest-ubuntu";

        /// <summary>
        /// Default db port
        /// </summary>
        public const int DefaultPort = 1433;

        private string _connectionString;

        /// <inheritdoc />
        public override string Username => "sa";

        /// <inheritdoc />
        protected override DbProviderFactory DbProviderFactory { get; } = SqlClientFactory.Instance;

        /// <inheritdoc />
        public MsSqlContainer(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base($"{DefaultImage}:{DefaultTag}", dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        public MsSqlContainer(string dockerImageName, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerImageName, dockerClient, loggerFactory)
        {
        }

        /// <inheritdoc />
        protected override async Task ConfigureAsync()
        {
            // rigorous password validation ...
            // see: https://hub.docker.com/_/microsoft-mssql-server?tab=description
            ValidatePassword(Password);

            await base.ConfigureAsync();

            ExposedPorts.Add(DefaultPort);
            Env.Add("ACCEPT_EULA", "Y");
            Env.Add("SA_PASSWORD", Password);
        }

        /// <inheritdoc />
        protected override Task ContainerStarted()
        {
            _connectionString = CreateConnectionStringBuilder().ConnectionString;
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

        /// <summary>
        /// Gets a connection string with a specific database name
        /// </summary>
        /// <param name="databaseName">database name to use</param>
        /// <returns>a connection string</returns>
        /// <exception cref="InvalidOperationException">when the container has yet to start</exception>
        public string GetConnectionString(string databaseName)
        {
            if (_connectionString == null)
            {
                throw new InvalidOperationException("Container must be started before the connection string is ready");
            }

            var builder = CreateConnectionStringBuilder();
            builder["database"] = databaseName;

            return builder.ConnectionString;
        }

        private DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            var builder = SqlClientFactory.Instance.CreateConnectionStringBuilder();
            if (builder == null)
            {
                throw new InvalidOperationException("SqlClientFactory.CreateConnectionStringBuilder returned null");
            }

            builder["server"] = GetDockerHostIpAddress() + "," + GetMappedPort(DefaultPort);
            builder["uid"] = Username;
            builder["password"] = Password;

            return builder;
        }

        private static void ValidatePassword(string password)
        {
            if (!IsLengthValid(password))
            {
                throw new InvalidOperationException("Password length must be at least 8");
            }

            var score = 0;
            score += HasUpperCase(password) ? 1 : 0;
            score += HasLowerCase(password) ? 1 : 0;
            score += HasNumber(password) ? 1 : 0;
            score += HasNonAlphaNumeric(password) ? 1 : 0;

            if (score < 3)
            {
                throw new InvalidOperationException(
                    "Password must be of at least three of these four categories: uppercase letters, " +
                    "lowercase letters, numbers and non-alphanumeric symbols");
            }
        }

        private static bool IsLengthValid(string password)
        {
            return password.Length >= 8;
        }

        private static bool HasUpperCase(string password)
        {
            return password.ToLower() != password;
        }

        private static bool HasLowerCase(string password)
        {
            return password.ToUpper() != password;
        }

        private static bool HasNumber(string password)
        {
            return !password.Any(char.IsDigit);
        }

        private static bool HasNonAlphaNumeric(string password)
        {
            return !password.All(char.IsLetterOrDigit);
        }
    }
}
