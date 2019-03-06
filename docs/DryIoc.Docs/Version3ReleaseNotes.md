# DryIoc v3.0.0 Release Notes

[TOC]

## Breaking changes

I was trying to keep API surface the same or equivalent to the V2, especially for most general use-cases.
If lucky, you just need to recompile. Anyway, I suggest to read the details below. 


## Goals

- Less features and more feature compose-ability
- Less code in general and less hacks
- Smaller and more focused interfaces
- More polished and gap-less API
- Features to simplify integration with other libraries
- Cleanup obsolete code


## In detail

### OpenScope changes

Now `OpenScope` is returning `IResolverContext` instead of full `IContainer`.

The consequence is that __you won't be able to Register__ on returned scope object. 
But this is OK because even before, any registration done on scope was actually done on container.
This was confusing, because someone may think that registration in scope is separate and will be disposed with the scope.

Old code:
```
    var container = new Container();
    container.Register<A>();

    using (var scope = container.OpenScope())
    {
        scope.Register<B>();
        scope.Resolve<A>();
    }
```
 
New code:
```
    var container = new Container();
    container.Register<A>();
    container.Register<B>();

    using (var scope = container.OpenScope())
    {
        scope.Resolve<A>();
    }
```

Note: It is still valid to call `UseInstance` and `InjectPropertiesAndFields` -
the methods are moved to `IResolverContext`.


### No more ImplicitOpenedRootScope

This rule was added to conform to [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) 
specification to enable the resolution of scoped service both from scoped and the root container.
The latter means that the resolved service will be a singleton despite the fact it is registered as scoped.

The rule instructed the DryIoc to open scope on container creation and most importantly, to dispose this scope
together with container.

Starting from __DryIoc v2.12__ the new hybrid `Reuse.ScopedOrSingleton` was added, so you may not need to open the scope
to resolve such a service. This reuse means the `Rules.WithImplicitOpenedRootScope` is no longer necessary.

Old code:
```
    var container = new Container(rules => rules.WithImplicitOpenedRootScope());

    container.Register<A>(Reuse.Scoped);
    
    container.Resolve<A>(); // Worked, even without an open scope due the rule
```

New code:
```
    var container = new Container();

    container.Register<A>(Reuse.ScopedOrSingleton); // The fine-grained and clean intent
    
    container.Resolve<A>(); 
```


### Reuse.InResolutionScope changes

#### InResolutionScope reuse now is just a Scoped reuse

Resolution scope reuse is the lifetime behavior associated with the node in service object graph.
Previously resolution scope reuse was separate from scope reuse. It means that scope created
via `OpenScope` did not have any link to scope created for resolved or injected service.

Now it is different and resolution scope is the part of nested open scopes.
Therefore resolution scope reuse is just a scoped reuse with the special name, consisting of
target resolved/injected service type and/or service key.

That also means `Reuse.InResolutionScope` which does not specify the type of bound service is
just a `Reuse.Scoped`.

Old code:
```
    var container = new Container();
    
    container.Register<A>(Reuse.InResolutionScopeOf(serviceKey: "X"));

    container.Resolve<A>(); // Error! No service with service key "X" is found
```

New code:
```
    var container = new Container();
    
    container.Register<A>(Reuse.ScopedTo(serviceKey: "x"));

    // resolution scope is just a normal scope that can be opened manually
    using (var scope = container.OpenScope(ResolutionScopeName.Of(serviceKey: "x")))
    {
        container.Resolve<A>(); // Works.
    }
```

#### Scope is no longer automatically created on Resolve

Previously for any registered service, the call to `Resolve` may create the scope associated 
with resolved service, as long the service had a dependency registered with `Reuse.InResolutionScope`.
Now it is no longer happen. The scope will be created only if resolved service is registered
with `setup: Setup.With(openResolutionScope: true)` option.

Old code:
```
    var container = new Container();
    
    container.Register<A>();
    container.Register<Dependency>(Reuse.InResolutionScopeOf<A>());
    
    container.Resolve<A>(); // opens scope and Dependency is successfully injected
```

New code:
```
    var container = new Container();
    
    container.Register<A>(setup: Setup.With(openResolutionScope: true));
    container.Register<Dependency>(Reuse.ScopedTo<A>()); // new syntax, old syntax is obsolete
    
    container.Resolve<A>(); // opens scope and Dependency is successfully injected
```


### RegisterDelegate parameter changes

`IResolver` parameter in `RegisterDelegate((IResolver resolver) => ...)` was extended to `IResolverContext`.
I said 'extended' because `IResolverContext` implements the `IResolver`. Because of this, there is a high chance
that your code will compile without changes. 

Using `IResolverContext` in delegate will allow you to `OpenScope`, `UseInstance`, 
etc. without need for bringing the correct container instance inside delegate.

Old code:
```
    var container = new Container();
    
    container.RegisterDelegate(r => 
    {
        using (var scope => r.Resolver<IContainer>().OpenScope()) { ... }
    });
```

New code:
```
    var container = new Container();
    
    container.RegisterDelegate(r => 
    {
        using (var scope => r.OpenScope()) { ... }
    });
```

### No more FallbackContainers and CreateFacade changes

`FallbackContainers` were not working fully with the rest of DryIoc features. Therefore multiple issues were reported.
So I am happily removing them.

`CreateFacade` was implemented on top of `FallbackContainer` and allowed to 'override' facaded container registrations.
This behavior may be useful in Tests to override prod service with test mock.

Now `CreateFacade` is just a sugar on top of `Rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey))`.

Old code:
```
    var container = new Container();
    container.Register<A>();
    container.Register<IDepOfA, DepOfA>();

    var facade = container.CreateFacade();
    facade.Register<IDepOfA, TestDeoOfA>();

    facade.Resolve<A>(); // will have TestDeoOfA
```

New code:
```
    var container = new Container();
    container.Register<A>();
    container.Register<IDepOfA, DepOfA>();

    var facade = container.CreateFacade();
    facade.Register<IDepOfA, TestDeoOfA>(ContainerTools.FacadeKey);

    // or with custom key
    var facade = container.CreateFacade("test");
    facade.Register<IDepOfA, TestDeoOfA>("test");

    facade.Resolve<A>(); // will have TestDeoOfA
```

### Property injection changes

Previously, the properties and fields were injected with `IfUnresolved.ReturnDefault` policy.
That means, if matching service was not found (not registered) or there was some other resolution error,
the property won't be set. Client does not have any indication of the error, and may assume that property is set.
That was done because the property injection is already too relaxed way of injection. 
If you turning it on, be ready for issues in run-time. 

In V3 the situation is improved via new default policy `IfUnresolved.ReturnDefaultIfNotRegistered`.
The policy says that if matching service is not registered, only then skip property setting. For all other kinds of errors,
including missing service dependencies, or reuse mismatch, etc. etc., you will get the exception with explaining message.

Given the setup:
```
class A
{
    public B B { get; set; }
}

class B 
{
    public B(C c) {}
}

class C {}
```

Old code:
```
    var container = new Container();
    container.Register<A>(made: PropertiesAndFields.Auto); // enable property injection for A
    container.Register<B>();
    // Skip registration for C

    // Property is not set and no error
    var a = container.Resolve<A>(); 
    Assert.IsNull(a.B);
```

New code:
```
    var container = new Container();
    container.Register<A>(made: PropertiesAndFields.Auto); // enable property injection for A
    container.Register<B>();
    // Skip registration for C

    // Throws an exception because B is registered but cannot be resolved due its missing dependency C
    container.Resolve<A>();
```

### RequestInfo is merged with Request

`RequestInfo` was replaced in all APIs with `Request`. Theoretically, it should not affect the consumers,
cause both classes had an identical public API.

`RequestInfo` was the lightweight version of `Request` without the runtime information, e.g. reference to 
container. The separation was required for code generation scenarios, where there are no run-time state available. There were also a performance considerations.

It was always a pain to synchronize between both classes.

Now it is no longer needed, `Request` now holds an optional runtime information (Container), 
and may be used as `RequestInfo` when required.


### WeaklyReferenced changes

`IDisposable` services registered with `setup: Setup.With(weaklyReferenced: true)` are no longer disposed.

The disposal was not guarantied even before, because the weakly referenced service may be garbage collected at any time.


## New Features

### Reuse.Scoped and Reuse.ScopedTo for all kinds of scope reuse

#### More general reuse API

Before:
```
container.Register<A>(Reuse.InCurrentScope);
container.Register<B>(Reuse.InCurrentNamedScope("x"));
container.Register<C>(Reuse.InResolutionScopeOf<A>());
```

Now:
```
container.Register<A>(Reuse.Scoped);
container.Register<B>(Reuse.ScopedTo(serviceKey: "x"));
container.Register<C>(Reuse.ScopedTo<A>());
container.Register<C>(Reuse.ScopedTo(ResolutionScopeName.Of<A>(serviceKey: "x")));
```

__The previous reuse names are not obsolete yet__, so no need to change the existing code.
The exception is `Reuse.InResolutionScope` which is marked as `Obsolete`, 
and should be replaced either by `Reuse.Scoped` or specific `Reuse.ScopedTo<X>()`;

#### Reuse.ScopedTo multiple scope names

Enables to use a single registration for reuse in scopes with specified names:

```
container.Register<A>(Reuse.ScopedTo("x", "y", "z"));
```

Resolution scope reuse:
```
container.Register<C>(Reuse.ScopedTo(ResolutionScopeName.Of<A>(), ResolutionScopeName.Of<B>()));
```


### Register open-generic service with closed implementation

DryIoc now may derive open-generic `IFoo<>` from closed-generic implementation `Foo<object>`.
In a sense, nothing prevents to do that, but in V2 such registrations where prohibited. 

It also may be helpful in some edge `RegisterMany` scenarios:

```csharp
container.RegisterMany(implTypes, getServiceTypes: implType => 
    (implType.GetGenericDefinitionOrNull() ?? implType).GetImplementedServiceTypes());
```

Related issue: https://bitbucket.org/dadhi/dryioc/issues/554/allow-register-an-open-generic-service 


### RegisterMany with exact service types you need

The feature is actually demonstrated above. Before, it was possible to provide only the predicate to filter discovered service types. Now you may specify and arbitrary types.

```csharp
container.RegisterMany(implTypes, getServiceTypes: implType => 
    implType == typeof(Foo<int>) ? new[] { typeof(IFoo<>) } 
        : implType.GetImplementedServiceTypes());
```


### Resolve with user-specified arguments for injection

`Resolve` method now has an overload to provide custom arguments for injection instead of
ones from registered services. It may be useful to pass the runtime data, e.g. connection string.
The passed argument is injected into all parameters of matched type in resolved object graph.

```
    public class X 
    {
        public X(string message, int number) {..}
    }

    container.Register<X>();

    var x = container.Resolve<X>(args: new object[] { "hey", 124 });
```

__Note:__ Passing arguments to `Resolve` may be combined with other ways to providing custom values, e.g.
with `Func<TArg1, TArg2.., TResult>` wrapper and with per container `WithDependencies(..)` method.


### IfUnresolved.ReturnDefaultIfNotRegistered

Additional option to control when to throw exception when service is not resolved / injected.

Resolving with:

- `IfUnresolved.Throw` - throws the exception when service is not registered or its dependency is not registered, or cannot be resolved for any other reason.
- `IfUnresolved.ReturnDefault` - returns `default` service value for any above reason
- `IfUnresolved.ReturnDefaultIfNotRegistered` - returns `default` only if service itself is not registered and throws otherwise, e.g. for missing dependency.

__Note:__ The last option now is the default for property injection with `PropertiesAndFields.Auto()` and `.All()` selectors.


### PropertiesAndFields.Properties

Like `Auto` but excludes the fields - __may be the only selector you need for property injection__. 


### Func with arguments can be used in Lazy and LazyEnumerable

Now possible:

```
container.Resolve<LazyEnumerable<Func<string, IService>>>();
```

This possibility was un-blocked by adding custom `args` to `Resolve(... object[] args)` method - 
so the generated code for Lazy and LazyEnumerable may propagate arguments through `Resolve` method boundary.
Hope, this info may help someone :)


### Support used-defined disposal order

By default disposal happens in reverse resolution order - resolved first will be disposed the last. 
For the service with dependency it means that service will be disposed first then the dependency.

In addition, now you may specify relative disposal order - smallest numbers will be disposed first.

```
    container.Register<A>(Reuse.Scoped, setup: Setup.With(disposalOrder: -1));
```

## Bug Fixes

- 505 Failed to register object with ArgumentNullException
- 521 Rule ConcreteTypeDynamicRegistrations: Exception while resolving instance of class with constructor-injected generic instance of not registered class
- 538 Ensure that Resolve with provided args is properly cached between multiple open scopes
- 541 Dynamic registrations: circular dependency is not detected
- 544 WithTrackingDisposableTransients may downgrade Singletons to Transients
- 546 Recursive dependencies are not detected in the large object graphs
- 553 Made.Of(null) causes crash in error handler changed
- 570 ArgumentNullException thrown when multiple constructors and args dependencies provided
- 574 IResolverContext.UseInstance() should not have any side effects on other scopes
- 576 ContainerException should include Type/Registration data
- 577 Resolve many/keyed factory delegate cache anomaly added: Back RegisterMany with service 
- 579 VerifyResolutions strange behavior
- 580 Same service instance resolved twice when decorator is used
- 581 Constructor injection with array parameter


## Other changes

1. _Container.cs_ lost ~3000 LOC.
2. Using C# 6 through codebase (not a C# 7.* yet)
3. Umbrella issue is closed: https://bitbucket.org/dadhi/dryioc/issues/259/gather-and-discuss-proposals-for-v3-api
2. `IReuse` contents is replaced with `IReuseV3` contents, `IReuseV3` is removed.
3. Removed unused `compositeParentKey` and `compositeRequiredType` parameters from `IResolver.ResolveMany` both in DryIoc and DIZero
4. Removed `scope` parameter from `Resolve` and `ResolveMany`
4. Removed `state` and `scope` parameter from FactoryDelegate due #288 both in DryIoc and DryIocZero
5. Removed `Rules.FallbackContainers`
6. `Container.CreateFacade` implementation is changed from fallback containers to 
`rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey))`
7. Removed `IScopeAccess` interface, replaced with `IResolverContext` and extension methods.
8. Removed `ContainerWeakRef` implementation of `IResolverContext`. Now `IResolverContext` is implemented by `Container` itself.
9. Added `IReuse.Name` to support reuse name
10. Renamed `IContainer.ContainerWeakRef` into `IContainer.ResolverContext`
11. Removed `ContainerTools.GetCurrentScope` extension. It is replaced by `IResolverContext.CurrentScope`
12. Removed obsolete `IContainer.EmptyRequest` and `Request.CreateEmpty`
13. Removed obsolete `IContainer.ResolutionStateCache` and `IContainer.GetOrAddStateItem`
14. Removed obsolete `Request.ToRequestInfo`
15. Removed feature `outermost` parameter of `Reuse.InResolutionScopeOf`
16. Removed not necessary `trackTransientDisposable` parameter from `IReuse.Apply` method
17. Removed `ResolutionScopeReuse`, replaced by `CurrentScopeReuse`
18. Changed `Reuse.InResolutionScope` to be just `Reuse.Scoped` underneath
19. Changed obsolete `RegisterInstance` implementation to just call `UseInstance`
20. Removed `InstanceFactory` which was used by obsolete `RegisterInstance`
21. Changed `IResolverContext` to implement the `IResolver` instead of holding it as property
to simplifies the path to `IResolver` from the object graph.
22. Removed unused `Request.IsWrappedInFuncWithArgs` method
23. Changed `IContainer` to implement `IResolverContext` as it is already does this
24. Removed `IContainer.ResolverContext` property
25. Removed no longer used `Request.WithFuncArgs`
26. Changed parameter `bool ifUnresolvedReturnDefault` to `IfUnresolved ifUnresolved` in 
`IResolver.Resolve` methods to allow to add more `IfUnresolved` options
27. renamed: `IfAlreadyRegistered` parameter to `ifAlreadyRegistered` in `UseInstance` methods
28. Moving `OpenScope`, `UseInstance`, `InjectPropertiesAndFields` from `IContainer` to `IResolverContext`
29. `OpenScope` no longer accepts the `Action<Rules>`, but you can always use `container.With(Action<Rules>)` before opening scope
30. `InjectPropertiesAndFields` may define the names of members to inject instead of full blown `PropertiesAndFieldsSelector`,
but there is still possibility to define the selector on container level
31. Added `object[] args` parameter into `Resolve` and `ResolveMany`
32. Removed special SingletonScope, using one implementation for both scope and singletons
34. Removed `IScope.GetScopedItemIdOrSelf` as it was required only by `SingletonScope`
35. Added `IScope.TryGet`
36. Moved `IContainer.ScopeContext` into `IResolverContext.ScopeContext`
37. Removed `IScopeContext.ScopeContextName`, you may provide your name instead
38. Removed `Container.NonAmbientRootScopeName`
39. Disposable services registered with `WeaklyReferenced` setup are no longer disposed.
Because the disposal in this case is optional anyway and the instance may be collected in any given time.
40. Removed `ImplicitOpenedRootScope`
41. Obsoleting `WithDefaultReuseInsteadOfTransient` replaced by `WithDefaultReuse`
42. Changed delegate parameters in `RegisterDelegate`, `RegisterInitializer` and `RegisterDisposer` to accept `IResolverContext` instead of `IResolver`
43. Obsoleting `WithAutoFallbackResolution` replaced by `WithAutoFallbackDynamicRegistration`
44. Moved `IContainer.With..` methods to `ContainerTools` extension methods
45. Obsoleting AutoFallback and ConcreteType resolution rules
46. Changed `PropertiesAndFields.All` to include `withBase` parameter
47. Obsoleting the `Reuse.InResolutionScope`
48. Removed `VerifyResolutions`. Replaced with `Validate` overloads.
49. Issue #287. Properties default injection policy is changed from `IfUnresolved.ReturnDefault` to 
`IfUnresolved.ReturnDefaultIfNotRegistered`
50. In `IRegister.Register` method parameter type `IfAlreadyRegistered` is changed to `IfAlreadyRegistered?`
52. Renamed `IRequest.RuntimeParent` to `IRequest.DirectParent`
53. Renamed `IContainer.GetOrAddStateItemExpression` to `GetConstantExpression`
54. Changed `Func<RequestInfo, bool>` conditions from APIs to full `Request`, `Func<RequestInfo, bool>`
55. Replacing `OpenScope(string name)` with `WithScope(IScope scope)` and making `openScope` just an extension method in `ResolverContext`
56. Removed `RequestInfo`
57. Changed `GenerateResolutionExpressions` argument
58. Moved `SetupAsResolutionRoot` to `ContainerTools`
59. `MaxObjectGraphSize` is replaced by `DependencyDepthToSplitObjectGraph`

