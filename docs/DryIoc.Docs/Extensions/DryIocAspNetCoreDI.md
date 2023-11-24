# DryIoc.Microsoft.DependencyInjection

## General information

```
dotnet add package DryIoc.Microsoft.DependencyInjection
```

or the source code package

```
dotnet add package DryIoc.Microsoft.DependencyInjection.src
```


The [package](https://www.nuget.org/packages/DryIoc.Microsoft.DependencyInjection) provides the implementation and the replacement of [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) container with the DryIoc library.

Ultimately you will get an access to the [features](https://github.com/dadhi/DryIoc#features) and [speed](https://github.com/dadhi/DryIoc#realistic-scenario-with-the-unit-of-work-scope-and-object-graph-of-40-dependencies-4-levels-deep) of the DryIoc at the same time conforming to the MS.DI contract.

The best way to learn is to play with the samples in the *samples* folder:

- [ASP .NET Core 6.0 application](https://github.com/dadhi/DryIoc/blob/master/samples/DryIoc.AspNetCore.Sample/Program.cs#L25)
- [Minimal API application](https://github.com/dadhi/DryIoc/blob/master/samples/MinimalWeb/Program.cs)


## Conforming to the rules

To conform to the behavior of Microsoft.DependencyInjection the DryIoc applies a set of rules to the new or the **existing** container 
via `DryIocAdapter.WithMicrosoftDependencyInjectionRules` method.

Comparing to the default DryIoc rules the MS.DI rules differ in following:

Enabling:

- `TrackingDisposableTransients` - so you may register Transient services implmenting `IDisposable` 
- `SelectLastRegisteredFactory`  - when multiple registrations are found for the same service type, it selects the last registered
- `ConstructorWithResolvableArguments` - when multiple constructors are found, the one with the maximum parameters where all are resolvable is selected
  
Disabling:

- `ThrowOnRegisteringDisposableTransient` - simultaneously with enabling `TrackingDisposableTransients` it disables the error when `IDisposable` transient is registered
- `VariantGenericTypesInResolvedCollection` - disables the variant generic types in the resolved collection, so the collection will contain only the exact service type without the other covariant types

You may decide to add or remove other rules but be aware that the consumer side may be surprised when the conventions are not in place.
