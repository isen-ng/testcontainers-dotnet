using Microsoft.Extensions.DependencyInjection;
using TestContainers.Container.Abstractions.Hosting;

namespace Container.Database.Hosting
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder<T> ConfigureDatabaseConfiguration<T>(this ContainerBuilder<T> builder,
            string username, string password, string databaseName)
            where T : DatabaseContainer
        {
            return builder.ConfigureServices(s =>
            {
                s.AddSingleton<IDatabaseContext>(new DatabaseContext
                {
                    Username = username,
                    Password = password,
                    DatabaseName = databaseName
                });
            });
        }
    }
}