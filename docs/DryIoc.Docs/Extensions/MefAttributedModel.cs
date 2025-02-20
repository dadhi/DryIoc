/*md
<!--Auto-generated from .cs file, the edits made to .md file will be lost! -->

# MefAttributedModel


- [MefAttributedModel](#mefattributedmodel)
  - [Overview](#overview)
  - [Comparing to MEF](#comparing-to-mef)
    - [Adds on top](#adds-on-top)
    - [Not supported](#not-supported)
  - [Concept](#concept)
    - [What is MefAttributedModel extension](#what-is-mefattributedmodel-extension)
    - [Relation with DryIoc.Attributes extension](#relation-with-dryiocattributes-extension)
    - [Export and Import attributes may be used separately](#export-and-import-attributes-may-be-used-separately)
    - [Assembly scan](#assembly-scan)
  - [Exporting](#exporting)
    - [MEF attributes](#mef-attributes)
      - [ExportAttribute](#exportattribute)
      - [InheritedExport](#inheritedexport)
    - [DryIoc.Attributes](#dryiocattributes)
      - [ExportEx](#exportex)
      - [ExportMany](#exportmany)
  - [CreationPolicy and Reuse](#creationpolicy-and-reuse)
  - [Importing](#importing)
    - [MEF attributes](#mef-attributes-1)
      - [Import](#import)
    - [DryIoc.Attributes](#dryiocattributes-1)
    - [ImportEx](#importex)
      - [ImportExternal](#importexternal)
  - [Metadata support](#metadata-support)
    - [WithMetadata](#withmetadata)
  - [Exporting disposable transient](#exporting-disposable-transient)
    - [AllowDisposableTransient](#allowdisposabletransient)
    - [TrackDisposableTransient](#trackdisposabletransient)
  - [Decorators and Wrappers](#decorators-and-wrappers)
    - [AsDecorator attribute](#asdecorator-attribute)
    - [AsWrapper attribute](#aswrapper-attribute)
  - [Other export options](#other-export-options)
    - [PreventDisposalAttribute](#preventdisposalattribute)
    - [WeaklyReferencedAttribute](#weaklyreferencedattribute)
    - [AsResolutionCall](#asresolutioncall)


## Overview

Enables the [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) in DryIoc.

## Comparing to MEF

### Adds on top

* Attributes for DryIoc specific reuse types: `CurrentScopeReuse`, `ResolutionScopeReuse`, `WebRequestReuse`, and `ThreadReuse`.
* Support for DryIoc wrappers and decorators: `AsWrapper`, `AsDecorator`.
* All wrappers supported: `IEnumerable<T>`, `T[]`, `Func<T>`, `KeyValuePair<K,V>`, etc. 
* Convention based registration with `ExportMany`.
* Easy export `AsFactory` to provide `Export` on methods and members.
* Simplifies Metadata usage with `ExportWithMetadata` and `ImportWithMetadata`.
* Supports compile-time assembly scan and types discovery to speed-up application startup.

### Not supported

* MEF catalog system. DryIoc implements its own assembly scan defined in `AttributedModel` class. Scan methods produce serializable DTOs with registration information.
* `RequiredCreationPolicyAttribute`.
* `ExportMetadata` is replaced with `ExportWithMetadata` instead, but use of custom exports  with `MetadataAttributeAttribute` is supported.
* Dynamic re-composition is not supported. You may use `LazyEnumerable<TService>` to be aware of newly registered services.


## Concept

### What is MefAttributedModel extension

__DryIoc.MefAttributedModel__ is the set of extension methods to support:

1. Configuring the dependency injection using the Import attributes
2. Registering the services into the Container using the Export attributes
3. Supports most of the DryIoc features with the extended set of attributes provided by `DryIocAttributes` package.

<details><summary>using ...</summary>

```cs md*/
namespace DryIoc.Docs;
using System;
using System.ComponentModel.Composition; // for the Export and Import attributes
using DryIocAttributes;                  // for the ExportEx and ExportMany attributes
using DryIoc.MefAttributedModel;
using DryIoc;
using NUnit.Framework;
/*md
```

</details>

```cs md*/
public class Basic_example
{
    [Test]
    public void Example()
    {
        // instructs to use the Import for DI when they found but it is not needed to use the Export
        var container = new Container().WithMefAttributedModel();

        // registers exported types
        container.RegisterExports(typeof(Foo), typeof(Bar));
        // or via assemblies
        // container.RegisterExports(new[] { typeof(Foo).GetAssembly() });

        // creates Foo with injected Bar
        var foo = container.Resolve<IFoo>();
        Assert.IsNotNull(foo);
    }

    public interface IFoo { }

    [Export(typeof(IFoo))]
    public class Foo : IFoo
    {
        public Foo([Import("some-key")] Bar bar) { }
    }

    [Export("some-key")]
    public class Bar { }
}
/*md
```

### Relation with DryIoc.Attributes extension

__DryIoc.MefAttributedModel__ depends on __DryIoc.Attributes__ which does two things:

1. Re-defines MEF Export and Import attributes for platforms without __System.ComponentModel.Composition__ support.
2. Extends set of attributes to support all DryIoc features: reuses, decorators, wrappers, arbitrary object service keys, etc.

_Why to separate attributes into its own assembly?_

To get rid off not used functionality. For instance, you can mark types for Exports with
 __DryIoc.Attributes__ and use [DryIocZero](Companions\DryIocZero) for DI. In that case you don't need
__DryIoc.MefAttributedModel__ functionality at runtime. All registrations will be scanned and factory delegates 
generated at compile-time by __DryIocZero__.

Another reason is that the set of features covered by __DryIoc.Attributes__ 
may be used by other IoC libraries without introducing their own attributes (ha-ha, it likely won't materialize).

### Export and Import attributes may be used separately 

That means I can register services into container manually as usual, 
but utilize the `Import` and `ImportingConstructor` attributes for dependency injection. 

Or other way around: you don't need to put `Export` attributes everyware to make advantage of Imports. 

``` md*/
public class Export_and_Import_used_separately
{
    [Test]
    public void Example()
    {
        var container = new Container().WithMefAttributedModel();

        container.Register<IFoo, Foo>(Reuse.Singleton);
        container.Register<Bar>(Reuse.Singleton, serviceKey: "some-key");

        var foo = container.Resolve<IFoo>();
        Assert.IsNotNull(foo);
    }

    // The types are without Exports
    public interface IFoo { }

    public class Foo : IFoo
    {
        public Foo([Import("some-key")] Bar bar) { }
    }

    public class Bar { }
}
/*md
```


### Assembly scan

__DryIoc.MefAttributedModel__ provides convention methods to scan assembly for all types marked with Export attributes 
and batch register them in DryIoc container.

```cs
    container.RegisterExports(assemblies);
```

The method actually works by:

- Scanning given assemblies and producing serializable DTOs with registration information.
- Iterating over collection of registration DTOs and registering each in container.

You can explicitly split these two steps:

```cs
    IEnumerable<ExportedRegistrationInfo> exports = container.Scan(assemblies);
    container.RegisterExports(exports);
```

__Note:__ Making `ExportedRegistrationInfo` serializable opens up possibility to __scan assemblies at compile-time__,
serialize the result, and then just de-serialize and register into container at run-time. 
Which should be generally faster than heavy-weight reflection scanning at run-time.


## Exporting

### MEF attributes

#### ExportAttribute
 
Allows to specify service type and service key, aka `ContractType` and `ContractName` in MEF terms.

```cs
md*/
public class Export_example
{
    [Test]
    public void Example()
    {
        // Using `WithMefAttributedModel` applies the MEF rules where the default reuse is singleton
        var container = new Container().WithMefAttributedModel();
        // alternatively you may apply just the rules
        container = new Container(rules => rules.WithMefAttributedModel());

        container.RegisterExports(
            typeof(A),
            typeof(B),
            typeof(C)
        );

        Assert.AreSame(container.Resolve<I>(), container.Resolve<J>());
    }

    public interface I { }
    public interface J { }

    [Export] // exports implementation A as service A
    public class A { }

    [Export(typeof(I))] // exports I and J to share the same implementation B, so that
    [Export(typeof(J))] // resolving I and J will return the same singleton object B
    [Export("xyz")]     // exports B with the service key "xyz", which also returns the same B
    public class B : I, J { }

    [Export("abc", typeof(I))] // exports С as a service I with the service key "abc"
    public class C : I { }

}
/*md
```

__Note:__ Using multiple exports on one implementation type will register the same factory for each export,
that means the same `I` and `J` singleton for exported singleton. 

#### InheritedExport

Allows to mark interface or base type as a service type once, and consider all the implementations as exported.
```cs
md*/
public class Using_InheritedExport
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.RegisterExports(typeof(A), typeof(B));

        Assert.IsNotNull(container.Resolve<I>());
        Assert.IsNotNull(container.Resolve<J>(serviceKey: "xyz"));
    }

    [InheritedExport]
    public interface I { }

    [InheritedExport("xyz")]
    public interface J { }

    // exported as a service I
    class A : I { }

    // exported as a service J with the service key "xyz"
    class B : J { }
}

/*md
```

### DryIoc.Attributes

Adds extended set of features supported by DryIoc
 
#### ExportEx

The difference from the normal MEF `ExportAttribute`, the extended DryIoc `ExportExAttribute` supports:

- `ContractKey` of arbitrary type instead of string only `ContractName`
- `IfAlreadyExported` option mapped to `DryIoc.IfAlreadyRegistered`

For example to ensure the _register-once_ semantics you can export type with the `IfAlreadyExported.Keep` option:
```cs
md*/
class DryIocAttributes_ExportEx
{
    [ExportEx(typeof(IService), IfAlreadyExported = IfAlreadyExported.Keep)]
    public class InOneFile : IService { }

    [ExportEx(typeof(IService), IfAlreadyExported = IfAlreadyExported.Keep)]
    public class InAnotherFile : IService { }

    public interface ICommandHandler { }

    // exports with the enumeration keys
    public enum CommandKey { Add, Delete }

    [ExportEx(CommandKey.Add, typeof(ICommandHandler))]
    public class AddCommandHandler : ICommandHandler { }

    [ExportEx(CommandKey.Delete, typeof(ICommandHandler))]
    public class DeleteCommandHandler : ICommandHandler { }
}
/*md
```

#### ExportMany

`ExportMany` maps directly to DryIoc [RegisterMany](..\RegisterResolve#registermany) method.

It allows to omit the `typeof(IService)` contract because it will be figured-out automatically by the `ExportMany`:

```cs
md*/
public class DryIocAttributes_ExportMany
{
    [Test]
    public void Example()
    {
        var c = new Container().WithMefAttributedModel(); // WithMefAttributedModel is not required for the Exports to work, it is required for the Imports

        c.RegisterExports(typeof(ServiceImpl));

        Assert.IsNotNull(c.Resolve<IService>());
        Assert.IsNull(c.Resolve<ServiceImpl>(ifUnresolvedReturnDefault: true));
    }

    public interface IService { }

    [ExportMany] // automatically discovers the `IService` interface for registration
    class ServiceImpl : IService { }
}
/*md
```

Additionally `ExportMany` provides the facilities to:

- Exclude some types from registration via `Except` property 
- Allow `NonPublic` types for registration

```cs
md*/
public class ExportMany_with_Except_and_NonPublic_options
{
    [Test]
    public void Example()
    {
        var c = new Container().WithMefAttributedModel();

        c.RegisterExports(typeof(X));

        Assert.IsNotNull(c.Resolve<X>());
        Assert.IsNotNull(c.Resolve<IA>());
        Assert.IsNull(c.Resolve<IB>(ifUnresolvedReturnDefault: true));
    }

    // Exports X, IA and IC, but not IB
    [ExportMany(NonPublic = true, Except = new[] { typeof(IB) })]
    class X : IA, IB { }

    interface IA { }
    public interface IB { }
}
/*md
```

## CreationPolicy and Reuse

__By default MEF treat all exports as Singletons__: It means that if you do not specify `PartCreationPolicy` attribute for the exported type, it will be registered as Singleton.

To register Transient  service you need to specify `PartCreationPolicy(CreationPolicy.NonShared)` attribute:

```cs
md*/
class Using_CreationPolicy
{
    [Export] // exports Singleton
    public class A { }

    [Export] // exports Transient
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class B { }

    [Export] // again Singleton but specified explicitly
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class C { }
}
/*md
```

DryIoc converts MEF `CreationPolicy` into the `DryIoc.IReuse` as following:

- CreationPolicy.Shared    -> Reuse.Singleton
- CreationPolicy.NonShared -> Reuse.Transient
- CreationPolicy.Any       -> Reuse.Singleton

Alternatively __DryIocAttributes__ library introduces the Reuse attributes to support the whole set of DryIoc reuses:

- TransientReuseAttribute    -> Reuse.Transient
- SingletonReuseAttribute    -> Reuse.Singleton
- CurrentScopeReuseAttribute -> Reuse.InCurrentScope. Supports the Scope name as attribute parameter.
- WebRequestReuseAttribute   -> Reuse.InWebRequest. Inherits from the CurrentScopeReuseAttribute.

Example:
```cs
md*/
class Using_Reuse_attribute
{
    [Export, TransientReuse]
    class A { }

    [Export, WebRequestReuse]
    class B { public B(A a) { } }

    [Export, CurrentScopeReuse("my-scope-name")]
    class C { public C(A a) { } }
}
/*md
```


## Importing

### MEF attributes

#### Import

All properties of `ImportAttribute` are supported by DryIoc: 

- `ContractName`, is mapped to the Service Key
- `ContractType`, is mapped to the [Required Service Type](..\RequiredServiceType)
- `AllowDefault`, if set is mapped to the `IfUnresolved.ReturnDefault` otherwise is mapped to the `IfUnresolved.Throw`

Example:
```cs md*/
public class Import_specification
{
    [Test]
    public void Example()
    {
        var container = new Container().WithMefAttributedModel();
        container.RegisterExports(typeof(A), typeof(B));
        var b = container.Resolve<B>();
        Assert.IsNotNull(b.A);
    }

    public interface IA { }

    [Export("some-key", typeof(IA))]
    public class A : IA { }

    [Export]
    public class B
    {
        public IA A { get; private set; }

        public B([Import("some-key", AllowDefault = true)] IA a) { A = a; }
    }
}
/*md
```

__Note:__ Again, you can just replace `RegisterExports` with the normal `Register`, and remove all exports, 
and example will still work.

### DryIoc.Attributes

### ImportEx

Inherits from `ImportAttribute` and adds ability to specify required service key of arbitrary type via `ContractKey` 
instead of string `ImportAttribute.ContractName`.

```cs md*/
class Using_ImportEx_attribute
{
    public enum Speed { Fast, Slow }

    [ExportMany(ContractKey = Speed.Fast)]
    public class A : I { }

    [ExportMany(ContractKey = Speed.Slow)]
    public class B : I { }

    [Export]
    public class C
    {
        public C([ImportEx(Speed.Fast)] I i) { /* will import i as A */ }
    }
}
/*md
```

#### ImportExternal

Imports the specified service normally if the service is registered. 
But in the case the service is not registered, attribute will __exports the service in-place__ for registration with provided implementation info.

This is useful for ad-hoc registration of types from not controlled libraries.
```cs md*/
class Using_ImportExternal
{
    // A third-party MEF-ignorant library
    class AwesomeService : IService { }

    // My library code
    [Export]
    public class NativeClient
    {
        [ImportExternal(typeof(IService), contractType: typeof(AwesomeService)), SingletonReuse]
        public IService Service { get; set; }
    }
}
/*md
```

In this example `AwesomeService` is not exported and not aware of MEF composition in your code.
But I can export the service in place of import, __without need for introducing the exporting proxy__.

In addition `ImportExternal` allows to specify:

- Constructor to be used for registration via `ConstructorSignature`
- Associated export `Metadata`


## Metadata support

### WithMetadata

Metadata has two goals:

- Allows to specify metadata object associated with exported implementation.
- Specifies the required metadata object for imported part, similar to MEF2 `RequiredMetadataAttribute`.

```cs md*/
class Using_WithMetadata
{
    [ExportMany]
    [WithMetadata("a")]
    public class X : I { }

    [ExportMany]
    [WithMetadata("b")]
    public class Y : I { }

    [Export]
    public class Client
    {
        public Client([WithMetadata("b")] I it) { /* will import it as Y */}
    }
}
/*md
```

## Exporting disposable transient

The attributes described below correspond to the specific [DryIoc registration options](..\ReuseAndScopes.md#disposable-transient).

By default exporting transient service implementing `IDisposable` will throw the exception,
until the container-wide rule is set `Rules.WithoutThrowOnRegisteringDisposableTransient`.

### AllowDisposableTransient

To prevent the exception for specific export you can mark it with `AllowDisposableTransientAttribute`:

```cs md*/
public class Exporting_disposable_transient
{
    [Test]
    public void Example()
    {
        var c = new Container().WithMefAttributedModel();

        // will throw if the export is not marked with the AllowDisposableTransient
        c.RegisterExports(typeof(Foo));

        using (var scope = c.OpenScope())
        {
            var foo = c.Resolve<Foo>();
            Assert.IsNotNull(foo);

            // disposing is the client responsibility
            foo.Dispose();
        }
    }

    [Export, AllowDisposableTransient]
    public class Foo : IDisposable
    {
        public void Dispose() { }
    }
}
/*md
```

__Note:__ Container won't track the disposable transient and its disposal is the client responsibility. 
To enable the tracking see the `TrackDisposableTransientAttribute` below.


### TrackDisposableTransient

Exported disposable transient marked with this attributed will be tracked by container and disposed on disposing the tracking scope.
The attribute corresponds to DryIoc registration option [trackDisposableTransient](..\ReuseAndScopes.md#disposable-transient)

```cs md*/
public class Exporting_with_TrackDisposableTransient
{
    [Export]
    [TrackDisposableTransient]
    public class Foo : IDisposable
    {
        public void Dispose() { }
    }

    [Test]
    public void Example()
    {
        var c = new Container().WithMefAttributedModel();

        c.RegisterExports(typeof(Foo));

        using (var scope = c.OpenScope())
        {
            var foo = c.Resolve<Foo>();
            Assert.IsNotNull(foo);
        } // foo will be disposed automatically by Container
    }
}
/*md
```

## Decorators and Wrappers

### AsDecorator attribute

Check the [Decorators](../Decorators.md#overview)

### AsWrapper attribute

Check the [Wrappers](../Wrappers.md#overview)

## Other export options

### PreventDisposalAttribute

Sets the registration option [preventDisposal](..\ReuseAndScopes.md#prevent-disposal-of-reused-service).

### WeaklyReferencedAttribute

Set the registration option [weaklyReferenced](..\ReuseAndScopes.md#weakly-referenced-reused-service).

### AsResolutionCall

Set the registration option [asResolutionCall](..\RulesAndDefaultConventions.md#injecting-dependency-asresolutioncall).
md*/
