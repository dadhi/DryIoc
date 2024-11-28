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
- Targets: net45;netstandard2.0;netstandard2.1;net6.0;net7.0;net8.0
- NuGet packages:

    - __DryIoc.dll__ [![NuGet Version](https://img.shields.io/nuget/v/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)![NuGet Downloads](https://img.shields.io/nuget/dt/DryIoc.dll)

    - __DryIoc__ (source code) [![NuGet Version](https://img.shields.io/nuget/v/DryIoc)](https://www.nuget.org/packages/DryIoc)![NuGet Downloads](https://img.shields.io/nuget/dt/DryIoc)

    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Version](https://img.shields.io/nuget/v/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)![NuGet Downloads](https://img.shields.io/nuget/dt/DryIoc.Internal)

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v5.4.3) :: [Previous Versions](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/VersionHistory.md)
- [Extensions and Companions](Extensions.md)
- [Live Documentation][WikiHome] created with [CsToMd](https://github.com/dadhi/CsToMd)
- [Contribution guide](CONTRIBUTING.md)
- Two original parts of the DryIoc are now the separate projects: [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) and [ImTools](https://github.com/dadhi/ImTools)

## Benchmarks

### [Performance overview](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)

### Realistic scenario with the unit-of-work scope and object graph of 40 dependencies 4 levels deep

More details in [#44](https://github.com/dadhi/DryIoc/issues/44#issuecomment-466440634) and [#26](https://github.com/dadhi/DryIoc/issues/26#issuecomment-466460255).

The listed *.MsDI* packages are respective [Microsoft.Extensions.DependencyInjection adapters](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#default-service-container-replacement)

#### Cold start - Registering services then opening the scope and resolving the root scoped service (e.g. controller) for the first time

DryIoc 6.0.0 (.MsDI 8.0.0), MsDI 9.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 8.1.1 (.MsDI 10.0.0), Lamar 14.0.1

```md
BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4391/23H2/2023Update/SunValley3)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

| Method       |         Mean |      Error |     StdDev |       Median |  Ratio | RatioSD |     Gen0 |    Gen1 |    Gen2 | Allocated | Alloc Ratio |
| ------------ | -----------: | ---------: | ---------: | -----------: | -----: | ------: | -------: | ------: | ------: | --------: | ----------: |
| DryIoc       |     65.70 us |   1.309 us |   2.553 us |     64.46 us |   1.00 |    0.05 |   5.2490 |  0.4883 |       - |  32.74 KB |        1.00 |
| DryIoc_MsDI  |     97.96 us |   1.959 us |   4.382 us |     96.59 us |   1.49 |    0.09 |   6.5918 |  0.6104 |       - |  40.89 KB |        1.25 |
| MsDI         |     81.25 us |   1.624 us |   4.686 us |     82.73 us |   1.24 |    0.08 |  14.8926 |       - |       - |  91.15 KB |        2.78 |
| Autofac      |    323.50 us |   6.408 us |   8.555 us |    320.06 us |   4.93 |    0.23 |  49.8047 |       - |       - | 306.93 KB |        9.37 |
| Autofac_MsDI |    367.96 us |   7.324 us |  14.111 us |    362.58 us |   5.61 |    0.30 |  59.0820 |       - |       - | 364.77 KB |       11.14 |
| Lamar_MsDI   |  3,643.30 us |  56.678 us |  55.666 us |  3,630.33 us |  55.53 |    2.25 |  82.0313 |  3.9063 |       - | 524.96 KB |       16.03 |
| Grace        | 13,870.26 us | 282.593 us | 824.337 us | 13,837.66 us | 211.41 |   14.82 | 109.3750 | 93.7500 | 15.6250 | 686.94 KB |       20.98 |
| Grace_MsDI   | 17,079.41 us | 318.034 us | 326.598 us | 17,025.77 us | 260.33 |   10.92 | 125.0000 | 93.7500 |       - | 854.11 KB |       26.09 |

```

<details>
  <summary>Older versions for the comparison</summary>

DryIoc 5.0.0 (.MsDI 6.0.0), MsDI 6.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 6.3.0 (.MsDI 7.2.0), Lamar 8.0.1

```md
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT

| Method       |         Mean |      Error |     StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
| ------------ | -----------: | ---------: | ---------: | -----: | ------: | -------: | ------: | -----: | --------: |
| DryIoc       |     82.22 us |   1.209 us |   1.072 us |   1.00 |    0.00 |   6.3477 |  0.3662 |      - |  39.42 KB |
| DryIoc_MsDI  |     94.18 us |   1.207 us |   1.070 us |   1.15 |    0.02 |   8.0566 |  0.6104 |      - |  49.87 KB |
| MsDI         |     94.60 us |   0.715 us |   0.597 us |   1.15 |    0.01 |  11.8408 |  4.2725 |      - |  72.59 KB |
| Autofac      |    543.45 us |   4.570 us |   3.568 us |   6.60 |    0.10 |  51.7578 | 25.3906 | 1.9531 | 317.19 KB |
| Autofac_MsDI |    534.64 us |   5.919 us |   5.247 us |   6.50 |    0.10 |  54.6875 | 27.3438 | 1.9531 | 340.17 KB |
| Lamar_MsDI   |  7,053.46 us | 140.273 us | 402.469 us |  77.97 |    2.84 |        - |       - |      - | 649.68 KB |
| Grace        | 15,990.58 us | 123.798 us | 109.744 us | 194.52 |    2.21 |  93.7500 | 31.2500 |      - | 736.12 KB |
| Grace_MsDI   | 18,884.30 us | 321.388 us | 268.373 us | 229.50 |    4.25 | 125.0000 | 62.5000 |      - |  904.7 KB |
```
</details>


#### Hot run - Opening the scope and resolving the root scoped service for the Nth time

DryIoc 6.0.0 (.MsDI 8.0.0), MsDI 9.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 8.1.1 (.MsDI 10.0.0), Lamar 14.0.1

```md
BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4391/23H2/2023Update/SunValley3)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

| Method       |      Mean |     Error |    StdDev | Ratio | RatioSD |    Gen0 |   Gen1 | Allocated | Alloc Ratio |
| ------------ | --------: | --------: | --------: | ----: | ------: | ------: | -----: | --------: | ----------: |
| DryIoc       |  1.357 us | 0.0105 us | 0.0093 us |  1.00 |    0.01 |  0.4730 | 0.0038 |   2.91 KB |        1.00 |
| DryIoc_MsDI  |  1.815 us | 0.0126 us | 0.0118 us |  1.34 |    0.01 |  0.5074 | 0.0038 |   3.11 KB |        1.07 |
| MsDI         |  2.800 us | 0.0146 us | 0.0114 us |  2.06 |    0.02 |  0.7896 | 0.0114 |   4.85 KB |        1.67 |
| Grace        |  1.535 us | 0.0125 us | 0.0117 us |  1.13 |    0.01 |  0.5169 | 0.0038 |   3.17 KB |        1.09 |
| Grace_MsDI   |  1.709 us | 0.0249 us | 0.0256 us |  1.26 |    0.02 |  0.5493 | 0.0038 |   3.37 KB |        1.16 |
| Lamar_MsDI   |  5.314 us | 0.0197 us | 0.0184 us |  3.92 |    0.03 |  0.9689 | 0.9613 |   5.95 KB |        2.05 |
| Autofac      | 36.715 us | 0.2231 us | 0.1863 us | 27.06 |    0.22 |  7.2021 | 0.4883 |  44.49 KB |       15.31 |
| Autofac_MsDI | 48.139 us | 0.6987 us | 0.6194 us | 35.48 |    0.50 | 10.1318 | 0.6714 |  62.27 KB |       21.42 |
```

<details>
<summary>Older versions for the comparison</summary>

DryIoc 5.0.0 (.MsDI 6.0.0), MsDI 6.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 6.3.0 (.MsDI 7.2.0), Lamar 8.0.1

```md
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT

| Method       |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
| ------------ | --------: | --------: | --------: | ----: | ------: | ------: | -----: | ----: | --------: |
| DryIoc       |  1.535 us | 0.0143 us | 0.0111 us |  1.00 |    0.00 |  0.4749 | 0.0076 |     - |   2.91 KB |
| DryIoc_MsDI  |  2.405 us | 0.0277 us | 0.0246 us |  1.57 |    0.02 |  0.4807 | 0.0076 |     - |   2.96 KB |
| MsDI         |  3.655 us | 0.0726 us | 0.0807 us |  2.40 |    0.05 |  0.7629 | 0.0114 |     - |   4.68 KB |
| Grace        |  1.807 us | 0.0241 us | 0.0213 us |  1.18 |    0.02 |  0.5169 | 0.0076 |     - |   3.17 KB |
| Grace_MsDI   |  2.576 us | 0.0421 us | 0.0394 us |  1.68 |    0.03 |  0.5569 | 0.0076 |     - |   3.41 KB |
| Lamar_MsDI   |  6.673 us | 0.0876 us | 0.0732 us |  4.35 |    0.06 |  0.9995 | 0.4959 |     - |   6.16 KB |
| Autofac      | 47.040 us | 0.7367 us | 0.6531 us | 30.65 |    0.48 |  7.7515 | 0.6104 |     - |  47.73 KB |
| Autofac_MsDI | 59.566 us | 0.8734 us | 0.7742 us | 38.76 |    0.61 | 11.3525 | 0.9155 |     - |  69.59 KB |
```
</details>


## Reliability

* More than 2000 of test cases covered.
* Thread-safe and lock-free registrations and resolutions. 
* Detects recursive dependencies (cycles) in object graph.
* Throws exceptions as early as possible with a lot of details.
* Provides diagnostics for potential resolution problems via `container.Validate()`.


## Features

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
