DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

<img src="./logo/logo.svg" alt="logo" width="100px"/>

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
[WikiHome]: https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/Home.md#users-guide
[MefAttributedModel]: https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/MefAttributedModel.md
[PCL]: http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx

- Designed for low-ceremony use, performance, and extensibility.
- Supports: .NET 3.5+, [.NET Standard 1.0, 1.3, 2.0](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md), PCL Profiles 259, 328
- NuGet packages:
 
    - __DryIoc.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)
    - __DryIoc__ (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc)](https://www.nuget.org/packages/DryIoc)
    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v4.7.6) :: [Previous Versions](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/VersionHistory.md)
- [Extensions and Companions](Extensions.md)
- [Documentation][WikiHome]
- [Contribution guide](CONTRIBUTING.md)
- Two original parts of the DryIoc are now the separate projects: [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) and [ImTools](https://github.com/dadhi/ImTools)

## Benchmarks

### [Performance overview](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)

### Realistic scenario with the unit-of-work scope and object graph of 40 dependencies 4 levels deep

More details in [#44](https://github.com/dadhi/DryIoc/issues/44#issuecomment-466440634) and [#26](https://github.com/dadhi/DryIoc/issues/26#issuecomment-466460255).

The listed *.MsDI* packages are respective [Microsoft.Extensions.DependencyInjection adapters](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#default-service-container-replacement)

#### Cold start - Registering services then opening the scope and resolving the root scoped service (e.g. controller) for the first time

DryIoc 4.5.0 (.MsDI 5.0.0), MsDI 3.1.8, Grace 7.1.1 (.MsDI 7.0.1), Autofac 6.0.0 (.MsDI 7.0.2), Lamar 4.3.1

```md
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


|       Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|------------- |------------:|----------:|----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|         MsDI |    150.8 us |   2.83 us |   3.03 us |   1.00 |    0.00 |  18.0664 |  0.2441 |      - |  73.86 KB |
|       DryIoc |    129.6 us |   1.90 us |   1.68 us |   0.86 |    0.02 |  16.3574 |  0.2441 |      - |  67.52 KB |
|  DryIoc_MsDI |    161.9 us |   1.74 us |   1.63 us |   1.07 |    0.03 |  21.4844 |  0.2441 |      - |   88.6 KB |
|        Grace | 21,380.9 us | 375.46 us | 351.21 us | 141.65 |    2.83 | 156.2500 | 62.5000 |      - | 729.12 KB |
|   Grace_MsDI | 24,102.4 us | 243.21 us | 203.09 us | 159.26 |    3.52 | 187.5000 | 93.7500 |      - | 894.57 KB |
|   Lamar_MsDI | 10,938.2 us | 308.25 us | 874.46 us |  70.86 |    4.29 |        - |       - |      - | 696.16 KB |
|      Autofac |    789.4 us |  19.84 us |  20.38 us |   5.24 |    0.18 |  50.7813 | 25.3906 | 1.9531 | 311.12 KB |
| Autofac_MsDI |    784.9 us |  15.04 us |  18.47 us |   5.20 |    0.15 |  54.6875 | 27.3438 | 1.9531 | 335.07 KB |
```

<details>
  <summary>Older versions for comparison: DryIoc 4.1.3 (.MsDI 3.0.3), MsDI 3.1.3, Grace 7.1.0 (.MsDI 7.0.1), Autofac 5.1.2 (.MsDI 6.0.0), Lamar 4.2.1</summary>

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

</details>


#### Hot run - Opening the scope and resolving the root scoped service for the Nth time

DryIoc 4.5.0 (.MsDI 5.0.0), MsDI 3.1.8, Grace 7.1.1 (.MsDI 7.0.1), Autofac 6.0.0 (.MsDI 7.0.2), Lamar 4.3.1

```md
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


|       Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|         MsDI |  4.530 us | 0.0437 us | 0.0388 us |  1.00 |    0.00 |  1.0605 |      - |     - |   4.35 KB |
|       DryIoc |  1.653 us | 0.0118 us | 0.0104 us |  0.37 |    0.00 |  0.7229 |      - |     - |   2.96 KB |
|  DryIoc_MsDI |  2.629 us | 0.0524 us | 0.0644 us |  0.58 |    0.01 |  0.7286 |      - |     - |   2.98 KB |
|        Grace |  2.229 us | 0.0432 us | 0.0546 us |  0.49 |    0.02 |  0.7744 |      - |     - |   3.17 KB |
|   Grace_MsDI |  3.007 us | 0.0586 us | 0.0675 us |  0.67 |    0.02 |  0.8354 |      - |     - |   3.41 KB |
|   Lamar_MsDI |  9.270 us | 0.0788 us | 0.0737 us |  2.05 |    0.03 |  0.9308 | 0.4578 |     - |    5.7 KB |
|      Autofac | 60.151 us | 0.5309 us | 0.4707 us | 13.28 |    0.15 | 11.4746 |      - |     - |  47.28 KB |
| Autofac_MsDI | 74.027 us | 0.5597 us | 0.4370 us | 16.36 |    0.21 | 16.1133 |      - |     - |  66.09 KB |
```

<details>
<summary>Older versions for comparison: DryIoc 4.1.3 (.MsDI 3.0.3), MsDI 3.1.3, Grace 7.1.0 (.MsDI 7.0.1), Autofac 5.1.2 (.MsDI 6.0.0), Lamar 4.2.1</summary>

```md
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.200
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT


|       Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|         MsDI |  3.551 us | 0.0142 us | 0.0126 us |  1.00 |    0.00 |  0.9460 | 0.0153 |     - |   4.35 KB |
|       DryIoc |  1.647 us | 0.0050 us | 0.0042 us |  0.46 |    0.00 |  0.6428 |      - |     - |   2.96 KB |
|  DryIoc_MsDI |  2.400 us | 0.0172 us | 0.0161 us |  0.68 |    0.01 |  0.6485 | 0.0076 |     - |   2.98 KB |
|        Grace |  1.699 us | 0.0047 us | 0.0037 us |  0.48 |    0.00 |  0.6886 |      - |     - |   3.17 KB |
|   Grace_MsDI |  2.322 us | 0.0163 us | 0.0136 us |  0.65 |    0.00 |  0.7401 | 0.0076 |     - |   3.41 KB |
|   Lamar_MsDI |  7.281 us | 0.0586 us | 0.0520 us |  2.05 |    0.02 |  0.9308 | 0.4654 |     - |    5.7 KB |
|      Autofac | 50.146 us | 0.5242 us | 0.4377 us | 14.13 |    0.14 | 10.4980 |      - |     - |  48.54 KB |
| Autofac_MsDI | 62.118 us | 0.1595 us | 0.1492 us | 17.50 |    0.07 | 12.9395 | 0.8545 |     - |  59.89 KB |
```

</details>


## Reliability

* 1500+ of acceptance tests.
* Thread-safe and lock-free registrations and resolutions. 
* Detects recursive dependencies (cycles) in object graph.
* Throws exceptions as early as possible with a lot of details.
* Provides diagnostics for potential resolution problems via `container.Validate()`.


## Features

### [Feature matrix](http://featuretests.apphb.com/DependencyInjection.html)

### Incomplete feature list 

* Registration of service to implementation type mapping (additionally supported: registering once, registration replace, registration removal). 
* Registration of delegate factory and already created service instance.
* Batch registration of types from the provided assemblies.
* Registration identified with a service key of arbitrary type and registration of multiple non-keyed implementations for a single service.
* Registration with the condition depending on context.
* Registration with the associated metadata object of arbitrary type.
* Resolve and ResolveMany. 
* Service lifetime control via *Reuse* and lifetime scoping:
    * Nested disposable scopes with optional names 
    * Optional ambient scope context
    * Reuse types: `Transient`, `Singleton`, `Scoped` in multiple flavors including the scoping to the specific service ancestor in the object graph
    * Option to `useParentReuse` and to `useDecorateeReuse` (for decorators)
    * Option to `preventDisposal` and `weaklyReferenced`
* Open-generics support including type constraints, variance, complex nesting and recurring definitions.
* Constructor parameters injection and optional property and field injection.
* Static and instance factory methods with the parameter injection similar to the constructor parameter injection.
* Injection of properties and fields into the existing object.
* [Decorators](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/Decorators.md):
    * Nested with the relative order control
    * Generic and non-generic
    * With the Reuse possibly different from the decorated service
    * Decorators of the Wrappers
* [Wrappers](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/Wrappers.md):
    * Service collections: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and  `I(ReadOnly)Collection|List<T>`
    * Single service wrappers: `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>` or `Tuple<TMetadata, T>`, `KeyValuePair<TKey, T>`
    * [Currying](http://en.wikipedia.org/wiki/Currying) of constructor or factory method parameters with `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc
    * Nested wrappers: e.g. `Tuple<SomeMetadata, Func<ISomeService>>[]`
    * User-defined wrappers
    * [Composite](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/Wrappers.md#composite-pattern-support)
* User-provided strategies for resolution of unknown service.
    * Dynamic registration providers
    * Optional automatic concrete types resolution

### Resolution options

DryIoc implements a service resolution and injection via expression compilation and interpretation.
The interpretation is the only option for the target platforms without the `System.Reflection.Emit` like the Xamarin iOS.
Check the [Resolution Pipeline](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/ResolutionPipeline.md) document for more details. 

---
<small>Icon made by <a href="http://www.freepik.com" title="Freepik">Freepik</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></small>
