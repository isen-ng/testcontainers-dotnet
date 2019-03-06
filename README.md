# TestContainers dotnet

> Testcontainers is a dotnet standard 2.0 library that supports NUnit and XUnit tests, providing lightweight, throwaway 
instances of common databases or anything else that can run in a Docker container.
> 
> Uses common Microsoft dependency injection patterns, app and host settings, and Microsoft Extensions Logging (MEL).
>  
> This is a port of [testcontainers-java](https://github.com/testcontainers/testcontainers-java) for dotnet.

[![codecov](https://codecov.io/gh/isen-ng/testcontainers-dotnet/branch/master/graph/badge.svg)](https://codecov.io/gh/isen-ng/testcontainers-dotnet)
[![Sonarcloud Status](https://sonarcloud.io/api/project_badges/measure?project=testcontainers-dotnet&metric=alert_status)](https://sonarcloud.io/dashboard?id=testcontainers-dotnet)

Linux: [![Build Status](https://travis-ci.org/isen-ng/testcontainers-dotnet.svg?branch=master)](https://travis-ci.org/isen-ng/testcontainers-dotnet)

Windows: [![Build status](https://ci.appveyor.com/api/projects/status/4hcmw8qnlp86vag0/branch/master?svg=true)](https://ci.appveyor.com/project/isen-ng/testcontainers-dotnet/branch/master)

---

## Feature parity

## Linux environment

* Container management
* Todo: Start Container from Dockerfile
* Ryuk resource reaper

## Windows environment

* Container management
* Todo: Start Container from Dockerfile
* Todo: Windows version of Ryuk [Help wanted]

## Built-in containers

| Container            | Version
|----------------------|------
| Generic Container    | [![Generic](https://img.shields.io/nuget/v/TestContainers.Container.Abstractions.svg)](https://www.nuget.org/packages/TestContainers.Container.Abstractions/)
| MsSql Container      | [![Generic](https://img.shields.io/nuget/v/TestContainers.Container.Database.MsSql.svg)](https://www.nuget.org/packages/TestContainers.Container.Database.MsSql/)
| PostgreSql Container | [![Generic](https://img.shields.io/nuget/v/TestContainers.Container.Database.PostgreSql.svg)](https://www.nuget.org/packages/TestContainers.Container.Database.PostgreSql/)
| ArangoDb Container   | [![Generic](https://img.shields.io/nuget/v/TestContainers.Container.Database.ArangoDb.svg)](https://www.nuget.org/packages/TestContainers.Container.Database.ArangoDb/)


## Example code

For more examples, see [integration tests](test/Container.Abstractions.Integration.Tests/Fixtures/GenericContainerFixture.cs)

```
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

