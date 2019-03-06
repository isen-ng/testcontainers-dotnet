using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestContainers.Container.Abstractions.Hosting
{
    public partial class ContainerBuilder<T> where T : IContainer
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

        public ContainerBuilder<T> ConfigureDockerImageName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return ConfigureDockerImageName(c => name);
        }

        public ContainerBuilder<T> ConfigureDockerImageName(Func<HostContext, string> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            _dockerImageNameProvider = @delegate;
            return this;
        }

        public ContainerBuilder<T> ConfigureHostConfiguration(Action<IConfigurationBuilder> @delegate)
        {
            _configureHostActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public ContainerBuilder<T> ConfigureAppConfiguration(Action<HostContext, IConfigurationBuilder> @delegate)
        {
            _configureAppActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public ContainerBuilder<T> ConfigureContainer(Action<HostContext, T> @delegate)
        {
            _configureContainerActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        public ContainerBuilder<T> ConfigureServices(Action<HostContext, IServiceCollection> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            _configurationActions.Add(@delegate);
            return this;
        }

        public ContainerBuilder<T> ConfigureServices(Action<IServiceCollection> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            return ConfigureServices((context, collection) => @delegate(collection));
        }

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
                    services.AddSingleton<DockerClientFactory>();
                    services.AddScoped(provider => provider.GetRequiredService<DockerClientFactory>()
                        .Create());

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