DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

<img src="./logo/logo.svg" alt="logo" width="200px"/>

[![Windows build](https://ci.appveyor.com/api/projects/status/8eypvhn6ae70vk09?svg=true)](https://ci.appveyor.com/project/MaksimVolkau/dryioc-qt8fa)
[![Linux and MacOS build](https://travis-ci.org/dadhi/ImTools.svg?branch=master)](https://travis-ci.org/dadhi/ImTools)
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
- Supports: .NET 3.5+, [.NET Standard 1.0, 1.3, 2.0](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md), PCL Profiles 259, 328
- NuGet packages:
 
    - __DryIoc.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)
    - __DryIoc__ (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc)](https://www.nuget.org/packages/DryIoc)
    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v4.0.4) :: [Previous Versions](https://bitbucket.org/dadhi/dryioc/wiki/VersionHistory)
- [Extensions and Companions](Extensions.md)
- [Documentation][WikiHome]
- [Contribution guide](CONTRIBUTING.md)
- Check the two original parts of DryIoc and now standalone projects: [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) and [ImTools](https://github.com/dadhi/ImTools)

## Benchmarks

* [Features](http://featuretests.apphb.com/DependencyInjection.html)
* [Performance overview](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)
* Realistic performance of unit-of-work with the modest size object graph ([#44](https://github.com/dadhi/DryIoc/issues/44#issuecomment-466440634), [#26](https://github.com/dadhi/DryIoc/issues/26#issuecomment-466460255)):

### Creating container, registering services, then `OpenScope` and resolve the root service

```md
                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |    160.1 us |   2.670 us |   2.498 us |   0.96 |    0.02 |     30.2734 |      0.4883 |           - |           140.03 KB |
 BmarkMicrosoftDependencyInjection |    166.9 us |   2.035 us |   1.804 us |   1.00 |    0.00 |     13.6719 |      0.2441 |           - |            58.66 KB |
                   BmarkDryIocMsDi |    180.2 us |   2.420 us |   2.263 us |   1.08 |    0.02 |     32.4707 |      0.2441 |           - |           150.03 KB |
                  BmarkAutofacMsDi |    747.8 us |   7.209 us |   6.391 us |   4.48 |    0.07 |    105.4688 |      7.8125 |           - |            487.8 KB |
                      BmarkAutofac |    790.0 us |   5.206 us |   4.615 us |   4.74 |    0.06 |    101.5625 |      6.8359 |           - |           470.32 KB |
                        BmarkGrace | 20,058.3 us | 290.376 us | 257.411 us | 120.22 |    1.59 |    156.2500 |     62.5000 |           - |           755.11 KB |
                    BmarkGraceMsDi | 23,546.7 us | 294.414 us | 275.395 us | 141.02 |    2.04 |    187.5000 |     93.7500 |     31.2500 |           926.86 KB |
```

### `OpenScope` and resolve the root service

```md
                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  3.308 us | 0.0178 us | 0.0166 us |  1.00 |    0.00 |      0.8354 |           - |           - |             3.87 KB |
                       BmarkDryIoc |  4.331 us | 0.0239 us | 0.0223 us |  1.31 |    0.01 |      1.9531 |           - |           - |             9.02 KB |
                        BmarkGrace |  4.374 us | 0.0806 us | 0.0754 us |  1.32 |    0.03 |      1.9684 |           - |           - |              9.1 KB |
                   BmarkDryIocMsDi |  5.144 us | 0.0819 us | 0.0766 us |  1.56 |    0.03 |      2.1439 |           - |           - |             9.91 KB |
                    BmarkGraceMsDi |  5.172 us | 0.0858 us | 0.0803 us |  1.56 |    0.03 |      2.1133 |           - |           - |             9.74 KB |
                      BmarkAutofac | 40.098 us | 0.6651 us | 0.6221 us | 12.12 |    0.17 |      9.8267 |           - |           - |            45.37 KB |
                  BmarkAutofacMsDi | 51.747 us | 1.0334 us | 1.4821 us | 15.47 |    0.52 |     12.6953 |           - |           - |            58.53 KB |
```

## Performance

* General use-cases optimized for max speed.
* Memory footprint preserved as small as possible.


## Code

* No dependencies on the other libraries.
* Public API is fully documented.


## Reliability

* 1500+ of acceptance tests.
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
* Unknown service resolution:
    * Optional automatic concrete types resolution
* Instance lifetime control or *Reuse* in DryIoc terms:
    * Nested disposable scopes with optional names 
    * Ambient scope context
    * Supported out-of-the-box: `Transient`, `Singleton`, `Scoped` in multiple flavors, including scoped to specific service in object graph
    * `useParentReuse` and use `useDecorateeReuse` option for injected dependencies
    * Control reused objects behavior with `preventDisposal` and `weaklyReferenced`.
* Extensive Open-generics support: constraints, variance, complex nested, recurring generic definitions
* Constructor, and optional property and field injection.
* Static and Instance factory methods in addition to constructor. Factory method supports parameter injection the same way as constructor!
* Injecting properties and fields into existing object.
* Creating concrete object without registering it in Container but with injecting its parameters, properties, and fields.
* Generic wrappers:
    * Service collections: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and  `I(ReadOnly)Collection|List<T>`.
    * Other: `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>` or `Tuple<TMetadata, T>`, `KeyValuePair<TKey, T>`, and user-defined wrappers.
    * [Currying](http://en.wikipedia.org/wiki/Currying) over constructor (or factory method) arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Nested wrappers: e.g. `Tuple<SomeMetadata, Func<ISomeService>>[]`.
* [Composite pattern](https://bitbucket.org/dadhi/dryioc/wiki/Wrappers#markdown-header-composite-pattern-support): Composite itself is excluded from result collection.
* [Decorator pattern](https://bitbucket.org/dadhi/dryioc/wiki/Decorators).


---
<small>Icon made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></small>
