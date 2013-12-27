DryIoc is fast, small and rich IoC Container.
--------------------------------------------
**Container with [Autofac]+[MEF] level of functionality BUT much faster and small enough to embed as code.**

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/

Implemented using [Expression Trees API](http://msdn.microsoft.com/en-us/library/bb397951.aspx).  
Working on .NET 3.5, 4.0, 4.5.  
Available via **NuGet** as code [DryIoc], [DryIoc.MefAttributedModel] or as assembly [DryIoc.dll], [DryIoc.MefAttributedModel.dll].

[DryIoc]: https://www.nuget.org/packages/DryIoc/
[DryIoc.MefAttributedModel]: https://www.nuget.org/packages/DryIoc.MefAttributedModel/
[DryIoc.dll]: https://www.nuget.org/packages/DryIoc.dll/
[DryIoc.MefAttributedModel.dll]: https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll/

#### Fast ####
* On par with fastest containers listed in [IoC Container Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* General use-cases optimized for max speed.
* Memory footprint preserved as low as possible.
* Callstack preserved as shallow as possible. 

#### Small ####
* Single source file is required: *Container.cs*. 
* No more than 2200 lines of code including comments.
* Code written to be readable.

#### Reliable ####
* Unit-tested with 100% coverage.
* Thread-safe: registrations and resolutions could be done in parallel without corrupting container state. 
* Recursive dependency detection (cycle in object graph).
* Error handling with `ContainerException` inherited from `InvalidOperationException` to filter container related exceptions.
* Meaningful error messages with all available information about problem Cause and Location.
* Resolving as `DebugExpression<T>` to find underlying expression used for instance creation.

#### Supports ####
* Constructor, property and field injection. You can select What and Where to inject.
* Delegate factory registration.
* `IsRegistered` check.
* Open-generics without special syntax.
* Arbitrary metadata object accociated with implementation.
* Multiple named and unnamed implementations of single service.
* Multiple services of single impelementation.
* Resolution of multiple implementations as:
    * `IEnumerable<T>` or `T[]`. Static view - next resolution woN't see new registrations.
    * `Many<T>`. Dynamic view - next resolution Will see new registrations.
    *  [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern). Composite implementation will be exlcuded from itself.
* Generic wrappers:
    * `Lazy<T>`, `Func<T>`, `Meta<TMetadata, T>`.
    * Func with parameters to specify constructor arguments: `Func<TArg, T>`, `Func<TArg1, TArg2, T>`, etc.
    * Registration of user-defined wrappers.
* Generic wrappers and multiple implementations could be nested, e.g. `Meta<SomeMetadata, Func<ISomeService>>[]`.
* Instance lifetime control (*instance reuse* in DryIoc terms):
    * `Transient`, `Singleton`, `InCurrentScope`, `InResolutionScope`.
    * Custom reuse by implementing `IReuse`. Check `ThreadReuse` in unit-tests for example.
    * Nested disposable scopes of reuse.
    * Disposing of `Singleton` on Container dispose.
* [Decorators](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based implementation selection.
* Unregistered service resolution via `ResolutionRules`.
* Toggling features On/Off via `ContanerSetup`.

#### Optionally supports [MEF attributed programming model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx).
* Standard attributes with some exceptions (check docs in *AttributedModel.cs*): `Export`, `InheritedExport`, `Import`, `PartCreationPolicy`, etc.
* Additional DryIoc attributes: `ExportAll`, `ExportAsDecorator`, `ExportAsGenericWrapper`, `ExportWithMetadata`, `ImportWithMetadata`.
* Dealing with foreign code via implementing `IFactory<T>` or use of `ExportOnce` attribute.
* Provides ability for compile-time types discovery. To speed-up application startup.