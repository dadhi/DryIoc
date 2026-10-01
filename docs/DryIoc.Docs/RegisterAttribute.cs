/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# RegisterAttribute


- [RegisterAttribute](#registerattribute)
  - [Overview](#overview)
  - [Basic registration](#basic-registration)
  - [Reuse, keys, and IfAlreadyRegistered](#reuse-keys-and-ifalreadyregistered)
  - [RegisterMany and multiple service types](#registermany-and-multiple-service-types)
  - [Setup flags](#setup-flags)
  - [Decorators and wrappers](#decorators-and-wrappers)
  - [Conditions and metadata](#conditions-and-metadata)
  - [Factory methods](#factory-methods)
  - [Constructor selection (Made)](#constructor-selection-made)
  - [What is intentionally not covered](#what-is-intentionally-not-covered)


## Overview

`RegisterAttribute` is DryIoc's native, MEF-free attribute model for declarative registrations.
Apply it on implementation types or on factory methods/properties/fields, then call:

```cs
container.RegisterByRegisterAttributes(typeof(MyRegistrations));
// or
container.RegisterByRegisterAttributes(typeof(MyServiceImpl));
```

It maps to the same concepts as `Register` / `RegisterMany`:
service/implementation types, `IReuse`, `Made` (constructor or factory member), `Setup`, `IfAlreadyRegistered`, and `serviceKey`.

Compared to MEF `Export` attributes (`DryIocAttributes` + `DryIoc.MefAttributedModel`), `RegisterAttribute`
lives in the core `DryIoc` assembly and is designed for compile-time / source-generator scenarios as well as runtime use.

## Basic registration

Place attributes on a configuration class, or directly on the implementation:
```cs md*/
namespace DryIoc.Docs;

using System;
using DryIoc;
using NUnit.Framework;

public class RegisterAttribute_basic
{
    public interface IService { }
    public class Service : IService { }

    // Config-class style: both service and implementation types are explicit
    [Register(typeof(IService), typeof(Service), ReuseAs.Singleton)]
    public static class Config { }

    // Inline style: implementation type is the attributed type
    [Register(typeof(IService), ReuseAs.Scoped)]
    public class ScopedService : IService { }

    // Both types inferred from the target class
    [Register(ReuseAs.Transient)]
    public class SelfService { }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Config));
        c.RegisterByRegisterAttributes(typeof(ScopedService));
        c.RegisterByRegisterAttributes(typeof(SelfService));

        Assert.IsInstanceOf<Service>(c.Resolve<IService>());
        using var scope = c.OpenScope();
        Assert.IsInstanceOf<ScopedService>(scope.Resolve<IService>());
        Assert.IsNotNull(c.Resolve<SelfService>());
    }
} /*md
```

Generic variants provide compile-time type checking. They require **.NET 7+**
(generic attributes are not supported by .NET Framework / older runtimes — reflection throws
`NotSupportedException: Generic types are not valid`). On older targets use the non-generic form above.

```cs
// .NET 7+ only
[Register<IService, Service>(ReuseAs.Singleton)]
[Register<SelfService>]
public static class TypedConfig { }
```

## Reuse, keys, and IfAlreadyRegistered

Supported `ReuseAs` values: `ContainerRulesDefaultReuse`, `Transient`, `Singleton`, `Scoped`,
`ScopedToService`, `ScopedOrSingleton`. Named scopes use `ReuseScopeName` / `ReuseScopeNames`.
Custom `IReuse` implementations use `CustomReuseType`.

```cs md*/
public class RegisterAttribute_reuse_and_key
{
    public interface ISvc { }
    public class Svc : ISvc { }

    [Register(typeof(ISvc), typeof(Svc), ReuseAs.Scoped, ReuseScopeName = "api", ServiceKey = "main")]
    public static class Config { }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Config));

        using var scope = c.OpenScope("api");
        var s = scope.Resolve<ISvc>(serviceKey: "main");
        Assert.IsInstanceOf<Svc>(s);
    }
} /*md
```

## RegisterMany and multiple service types

```cs md*/
public class RegisterAttribute_register_many
{
    public interface IA { }
    public interface IB { }
    public class Impl : IA, IB { }

    // Discovers sensible public service types (same idea as RegisterMany)
    [Register(typeof(Impl), typeof(Impl), RegisterMany = true, Except = new[] { typeof(IB) })]
    public static class ManyConfig { }

    // Or list service types explicitly
    [Register(typeof(Impl), typeof(Impl), ServiceTypes = new[] { typeof(IA), typeof(IB) })]
    public static class ExplicitConfig { }

    [Test]
    public void Example_many()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(ManyConfig));
        Assert.IsInstanceOf<Impl>(c.Resolve<IA>());
        Assert.Throws<ContainerException>(() => c.Resolve<IB>());
    }

    [Test]
    public void Example_explicit()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(ExplicitConfig));
        Assert.IsInstanceOf<Impl>(c.Resolve<IA>());
        Assert.IsInstanceOf<Impl>(c.Resolve<IB>());
    }
} /*md
```

## Setup flags

Boolean / value properties map 1:1 to `Setup.With` / `Setup.DecoratorWith` / `Setup.WrapperWith`:

| Property | Setup |
|---|---|
| `OpenResolutionScope` | `openResolutionScope` |
| `AsResolutionCall` | `asResolutionCall` |
| `AsResolutionRoot` | `asResolutionRoot` |
| `PreventDisposal` | `preventDisposal` |
| `WeaklyReferenced` | `weaklyReferenced` |
| `UseParentReuse` | `useParentReuse` |
| `PreferInSingleServiceResolve` | `preferInSingleServiceResolve` |
| `AvoidResolutionScopeTracking` | `avoidResolutionScopeTracking` |
| `DisposalOrder` | `disposalOrder` |
| `TrackDisposableTransient` | allow / track / throw disposable transient |

```cs md*/
public class RegisterAttribute_setup_flags
{
    public class DisposableSvc : IDisposable
    {
        public bool IsDisposed;
        public void Dispose() => IsDisposed = true;
    }

    [Register(typeof(DisposableSvc), typeof(DisposableSvc),
        ReuseAs.Singleton, PreventDisposal = true)]
    public static class Config { }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Config));
        var s = c.Resolve<DisposableSvc>();
        c.Dispose();
        Assert.IsFalse(s.IsDisposed);
    }
} /*md
```

## Decorators and wrappers

Set `FactoryType = FactoryType.Decorator` or `FactoryType.Wrapper`.
Decorator-specific: `DecoratorOrder`, `UseDecorateeReuse`, `DecorateesType`, `DecorateeServiceKey`.
Wrapper-specific: `WrappedServiceTypeArgIndex`, `AlwaysWrapsRequiredServiceType`.

```cs md*/
public class RegisterAttribute_decorators
{
    public interface IHandler { string Name { get; } }
    public class Handler : IHandler { public string Name => "base"; }
    public class LoggingHandler : IHandler
    {
        private readonly IHandler _inner;
        public LoggingHandler(IHandler inner) => _inner = inner;
        public string Name => "log:" + _inner.Name;
    }

    [Register(typeof(IHandler), typeof(Handler))]
    [Register(typeof(IHandler), typeof(LoggingHandler),
        FactoryType = FactoryType.Decorator, UseDecorateeReuse = true)]
    public static class Config { }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Config));
        Assert.AreEqual("log:base", c.Resolve<IHandler>().Name);
    }
} /*md
```

## Conditions and metadata

Implement `RegisterConditionAttribute` and set `ConditionType`. Use `Metadata` for `Meta<,>` wrappers.

```cs md*/
public class RegisterAttribute_condition_and_metadata
{
    public interface IFeature { }
    public class FeatureA : IFeature { }
    public class FeatureB : IFeature { }

    public class RootOnly : RegisterConditionAttribute
    {
        public override bool Evaluate(Request request) => request.DirectParent.IsEmpty;
    }

    [Register(typeof(IFeature), typeof(FeatureA), ConditionType = typeof(RootOnly))]
    [Register(typeof(IFeature), typeof(FeatureB), IfAlreadyRegistered = RegisterIfAlready.AppendNotKeyed)]
    [Register(typeof(IFeature), typeof(FeatureA), ServiceKey = "meta", Metadata = "A")]
    public static class Config { }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Config));
        // Condition applies to the default (unkeyed) FeatureA registration at the resolution root
        Assert.IsInstanceOf<FeatureA>(c.Resolve<IFeature>());
        // Metadata is available via the Meta<,> wrapper for the keyed registration
        Assert.AreEqual("A", c.Resolve<Meta<IFeature, string>>(serviceKey: "meta").Metadata);
    }
} /*md
```

## Factory methods

`RegisterAttribute` may be placed on static or instance methods, properties, or fields.
Instance members cause the declaring type to be registered so it can act as the factory.

```cs md*/
public class RegisterAttribute_factory_methods
{
    public interface IClock { DateTime UtcNow { get; } }
    public class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }

    public static class Factories
    {
        [Register(typeof(IClock), ReuseAs.Singleton)]
        public static IClock CreateClock() => new SystemClock();
    }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Factories));
        Assert.IsInstanceOf<SystemClock>(c.Resolve<IClock>());
    }
} /*md
```

## Constructor selection (Made)

`FactoryMethod` selects the same constructor strategies as `Register(..., made: FactoryMethod....)`:

- `MadeFactoryMethod.Default` — container rules
- `MadeFactoryMethod.ConstructorWithResolvableArguments`
- `MadeFactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic`

```cs md*/
public class RegisterAttribute_constructor_selection
{
    public interface IDep { }
    public class Dep : IDep { }

    public class Consumer
    {
        public string Via;
        public Consumer() => Via = "default";
        public Consumer(IDep dep) => Via = "with-dep";
    }

    [Register(typeof(IDep), typeof(Dep))]
    [Register(typeof(Consumer), typeof(Consumer),
        FactoryMethod = MadeFactoryMethod.ConstructorWithResolvableArguments)]
    public static class Config { }

    [Test]
    public void Example()
    {
        var c = new Container();
        c.RegisterByRegisterAttributes(typeof(Config));
        Assert.AreEqual("with-dep", c.Resolve<Consumer>().Via);
    }
} /*md
```

## What is intentionally not covered

Attribute registration targets **type-based** (and factory-member) registrations. These imperative APIs stay code-only:

- `RegisterInstance` / `Use` — needs a live instance
- `RegisterDelegate` — needs a delegate body
- `RegisterPlaceholder` — placeholder without implementation
- `RegisterMapping` — maps to an existing factory by key
- `RegisterInitializer` / `RegisterDisposer` — callback-based
- `Setup.WrapperWith(unwrap: ...)` custom unwrap function — not expressible as attribute data
- Dynamic `Made.Of(() => ...)` expression trees beyond constructor selectors and factory members

For MEF-style `Import` injection into constructors/properties, continue using `DryIoc.MefAttributedModel`.

md*/
