using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Sockets;
using System.Threading.Tasks;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.WaitStrategies;

namespace TestContainers.Container.Database.AdoNet.WaitStrategies
{
    /// <summary>
    /// Probing strategy that uses ADO.Net classes
    /// </summary>
    /// <inheritdoc />
    public class AdoNetSqlProbeStrategy : AbstractProbingStrategy
    {
        private readonly DbProviderFactory _dbProviderFactory;

        /// <inheritdoc />
        protected override IEnumerable<Type> ExceptionTypes { get; } =
            new[] { typeof(SocketException), typeof(DbException) };


        /// <inheritdoc />
        public AdoNetSqlProbeStrategy(DbProviderFactory dbProviderFactory)
        {
            _dbProviderFactory = dbProviderFactory;
        }

        /// <inheritdoc />
        protected override async Task Probe(IContainer container)
        {
            if (!(container is AdoNetContainer adoNetContainer))
            {
                throw new InvalidOperationException("Container must be an AdoNetContainer for AdoNetSqlProbeStrategy");
            }

            using (var connection = _dbProviderFactory.CreateConnection())
            {
                if (connection == null)
                {
                    throw new InvalidOperationException(
                        $"Database provider factory: '{_dbProviderFactory.GetType().Name}' did not return a connection object.");
                }

                connection.ConnectionString = adoNetContainer.GetConnectionString();

                await connection.OpenAsync().ConfigureAwait(false);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    await command.ExecuteScalarAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
