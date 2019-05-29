using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestContainers.Container.Abstractions.DockerClient;

namespace TestContainers.Container.Abstractions.Hosting
{
    /// <summary>
    /// Builder class to consolidate services and inject them into an IContainer implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ContainerBuilder<T> where T : IContainer
    {
        private const string ApplicationNameKey = "applicationName";
        private const string EnvironmentKey = "environment";
        private const string DefaultEnvironment = "Production";

        private readonly List<Action<HostContext, IServiceCollection>> _configurationActions =
            new List<Action<HostContext, IServiceCollection>>();

        private readonly List<Action<IConfigurationBuilder>> _configureHostActions =
            new List<Action<IConfigurationBuilder>>();

        private readonly List<Action<HostContext, IConfigurationBuilder>> _configureAppActions =
            new List<Action<HostContext, IConfigurationBuilder>>();

        private readonly List<Action<HostContext, T>> _configureContainerActions = new List<Action<HostContext, T>>();

        private Func<HostContext, string> _dockerImageNameProvider;

        /// <summary>
        /// Sets the docker image name
        /// </summary>
        /// <param name="name">image name and tag</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when name is null</exception>
        public ContainerBuilder<T> ConfigureDockerImageName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return ConfigureDockerImageName(c => name);
        }

        /// <summary>
        /// Sets the docker image name
        /// </summary>
        /// <param name="delegate">a delegate to provide a name</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureDockerImageName(Func<HostContext, string> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            _dockerImageNameProvider = @delegate;
            return this;
        }

        /// <summary>
        /// Allows the configuration of host settings
        /// </summary>
        /// <param name="delegate">a delegate to configure host settings</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureHostConfiguration(Action<IConfigurationBuilder> @delegate)
        {
            _configureHostActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        /// <summary>
        /// Allows the configuration of app settings
        /// </summary>
        /// <param name="delegate">a delegate to configure app settings</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureAppConfiguration(Action<HostContext, IConfigurationBuilder> @delegate)
        {
            _configureAppActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        /// <summary>
        /// Allows the configuration of container
        /// </summary>
        /// <param name="delegate">a delegate to configure the container</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureContainer(Action<HostContext, T> @delegate)
        {
            _configureContainerActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        /// <summary>
        /// Allows the configuration of services
        /// </summary>
        /// <param name="delegate">a delegate to configure services</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureServices(Action<HostContext, IServiceCollection> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            _configurationActions.Add(@delegate);
            return this;
        }

        /// <summary>
        /// Allows the configuration of services
        /// </summary>
        /// <param name="delegate">a delegate to configure services</param>
        /// <returns>builder</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureServices(Action<IServiceCollection> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            return ConfigureServices((context, collection) => @delegate(collection));
        }

        /// <summary>
        /// Builds the container
        /// </summary>
        /// <returns>An implementation of the container with services injected</returns>
        public T Build()
        {
            var hostConfig = BuildHostConfiguration();
            var hostContext = new HostContext
            {
                ApplicationName = hostConfig[ApplicationNameKey],
                EnvironmentName = hostConfig[EnvironmentKey] ?? DefaultEnvironment,
                Configuration = hostConfig
            };

            var appConfig = BuildAppConfiguration(hostContext, hostConfig);
            hostContext.Configuration = appConfig;

            ConfigureServices(
                services =>
                {
                    services.AddSingleton<DockerClientFactory2>();
                    services.AddScoped(provider => 
                        provider.GetRequiredService<DockerClientFactory2>()
                        .Create()
                        .Result);

                    services.AddLogging();
                });

            var dockerImageName = _dockerImageNameProvider?.Invoke(hostContext);
            var serviceProvider = BuildServiceProvider(hostContext);

            var container = dockerImageName == null
                ? ActivatorUtilities.CreateInstance<T>(serviceProvider)
                : ActivatorUtilities.CreateInstance<T>(serviceProvider, dockerImageName);

            foreach (var action in _configureContainerActions)
            {
                action.Invoke(hostContext, container);
            }

            return container;
        }

        private IConfiguration BuildHostConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            foreach (var buildAction in _configureHostActions)
            {
                buildAction(configBuilder);
            }

            return configBuilder.Build();
        }

        private IConfiguration BuildAppConfiguration(HostContext hostContext, IConfiguration hostConfiguration)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddConfiguration(hostConfiguration);

            foreach (var buildAction in _configureAppActions)
            {
                buildAction(hostContext, configBuilder);
            }

            return configBuilder.Build();
        }

        private IServiceProvider BuildServiceProvider(HostContext hostContext)
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configurationActions)
            {
                configureServices(hostContext, services);
            }

            return new DefaultServiceProviderFactory().CreateServiceProvider(services);
        }
    }
}