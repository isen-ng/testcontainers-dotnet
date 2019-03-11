using TestContainers.Container.Abstractions.Hosting;

namespace TestContainers.Container.Database.Hosting
{
    public static class ContainerBuilderExtensions
    {
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