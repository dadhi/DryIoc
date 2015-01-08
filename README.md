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
* In the top of [Performance Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* In the top of [Features Benchmark](http://featuretests.apphb.com/DependencyInjection.html) _(v2.0)_.

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
* Resolving as `FactoryExpression<T>` wrapper to look at underlying service creation expression.

#### Features

* Register interface/type mapping, additionally supported registering once, registration update, unregister, is registered check. 
* Register user-defined delegate factory and register existing instance.
* Register from assembly(ies) to automatically locate implementation types.
* Register with arbitrary name or key, multiple default registrations.
* Resolve and additionally: Try Resolve, ResolveMany.
* Instance lifetime control or *Reuse* in DryIoc terms ([wiki](https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes)) :
    * Nested disposable scopes and ambient scope context.
    * Supported out-of-the-box: `Singleton`, `InResolutionScope`, `InCurrentScope`, `InCurrentNamedScope`, or define your own.
    * Changing default reuse type per container via  `Rules.WithReuseMapping()`.
    * Control over reused objects behavior via ReuseWrappers: `HiddenDisposable`, `WeakReference`, `Swapable`, `Recyclable`, and user-defined.
* Open-generics without special syntax.
* Constructor, property and field injection.
* Static or instance Factory Methods in addition to constructor. Parameter injection is supported the same way as for constructor.
* Injecting properties/fields into existing object.
* Creating concrete object without registering it in Container but with injecting its parameters, properties, and fields.
* Associated metadata object.
* Generic wrappers:
    * Of multiple implementations: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and as `I(ReadOnly)Collection|List`.
    * `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>`, `KeyValuePair<TKey, T>`, and user-defined wrappers.
    * [Currying](http://en.wikipedia.org/wiki/Currying) over constructor of factory method arguments with: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Generic wrappers could be nested, e.g. `Meta<SomeMetadata, Func<ISomeService>>[]`.
* [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern): Composite itself is excluded from result collection.
* [Decorator Pattern](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based implementation selection.
* Unknown service resolution via `Rules.WithUnknownServiceResolvers()`.

#### Extensions
* [MefAttributedModel] - supports [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) and enables automatic types discovery and wiring.
* [CommonServiceLocator](https://commonservicelocator.codeplex.com/)
* _Asp.NET support is on the way with V2 ..._