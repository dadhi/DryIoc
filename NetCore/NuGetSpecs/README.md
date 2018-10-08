# DryIoc v3.0.0 Release Notes

## From user perspective

### OpenScope behavior changes

Now `OpenScope` is returning `IResolverContext` instead of full `IContainer`.

The consequence is that you won't be able to Register on returned object. 
But this is OK because even before, any registration done on scope was actually done on container.
This was confusing, cause someone may think that registration in scope is separate.

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

Note: It is still valid to call `UseInstance` and `InjectPropertiesAndFields` on the scope,
cause `IResolverContext` defines both methods. 


### No more ImplicitOpenedRootScope

This rule was added to conform to Microsoft.Extensions.DependencyInjection specification
to enable the resolution of scoped service both from scoped and the root container.
The latter means that the resolved service will be a singleton despite the fact it is registered as scoped.

The rule instructed the DryIoc to open scope on container creation and most importantly, to dispose this scope
together with container.

As of DryIoc v2.12 the new hybrid `Reuse.ScopedOrSingleton` was added, so you may not need to open the scope
to resolve such a service. This reuse means the `Rules.WithImplicitOpenedRootScope` is no longer neccessary.

Old code:
```
    var container = new Container(rules => rules.WithImplicitOpenedRootScope());

    container.Register<A>(Reuse.Scoped);
    
    container.Resolve<A>(); // Works, even without an open scope due the rule
```

New code:
```
    var container = new Container();

    container.Register<A>(Reuse.ScopedOrSingleton);
    
    container.Resolve<A>(); // Works, and much more clean given the service reuse
```


### Reuse.InResolutionScope changes

#### Resolution scope is no longer automatically created on Resolve

Previosly for any registered service the call to `Resolve` may create the scope associated 
with resolved service, as long the service had a dependency registered with `Reuse.InResolutionScope`.
Now it is no longer happen. The scope will be created only if resolved service is registered
with `setup: Setup.With(openResolutionScope: true)` option.

Old code:
```
    var container = new Container();
    
    container.Register<A>();
    container.Register<DepOfA>(Reuse.InResolutionScopeOf<A>());
    
    container.Resolve<A>(); // opens scope and DepOfA is successfully injected
```

New code:
```
    var container = new Container();
    
    container.Register<A>(setup: Setup.With(openResolutionScope: true));
    container.Register<DepOfA>(Reuse.ScopedTo<A>()); // the new syntax, old is still valid
    
    container.Resolve<A>(); // opens scope and DepOfA is successfully injected
```

#### InResolutionScope reuse now is just a Scoped reuse

Resolution scope reuse is the lifetime behavior accosiated with the node in service object graph.
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
    
    container.Register<A>(Reuse.ScopedTo(serviceKey: "X"));

    // resolution scope is just an open scope with the special name
    using (var scope = container.OpenScope(ResolutionScopeName.Of(serviceKey: "X")))
    {
        container.Resolve<A>(); // Works
    }
```


### RegisterDelegate parameter changes

`IResolver` parameter in `RegisterDelegate((IResolver resolver) => ...)` was extended by `IResolverContext`.
I said 'extended' because `IResolverContext` implements the `IResolver`. Because of this, there is a high chance
that your code will compile as before. 

Using `IResolverContext` in delegate will allow you to `OpenScope`, `UseInstance`, etc. without bringing the
correct container instance inside delegate.

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

### CreateFacade changes and no more FallbackContainers

`FallbackContainers` were not working fully and have a different un-expected issues. 
This feature was an orthogonal to the rest of DryIoc architecture, so I am happily removed it.

`CreateFacade` was implemented on top of `FallbackContainer` and allow to 'override' facaded container registrations.
This behavior for instance may be suitable in Tests to override prod service with test mock.

Now `CreateFacade` is just a sugar no top of `Rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey))`.

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

### WeaklyReferenced changes

`IDisposable` services registered with `setup: Setup.With(weaklyReferenced: true)` are no longer disposed.

The disposal was not guarantied even before, because the weakly referenced service may be garbage collected at any time.


## Full change list

1. Using C# 6 through codebase.
2. `IReuse` contents is replaced with `IReuseV3` contents, `IReuseV3` is removed.
3. Removed unused `compositeParentKey` and `compositeRequiredType` parameters from `IResolver.ResolveMany` both in DryIoc and DIZero
4. Removed `scope` parameter from `Resolve` and `ResolveMany`
4. Removed `state` and `scope` parameter from FactoryDelegate due #288 both in DryIoc and DryIocZero
5. Removed `Rules.FallbackContainers`
6. `Container.CreateFacade` implementation is changed from the use of fallback containers to 
`rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey))`
7. Removed `IScopeAccess` interface, replaced with `IResolverContext.OpenedScope` and extension methods.
8. Removed `ContainerWeakRef` implementation of `IResolverContext`. Now `IResolverContext` is implemented by `Container` itself.
9. Added `IReuse.Name` to support reuse name
10. Renamed `IContainer.ContainerWeakRef` into `IContainer.ResolverContext`
11. Removed `ContainerTools.GetCurrentScope` extension. It is replaced by `IResolverContext.CurrentScope`
12. Removed obsolete `IContainer.EmptyRequest` and `Request.CreateEmpty`
13. Removed obsolete `IContainer.ResolutionStateCache` and `IContainer.GetOrAddStateItem`
14. Removed obsolete `Request.ToRequestInfo`
15. Removed feature `outemost` parameter of `Reuse.InResolutionScopeOf`
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
42. Changed RegisterDelegate to accept Func{IResolverContext, object} instead of Func{IResolver, object}
43. Obsoleting `WithAutoFallbackResolution` replaced by `WithAutoFallbackDynamicRegistration`
44. Moved `IContainer.With..` methods to `ContainerTools` extension methods
45. Obsoleting AutoFallback and ConcreteType resolution rules
46. Changed `PropertiesAndFields.All` to include `withBase` parameter
47. Obsoleting the `Reuse.InResolutionScope`