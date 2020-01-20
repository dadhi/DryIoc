Version History
---------------

## v4.1.0 Feature release

### Highlights

- Better memory use and massively improved performance for the real-world applications - big object graphs and the unit-of-work scenarios. See the updated benchmark results in [readme.md].(https://github.com/dadhi/DryIoc/blob/master/README.md#creating-the-container-registering-the-services-then-opening-a-scope-and-resolving-the-scoped-service-eg-controller-for-the-first-time))
- Improve parallelism / reduce thread blocking #137
- Full and fast DryIoc own Interpretation support via `Rules.WithUseInterpretation()` - useful for iOS and other platforms without compilation support #90
- Add DryIocZero capabilities to the DryIoc source package #101
- Copied DryIoc sample project from the MediatR but with DryIoc.Messages #117 

### Features

- Interpretation only option #90 
- Add RegisterDelegate with the list of dependencies to Inject and not to Resolve #147
- Lift restrictions for Scope disposal when using ambient ScopeContext #168 
- FactoryDelegate<T> wrapper support #191
- Add Rules.UseDecorateeReuseForDecorators #201

### Fixes

- Resolving a component for the second time throws #130
- Stackoverflow exception #139
- Xamarin Forms iOS: Operation is not supported on this platform. #156
- DryIoc 4.0.5 withoutFastExpressionCompiler deadlock issue #157
- Rules.WithDependencyDepthToSplitObjectGraph is not propagated through most of `Rules.With` methods #163
- Rules.WithDependencyDepthToSplitObjectGraph was not applied in some cases with Decorators in graph #164
- Made.Of() Parameters don't follow Reuse setting #179
- ThreadScopeContext not working in DryIoc.dll 4.1.0-preview-02? #183
- ReflectionTools methods unexpectedly return static constructors #184
- Container creates multiple instances if TryInterpret fails #188
- Open-generic implementation may be wrongly considered for service with many type args #190
- DryIOC new Transient Disposable #192
- Private and public Constructors in generic classes #196
- Fix documentation regarding implicitly injected scope as IDisposable #197
- Open generics resolve fails if there is a static constructor #198
- 4.1.0 Unhandled Exception: System.NullReferenceException: Object reference not set to an instance of an object. #205
- Avoid wasteful compilation of the same cached expression by multiple threads #208


## v4.0.7 Bug-fix release / 2019-09-04

- fixed: #173 Validate-Method throws System.TypeInitializationException : The type initializer for 'DryIoc.OpenGenericTypeKey' threw an exception.

## v4.0.6 Bug-fix release / 2019-08-29

- fixed: Wrong IContainer resolved #171 

## v4.0.5 Release notes / 2019-06-08

- fixed: #133 Validate method call hangs
- fixed: #134 with finding is th property is static in VB

## v4.0.4 Release notes / 2019-05-01

- fixed: #116: DryIoc Resolve with decorators goes wrong for parallel execution
- fixed: #119: v4.0.3 packages were targeting only .NET 4.5 and .NET Standard 2.0
- fixed: #120: V4.0.3 double dryioc break the build
- fixed: #121: FastExpressionCompiler.cs and Expression.cs code comment out in .NetFramework 4.7
- fixed: #124: Stackoverflow exception 4.0.3

## v4.0.3 Release notes

- fixed: #109: Cannot inject a struct using a Func wrapper
- fixed: #114: Resolve Action{T}

## v4.0.2 Release notes / 2019-03-30

- fixed: #100: ResolveMany with Meta does NOT work but collection with Meta does work

## v4.0.1 Release notes

- added: #95: Serializable ContainerException for supported targets
- fixed: #97: Resolving last registration from multiple default services bug
- fixed: #89: DryIoC.CommonServiceLocator.dll package out-of-date link

## v4.0.0 Release notes

### Highlights

- DryIoc.dll and all extensions are strongly-signed.
- Greatly improved performance and decreased memory allocations for bootstrapping and first-time resolution, as well as for the rest of operations. [The results](https://github.com/dadhi/DryIoc/issues/26#issuecomment-466460255) were measured on realistic mid-sized object graph with ~40 dependencies and mixed registration types.
- `IResolver` is directly implementing (`IServiceProvider`)[https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider?view=netframework-4.7.2] for supported platforms
- `UseInstance` is split into `RegisterInstance` and `Use` methods #78
- The docs now are generated from `.cs` files in _DryIoc.Docs_ project with up-to-date runnable examples using [CsToMd](https://github.com/dadhi/Cstomd) project.

### Features

- added: #4 Rule for Func and Lazy to be resolved even without requested service registered
- added: #8 Parity of registration methods between IContainer and IRegistrator (RegisterMapping and RegisterPlaceholder are available in IRegistrator)
- added: #9 RegisterMany should indicate if no registration was made
- added: #11 Resolution root bound dependency expression de-duplication
- added: #17 Rules.DefaultRegistrationServiceKey enhancement
- added: #20 Enhance error message with current container Rules info
- added: #32 Integrate MediatR like middleware directly to DryIoc #32
- added: #39 For troubleshooting purposes add ability to opt-out FastExpressionCompiler 
- Added: #45 Consider expression interpretation to speed-up first time resolution
- added: #78 Split UseInstance two roles into separate RegisterInstance and Use
- added: DryIoc IResolver now directly implements IServiceProvider - no need for BuildServiceProvider anymore
- added: DryIoc.Rules.Rules.MicrosoftDependencyInjectionRules - the set of rules for MS.DI available directly in DryIoc
- added: FactoryInfo.Of(MemberInfo factoryMember, object factoryInstance)
- added: Explicit Factory.RegistrationOrder and adapted its usage for open-generics
- added: IRegistrator.GetRegisteredFactories
- added: FactoryType to Registrator.RegisterMapping
- added: AsyncExecutionFlowScopeContext.Default member
- added: Support for FEC v2.0

### Fixes

- fixed: #6 Open generic singleton service registration that satisfies multiple interfaces
- fixed: #7 Context-based injection
- fixed: #16 AutoConcreteTypeResolution should not consider a primitive type
- fixed: #25 Decorator with serviceKey throws exception which gives invalid advice on how to fix the issue for the most common reason to use the servicekey
- fixed: #26 Speed Optimization for short living applications
- fixed: #27 DryIoc cold start performance
- fixed: #28 FastExpressionCompiler is not used in Net Standard 1.3, 2.0 packages and not used in .Net Core
- fixed: #29 Resolve caches object[] args values
- fixed: #33 Memory leak with ResolveManyBehavior.AzLazyEnumerable?
- fixed: #41 ErrorCode: RegisteredFactoryMethodResultTypesIsNotAssignableToImplementationType
- fixed: #46 Operation is not supported on this platform exception on Xamarin iOS
- fixed: #61 Rules.SelectLastRegisteredFactory() does not account for OpenGenerics
- fixed: #63 Func wrapper resolving #63
- fixed: #75 Scoped call to Resolve() with args seems to leak memory
- fixed: BB-593 Add auto-generated tag to PCL FEC
- fixed: BB-594 Conflicting type is not working in .NET Core 2.1
- fixed: BB-596 The problem was in non-public service type- fixed: RegisterMapping for open-generic service type
- fixed: Using facadeKey in CreateFacade


### v3.0.0-preview-04 to v3.0.0-preview-12 / 2018-06-10

[Release Notes](Version3ReleaseNotes)

### v2.12.6 / 2017-12-20

  - fixed: #544: WithTrackingDisposableTransients may downgrade Singletons to Transients

### v3.0.0-preview-03 / 2017-12-05

[Release Notes](Version3ReleaseNotes)

### v3.0.0-preview-03 / 2017-11-15

[Release Notes](Version3ReleaseNotes)

### v2.12.5 / 2017-10-30

  - fixed: #533: Exporting WPF UserControl causes NullReferenceException

### v2.12.4 / 2017-10-17

  - fixed: Race condition when creating or storing the scoped service

### v2.12.3 / 2017-10-02

  - fixed: #527 Error ResolveMany after Unregister
  - fixed: Bug in ImTools ArrayTools.Append in certain cases

### v3.0.0-preview-01 / 2017-10-01

[Release Notes](Version3ReleaseNotes)

### v2.12.2 / 2017-09-17

  - fixed: #519 Dependency of singleton not working when using "child" container
  - fixed: #520 AccessViolationException on some machines
  - fixed: #521 Rule ConcreteTypeDynamicRegistrations: Exception while resolving instance of class with constructor-injected generic instance of not registered class

### v2.12.1 / 2017-09-09

  - fixed: #512 InResolutionScopeOf in combination with SelectLastRegisteredFactory
  - changed: Updated to FEC v1.4 - now all DryIoc expressions are covered by FEC. This means perf improvements for asResolutionCall injection

### v2.12.0 / 2017-09-01

  - added: #499: Add RegisterPlaceholder to enable delayed registration
  - added: Setup.DecoratorOf{T}(key) and runtime version to simplify specifying condition for matching decoratee type and key
  - added: Missing overload for Made.Of to consider request -> instance factory info.
  - added: Rules.WithDynamicRegistrationsAsFallback
  - changed: Updated FEC to v1.3.0
  - fixed: #492: Lazy imports disguised as non-lazy
  - fixed: #497: ConstructorWithResolvableArguments is not working properly
  - fixed: #500: Rule WithConcreteTypeDynamicRegistrations disables allowDisposableTransient
  - fixed: #506: Cannot resolve string[]
  - fixed: #507: Resolved collection of mixed open and closed generics does not preserve order of registration
  - fixed: #508: SelectLastRegisteredFactory and resolving collection of open-generic is not working as intended


### v2.11.6 / 2017-07-18

  - fixed: #495: InvalidCastException on type resolution

### v2.11.5 / 2017-07-12

  - fixed (!!!): #493 Some resolving of the controller ends up with exception "Container is disposed..."
  - fixed: #175 Deterministic implicit rule to select from multiple services for specific scope

### v2.11.4 / 2017-06-27

  - fixed: #488 DryIoc.ContainerException if using WithDependencies

### v2.11.3 / 2017-06-01

  - fixed: Backward compatibility of DryIoc 2.11 WrapperSetup.Condition with 2.10

### v2.11.2 / 2017-06-01

  - fixed: Backward compatibility of DryIoc 2.11 Setup.Condition with 2.10
  - fixed: #480: DryIoc.Microsoft.DependencyInjection - WithDependencyInjectionAdapter() exception

### v2.11.1 / 2017-05-31

  - fixed: UseInstance is not backward compatible with v2.10
  - fixed: #478: DryIoc.Owin.dll 2.2.0 not compatible with DryIoc.dll 2.11.0

### v2.11.0 / 2017-05-30

  - added: #449: Provide a way to log unresolved dependencies via Rules.WithUnknownServiceHandler
  - added: #463: Add IfAlreadyRegistered option for UseInstance
  - added: #475: Dynamic Registration Providers to enable lazy and on-demand registrations
  - added: #476: AutoFallback and AutoConcreteType resolution based on Dynamic Registration providers
  - fixed: #396: Enable ResolveMany to use auto fallback resolution via dynamic registrations
  - fixed: #451: DryIoc should know about static members
  - fixed: #474: Exclude supported collection wrapper interfaces from RegisterMany / ExportMany
  - fixed: #477: ArgumentException while resolving
  - fixed: DryIocAttributes dependency to System.Diagnostics.Tools from ..Debug

### v2.10.7 / 2017-05-03

  - fixed: #465 Installing DryIoc.dll for desktop/4.5 installs tons of .NEt Standard packages
  - changed: Attempt to fix InvalidCastException found in scope of #433

### v2.10.6 / 2017-04-20

  - added: Xamarin.Forms for MacOS support, based on PCL Profile259/netstandard1.0 using xamarinmac2.0 TFM
  - added: Reuse.ScopedOrSingleton to simplify scenarios which currently use Rules.WithImplicitRootOpenScope, and simplify MS.DI adapter
  - fixed: Performance issue with #459 (Container.Dispose stack trace) and made it turned Off by default
  - fixed: Updated to FEC v1.0.0+ with fix for #6
  - changed: More cache agnostic and flexible disposable transient tracking with Reuse.ScopedOrSingleton
  - changed: Improved perf around resolution of scoped service

### v2.10.5 / 2017-04-04

  - fixed: #459: Add Container.Dispose stack trace to ContainerIsDisposed error message
  - changed: Updated to FastExpressionCompiler v1.0.0

### v2.10.4 / 2017-03-28

  - fixed: #456: Abstract service type is not supported with AutoConcreteTypeResolution and required service type
  - fixed: #431: Make AutoConcreteTypeResolution rule to check if type is resolvable and fallback to next rule
  - added: FactoryMethod.DefaultConstructor

### v2.10.3 / 2017-03-25

  - fixed: #438: Remove self WeakReference from Container to make it more simple
  - changed: Using the latest FastExpressionCompiler with nested lambda compilation support
  - removed: AggressiveInlining

### v2.10.2 / 2017-03-14

  - fixed: #454: Performance degradation for resolution of InWebRequest and generally named scope services

### v2.10.1 / 2017-02-18

  - fixed: #446 Select single open generic impl based on matching closed service type

### v2.10 / 2017-02-01

  - fixed: #440: Singleton optimazation causes exception for internal type in some cases
  - fixed: #435: Reuse.Singleton prevents the correct container injection within explicit resolve
  - fixed: #437: General perf degradation introduced in 2.9 due skipping fast FactoryCompiler
  - added: #430: Add rule to change or to disable split level of large object graph
  - changed: Moving HashTrees to ImTools namespace.
  - changed: #254 [Performance] Remove state parameter from FactoryDelegate - use FastExpressionCompiler with closure support

### v2.9.7 / 2017-01-12

  - fixed: #426: dead lock
  - fixed: #423: Inner scope is injected to singleton
  - fixed: #429: Resolve instance from named scope with Func
  
### v2.9.6 / 2016-12-30

  - fixed: #421: MissingMethodException
  - fixed: Perf of first Resolve in some cases - now is up to ~10x faster

### v2.9.5 / 2016-12-26

  - fixed: #416 (now for real): Adding always true condition to decorator changes the decorated outcome - but should not
  - fixed: #417 Performance issue with Func of singleton
  - fixed: #418  Performance issue with resolving singletons
  - fixed: #419 In some cases ResolveMany unable to resolve Decorator with dependency due issue with required service type
  - fixed: #420: Singleton resolved and created via Func with arguments then may be resolved directly
  - fixed: GetMembers did not considered base of the base class

### v2.9.4 / 2016-12-10

  - fixed: #416: Adding always true condition to decorator changes the decorated outcome - but should not

### v2.9.3 / 2016-12-08

  - fixed: #415: Decorators useDecorateeReuse should permit decorating disposable transient

### v2.9.2 / 2016-12-01

  - fixed: #409: Resolution scope is created with the service type instead of actual required service type
  - fixed: #410: ResolveMany is treated differently comparing to Resolve IEnumerable

### v2.9.1 / 2016-11-19
 
  - fixed: #404: ConstructorWithResolvableArguments does not take into account parameter service key

### v2.9.0 / 2016-11-17

  - added: #386: Support for resolving void Factory Method as Action
  - added: Support for array decorators
  - added: #405: Support for resolving unknown services in ResolveMany via UnknownManyServiceResolvers

### v2.8.5 / 2016-11-02

  - fixed: #387: ArgumentException with initializer

### v2.8.4 / 2016-10-28

  - fixed: #382: Different instances of interface with Reuse.InCurrentNamedScope
  - fixed: #383: Support open-generic type to specify Reuse.InResolutionScopeOf(open-generic)
  - added: DryIoc.Internal source code package with the public types made internal.

### v2.8.3 / 2016-10-20

  - fixed: NetStandard1.0 package dependencies
  - fixed: Removing remains of IServiceProvider

### v2.8.2 / 2016-10-20

  - fixed: NetStandard1.0 package dependencies 
  - removed: #342: Make IContainer implement System.IServiceProvider interface

### v2.8.1 / 2016-10-19

  - fixed: #368 Registration Made.Of should override the global Container Made.Of settings

### v2.8 / 2016-10-18

  - added: #269: Distinguish Transient reuse from non specified reuse
  - added: #331: Track transient disposables in singleton scope if nor reused parent nor the current scope available
  - added: #342: Make IContainer implement System.IServiceProvider interface
  - fixed: #327: UseInstance non-deterministic behavior in multi-threading tests
  - fixed: #328: Lazy collection resolve behavior in and out of scope
  - fixed: #330: Looks like rules do not work with TrackingDisposableTransients
  - fixed: #338: after re-register instance in scope used old version of object(cached)
  - fixed: #347: Not resolved instance in child scope of scope
  - fixed: #344: Transient disposable validation
  - fixed: #356: Portable.GetAssemblyTypesMethod() crash with ArgumentException
  - fixed: #359: Some rules and setup flags applied twice will wrongly unset the flag
  - fixed: #365: Container is not marked and treated as Disposed when Singletons are disposed

### v2.7.1 / 2016-09-01

  - fixed: #334 [DryIoc.Microsoft.DependencyInjection] MissingMethodException in WithDependencyInjectionAdapter
  - fixed: #335 Decorator of IEnumerable wrapper taking nested in IEnumerable dependency fails with StackOverflow

### v2.7 / 2016-08-19

  - added: #195: Composable Metadata as a IDictionary&lt;string, object&gt;
  - added: #232: Change RegisterInstance defaults to IfUnresolved.Replace and to Reuse.InCurrentScope
  - added: #304: Add option to pass values for some dependencies on Resolve
  - added: #313: Support non public constructors with ConstructorWithResolvableArguments
  - added: #314: Allow to inject non primitive external values
  - added: #315: Support decorator of wrappers, e.g. decorate IEnumerable&lt;T&gt;
  - fixed: #298: AspNetCore.DependencyInjection exceptions when verifying resolutions
  - fixed: #300: Exception when reusing objects.
  - fixed: #310: Problems with Decorators and Service Keys
  - fixed: #317: Skip Enums in RegisterMany(assemblies)

### v2.6.4 with NetStandard1.0 RTM / 2016-07-29

  - fixed: #301: Issue with Lazy in Func with arguments

### v2.6.3 / 2016-07-10

  - fixed: #299 Race condition between multiple containers

### v2.6.2, v2.6.2-netcore-rc2 / 2016-06-13

  - fixed: #290 Disposable Transient without reused parent should be tracked in nearest open scope

### v2.6.1, v2.6.1-netcore-rc2 / 2016-06-10

  - changed: increasing version for fixing NetStandard1.0 support in dll package

### v2.6.0 / 2016-06-10

  - changed: increasing version for adding NetStandard1.0 support in dll package

### v2.5.1 / 2016-06-07

  - fixed: #247: Collection wrapper resolved from Facade does not count parent container registrations

### v2.5.0 / 2016-05-24

  - fixed: #280: LazyEnumerable dependency silences missing dependency error - but works (throws) for fixed array
  - fixed: #279: IfUnresolved.ReturnDefault propagates down the dependency chain
  - fixed: #285: Recursive dependency in the case of using Composite Pattern
  - fixed: #219: Minimize memory allocations in Request and RequestInfo
  - added: #282: Enable to resolve func with more than 4 parameters (up to 7)
  - added: #255: Make GetCurrentScope available on IContainer via extension method
  - added: #172: Reuse passed Func argument in nested dependencies, but not in the same service
  - added: #257: Support for contentFiles to enable source packages in NuGet3.3
  - added: Uap (Universal Windows Platform) content source code DryIoc package

### v2.4.3 / 2016-05-08

  - fixed: #277 Custom value for dependency evaluated to null is interpreted as no custom value and tries to resolve the dependency
  - fixed: #278 Arg.Of does not recognize service key of non-primitive type

### v2.4.2 / 2016-04-26 

  - fixed: #274: Lazy resolution of dependency registered with Reuse.InResolutionScope breaks subsequent resolutions without wrappers

### v2.4.1 / 2016-04-16

  - fixed: #267: False alarm about recursive dependency when using decorator

### v2.4.0 / 2016-04-14

  - added: #263: Add IfAlreadyRegistered.AppendNewImplementation option for better collection handling
  - fixed: #264: IfAlreadyRegistered.Replace can span multiple registrations
  - fixed: #262: Using attributes to inject primitive variables
  - fixed: #261: Make Disposal work in reverse resolution order

### v2.3.0 / 2016-03-30

  - fixed: #260: Cannot interpret GetOrCreateResolutionScope expression on iOS
  - fixed: #258: Setup.AllowDisposableTransient is overridden by container.WithTrackingDisposableTransient
  - changed: In source code DryIoc package split immutable data structures and tool to separate ImTools.cs
  - removed: CloseCurrentScope from Container as it was added recently but not used anywhere.
  - added: method New overload with strongly type Made.Of

### v2.2.2 / 2016-03-10

  - fixed: #251: Auto register types from different namespace and different assemblies
  - fixed: #252: Make work together MefAttributedModel constructor selection and ConstructorWithResolvableArguments rule
  - fixed: #253 Add Container.ToString method to at least indicate scope for scoped container

### v2.2.1 / 2016-03-08

  - fixed: #245: Automatically add condition to check T constraints in Decorator of T
  - fixed: #250: DI.Mef: Non static class of static factory method is exported and can be resolved
  - changed: Minor Resolve speedup

### v2.2.0 / 2016-02-26

  - added: #141: Support Decorators with open-generic factory methods of T
  - added: #206: Track IDisposable Transients in scoped consumer's Scope
  - added: #215: Option to specify Release action for reused services to implement pooling etc
  - added: #227: Missing Arg.Of overload to specify default value for unresolved dependency
  - added: #228: Make IContainer implicitly available for injection without need for registration
  - added: #229: Container rule to use Singleton scope as implicitly Open current scope for scoped registrations
  - added: #239: Decorator setting to use Decoratee Reuse
  - added: #241: Registration option to useParentReuse for dependency
  - added: #242: Container rule for automatic concrete types resolution
  - fixed: #94: Support for creating concrete type without storing it in Container
  - fixed: #220: Minimize locking and therefore chances for deadlock in Singleton Scope
  - fixed: #230: Custom initializer attached to lazily resolved dependency is called once per resolution, not once per construction
  - fixed: #240: ConstructorWithAllResolvableArguments ignores implicitly injected dependencies and custom values

### v2.1.3 / 2016-01-16

  - fixed: #224: Enumerable wrapped in Func loses the information about Func wrapper, causing incorrect scope lifetime validation

### v2.1.2 / 2016-01-15

  - fixed: #222: Resolving as IEnumerable silence the dependency resolution errors and just skips the unresolved item
  - fixed: #218: Apply decorators registered for service type when resolving with required service type

### v2.1.1 / 2016-01-04

  - fixed: #213: Lazy Singletons should be resolved after container is disposed
  - fixed: #212: ResolveMany of object with generic required service type is failing with ArgumentException

### v2.1.0 / 2015-12-04

  - added: #205: Add customizable IfAlreadyRegistered default value per container
  - added: #204: Add ResolveMany of objects wout need to specify ResolveMany{Object}
  - fixed: #203: RegisterMany should exclude ValueType and general purpose service types IEquatable, IComparable
  - Small performance improvements

### v2.0.2 / 2015-12-01

  - fixed: #201: Mutithreading issue when RegisterInstance() is used within OpenScope()

### v2.0.1 / 2015-11-27

- fixed: #200: Multiple instances for Singleton created when Container is shared among multiple threads

### v2.0 / 2015-11-19

This release pushes DryIoc towards the mature [full-featured](http://featuretests.apphb.com/DependencyInjection.html) library with:

- Support for [PCL](http://msdn.microsoft.com/en-us/library/gg597391(v=vs.110).aspx) and [.NET Core](https://oren.codes/2015/07/29/targeting-net-core).
- [Documentation](Home#markdown-header-usage-guide).
- More complete and consistent API surface.
- Bug fixes.
- Diagnostics for potential resolution problems with `container.VerifyResolutions()`.
- Improved registration and first resolution time.
- Support of really large object graphs.
- Possibility of compile-time factory delegate generation (utilized by [DryIocZero](Companions/DryIocZero)).
- Ambient current scope and `Reuse.InWebRequest` for ASP.NET integration.
- Reuse per service in resolution graph via `Reuse.InResolutionScopeOf()`.
- Support for static and instance factory methods in addition to constructor, including support for method parameters injection.
- Powerful open-generics support including variance, constraints, open-generic factory methods in open-generic classes.
- Batch registration from assemblies and type collections via `RegisterMany`.
- Support for service key of arbitrary type. The only requirement for key type is to implement `object.GetHashCode` and `object.Equals`.
- Resolve as `KeyValuePair` to get service key with service object.
- Register with condition for resolution.
- Required service type support: e.g. `var serviceObjects = c.Resolve<object[]>(typeof(Service));`.
- Optional parameters support.
- Fine-grained control over injection of parameters, properties, and fields.
- Injection of primitive values.
- Control how reused service is stored and disposed via `weaklyReferenced` and `preventDisposal` setups.
- Resolve service collection as `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`.  
- Register once, existing registration update, unregister.
- __removed:__ Compilation to DynamicAssembly. DryIoc is fast enough without its complexity. 

### v1.4.1 / 2014-10-09

- fixed: #70: .Net 4.0 related: Unable to resolve types from unsigned assembly if compiling to dynamic assembly is turned On (it is by default)

### v1.4.0 / 2014-09-10

- fixed: #56: ResolvePropertyAndFields/SatisfyImports for already created instance with regard of ResolutionRules, e.g. provided by MefAttributedModel

### v1.3.1 / 2014-08-18

- fixed: #46: Reuse.InCurrentScope for nested dependencies is not working

### v1.3.0 / 2014-07-16

- fixed: #5: Friend assembly reference 'DryIoC.CompiledFactoryProvider.DynamicAssembly' is invalid. Strong-name signed assemblies must specify a public key in their InternalsVisibleTo declarations.
- fixed: #4: DryIoC\InternalsVisibleToFactoryCompilerDynamic.cs.
- fixed: #37: Support symbol packages for SymbolSource.org.
- fixed: #38: Add optional script to Sign NuGet package assemblies with Strong Name

### v1.2.2 / 2014-07-02

- fixed: #29: DryIoc.dll v1.2.1 Nuget package contains wrong DLL from dev branch

### v1.2.1 / 2014-06-30

- fixed: #26: Singleton creation from new scope fails

### v1.2.0 / 2014-01-09

- fixed: #1: Reordering and nesting of type arguments in generics is not supported

### v1.1.1 / 2013-12-26

- DryIoc first public appearance.