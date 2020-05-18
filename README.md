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

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v4.2.0) :: [Previous Versions](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/VersionHistory.md)
- [Extensions and Companions](Extensions.md)
- [Documentation][WikiHome]
- [Contribution guide](CONTRIBUTING.md)
- Check the old issues on [BitBucket](https://bitbucket.org/dadhi/dryioc)
- Two original parts of DryIoc are now separate projects: [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) and [ImTools](https://github.com/dadhi/ImTools)

## Benchmarks

* [Features](http://featuretests.apphb.com/DependencyInjection.html)
* [Performance overview](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)

### Performance of the *realistic* unit-of-work object graph with 40 dependencies on 4 levels

Related issues are [#44](https://github.com/dadhi/DryIoc/issues/44#issuecomment-466440634) and [#26](https://github.com/dadhi/DryIoc/issues/26#issuecomment-466460255).

#### Registering services then opening scope and resolving the root scoped service (e.g. controller) for the first time

DryIoc 4.1.3 (.MsDI 3.0.3), MsDI 3.1.3, Grace 7.1.0 (.MsDI 7.0.1), Autofac 5.1.2 (.MsDI 6.0.0), Lamar 4.2.1

```md
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.200
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT

|       Method |         Mean |     Error |    StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 | Gen 2 | Allocated |
|------------- |-------------:|----------:|----------:|-------:|--------:|---------:|--------:|------:|----------:|
|         MsDI |     99.02 us |  1.956 us |  2.806 us |   1.00 |    0.00 |  16.1133 |  0.2441 |     - |  74.24 KB |
|       DryIoc |     97.25 us |  0.493 us |  0.461 us |   0.97 |    0.03 |  15.1367 |  1.3428 |     - |  69.79 KB |
|  DryIoc_MsDI |    124.04 us |  1.770 us |  1.655 us |   1.24 |    0.04 |  19.2871 |  1.8311 |     - |   89.1 KB |
|        Grace | 16,869.55 us | 80.435 us | 75.239 us | 168.94 |    5.72 | 156.2500 | 62.5000 |     - | 727.59 KB |
|   Grace_MsDI | 20,468.19 us | 66.869 us | 62.549 us | 204.98 |    7.02 | 187.5000 | 93.7500 |     - | 898.37 KB |
|   Lamar_MsDI |  6,060.29 us | 23.102 us | 20.479 us |  60.55 |    2.06 | 140.6250 | 23.4375 |     - | 646.33 KB |
|      Autofac |    583.26 us | 18.342 us | 17.157 us |   5.84 |    0.21 | 102.5391 | 28.3203 |     - | 472.86 KB |
| Autofac_MsDI |    561.82 us |  4.129 us |  3.862 us |   5.63 |    0.20 | 101.5625 | 27.3438 |     - | 467.85 KB |
```

DryIoc v4.0 and the older libs - kept for comparison:

```md
|              Method |        Mean  |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------------- |-------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
|                MsDI |    166.90 us |   2.035 us |   1.804 us |   1.00 |    0.00 |     13.6719 |      0.2441 |           - |            58.66 KB |
|              DryIoc |    160.10 us |   2.670 us |   2.498 us |   0.96 |    0.02 |     30.2734 |      0.4883 |           - |           140.03 KB |
|  DryIoc_MsDIAdapter |    180.20 us |   2.420 us |   2.263 us |   1.08 |    0.02 |     32.4707 |      0.2441 |           - |           150.03 KB |
|               Grace | 20,058.30 us | 290.376 us | 257.411 us | 120.22 |    1.59 |    156.2500 |     62.5000 |           - |           755.11 KB |
|   Grace_MsDIAdapter | 23,546.70 us | 294.414 us | 275.395 us | 141.02 |    2.04 |    187.5000 |     93.7500 |     31.2500 |           926.86 KB |
|             Autofac |    790.00 us |   5.206 us |   4.615 us |   4.74 |    0.06 |    101.5625 |      6.8359 |           - |           470.32 KB |
| Autofac_MsDIAdapter |    747.80 us |   7.209 us |   6.391 us |   4.48 |    0.07 |    105.4688 |      7.8125 |           - |            487.8 KB |
```

#### Opening scope and resolving the root scoped service (e.g. controller) after the warm-up

DryIoc 4.1.3 (.MsDI 3.0.3), MsDI 3.1.3, Grace 7.1.0 (.MsDI 7.0.1), Autofac 5.1.2 (.MsDI 6.0.0), Lamar 4.2.1

```md
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.200
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT

|              Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|                MsDI |  3.551 us | 0.0142 us | 0.0126 us |  1.00 |    0.00 |  0.9460 | 0.0153 |     - |   4.35 KB |
|              DryIoc |  1.647 us | 0.0050 us | 0.0042 us |  0.46 |    0.00 |  0.6428 |      - |     - |   2.96 KB |
|  DryIoc_MsDIAdapter |  2.400 us | 0.0172 us | 0.0161 us |  0.68 |    0.01 |  0.6485 | 0.0076 |     - |   2.98 KB |
|               Grace |  1.699 us | 0.0047 us | 0.0037 us |  0.48 |    0.00 |  0.6886 |      - |     - |   3.17 KB |
|   Grace_MsDIAdapter |  2.322 us | 0.0163 us | 0.0136 us |  0.65 |    0.00 |  0.7401 | 0.0076 |     - |   3.41 KB |
|          Lamar_MsDI |  7.281 us | 0.0586 us | 0.0520 us |  2.05 |    0.02 |  0.9308 | 0.4654 |     - |    5.7 KB |
|             Autofac | 50.146 us | 0.5242 us | 0.4377 us | 14.13 |    0.14 | 10.4980 |      - |     - |  48.54 KB |
| Autofac_MsDIAdapter | 62.118 us | 0.1595 us | 0.1492 us | 17.50 |    0.07 | 12.9395 | 0.8545 |     - |  59.89 KB |
```

DryIoc v4.0 and older libs - kept for comparison:

```md
|                  Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|                    MsDI |  3.308 us | 0.0178 us | 0.0166 us |  1.00 |    0.00 |      0.8354 |           - |           - |             3.87 KB |
|                  DryIoc |  4.331 us | 0.0239 us | 0.0223 us |  1.31 |    0.01 |      1.9531 |           - |           - |             9.02 KB |
|      DryIoc_MsDIAdapter |  5.144 us | 0.0819 us | 0.0766 us |  1.56 |    0.03 |      2.1439 |           - |           - |             9.91 KB |
|                   Grace |  4.374 us | 0.0806 us | 0.0754 us |  1.32 |    0.03 |      1.9684 |           - |           - |              9.1 KB |
|       Grace_MsDIAdapter |  5.172 us | 0.0858 us | 0.0803 us |  1.56 |    0.03 |      2.1133 |           - |           - |             9.74 KB |
|                 Autofac | 40.098 us | 0.6651 us | 0.6221 us | 12.12 |    0.17 |      9.8267 |           - |           - |            45.37 KB |
|     Autofac_MsDIAdapter | 51.747 us | 1.0334 us | 1.4821 us | 15.47 |    0.52 |     12.6953 |           - |           - |            58.53 KB |
```


## Reliability

* 1500+ of acceptance tests.
* Thread-safe and lock-free registrations and resolutions. 
* Detects recursive dependencies (cycles) in object graph.
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
