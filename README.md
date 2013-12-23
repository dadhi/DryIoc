DryIoc - fast, small and rich IoC Container.
--------------------------------------------
**Container with [Autofac]+[MEF] level of functionality BUT much faster and small enough to embed as code.**

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/

Implemented using [Expression Trees API](http://msdn.microsoft.com/en-us/library/bb397951.aspx).<br/>
Working with **.NET 3.5** and higher.<br/>
Available via nuget: [TODO]<br/>

### Fast ###
* On par with fastest containers listed in [IoC Container Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* General use-cases optimized for max speed.
* Memory footprint preserved as low as possible.
* Call stack depth preserved as shallow as possible. 

### Small ###
* Single source file is required: *Container.cs*. 
* No more than 2200 lines of code including comments.
* Code written to be readable.

### Safe ###
* Unit-tested with 100% coverage.
* Thread-safe: registrations and resolutions could be done in parallel whithout corrupting container state.
* Error handling is done with `ContainerException` inherited from `InvalidOperationException`, to filter container related exceptions.
* You could check on underlying expression used for service creation by resolving as `DebugFactory<T>`.

### Rich ###
* Supports constructor and property/field injection.
* Supports named service registration.
* Supports delegate/custom factory registration. 
* Supports arbitrary metadata accociated with service implementation.
* Supports open-generics without special syntax.
* Supports resolution of generic wrappers: `Lazy<T>`, `Func<T>`, `Func<..., T>`, `IEnumerable<T>` or `T[]`, `Meta<TMetadata, T>` plus user-defined wrappers. Wrappers could be freely nested, e.g.  `Meta<SomeMetadata, Func<SomeService>>[]`.
* Supports instance lifetime or *reuse* in DryIoc terms:
    * `Transient`, `Singleton`, `InCurrentScope`, `InResolutionScope`.
    * Custom reuse by implementing `IReuse` interface, check `ThreadReuse` in unit-tests for example.
    * Nested disposable scopes of reuse.
    * Disposing of `Singleton` on Container dispose. 
* Supports [Decorator Pattern](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Supports [Composite Pattern](http://en.wikipedia.org/wiki/Composite_pattern).
* Supports unregistered service resolution via `ResolutionRules`.
* Supports toggling features on/off via `ContanerSetup`
* Optionally supports [MEF attributed programming model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) with some exceptions (see *AttributedModel.cs* docs for details).
    * MEF attributes: `Export`, `InheritedExport`, `Import`, `PartCreationPolicy`, metadata export attributes and more.  
    * Additional DryIoc attributes: `ExportAsDecorator`, `ExportAsGenericWrapper`, `ExportWithMetadata`, `ExportOnce`, etc.
    * Custom factories to deal with foreign code via exporting `IFactory<T>`. 
    * Provides ability for compile-time types discovery. To speed-up application startup.