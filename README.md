DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

[![Build status](https://ci.appveyor.com/api/projects/status/te0oktwwf7xx5e3k/branch/dev)](https://ci.appveyor.com/project/MaksimVolkau/dryioc-426/branch/dev)
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
[v2.0]: https://bitbucket.org/dadhi/dryioc/wiki/Home

* Designed for low-ceremony use, performance, and extensibility.
* Supports .NET 3.5+; PCL Profiles 259 and 328, [.NET Core via "dotnet"](https://oren.codes/2015/07/29/targeting-net-core)
* [Documentation wiki][WikiHome]
* [Contributing guide](CONTRIBUTING.md)
* Available at NuGet as [DryIoc.dll] or source [DryIoc] (_in NuGet < 3.0_)
* __DryIoc v2.0-rc4__ is the latest: `PM> Install-Package DryIoc.dll -Pre`
    * [release notes](https://bitbucket.org/dadhi/dryioc/wiki/Home#markdown-header-latest-versions)
* __DryIoc v1.4.1__ is stable: `PM> Install-Package DryIoc.dll`

## Benchmarks
* [Performance](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)
* [Features (V2)](http://featuretests.apphb.com/DependencyInjection.html)

## Performance
* General use-cases optimized for max speed.
* Memory footprint preserved as small as possible.

## Code/Library
* No dependencies on the other libraries.
* Public API is fully documented.

## Reliability
* Unit-test suit with ~700 tests.
* Thread-safe and lock-free: registrations and resolutions could be made in parallel without corrupting container state. 
* Detects recursive dependencies - cycles in object graph.
* Throws exceptions as early as possible. Exception provides meaningful information about problem and context.
* Resolve as `LambdaExpression` to be clear about service creation expression.

## Features

* Register interface/type mapping, additionally supported: registering service once, registration update, removing registration. 
* Register user-defined delegate factory and register existing instance.
* Register from assembly(ies) implementation types with automatically determined service types.
* Register with arbitrary key and condition, multiple default registrations.
* Resolve and ResolveMany. 
* Unknown service resolution with `Rules.WithUnknownServiceResolvers()`. 
* Instance lifetime control or *Reuse* in DryIoc terms:
    * Nested disposable scopes, ambient scope context.
    * Supported out-of-the-box: `Singleton`, `InResolutionScope`, `InCurrentScope`, `InCurrentNamedScope`. Plus you can define your own.
    * Control reused objects behavior with `preventDisposal` and `weaklyReferenced`.
* Open-generics without special syntax.
* Constructor, property and field injection.
* Static or instance factory methods in addition to constructor. Factory methods support parameter injection same as constructors.
* Injecting properties/fields into existing object.
* Creating concrete object without registering it in Container but with injecting its parameters, properties, and fields.
* Metadata object associating with registration.
* Generic wrappers:
    * Service collections: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and  `I(ReadOnly)Collection|List<T>`.
    * Other: `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>` or `Tuple<TMetadata, T>`, `KeyValuePair<TKey, T>`, and user-defined wrappers.
    * [Currying](http://en.wikipedia.org/wiki/Currying) over constructor (or factory method) arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Nested wrappers: e.g. `Tuple<SomeMetadata, Func<ISomeService>>[]`.
* [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern): Composite itself is excluded from result collection.
* [Decorator Pattern](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based resolution.


## Companions

### __DryIocAttributes__

NuGet: `PM> Install-Package DryIocAttributes.dll -Pre`

- Extends [MEF](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) attributes to cover DryIoc features: metadata, advanced reuses, context based registration, decorators, etc.
- Does not depend on DryIoc and may be used by other IoC frameworks. 


### DryIocZero

NuGet: `PM> Install-Package DryIocZero -Pre`

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
- OWIN: [DryIoc.Owin](), [DryIoc.Mvc.Owin](), [DryIoc.WebApi.Owin]()