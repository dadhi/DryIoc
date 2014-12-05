DryIoc is small, fast, capable IoC Container for .NET
=====================================================

[Autofac]: https://code.google.com/p/autofac/
[MEF]: http://mef.codeplex.com/
[DryIoc]: https://www.nuget.org/packages/DryIoc/
[DryIoc.MefAttributedModel]: https://www.nuget.org/packages/DryIoc.MefAttributedModel/
[DryIoc.dll]: https://www.nuget.org/packages/DryIoc.dll/
[DryIoc.MefAttributedModel.dll]: https://www.nuget.org/packages/DryIoc.MefAttributedModel.dll/
[Wiki]: https://bitbucket.org/dadhi/dryioc/wiki/Home
[MefAttributedModel]: https://bitbucket.org/dadhi/dryioc/wiki/MefAttributedModel
[PCL]: http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx
[v2.0]: https://bitbucket.org/dadhi/dryioc/wiki/Home

<a href="https://twitter.com/DryIoc" class="twitter-follow-button" data-show-count="false" data-size="large">Follow @DryIoc</a>
<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>

* Designed for low-ceremony use, performance and extensibility.
* Supports .NET 3.5, 4.0, 4.5 _([PCL] is available in v2.0 pre-release packages)_
* Available at NuGet as [code][DryIoc] or [dll][DryIoc.dll].
* Extensions: [MefAttributedModel] as [code][DryIoc.MefAttributedModel] or [dll][DryIoc.MefAttributedModel.dll].
* [Docs are in a Wiki][Wiki] _(currently being updated for v2)_
* Latest stable version is **1.4.1** [![Build status](https://ci.appveyor.com/api/projects/status/jfq01d9wcs4vcwpf/branch/default)](https://ci.appveyor.com/project/MaksimVolkau/dryioc/branch/default)
* __[v2.0] pre-release packages:__ `PM> Install-Package DryIoc -IncludePrerelease`

#### Fast
* On par with fastest containers listed in [IoC Container Benchmark](http://www.palmmedia.de/blog/2011/8/30/ioc-container-benchmark-performance-comparison).
* General use-cases optimized for max speed.
* Call-stack depth preserved as shallow as possible.
* Memory footprint preserved as low as possible.

#### Small
* Minimal setup requires single source file: *Container.cs*. 
* No more than 2500 lines of code including comments.
* Code written to be readable.
* Uses [Caliburn.Micro](http://caliburnmicro.codeplex.com/) alike approach for customization.

#### Reliable
* Unit-tested with 100% coverage.
* Thread-safe: registrations and resolutions could be done in parallel without corrupting container state. 
* Recursive dependency detection (cycle in object graph).
* Error handling with `ContainerException` inherited from `InvalidOperationException` to filter container related exceptions.
* Throws exceptions as early as possible. 
* Meaningful error messages with all available information about problem Cause and Context.
* Resolving as `DebugExpression<T>` to find underlying expression used for instance creation.

#### Features
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
* Instance lifetime control (*instance reuse* in DryIoc terms):
    * `Transient`, `Singleton`, `InCurrentScope`, `InResolutionScope`.
    * Custom reuse by implementing `IReuse`. Check `DryIoc.UnitTests.ThreadReuse` for example.
    * Nested disposable scopes of reuse.
    * Disposing of `Singleton` on Container dispose.
* [Decorators](http://en.wikipedia.org/wiki/Decorator_pattern). 
* Context-based implementation selection.
* Unregistered service resolution via `ResolutionRules`.
* Toggling features On/Off via `ContanerSetup`.

#### Extensions
* [MefAttributedModel] - emulates [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) and enables automatic types discovery and wiring.