DryIoc is small, fast, capable IoC Container for .NET
=====================================================

[![Build status](https://ci.appveyor.com/api/projects/status/jfq01d9wcs4vcwpf/branch/default)](https://ci.appveyor.com/project/MaksimVolkau/dryioc/branch/default) |
[Follow @DryIoc](https://twitter.com/DryIoc) |
[StackOverflow Q'n'A](http://stackoverflow.com/questions/tagged/dryioc)

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
* Supports .NET PCL, 3.5, 4.0, 4.5.
* Available at NuGet as [code][DryIoc] or [dll][DryIoc.dll].
* [Wiki documentation][WikiHome] _(being updated for v2.0)_
* __v1.4.1__ is stable: `PM> Install-Package DryIoc` 
* __[v2.0]__ is coming: `PM> Install-Package DryIoc -Pre`

#### Benchmarks
* [Performance](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison)
* [Features](http://featuretests.apphb.com/DependencyInjection.html)

#### Performance
* General use-cases optimized for max speed.
* Call-stack depth preserved as shallow as possible.
* Memory footprint preserved as minimal as possible.

#### Code/Library
* Minimal setup requires single source file: *Container.cs*. 
* Code written to be readable.
* Public API is fully documented.

#### Reliability
* Unit-tested with 100% coverage.
* Thread-safe and lock-free: registrations and resolutions could be made in parallel without corrupting container state. 
* Detects recursive dependencies - cycles in object graph.
* Throws exceptions as early as possible. Exception provides meaningful information about problem and context.
* Resolve as `FactoryExpression<T>` to look into underlying service creation expression.

#### Features

* Register interface/type mapping, additionally supported: registering service once, registration update, unregister. 
* Register user-defined delegate factory and register existing instance.
* Register from assembly(ies) implementation types with automatically determined service types.
* Register with arbitrary key and condition, multiple default registrations.
* Resolve and ResolveMany. Unknown service resolution with `Rules.WithUnknownServiceResolvers()`. 
* Instance lifetime control or *Reuse* in DryIoc terms ([wiki](https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes)) :
    * Nested disposable scopes, ambient scope context.
    * Supported out-of-the-box: `Singleton`, `InResolutionScope`, `InCurrentScope`, `InCurrentNamedScope`, or define your own.
    * Changing default reuse type per container via  `Rules.WithReuseMapping()`.
    * Control over reused objects behavior via ReuseWrappers: `HiddenDisposable`, `WeakReference`, `Swapable`, `Recyclable`, and user-defined.
* Open-generics without special syntax.
* Constructor, property and field injection.
* Static or instance Factory Methods in addition to constructor. Parameter injection is supported the same way as for constructor.
* Injecting properties/fields into existing object.
* Creating concrete object without registering it in Container but with injecting its parameters, properties, and fields.
* Metadata associating with registration.
* Generic wrappers:
    * Service collections: `T[]`, `IEnumerable<T>`, `LazyEnumerable<T>`, and as `I(ReadOnly)Collection|List`.
    * Other: `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>`, `KeyValuePair<TKey, T>`, and user-defined wrappers.
    * [Currying](http://en.wikipedia.org/wiki/Currying) over constructor (or factory method) arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Nested wrappers: e.g. `Meta<SomeMetadata, Func<ISomeService>>[]`.
* [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern): Composite itself is excluded from result collection.
* [Decorator Pattern](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based service selection.

#### Extensions
* [MefAttributedModel] for [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx). Enables automatic types discovery and wiring.
* [CommonServiceLocator](https://commonservicelocator.codeplex.com/)
* ASP.NET: Web Forms, MVC, Web API, OWIN.