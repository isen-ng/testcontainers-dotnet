using TestContainers.Container.Abstractions.Hosting;

namespace TestContainers.Container.Database.Hosting
{
    /// <summary>
    /// Extensions to configure DatabaseContainers
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Configures common db parameters for DatabaseContainers
        /// </summary>
        /// <param name="builder">builder</param>
        /// <param name="username">db username</param>
        /// <param name="password">db password</param>
        /// <param name="databaseName">db name</param>
        /// <typeparam name="T">DatabaseContainer type</typeparam>
        /// <returns>Builder</returns>
        public static ContainerBuilder<T> ConfigureDatabaseConfiguration<T>(this ContainerBuilder<T> builder,
            string username, string password, string databaseName)
            where T : DatabaseContainer
        {
            return builder.ConfigureContainer((h, c) =>
            {
                c.Username = username;
                c.Password = password;
                c.DatabaseName = databaseName;
            });
        }
    }
}
