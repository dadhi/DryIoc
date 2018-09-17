# Wiki Home

[TOC]

## Getting Started

Starting from simple interface/implementation setup:
```csharp
public interface IService { }
public class SomeService : IService { }

public interface IClient { IService Service { get; } }
public class SomeClient : IClient
{
    public IService Service { get; private set; }
    public SomeClient(IService service) { Service = service; }
}
```

Let's compare creation of `SomeClient` manually and with help of DryIoc.

How to instantiate `SomeClient` with [DI principle](http://en.wikipedia.org/wiki/Dependency_inversion_principle) in mind:

```csharp
IClient client = new SomeClient(new SomeService());
```

That's hard-wired implementation. Let's try do it with DryIoc:
```csharp
var c = new Container();
c.Register<IClient, SomeClient>();
c.Register<IService, SomeService>();

// somewhere else
var client = c.Resolve<IClient>();
```

In DryIoc we are declaring/registering mapping between interfaces and implementations.
Then __in different space and time__ we are deciding to get/resolve `IClient` object with its dependencies by providing only the client interface.

Hey, it means that I can register different `IClient` implementations without touching consumer resolution code. And we are not speaking about [other goodies](ReuseAndScopes) yet.

As a result, IoC container is the tool to enforce [Open-Closed principle](http://msdn.microsoft.com/en-us/magazine/cc546578.aspx)
and to improve __Testability__ and __Extensibility__ of my software.


## User's Guide

- [Installing DryIoc](InstallationOptions)
- [Creating and Disposing Container](CreatingAndDisposingContainer)
- [Register and Resolve](RegisterResolve)
- [Open-generics](OpenGenerics)
- [Specify Constructor or Factory Method](SelectConstructorOrFactoryMethod)
- [Specify Dependency or Primitive Value](SpecifyDependencyAndPrimitiveValues)
- [Reuse and Scopes](ReuseAndScopes)
- [Wrappers](Wrappers)
- [Decorators](Decorators)
- [Error Detection and Resolution](ErrorDetectionAndResolution)
- [Rules and Default Conventions](RulesAndDefaultConventions)

- Advanced topics:

    - ["Child" Containers](KindsOfChildContainer)
    - [Required Service Type](RequiredServiceType)
    - [Examples of context based resolution](ExamplesContextBasedResolution)
    - [Un-registering service and Resolution Cache](UnregisterAndResolutionCache)
    - [Auto-mocking in tests](UsingInTestsWithMockingLibrary)
    - [Interception](Interception)
    - [Thread-Safety](ThreadSafety)

- FAQs

    - [FAQ - Migration from Autofac](FaqAutofacMigration)


## Companions

- [DryIocZero](Companions/DryIocZero)
- [DryIocAttributes](Companions/DryIocAttributes)

## Extensions

- [DryIoc.MefAttributedModel](Extensions/MefAttributedModel) 
for [MEF Attributed Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx)

- ASP.NET: 

    - [DryIoc.Web](https://www.nuget.org/packages/DryIoc.Web/) 
    - [DryIoc.Mvc](https://www.nuget.org/packages/DryIoc.WebApi.dll/)
    - [DryIoc.WepApi](https://www.nuget.org/packages/DryIoc.WebApi.dll/)
    - [DryIoc.SignalR](Extensions/SignalR)
    - [DryIoc.Microsoft.DependencyInjection](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection) adapter for [.NET Core DI](https://github.com/aspnet/DependencyInjection)

- OWIN:

    - [DryIoc.Owin](https://www.nuget.org/packages/DryIoc.Owin.dll/)
    - [DryIoc.WebApi.Owin](https://www.nuget.org/packages/DryIoc.WebApi.Owin.dll/)

- [Nancy.Bootstrappers.DryIoc](https://www.nuget.org/packages/Nancy.Bootstrappers.DryIoc/) for [NanxyFX](http://nancyfx.org/)
- [Common Service Locator](https://www.nuget.org/packages/DryIoc.CommonServiceLocator.dll/)

## Samples

Located in this repo:

- [DryIoc.WebApi.Owin.Sample](https://bitbucket.org/dadhi/dryioc/src/8e609b011beafd71236f9cfe3bb2d3e0589e76ae/Extensions/DryIoc.WebApi.Owin.Sample/?at=default)
- [DryIoc.AspNetCore.Sample](https://bitbucket.org/dadhi/dryioc/src/8e609b011beafd71236f9cfe3bb2d3e0589e76ae/NetCore/src/DryIoc.AspNetCore.Sample/?at=default)

External links:

- [DryIoc.WebApi sample](https://github.com/graftedbranch/dryiocwebapi.sample)
- [DryIoc with Owin, JWT, Angular.js](https://github.com/lcssk8board/owin-jwt-angularjs)
- [DryIoc for NancyFx](https://github.com/lcssk8board/DryIoc-Nancy) with 
[Nancy.SelfHosted.HelloWorld](https://github.com/lcssk8board/DryIoc-Nancy/tree/master/source/Nancy.Bootstrappers.DryIoc/Nancy.SelfHosted.HelloWorld)
- [DryIoc with MediatR](
https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.DryIoc/Program.cs)

## Latest Version

Get from NuGet:

  - __DryIoc.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)
  - __DryIoc__ (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc)](https://www.nuget.org/packages/DryIoc)
  - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)

### v3.1.0 / soon

Stay tuned!

### v3.0.0 / 2018-06-24

[Release Notes](Version3ReleaseNotes)

### [Previous Versions](VersionHistory)