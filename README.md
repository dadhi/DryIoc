DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

<img src="./logo.svg" alt="logo" width="200px"/>

[![Build status](https://ci.appveyor.com/api/projects/status/jfq01d9wcs4vcwpf/branch/default)](https://ci.appveyor.com/project/MaksimVolkau/dryioc/branch/default)
[![SOQnA](https://img.shields.io/badge/StackOverflow-QnA-green.svg)](http://stackoverflow.com/questions/tagged/dryioc)
[![Gitter](https://img.shields.io/gitter/room/nwjs/nw.js.svg)](https://gitter.im/dadhi/DryIoc)
[![Slack](https://img.shields.io/badge/Slack-Chat-blue.svg)](https://dryioc.slack.com)
[![Follow on Twitter](https://img.shields.io/twitter/follow/dryioc.svg?style=social&label=Follow)](http://twitter.com/intent/user?screen_name=DryIoc)

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/
[DryIoc.dll]: https://www.nuget.org/packages/DryIoc.dll/
[DryIoc]: https://www.nuget.org/packages/DryIoc/
[DryIoc.Internal]: https://www.nuget.org/packages/DryIoc.Internal/
[DryIoc.MefAttributedModel]: https://www.nuget.org/packages/DryIoc.MefAttributedModel/

[DryIoc.MefAttributedModel.dll]: https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll/
[WikiHome]: https://bitbucket.org/dadhi/dryioc/wiki/Home
[MefAttributedModel]: https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel
[PCL]: http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx

- Designed for low-ceremony use, performance, and extensibility.
- [MIT License](LICENSE.txt)
- Supported platforms: 
    - .NET 3.5+, PCL Profiles 259, 328
    - .Net Core via [Net Standard 1.0](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md)
- NuGet packages:
 
    - __DryIoc.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)
    - __DryIoc__ (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc)](https://www.nuget.org/packages/DryIoc)
    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)

- Latest stable DryIoc version is __3.0.0__ / 24.06.2018
    - [Release Notes](https://bitbucket.org/dadhi/dryioc/wiki/Version3ReleaseNotes)
    - [Previous versions](https://bitbucket.org/dadhi/dryioc/wiki/VersionHistory)

- [Documentation][WikiHome]
- [How to contribute](CONTRIBUTING.md)
- You may also check two original parts of DryIoc and now a standalone projects: [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) and [ImTools](https://github.com/dadhi/ImTools).

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

* More than 1000 of acceptance tests.
* Thread-safe and lock-free registrations and resolutions. 
* Detects recursive dependencies aka cycles in object graph.
* Throws exceptions as early as possible with a lot of details.
* Provides diagnostics for potential resolution problems via `container.Validate()`.


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

__DryIocAttributes.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIocAttributes.dll)](https://www.nuget.org/packages/DryIocAttributes.dll)  
__DryIocAttributes__ (sources) [![NuGet Badge](https://buildstats.info/nuget/DryIocAttributes)](https://www.nuget.org/packages/DryIocAttributes)

- Extends [MEF](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) attributes to cover DryIoc features: metadata, advanced reuses, context based registration, decorators, etc.
- Does not depend on DryIoc and may be used by other IoC libraries. 


### DryIocZero

__DryIocZero__ [![NuGet Badge](https://buildstats.info/nuget/DryIocZero)](https://www.nuget.org/packages/DryIocZero)

Slim IoC Container based on service factory delegates __generated at compile-time__ by DryIoc.

- __Does not depend on DryIoc at run-time.__
- Ensures _zero_ application bootstrapping time associated with IoC registrations.
- Provides verification of DryIoc registration setup at compile-time by generating service factory delegates. Basically you can see how DryIoc is creating things.
- Supports everything registered in DryIoc: reuses, decorators, wrappers, etc.
- Much smaller and simpler than DryIoc itself. Works standalone without any run-time dependencies.
- Allows run-time registrations too. You may register instances and delegates at run-time.

## Extensions

- [DryIoc.MefAttributedModel](Extensions/MefAttributedModel) 
for [MEF Attributed Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) 
[![NuGet Badge](https://buildstats.info/nuget/DryIoc.MefAttributedModel.dll?includePreReleases=true)](https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll)


- ASP.NET: 

    - [DryIoc.Web](https://www.nuget.org/packages/DryIoc.Web/) 
    - [DryIoc.Mvc](https://www.nuget.org/packages/DryIoc.Mvc.dll/)
    - [DryIoc.WepApi](https://www.nuget.org/packages/DryIoc.WebApi.dll/)
    - [DryIoc.SignalR](Extensions\SignalR)
    - [DryIoc.Microsoft.DependencyInjection](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection)

- OWIN:

    - [DryIoc.Owin](https://www.nuget.org/packages/DryIoc.Owin.dll/)
    - [DryIoc.WebApi.Owin](https://www.nuget.org/packages/DryIoc.WebApi.Owin.dll/)

- [Nancy.Bootstrappers.DryIoc](https://www.nuget.org/packages/Nancy.Bootstrappers.DryIoc/) for [NanxyFX](http://nancyfx.org/)
- [Common Service Locator](https://www.nuget.org/packages/DryIoc.CommonServiceLocator.dll/)

---
<small>Icon made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></small>