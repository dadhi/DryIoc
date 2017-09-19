# Breaking changes

## v3.0.0

1. Using C# 6 through codebase.
2. `IReuse` contents is replaced with `IReuseV3` contents, `IReuseV3` is removed.
3. Removed unused `compositeParentKey` and `compositeRequiredType` parameters from `IResolver.ResolveMany` both in DryIoc and DIZero
4. Removed `scope` parameter from FactoryDelegate due #288 both in DryIoc and DryIocZero
5. Removed `Rules.FallbackContainers`
6. `Container.CreateFacade` implementation is changed from the use of fallback containers to 
`rules.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(FacadeKey))`