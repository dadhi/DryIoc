DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

[![Build status](https://ci.appveyor.com/api/projects/status/jfq01d9wcs4vcwpf/branch/default)](https://ci.appveyor.com/project/MaksimVolkau/dryioc/branch/default)
[![TestCoverage](http://dadhi.bitbucket.org/dryioc-coverage/badge_linecoverage.svg)](http://dadhi.bitbucket.org/dryioc-coverage)
[![FollowInTwitter](https://img.shields.io/badge/Follow-%40DryIoc-blue.svg)](https://twitter.com/DryIoc) 
[![SOQnA](https://img.shields.io/badge/StackOverflow-QnA-green.svg)](http://stackoverflow.com/questions/tagged/dryioc)
[![Gitter](https://img.shields.io/gitter/room/nwjs/nw.js.svg)](https://gitter.im/dadhi/DryIoc)

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/
[DryIoc]: https://www.nuget.org/packages/DryIoc/
[DryIoc.MefAttributedModel]: https://www.nuget.org/packages/DryIoc.MefAttributedModel/
[DryIoc.dll]: https://www.nuget.org/packages/DryIoc.dll/
[DryIoc.MefAttributedModel.dll]: https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll/
[WikiHome]: https://bitbucket.org/dadhi/dryioc/wiki/Home
[MefAttributedModel]: https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel
[PCL]: http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx

* Designed for low-ceremony use, performance, and extensibility.
* Supports .NET 3.5+; PCL Profiles 259, 328, [dotnet](https://oren.codes/2015/07/29/targeting-net-core); [DNX/.NET Core](https://github.com/aspnet/dnx)
* [Documented][WikiHome] and [open for contributions](CONTRIBUTING.md)
* Available at NuGet as [DryIoc.dll] or as code [DryIoc] (_in NuGet < 3.0_) 
    * `PM> Install-Package DryIoc.dll`
    * get code `PM> Install-Package DryIoc`
    * for DNX `PM> Install-Package DryIoc.Dnx`
* __DryIoc v2.4.3__ is the latest stable version
    * [Release notes](https://bitbucket.org/dadhi/dryioc/wiki/Home#markdown-header-latest-version)
    * [Previous versions](https://bitbucket.org/dadhi/dryioc/wiki/VersionHistory)


## Benchmarks

* [Performance](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)
* [Features](http://featuretests.apphb.com/DependencyInjection.html)


## Performance

* General use-cases optimized for max speed.
* Memory footprint preserved as small as possible.


## Code/Library

* No dependencies on the other libraries.
* Public API is fully documented.


## Reliability

* Unit-test suit with ~700 tests.
* Thread-safe and lock-free - registrations and resolutions may proceed in parallel without corrupting container state. 
* Detects recursive dependencies aka cycles in object graph.
* Throws exceptions as early as possible. Exception provides meaningful information about problem and context.
* Provides diagnostics for potential resolution problems via `container.VerifyResolutions()`.


## Features

* Register interface/type mapping, additionally supported: registering service once, registration update, removing registration. 
* Register user-defined delegate factory and register existing instance.
* Register implementation types from provided assemblies with automatically determined service types.
* Register with service key of arbitrary type, or register multiple non-keyed services.
* Register with resolution condition.
* Register with associated metadata object of arbitrary type.
* Resolve and ResolveMany. 
* Unknown service resolution via `Rules.WithUnknownServiceResolvers()`:
    * Optional automatic concrete types resolution
* Instance lifetime control or *Reuse* in DryIoc terms:
    * Nested disposable scopes, ambient scope context.
    * Supported out-of-the-box: `Singleton`, `InResolutionScope`, `InCurrentScope`, `InCurrentNamedScope`. Plus you can define your own.
    * `useParentReuse` option for injected dependencies
    * Control reused objects behavior with `preventDisposal` and `weaklyReferenced`.
* Extensive Open-generics support without special syntax: supported constraints, variance, complex nested generic definitions
* Constructor, property and field injection.
* Static or instance factory methods in addition to constructor. Factory methods support parameter injection the same way as constructors.
* Injecting properties and fields into existing object.
* Creating concrete object without registering it in Container but with injecting its parameters, properties, and fields.
* Generic wrappers:
    * Service collections: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and  `I(ReadOnly)Collection|List<T>`.
    * Other: `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>` or `Tuple<TMetadata, T>`, `KeyValuePair<TKey, T>`, and user-defined wrappers.
    * [Currying](http://en.wikipedia.org/wiki/Currying) over constructor (or factory method) arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Nested wrappers: e.g. `Tuple<SomeMetadata, Func<ISomeService>>[]`.
* [Composite pattern](https://bitbucket.org/dadhi/dryioc/wiki/Wrappers#markdown-header-composite-pattern-support): Composite itself is excluded from result collection.
* [Decorator pattern](https://bitbucket.org/dadhi/dryioc/wiki/Decorators). 


## Companions

### __DryIocAttributes__

NuGet: `PM> Install-Package DryIocAttributes.dll`

- Extends [MEF](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) attributes to cover DryIoc features: metadata, advanced reuses, context based registration, decorators, etc.
- Does not depend on DryIoc and may be used by other IoC libraries. 


### DryIocZero

NuGet: `PM> Install-Package DryIocZero`

Slim IoC Container based on service factory delegates __generated at compile-time__ by DryIoc.

- __Does not depend on DryIoc at run-time.__
- Ensures _zero_ application bootstrapping time associated with IoC registrations.
- Provides verification of DryIoc registration setup at compile-time by generating service factory delegates. Basically - you can see how DryIoc is creating things.
- Supports everything registered in DryIoc: reuses, decorators, wrappers, etc.
- Much smaller and simpler than DryIoc itself. Works standalone without any run-time dependencies.
- Allows run-time registrations too. You may register instances and delegates at run-time.

## Extensions

- [DryIoc.MefAttributedModel](Extensions/MefAttributedModel) 
for [MEF Attributed Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx)

- ASP.NET: 

    - [DryIoc.Web](https://www.nuget.org/packages/DryIoc.Web/) 
    - [DryIoc.Mvc](https://www.nuget.org/packages/DryIoc.WebApi.dll/)
    - [DryIoc.WepApi](https://www.nuget.org/packages/DryIoc.WebApi.dll/)
    - [DryIoc.SignalR](Extensions\SignalR)
    - [DryIoc.Dnx.DependencyInjection-RC](https://www.nuget.org/packages/DryIoc.dnx.dependencyinjection) 
for AspNetCore (vNext / DNX) 

- OWIN:

    - [DryIoc.Owin](https://www.nuget.org/packages/DryIoc.Owin.dll/)
    - [DryIoc.WebApi.Owin](https://www.nuget.org/packages/DryIoc.WebApi.Owin.dll/)

- [Nancy.Bootstrappers.DryIoc](https://www.nuget.org/packages/Nancy.Bootstrappers.DryIoc/) for [NanxyFX](http://nancyfx.org/)
- [Common Service Locator](https://www.nuget.org/packages/DryIoc.CommonServiceLocator.dll/)