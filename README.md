DryIoc is fast, small and capable IoC Container
===============================================

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/
[DryIoc]: https://www.nuget.org/packages/DryIoc/
[DryIoc.MefAttributedModel]: https://www.nuget.org/packages/DryIoc.MefAttributedModel/
[DryIoc.dll]: https://www.nuget.org/packages/DryIoc.dll/
[DryIoc.MefAttributedModel.dll]: https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll/
[Wiki]: https://bitbucket.org/dadhi/dryioc/wiki/Home
[MefAttributedModel]: https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel
[PCL]: http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx

**Provides [Autofac]+[MEF] level of functionality but performs faster and small enough to be included as code.**

* Designed for low-ceremony use, performance and extensibility.
* Supports .NET 3.5, 4.0, 4.5 _([PCL] planned for v2)_
* Available at NuGet as [code][DryIoc] or [dll][DryIoc.dll].
* Extensions: [MefAttributedModel] as [code][DryIoc.MefAttributedModel] or [dll][DryIoc.MefAttributedModel.dll].
* [Docs are here][Wiki].

#### Fast
* On par with fastest containers listed in [IoC Container Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* General use-cases optimized for max speed.
* Callstack depth preserved as shallow as possible.
* Memory footprint preserved as small as possible.

#### Small
* Minimal setup requires single source file: *Container.cs*. 
* No more than 2500 lines of code including comments.
* Readable code.
* Uses [Caliburn.Micro](http://caliburnmicro.codeplex.com/) a-like approach for customization.

#### Reliable
* Unit-tested with 100% coverage.
* Thread-safe in sense that registrations and resolutions could be done in parallel without corrupting container state. 
* Recursive dependency detection (cycle in object graph).
* Error handling with `ContainerException` inherited from `InvalidOperationException` to filter container related exceptions.
* Throws exceptions as early as possible. 
* Meaningful error messages with all available information about problem Cause and Context.
* Resolve as `DebugExpression<TService>` to see underlying factory expression.

#### Features
* Constructor, property and field injection. *You can select What and Where to inject.*
* Delegate factory registration.
* Auto-registration via __MefAttributedModel__ extension (see below).
* Tools for custom auto-wiring and registation. Check `DryIoc.Samples.AutoWiring` for example.
* `IsRegistered` check.
* Open-generics without special syntax.
* Arbitrary metadata object accociated with implementation.
* Multiple named and unnamed implementations of single service.
* Multiple services of single impelementation.
* Resolution of multiple implementations as:
    * `IEnumerable<T>` or `T[]`. *Static view - next resolution woN't see new registrations.*
    * `Many<T>`. *Dynamic view - next resolution Will see new registrations.*
    *  [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern).
* Generic wrappers:
    * `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>`.
    * Func with parameters to specify constructor arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Registration of user-defined wrappers.
* Generic wrappers and multiple implementations could be nested, e.g. `Meta<SomeMetadata, Func<ISomeService>>[]`.
* Instance lifetime control (*instance reuse* in DryIoc terms):
    * `Transient`, `Singleton`, `InCurrentScope`, `InResolutionScope`.
    * Custom reuse by implementing `IReuse`. Check `DryIoc.UnitTests.ThreadReuse` for example.
    * Nested disposable scopes of reuse.
    * Disposing of `Singleton` on Container dispose.
* [Decorator Pattern](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based implementation selection.
* Unregistered service resolution via `ResolutionRules`.
* Toggling features On/Off via `ContanerSetup`.

#### Extensions
* [MefAttributedModel] - emulates [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) and enables automatic types discovery and wiring.