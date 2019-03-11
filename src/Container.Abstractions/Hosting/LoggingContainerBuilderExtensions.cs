using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestContainers.Container.Abstractions.Hosting
{
    /// <summary>
    /// Allows configurations of LoggerBuilder
    /// </summary>
    public static class LoggingContainerBuilderExtensions
    {
        /// <summary>
        /// Allows the configuration of logging builder
        /// </summary>
        /// <param name="builder">builder</param>
        /// <param name="configureLogging">delegate to configure logging with</param>
        /// <typeparam name="T">Container type</typeparam>
        /// <returns>builder</returns>
        public static ContainerBuilder<T> ConfigureLogging<T>(this ContainerBuilder<T> builder,
            Action<HostContext, ILoggingBuilder> configureLogging)
            where T : IContainer
        {
            return builder.ConfigureServices((context, collection) =>
                collection.AddLogging(loggingBuilder => configureLogging(context, loggingBuilder)));
        }

        /// <summary>
        /// Allows the configuration of logging builder
        /// </summary>
        /// <param name="builder">builder</param>
        /// <param name="configureLogging">delegate to configure logging with</param>
        /// <typeparam name="T">Container type</typeparam>
        /// <returns>builder</returns>
        public static ContainerBuilder<T> ConfigureLogging<T>(this ContainerBuilder<T> builder,
            Action<ILoggingBuilder> configureLogging)
            where T : IContainer
        {
            return builder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }
    }
}