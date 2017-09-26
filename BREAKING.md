# Breaking Changes

## v3.0.0

### User perspective

1. OpenScope behavior changes TODO
5. No more ImplicitOpenedRootScope TODO
2. RegisterDelegate changes TODO
4. CreateFacade changes TODO
3. WeaklyReferenced changes TODO

### Changes list

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