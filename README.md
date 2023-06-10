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
- Supports: .NET 4.5+, [.NET Standard 2.0+](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md)
- NuGet packages:
 
    - __DryIoc.dll__ [![NuGet Badge](https://buildstats.info/nuget/DryIoc.dll)](https://www.nuget.org/packages/DryIoc.dll)
    - __DryIoc__ (source code) [![NuGet Badge](https://buildstats.info/nuget/DryIoc)](https://www.nuget.org/packages/DryIoc)
    - __DryIoc.Internal__ (source code with public types made internal) [![NuGet Badge](https://buildstats.info/nuget/DryIoc.Internal)](https://www.nuget.org/packages/DryIoc.Internal)

- [Release Notes](https://github.com/dadhi/DryIoc/releases/tag/v5.4.1) :: [Previous Versions](https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/VersionHistory.md)
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

DryIoc 5.0.0 (.MsDI 6.0.0), MsDI 6.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 6.3.0 (.MsDI 7.2.0), Lamar 8.0.1

```md
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT

|       Method |         Mean |      Error |     StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|------------- |-------------:|-----------:|-----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|       DryIoc |     82.22 us |   1.209 us |   1.072 us |   1.00 |    0.00 |   6.3477 |  0.3662 |      - |  39.42 KB |
|  DryIoc_MsDI |     94.18 us |   1.207 us |   1.070 us |   1.15 |    0.02 |   8.0566 |  0.6104 |      - |  49.87 KB |
|         MsDI |     94.60 us |   0.715 us |   0.597 us |   1.15 |    0.01 |  11.8408 |  4.2725 |      - |  72.59 KB |
|      Autofac |    543.45 us |   4.570 us |   3.568 us |   6.60 |    0.10 |  51.7578 | 25.3906 | 1.9531 | 317.19 KB |
| Autofac_MsDI |    534.64 us |   5.919 us |   5.247 us |   6.50 |    0.10 |  54.6875 | 27.3438 | 1.9531 | 340.17 KB |
|   Lamar_MsDI |  7,053.46 us | 140.273 us | 402.469 us |  77.97 |    2.84 |        - |       - |      - | 649.68 KB |
|        Grace | 15,990.58 us | 123.798 us | 109.744 us | 194.52 |    2.21 |  93.7500 | 31.2500 |      - | 736.12 KB |
|   Grace_MsDI | 18,884.30 us | 321.388 us | 268.373 us | 229.50 |    4.25 | 125.0000 | 62.5000 |      - |  904.7 KB |
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
|       DryIoc |    129.6 us |   1.90 us |   1.68 us |   0.86 |    0.02 |  16.3574 |  0.2441 |      - |  67.52 KB |
|  DryIoc_MsDI |    161.9 us |   1.74 us |   1.63 us |   1.07 |    0.03 |  21.4844 |  0.2441 |      - |   88.6 KB |
|         MsDI |    150.8 us |   2.83 us |   3.03 us |   1.00 |    0.00 |  18.0664 |  0.2441 |      - |  73.86 KB |
|      Autofac |    789.4 us |  19.84 us |  20.38 us |   5.24 |    0.18 |  50.7813 | 25.3906 | 1.9531 | 311.12 KB |
| Autofac_MsDI |    784.9 us |  15.04 us |  18.47 us |   5.20 |    0.15 |  54.6875 | 27.3438 | 1.9531 | 335.07 KB |
|   Lamar_MsDI | 10,938.2 us | 308.25 us | 874.46 us |  70.86 |    4.29 |        - |       - |      - | 696.16 KB |
|        Grace | 21,380.9 us | 375.46 us | 351.21 us | 141.65 |    2.83 | 156.2500 | 62.5000 |      - | 729.12 KB |
|   Grace_MsDI | 24,102.4 us | 243.21 us | 203.09 us | 159.26 |    3.52 | 187.5000 | 93.7500 |      - | 894.57 KB |
```
</details>


#### Hot run - Opening the scope and resolving the root scoped service for the Nth time

DryIoc 5.0.0 (.MsDI 6.0.0), MsDI 6.0.0, Grace 7.2.1 (.MsDI 7.1.0), Autofac 6.3.0 (.MsDI 7.2.0), Lamar 8.0.1

```md
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT

|       Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|       DryIoc |  1.535 us | 0.0143 us | 0.0111 us |  1.00 |    0.00 |  0.4749 | 0.0076 |     - |   2.91 KB |
|  DryIoc_MsDI |  2.405 us | 0.0277 us | 0.0246 us |  1.57 |    0.02 |  0.4807 | 0.0076 |     - |   2.96 KB |
|         MsDI |  3.655 us | 0.0726 us | 0.0807 us |  2.40 |    0.05 |  0.7629 | 0.0114 |     - |   4.68 KB |
|        Grace |  1.807 us | 0.0241 us | 0.0213 us |  1.18 |    0.02 |  0.5169 | 0.0076 |     - |   3.17 KB |
|   Grace_MsDI |  2.576 us | 0.0421 us | 0.0394 us |  1.68 |    0.03 |  0.5569 | 0.0076 |     - |   3.41 KB |
|   Lamar_MsDI |  6.673 us | 0.0876 us | 0.0732 us |  4.35 |    0.06 |  0.9995 | 0.4959 |     - |   6.16 KB |
|      Autofac | 47.040 us | 0.7367 us | 0.6531 us | 30.65 |    0.48 |  7.7515 | 0.6104 |     - |  47.73 KB |
| Autofac_MsDI | 59.566 us | 0.8734 us | 0.7742 us | 38.76 |    0.61 | 11.3525 | 0.9155 |     - |  69.59 KB |
```

<details>
<summary>Older versions for the comparison</summary>

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

</details>


## Reliability

* ~2300 tests.
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
