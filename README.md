DryIoc is fast, small, full-featured IoC Container for .NET
===========================================================

<img src="./logo/logo.svg" alt="logo" width="100px"/>

[![CI](https://github.com/dadhi/DryIoc/actions/workflows/ci.yml/badge.svg)](https://github.com/dadhi/DryIoc/actions/workflows/ci.yml)
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
- Targets: net472;netstandard2.1;net8.0;net10.0;net11.0
- NuGet packages:

    - __DryIoc.dll__ [![NuGet Version](https://img.shields.io/nuget/v/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)![NuGet Downloads](https://img.shields.io/nuget/dt/DryIoc.dll)
      — includes **Compile-time DI**: on first `dotnet build` scaffolds T4 templates; run `dotnet t4` to emit a reflection-free, AOT-friendly `Container.Generated.cs`. See [`samples/MinimalWeb`](samples/MinimalWeb) for a working ASP.NET Minimal API example.

    - __DryIoc__ (source code) [![NuGet Version](https://img.shields.io/nuget/v/DryIoc)](https://www.nuget.org/packages/DryIoc)![NuGet Downloads](https://img.shields.io/nuget/dt/DryIoc)

    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Version](https://img.shields.io/nuget/v/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)![NuGet Downloads](https://img.shields.io/nuget/dt/DryIoc.Internal)

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v6.0.0) :: [Previous Versions](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/VersionHistory.md)
- [Pre-release `.nupkg` artifacts](https://github.com/dadhi/DryIoc/actions/workflows/ci.yml) — download from the latest CI run (Windows job -> Artifacts -> `packages`)
- [Extensions and Companions](Extensions.md)
- [Live Documentation][WikiHome] is created with [CsToMd](https://github.com/dadhi/CsToMd)
- [Contribution guide](CONTRIBUTING.md)
- Two original parts of the DryIoc are now the separate projects: [FastExpressionCompiler](https://github.com/dadhi/FastExpressionCompiler) and [ImTools](https://github.com/dadhi/ImTools)

## Benchmarks

### [Performance overview](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)

### Realistic scenario with the unit-of-work scope and object graph of 40 dependencies 4 levels deep

More details in [#44](https://github.com/dadhi/DryIoc/issues/44#issuecomment-466440634) and [#26](https://github.com/dadhi/DryIoc/issues/26#issuecomment-466460255).

The listed *.MsDI* packages are respective [Microsoft.Extensions.DependencyInjection adapters](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1#default-service-container-replacement)

#### Cold start - Registering services then opening the scope and resolving the root scoped service (e.g. controller) for the first time

.NET 11. DryIoc 6.0.0 (.MsDI 8.0.0), MsDI 11.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 9.3.4 (.MsDI 11.0.2), Lamar 16.0.0

```md
BenchmarkDotNet v0.16.0-preview.2, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Memory: 31.72 GB Total, 6.29 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3

| Method       |         Mean |      Error |     StdDev |  Ratio | RatioSD |     Gen0 |    Gen1 |    Gen2 | Allocated | Alloc Ratio |
| ------------ | -----------: | ---------: | ---------: | -----: | ------: | -------: | ------: | ------: | --------: | ----------: |
| DryIoc       |     68.53 us |   1.350 us |   2.727 us |   1.00 |    0.00 |   5.2490 |  0.3662 |       - |  32.82 KB |        1.00 |
| DryIoc_MsDI  |    100.36 us |   2.005 us |   3.404 us |   1.47 |    0.07 |   6.5918 |  0.6104 |       - |  40.96 KB |        1.25 |
| MsDI         |     89.59 us |   1.769 us |   3.956 us |   1.31 |    0.08 |  11.8408 |  0.1221 |       - |  72.95 KB |        2.22 |
| Autofac      |    362.56 us |   7.199 us |  14.042 us |   5.30 |    0.29 |  55.6641 |       - |       - | 343.93 KB |       10.48 |
| Autofac_MsDI |    385.59 us |   7.629 us |  11.877 us |   5.64 |    0.28 |  61.5234 |       - |       - | 378.63 KB |       11.54 |
| Lamar_MsDI   |  4,639.23 us |  74.310 us |  62.052 us |  67.80 |    2.76 |  85.9375 | 31.2500 |       - | 537.45 KB |       16.38 |
| Grace        | 15,895.19 us | 310.170 us | 259.006 us | 232.30 |    9.69 | 109.3750 | 93.7500 | 15.6250 | 682.89 KB |       20.81 |
| Grace_MsDI   | 18,288.04 us | 201.841 us | 178.927 us | 267.27 |   10.63 | 125.0000 | 93.7500 |       - | 848.74 KB |       25.86 |

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

.NET 11. DryIoc 6.0.0 (.MsDI 8.0.0), MsDI 11.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 9.3.4 (.MsDI 11.0.2), Lamar 16.0.0

```md
BenchmarkDotNet v0.16.0-preview.2, Windows 11 (10.0.26200.9711/25H2/2025Update/HudsonValley2)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Memory: 31.72 GB Total, 6.08 GB Available
.NET SDK 11.0.100
  [Host]     : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v3

| Method       |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
| ------------ | --------: | --------: | --------: | ----: | ------: | -----: | -----: | --------: | ----------: |
| DryIoc       |  1.607 us | 0.0319 us | 0.0733 us |  1.00 |    0.00 | 0.4730 | 0.0038 |   2.91 KB |        1.00 |
| DryIoc_MsDI  |  2.450 us | 0.0483 us | 0.0931 us |  1.53 |    0.09 | 0.5074 | 0.0038 |   3.13 KB |        1.08 |
| MsDI         |  3.821 us | 0.0744 us | 0.1180 us |  2.38 |    0.13 | 0.7858 | 0.0076 |   4.85 KB |        1.67 |
| Grace        |  1.510 us | 0.0286 us | 0.0663 us |  0.94 |    0.06 | 0.5169 | 0.0038 |   3.17 KB |        1.09 |
| Grace_MsDI   |  1.856 us | 0.0364 us | 0.0545 us |  1.16 |    0.06 | 0.5493 | 0.0038 |   3.37 KB |        1.16 |
| Lamar_MsDI   |  5.681 us | 0.1097 us | 0.1078 us |  3.54 |    0.17 | 0.9384 | 0.9308 |   5.78 KB |        1.99 |
| Autofac      | 40.734 us | 0.8131 us | 0.8700 us | 25.40 |    1.24 | 6.8970 | 0.4272 |  42.27 KB |       14.55 |
| Autofac_MsDI | 47.894 us | 0.9532 us | 1.4841 us | 29.86 |    1.60 | 9.6436 | 0.6104 |  59.12 KB |       20.34 |
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
