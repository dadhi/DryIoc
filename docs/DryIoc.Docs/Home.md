<!--Auto-generated from .cs file, the edits here will be lost! -->

# Wiki Home

[TOC]

## Getting Started

Let's define a simple interface and implementation setup:
```cs 
// these usings are for later
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedVariable

public interface IService { }
public class SomeService : IService { }

// A client consuming the `IService` dependency
public interface IClient
{
    IService Service { get; }
}

public class SomeClient : IClient
{
    public IService Service { get; }
    public SomeClient(IService service) { Service = service; }
} 
```

To illustrate the idea of Dependency Injection container, we will start from the problem.
Let's create `SomeClient` by hand, but with [DI principle](http://en.wikipedia.org/wiki/Dependency_inversion_principle) in mind:

```cs 
class Created_manually
{
    [Test]
    public void Example()
    {
        IClient client = new SomeClient(new SomeService());
    }
} 
```

The manual implementation is a hard-wired - we are using implementation types `SomeClient` and `SomeService` in-place of
creation of `IClient`. What if we need to change `SomeService` to the `TestService` later, may be after the code for
`IClient` creation is compiled. To enable such scenario, we need a kind of configuration what interface is implemented by
what type. The configuration should be decoupled from actual creation code, in order to be changed independently.

Here goes the DI / IoC container! 

Let's try DryIoc:

```cs 
class Created_by_DryIoc
{
    private IContainer _container;

    [SetUp]
    public void Configure_types()
    {
        _container = new Container();
        _container.Register<IClient, SomeClient>();
        _container.Register<IService, SomeService>();
    }

    [Test]
    public void Create_client()
    {
        var client = _container.Resolve<IClient>();
        Assert.IsInstanceOf<SomeClient>(client);
    }
} 
```

In the example with DryIoc, configuration of types is separate from the resolution, so that both can be changed independently.

Now we have a configurator and creator Container, the provider of the service instances for our program. 
Given that container controls the creation, we may logically extend it further to [control the lifetime](ReuseAndScopes) 
as well.

Summarizing, DI / IoC container is the tool to enforce [Open-Closed principle](http://msdn.microsoft.com/en-us/magazine/cc546578.aspx)
and to support __extensibility__ and __testability__ of our code.


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

