using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace TestContainers.Container.Database.MySql
{
    /// <summary>
    /// This class is used to give access to a compatible DbProviderFactory for MySql
    /// based on the available types at runtime.
    /// </summary>
    /// <remarks>
    /// It is needed because multiple provider exist and because MySqlConnector had a breaking change in v1.0
    /// so the namespace and class name of the provider may change based on the referenced packages.
    /// </remarks>
    internal static class ClientFactoryAccessor
    {
        private static readonly (string assemblyName, string typeName)[] ProviderFactoryTypes = {
            ("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory"),
            ("MySqlConnector", "MySql.Data.MySqlClient.MySqlClientFactory"),
            ("MySqlConnector", "MySqlConnector.MySqlConnectorFactory"),
        };

        /// <summary>
        /// Provides an instance of <see cref="DbProviderFactory"/> for MySql, based on the available types
        /// </summary>
        public static readonly DbProviderFactory ClientFactoryInstance = GetFactoryInstance();

        private static DbProviderFactory GetFactoryInstance()
        {
            //Try to find a valid `Instance` property for each know provider type
            foreach ((string assemblyName, string typeName) in ProviderFactoryTypes)
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(assemblyName));
                    var providerFactoryType = assembly.GetType(typeName);
                    var instanceProperty = providerFactoryType.GetFields().FirstOrDefault(p =>
                        string.Equals(p.Name, "Instance", StringComparison.OrdinalIgnoreCase) && p.IsStatic);
                    if (instanceProperty is null)
                    {
                        continue;
                    }
                    return (DbProviderFactory)instanceProperty.GetValue(null);
                }
                catch (Exception)
                {
                    // Could not load this factory, try with next one
                }
            }
            throw new TypeLoadException("Could not load any DbProviderFactory type. " +
                                        "Ensure that a suitable MySQL data provider is installed."
            );
        }
    }
}
