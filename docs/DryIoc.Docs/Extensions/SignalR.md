# SignalR Integration

[TOC]

# Overview

[SignalR](http://www.asp.net/signalr) is Asp.Net sibling technology for simplifying implementation of Push / Real-time web applications.

Here is the useful [SO discussion](https://stackoverflow.com/questions/10555791/using-simple-injector-with-signalr) regarding SignalR IoC integration.

DryIoc has two ways to setup things with SignalR:

- First way is to use `WithSignalR` container extension method for full blown integration. The method does the following:

    - Registering `DryIocHubActivator`
    - Registering the hubs from given assemblies. It is optional step and can be replaced by additional call to `RegisterHubs`.
    - Setting `GlobalHist.DependencyResolver` to instance of `DryIocDependencyResolver` 
 
- Second way is to select and use just what you need:

    - `DryIocHubActivator` is implementation of `IHubActivator`
    - `DryIocDependencyResolver` is implementation to use instead of `DefaultDependencyResolver`
    - Helper `RegisterHubs` methods


# Usage 

## Replacing Dependency Resolver WithSignalR method

Using max integration with `WithSignalR`:
```
#!c#
    var hubAssemblies = new[] { Assembly.GetExecutingAssembly() };
    container = new Container().WithSignalR(hubAssemblies);
    RouteTable.Routes.MapHubs();
```


## Using DryIocHubActivator with DefaultDependencyResolver

Just using `DryIocHubActivator` with default dependency resolver:
```
#!c#
    container = new Container();
    var hubAssemblies = new[] { Assembly.GetExecutingAssembly() };
    container.RegisterHubs(hubAssemblies);
    GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => new DryIocHubActivator(container));
    RouteTable.Routes.MapHubs();
```
