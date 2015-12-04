DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

[![Build status](https://ci.appveyor.com/api/projects/status/jfq01d9wcs4vcwpf/branch/default)](https://ci.appveyor.com/project/MaksimVolkau/dryioc/branch/default)
[![TestCoverage](http://dadhi.bitbucket.org/dryioc-coverage/badge_combined.svg)](http://dadhi.bitbucket.org/dryioc-coverage)
[![FollowInTwitter](https://img.shields.io/badge/Follow-%40DryIoc-blue.svg)](https://twitter.com/DryIoc) 
[![SOQnA](https://img.shields.io/badge/StackOverflow-QnA-green.svg)](http://stackoverflow.com/questions/tagged/dryioc)

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
* Supports .NET 3.5+; PCL Profiles 259 and 328, [.NET Core](https://oren.codes/2015/07/29/targeting-net-core) and [DNX](https://github.com/aspnet/dnx)
* [Documented][WikiHome] and [open for contributions](CONTRIBUTING.md)
* Available at NuGet as [DryIoc.dll] or as code [DryIoc] (_in NuGet < 3.0_) 
    * `PM> Install-Package DryIoc.dll`
    * get code `PM> Install-Package DryIoc`
    * for DNX `PM> Install-Package DryIoc.Dnx`
* __DryIoc v2.1.0__ is the latest stable version
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
* Thread-safe and lock-free: registrations and resolutions could be made in parallel without corrupting container state. 
* Detects recursive dependencies aka cycles in object graph.
* Throws exceptions as early as possible. Exception provides meaningful information about problem and context.
* Provides diagnostics for potential resolution problems via `container.VerifyResolutions()`.


## Features

* Register interface/type mapping, additionally supported: registering service once, registration update, removing registration. 
* Register user-defined delegate factory and register existing instance.
* Register from assembly(ies) implementation types with automatically determined service types.
* Register with service key of arbitrary type, or register multiple non-keyed services.
* Register with resolution condition.
* Register with associated metadata object of arbitrary type.
* Resolve and ResolveMany. 
* Unknown service resolution with `Rules.WithUnknownServiceResolvers()`. 
* Instance lifetime control or *Reuse* in DryIoc terms:
    * Nested disposable scopes, ambient scope context.
    * Supported out-of-the-box: `Singleton`, `InResolutionScope`, `InCurrentScope`, `InCurrentNamedScope`. Plus you can define your own.
    * Control reused objects behavior with `preventDisposal` and `weaklyReferenced`.
* Open-generics without special syntax.
* Constructor, property and field injection.
* Static or instance factory methods in addition to constructor. Factory methods support parameter injection the same way as constructors.
* Injecting properties and fields into existing object.
* Creating concrete object without registering it in Container but with injecting its parameters, properties, and fields.
* Generic wrappers:
    * Service collections: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and  `I(ReadOnly)Collection|List<T>`.
    * Other: `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>` or `Tuple<TMetadata, T>`, `KeyValuePair<TKey, T>`, and user-defined wrappers.
    * [Currying](http://en.wikipedia.org/wiki/Currying) over constructor (or factory method) arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Nested wrappers: e.g. `Tuple<SomeMetadata, Func<ISomeService>>[]`.
* Resolve [Composites](http://en.wikipedia.org/wiki/Composite_pattern): Composite itself is excluded from result collection.
* Specify [Decorators](http://en.wikipedia.org/wiki/Decorator_pattern). 


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

- [MefAttributedModel](https://bitbucket.org/dadhi/dryioc/wiki/Extensions/MefAttributedModel) for [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx). Enables automatic types discovery and wiring.
- [Common Service Locator](https://commonservicelocator.codeplex.com/)
- ASP.NET: [DryIoc.Web](), [DryIoc.Mvc](), [DryIoc.WepApi]() 
- ASP.NET 5 (vNext) DI adapter: [DryIoc.Dnx.DependencyInjection]()
- OWIN: [DryIoc.Owin](), [DryIoc.Mvc.Owin](), [DryIoc.WebApi.Owin]()