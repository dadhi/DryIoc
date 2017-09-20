# Breaking changes

## v3.0.0

1. Using C# 6 through codebase.
2. `IReuse` contents is replaced with `IReuseV3` contents, `IReuseV3` is removed.
3. Removed unused `compositeParentKey` and `compositeRequiredType` parameters from `IResolver.ResolveMany` both in DryIoc and DIZero
4. Removed `scope` parameter from FactoryDelegate due #288 both in DryIoc and DryIocZero
5. Removed `Rules.FallbackContainers`
6. `Container.CreateFacade` implementation is changed from the use of fallback containers to 
`rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey))`
7. Removed `IScopeAccess` interface, replaced with `IResolverContext.OpenedScoped` and extension methods.
8. Removed `ContainerWeakRef` implementation of `IResolverContext`. Now `IResolverContext` is implemented by `Container` itself.
9. Added `IReuse.Name` to support reuse name
10. Renamed `IContainer.ContainerWeakRef` into `IContainer.ResolverContext`
11. Removed `ContainerTools.GetCurrentScope` extension. It is replaced by `IResolverContext.OpenedScope`
12. Removed obsolete `IContainer.EmptyRequest` and `Request.CreateEmpty`
13. Removed obsolete `IContainer.ResolutionStateCache` and `IContainer.GetOrAddStateItem`
14. Removed obsolete `Request.ToRequestInfo`
15. Removed `outemost` parameter of `Reuse.InResolutionScopeOf` 