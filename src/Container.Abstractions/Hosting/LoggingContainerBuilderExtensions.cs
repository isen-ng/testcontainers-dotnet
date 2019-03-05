using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestContainers.Container.Abstractions.Hosting
{
    public static class LoggingContainerBuilderExtensions
    {
        public static ContainerBuilder<T> ConfigureLogging<T>(this ContainerBuilder<T> builder,
            Action<HostContext, ILoggingBuilder> configureLogging)
            where T : IContainer
        {
            return builder.ConfigureServices((context, collection) =>
                collection.AddLogging(loggingBuilder => configureLogging(context, loggingBuilder)));
        }

        public static ContainerBuilder<T> ConfigureLogging<T>(this ContainerBuilder<T> builder,
            Action<ILoggingBuilder> configureLogging)
            where T : IContainer
        {
            return builder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }
    }
}