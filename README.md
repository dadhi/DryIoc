DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

<img src="./logo/logo.svg" alt="logo" width="100px"/>

[![Windows, Linux, MacOS](https://ci.appveyor.com/api/projects/status/8eypvhn6ae70vk09?svg=true)](https://ci.appveyor.com/project/MaksimVolkau/dryioc-qt8fa)
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

- Designed for low-ceremony use, performance, and extensibility.
- Supports: .NET 4.5+, [.NET Standard 1.0, 1.3, 2.0](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md)
- NuGet packages:
 
    - __DryIoc.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)
    - __DryIoc__ (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc)](https://www.nuget.org/packages/DryIoc)
    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v4.8.7) :: [Previous Versions](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/VersionHistory.md)
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

DryIoc 5.0.0 (.MsDI 5.0.0), MsDI 5.0.1, Grace 7.2.0 (.MsDI 7.1.0), Autofac 6.2.0 (.MsDI 7.1.0), Lamar 5.0.3

```md
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19042.985 (20H2/October2020Update)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=5.0.202
  [Host]     : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT
  DefaultJob : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT


|              Method |         Mean |      Error |     StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|-------------------- |-------------:|-----------:|-----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|                MsDI |     95.35 us |   1.626 us |   2.805 us |   1.00 |    0.00 |  11.9629 |  3.1738 |      - |     73 KB |
|              DryIoc |     85.03 us |   0.868 us |   0.812 us |   0.88 |    0.03 |   8.4229 |  0.6104 |      - |     52 KB |
|  DryIoc_MsDIAdapter |     92.64 us |   0.807 us |   0.754 us |   0.96 |    0.03 |  10.6201 |  0.7324 |      - |     65 KB |
|               Grace | 15,516.60 us | 135.363 us | 126.619 us | 160.82 |    6.01 | 109.3750 | 46.8750 |      - |    743 KB |
|   Grace_MsDIAdapter | 18,376.58 us | 201.534 us | 188.515 us | 190.45 |    6.71 | 125.0000 | 62.5000 |      - |    911 KB |
|   Lamar_MsDIAdapter |  5,532.70 us |  70.539 us |  62.531 us |  57.31 |    2.16 |  93.7500 | 39.0625 |      - |    611 KB |
|             Autofac |    553.95 us |  10.116 us |   8.967 us |   5.74 |    0.18 |  51.7578 | 25.3906 | 1.9531 |    318 KB |
| Autofac_MsDIAdapter |    542.17 us |   4.962 us |   4.143 us |   5.60 |    0.21 |  54.6875 | 27.3438 | 1.9531 |    340 KB |
```

<details>
  <summary>Older versions for the comparison</summary>

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
</details>


#### Hot run - Opening the scope and resolving the root scoped service for the Nth time

DryIoc 5.0.0 (.MsDI 5.0.0), MsDI 5.0.1, Grace 7.2.0 (.MsDI 7.1.0), Autofac 6.1.0 (.MsDI 7.1.0), Lamar 5.0.3

```md
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.200
  [Host]     : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT
  DefaultJob : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT

|              Method |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------- |----------:|----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|                MsDI |  3.675 us | 0.0730 us | 0.1070 us |  3.699 us |  1.00 |    0.00 |  0.7095 | 0.0114 |     - |   4.35 KB |
|              DryIoc |  1.359 us | 0.0147 us | 0.0138 us |  1.354 us |  0.37 |    0.01 |  0.4768 | 0.0057 |     - |   2.93 KB |
|  DryIoc_MsDIAdapter |  2.051 us | 0.0408 us | 0.0437 us |  2.048 us |  0.56 |    0.02 |  0.4807 | 0.0038 |     - |   2.95 KB |
|               Grace |  1.751 us | 0.0339 us | 0.0377 us |  1.748 us |  0.47 |    0.02 |  0.5150 | 0.0076 |     - |   3.17 KB |
|   Grace_MsDIAdapter |  2.395 us | 0.0578 us | 0.0594 us |  2.402 us |  0.65 |    0.03 |  0.5569 |      - |     - |   3.41 KB |
|   Lamar_MsDIAdapter |  6.802 us | 0.0675 us | 0.0563 us |  6.800 us |  1.85 |    0.06 |  1.5335 | 0.7629 |     - |   9.44 KB |
|             Autofac | 50.699 us | 0.9995 us | 2.3947 us | 49.903 us | 14.13 |    0.81 |  7.7515 | 0.6104 |     - |  47.84 KB |
| Autofac_MsDIAdapter | 60.233 us | 1.1734 us | 1.2050 us | 60.089 us | 16.38 |    0.46 | 10.7422 | 0.8545 |     - |  66.26 KB |

```

<details>
<summary>Older versions for the comparison</summary>

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
