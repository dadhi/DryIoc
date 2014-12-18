DryIoc is small, fast, capable IoC Container for .NET
=====================================================

[![Build status](https://ci.appveyor.com/api/projects/status/jfq01d9wcs4vcwpf/branch/default)](https://ci.appveyor.com/project/MaksimVolkau/dryioc/branch/default)
[Follow @DryIoc](https://twitter.com/DryIoc)


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

* Designed for low-ceremony use, performance and extensibility.
* Supports .NET 3.5, 4.0, 4.5, _([PCL] in v2.0)_.
* Available at NuGet as [code][DryIoc] or [dll][DryIoc.dll].
* Extensions: [MefAttributedModel] as [code][DryIoc.MefAttributedModel] or [dll][DryIoc.MefAttributedModel.dll].
* [Wiki documentation][WikiHome] _(being updated for v2.0)_
* __v1.4.1__ is stable: `PM> Install-Package DryIoc` 
* __[v2.0]__ is coming soon: `PM> Install-Package DryIoc -Pre`

#### Benchmarks
* Very fast in [Performance Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* Supports a lot in [Features Benchmark](http://featuretests.apphb.com/DependencyInjection.html) _(v2.0)_.

#### Performance
* General use-cases optimized for max speed.
* Call-stack depth preserved as shallow as possible.
* Memory footprint preserved as low as possible.

#### Code/Library
* Minimal setup requires single source file: *Container.cs*. 
* Code written to be readable.
* Public API fully documented _(v2.0)_.

#### Reliability
* Unit-tested with 100% coverage.
* Thread-safe (lock-free in _v2.0_): registrations and resolutions could be made in parallel without corrupting container state. 
* Recursive dependency detection (cycle in object graph).
* Error handling with `ContainerException` inherited from `InvalidOperationException` to filter container related exceptions.
* Throws exceptions as early as possible. 
* Meaningful error messages with all available information about problem Cause and Context.
* Resolving as `DebugExpression<T>` to find underlying expression used for instance creation.

#### Features
* Instance lifetime control or *Reuse* in DryIoc terms ([wiki](https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes)) :
    * Nested disposable scopes and ambient scope context.
    * Supported out-of-the-box: `Singleton`, `InResolutionScope`, `InCurrentScope`, `InCurrentNamedScope`, or define your own.
    * Changing default reuse type per container with  `Rules.ReuseMapping`.
    * Control over storing of reused objects with Wrappers: `WeakReference`, `Disposable`, and more.
* Constructor, property and field injection. *You can select What and Where to inject.*
* Delegate factory registration.
* Auto-registration via __MefAttributedModel__ extension (see below).
* Tools for custom auto-wiring and registration. Check `DryIoc.Samples.AutoWiring` for example.
* `IsRegistered` check.
* Open-generics without special syntax.
* Arbitrary metadata object associated with implementation.
* Multiple named and unnamed implementations of single service.
* Multiple services of single implementation.
* Resolution of multiple implementations as:
    * `IEnumerable<T>` or `T[]`. *Static view - next resolution woN't see new registrations.*
    * `Many<T>`. *Dynamic view - next resolution Will see new registrations.*
    *  [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern). Composite implementation will be exlcuded from itself.
* Generic wrappers:
    * `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>`.
    * Func with parameters to specify constructor arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Registration of user-defined wrappers.
* Generic wrappers and multiple implementations could be nested, e.g. `Meta<SomeMetadata, Func<ISomeService>>[]`.
* [Decorators](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based implementation selection.
* Unregistered service resolution via `ResolutionRules`.
* Toggling features On/Off via `ContanerSetup`.

#### Extensions
* [MefAttributedModel] - emulates [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) and enables automatic types discovery and wiring.