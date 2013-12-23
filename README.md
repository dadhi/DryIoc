DryIoc - fast, small and rich IoC Container.
--------------------------------------------
**Container with [Autofac]+[MEF] level of functionality BUT much faster and small enough to embed as code.**

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/
* Implemented using [Expression Trees API](http://msdn.microsoft.com/en-us/library/bb397951.aspx).
* Working with **.NET 3.5** and higher.
* Available via nuget: [TODO]

### Fast ###
* On par with fastest containers listed in [IoC Container Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* General use-cases optimized for max speed.
* Memory footprint preserved as low as possible.
* Callstack depth preserved as shallow as possible. 

### Small ###
* Single source file is required: *Container.cs*. 
* No more than 2200 lines of code including comments.
* Code written to be readable.

### Safe ###
* Unit-tested with 100% coverage.
* Thread-safe: registrations and resolutions could be done in parallel whithout corrupting container state.
* Error handling is done with `ContainerException` inherited from `InvalidOperationException` to filter container related exceptions.
* You could check on underlying expression used for service creation by resolving as `DebugFactory<T>`.

### Supports ###
* Constructor, property and field injection.
* Delegate factory registration.
* Open-generics without special syntax.
* Multiple unnamed/named services with single/many implementations.
* Arbitrary metadata object accociated with service implementation.
* Resolution of generic wrappers:
	* `Lazy<T>`, `Func<T>`, `IEnumerable<T>`, `T[]`, `Meta<TMetadata, T>`.
	* Func with parameters to specify constructor arguments: `Func<A, T>`, `Func<A1, A2, T>`, etc.
	* `Many<T>` for dynamic resolution of available services.
	* User-defined wrappers.
	* Wrappers could be freely nested, e.g. `IEnumerable<Meta<SomeMetadata, Func<int, SomeService>>>`.
* Instance lifetime control (*instance reuse* in DryIoc terms):
    * `Transient`, `Singleton`, `InCurrentScope`, `InResolutionScope`.
    * Custom reuse by implementing `IReuse` interface, check `ThreadReuse` in unit-tests for example.
    * Nested disposable scopes of reuse.
    * Disposing of `Singleton` on Container dispose. 
* [Decorator Pattern](http://en.wikipedia.org/wiki/Decorator_pattern). 
* [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern).
* Unregistered service resolution via `ResolutionRules`.
* Toggling features on/off via `ContanerSetup`.
* Optionally [MEF attributed programming model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) with some exceptions (see *AttributedModel.cs* docs for details).
    * MEF attributes: `Export`, `InheritedExport`, `Import`, `PartCreationPolicy`, metadata export attributes and more.  
    * Additional DryIoc attributes: `ExportAsDecorator`, `ExportAsGenericWrapper`, `ExportWithMetadata`, `ExportOnce`, etc.
    * Custom factories to deal with foreign code via exporting `IFactory<T>`. 
    * Provides ability for compile-time types discovery. To speed-up application startup.