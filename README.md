# TestContainers dotnet

[![codecov](https://codecov.io/gh/isen-ng/testcontainers-dotnet/branch/master/graph/badge.svg)](https://codecov.io/gh/isen-ng/testcontainers-dotnet)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=testcontainers-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=testcontainers-dotnet)
[![Donation](https://img.shields.io/badge/Buy-me%20a%20coffee-orange.svg)](https://paypal.me/isenng)

> Testcontainers is a dotnet standard 2.0 library that supports NUnit and XUnit tests, providing lightweight, throwaway 
instances of common databases or anything else that can run in a Docker container.
> 
> Uses common Microsoft dependency injection patterns, app and host settings, and Microsoft Extensions Logging (MEL).
>  
> This is a port of [testcontainers-java](https://github.com/testcontainers/testcontainers-java) for dotnet.

Linux: [![Build Status](https://travis-ci.org/isen-ng/testcontainers-dotnet.svg?branch=master)](https://travis-ci.org/isen-ng/testcontainers-dotnet)

Windows: [![Build status](https://ci.appveyor.com/api/projects/status/4hcmw8qnlp86vag0/branch/master?svg=true)](https://ci.appveyor.com/project/isen-ng/testcontainers-dotnet/branch/master)

---

## Feature parity

## Linux environment

* Container management
* Docker providers
  - Unix socket
  - Environment
* Image management
  - Pulling from public repo
  - Building from docker file
* Network management
  - User defined networks
  - Network aliases
* Ryuk resource reaper

## Windows environment

* Container management
* Docker providers
  - Npipe
  - Environment
* Image management
  - Pulling from public repo
  - Building from docker file
* Network management
  - User defined networks
  - Network aliases
* Todo: Windows version of Ryuk [Help wanted]

## Built-in containers

| Container            | Readme                                                | Version
|----------------------|-------------------------------------------------------|--------
| Generic Container    | --                                                    | [![Generic](https://img.shields.io/nuget/v/TestContainers.Container.Abstractions.svg)](https://www.nuget.org/packages/TestContainers.Container.Abstractions/)
| MsSql Container      | [README](src/Container.Database.MsSql/README.md)      | [![MySql](https://img.shields.io/nuget/v/TestContainers.Container.Database.MsSql.svg)](https://www.nuget.org/packages/TestContainers.Container.Database.MsSql/)
| PostgreSql Container | [README](src/Container.Database.PostgreSql/README.md) | [![PostgreSql](https://img.shields.io/nuget/v/TestContainers.Container.Database.PostgreSql.svg)](https://www.nuget.org/packages/TestContainers.Container.Database.PostgreSql/)
| ArangoDb Container   | [README](src/Container.Database.ArangoDb/README.md)   | [![ArangoDb](https://img.shields.io/nuget/v/TestContainers.Container.Database.ArangoDb.svg)](https://www.nuget.org/packages/TestContainers.Container.Database.ArangoDb/)


## Example code

For more examples, see [integration tests](test/Container.Abstractions.Integration.Tests/Fixtures/GenericContainerFixture.cs)

### Start a container by pulling the image from a remote repository

```csharp
var container = new ContainerBuilder<GenericContainer>()
    .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection()) // host settings
    .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection()) // app settings
    .ConfigureDockerImageName(PlatformSpecific.TinyDockerImage)
    .ConfigureLogging(builder => builder.AddConsole()) // Microsoft extensions logging
    .ConfigureContainer((context, container) =>
    {
        // add labels
        container.Labels.Add(CustomLabel.Key, CustomLabel.Value);
        
        // add environment labels
        container.Env[InjectedEnvVar.Key] = InjectedEnvVar.Value;
        
        // add exposed ports (automatically mapped to higher port
        container.ExposedPorts.Add(ExposedPort);

        /*
         to do something like `docker run -p 2345:34567 alpine:latest`,
         both expose port and port binding must be set
         */
        container.ExposedPorts.Add(PortBinding.Key);
        container.PortBindings.Add(PortBinding.Key, PortBinding.Value);
        
        // add bind mounts
        container.BindMounts.Add(new Bind
        {
            HostPath = HostPathBinding.Key,
            ContainerPath = HostPathBinding.Value,
            AccessMode = AccessMode.ReadOnly
        });
        
        // set working directory
        container.WorkingDirectory = WorkingDirectory;
        
        // set command to run
        container.Command = PlatformSpecific.ShellCommand(
                $"{PlatformSpecific.Touch} {FileTouchedByCommand}; {PlatformSpecific.Shell}")
            .ToList();
    })
    .Build();
```

### Start a container by building the image from a Dockerfile

```csharp
var image = new ImageBuilder<DockerfileImage>()
    .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection()) // host settings
    .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection()) // app settings
    .ConfigureLogging(builder => builder.AddConsole()) // Microsoft extensions logging
    .ConfigureImage((context, image) =>
    {
        image.DockerfilePath = "Dockerfile";
        image.DeleteOnExit = false;

        // add the Dockerfile into the build context 
        image.Transferables.Add("Dockerfile", new MountableFile(PlatformSpecific.DockerfileImagePath));
        // add other files required by the Dockerfile into the build context
        image.Transferables.Add(".", new MountableFile(PlatformSpecific.DockerfileImageContext));
    })
    .Build();

var container = new ContainerBuilder<GenericContainer>()
    .ConfigureDockerImage(image)
    .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection()) // host settings
    .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection()) // app settings
    .ConfigureLogging(builder => builder.AddConsole()) // Microsoft extensions logging
    .ConfigureContainer((h, c) =>
    {
        c.ExposedPorts.Add(80);
    })
    .Build();
```

or

```csharp
var container = new ContainerBuilder<GenericContainer>()
    .ConfigureDockerImage((hostContext, builderContext) => 
    {
        return new ImageBuilder<DockerfileImage>()
            // share the app/host config and service collection from the parent builder context
            .WithContextFrom(builderContext)
            .ConfigureImage((context, image) =>
            {
                image.DockerfilePath = "Dockerfile";
                image.DeleteOnExit = false;
        
                // add the Dockerfile into the build context 
                image.Transferables.Add("Dockerfile", new MountableFile(PlatformSpecific.DockerfileImagePath));
                // add other files required by the Dockerfile into the build context
                image.Transferables.Add(".", new MountableFile(PlatformSpecific.DockerfileImageContext));
            })
            .Build();
    })
    .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection()) // host settings
    .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection()) // app settings
    .ConfigureLogging(builder => builder.AddConsole()) // Microsoft extensions logging
    .ConfigureContainer((h, c) =>
    {
        c.ExposedPorts.Add(80);
    })
    .Build();
```

### Start a container with a new network

```csharp
var container = new ContainerBuilder<GenericContainer>()
    .ConfigureNetwork((hostContext, builderContext) => 
    {
        return new NetworkBuilder<UserDefinedNetwork>()
            // share the app/host config and service collection from the parent builder context
            .WithContextFrom(builderContext)
            .ConfigureNetwork((context, network) =>
            {
                // be careful when setting static network names
                // if they already exists, the existing network will be used
                // otherwise, the default NetworkName is a random string 
                network.NetworkName = "my_network"
            })
            .Build();
    })
    .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection()) // host settings
    .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection()) // app settings
    .ConfigureLogging(builder => builder.AddConsole()) // Microsoft extensions logging
    .ConfigureContainer((h, c) =>
    {
        c.ExposedPorts.Add(80);
    })
    .Build();
```

## Configuring TestContainers-dotnet

There are some configurations to testcontainers-dotnet that cannot be performed in code or injected. 
These configuration can be set in environment variables before the first instance of your container is built.

 | Variable          | Default                             | Description
 |-------------------|-------------------------------------|------------
 | `REAPER_DISABLED` | (not-set)                           | When set to `1` or `true`, disables starting of the reaper container
 | `REAPER_IMAGE`    | `quay.io/testcontainers/ryuk:0.2.3` | Change which container image to use for reaper
