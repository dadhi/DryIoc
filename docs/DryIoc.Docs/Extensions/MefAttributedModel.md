# MefAttributedModel extension

Enables [MEF Attributed Programming Model](http://msdn.microsoft.com/en-us/library/ee155691(v=vs.110).aspx) in DryIoc.

[TOC]

## Comparing to MEF

### Works without System.ComponentModel.Composition

* The standard MEF attributes are re-defined in _Ported-net40.cs_.
* Additional DryIoc attributes are define in _Attributes.cs_.


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
* Dynamic re-composition and _Satisfied_ events are not supported. You may use `LazyEnumerable<TService>` to be aware of newly registered services.


## Concept

### What is MefAttribitedModel extension

__DryIoc.MefAttibutedModel__ is the set of extension methods to support:

1. Configuring dependency injection with DryIoc container using Import attributes
2. Registration of services into DryIoc container by convention using Export attributes

```
#!c#
    public interface IFoo {}

    [Export(typeof(IFoo))]
    public class Foo : IFoo 
    {
        public Foo([Import("some-key")]Bar bar) {}
    }

    [Export("some-key")]
    public class Bar {}

    
    // in composition root:

    // instructs to use Imports for DI when they found
    var container = new Container().WithMefAttributedModel(); 
        
    // registers exported types
    container.RegisterExports(typeof(Foo), typeof(Bar));
    // or via assemblies
    // container.RegisterExports(new[] { typeof(Foo).GetTypeInfo().Assembly });

    // creates Foo with injected Bar
    var foo = container.Resolve<IFoo>();
```

### Relation with DryIoc.Attributes extension

__DryIoc.MefAttibutedModel__ depends on __DryIoc.Attributes__ which does two things:

1. Re-defines MEF Export and Import attributes for platforms without __System.ComponentModel.Composition__ support.
2. Extends set of attributes to support all DryIoc features: reuses, decorators, wrappers, arbitrary object service keys, etc.

_Why to separate attributes into its own assembly?_

To get rid off not used functionality. For instance, you can mark types for Exports with
 __DryIoc.Attributes__ and use [DryIocZero](Companions\DryIocZero) for DI. In that case you don't need
__DryIoc.MefAttibutedModel__ functionality at runtime. All registrations will be scanned and factory delegates 
generated at compile-time by __DryIocZero__.

Another reason, though unlikely matirialized one. I believe that set of features covered by __DryIoc.Attributes__ 
may be used by other IoC libraries without introducing their own attributes.

### Export and Import attributes may be used separately 

That means I can register services into container manually as usual, 
but utilize the `Import` and `ImportingConstructor` attributes for dependency injection. 

Or other way around: you don't need to put `Export` attributes everyware to make advantage of Imports. 
```
#!c#
    // Types without exports:    

    public interface IFoo {}

    public class Foo : IFoo 
    {
        public Foo([Import("some-key")]Bar bar) {}
    }

    public class Bar {}


    // composition root:
    var container = new Container().WithMefAttributedModel(); 
    
    container.Register<IFoo, Foo>(Reuse.Singleton);
    container.Register<Bar>(Reuse.Singleton, serviceKey: "some-key");

    var foo = container.Resolve<IFoo>();
```

### Assembly scan

__DryIoc.MefAttributedModel__ provides convention methods to scan assembly for all types marked with Export attributes 
and batch register them in DryIoc container.
```
#!c#
    container.RegisterExports(assemblies);
```

The method actually works by:

- Scanning given assemblies and producing serializable DTOs with registration information.
- Iterating over collection of registration DTOs and registering each in container.

You can explicitly split these two steps:
```
#!c#
    IEnumerable<ExportedRegistrationInfo> exports = container.Scan(assemblies);   
    container.RegisterExports(exports);
```

__Note:__ Making `ExportedRegistrationInfo` serializable opens up possiblity to __scan assemblies at compile-time__,
serialize the result, and then just de-serialize and register into container at run-time. 
Which should be generally faster than heavy-weight reflection scanning at run-time.


## Exporting

### MEF attributes

#### ExportAttribute
 
Allows to specify service type and service key, aka `ContractType` and `ContractName` in MEF terms.
```
#!c#
    [Export] // exports implementation A as service A
    public class A {}

    [Export(typeof(I))] // exports implementation A as service I
    public class A : I {}

    [Export("some-key", typeof(I))] // exports implementation A as service I with key "some-key"
    public class A : I {}

    // exports I, J, and A as self with key "xyz"
    [Export(typeof(I))]
    [Export(typeof(J))]
    [Export("xyz")]
    public class B : I, J {}

    // the exporting B as I and J will share the same implementation factory, 
    // that measn the same instance for singleton
    Assert.AreSame(container.Resolve<I>(), container.Resolve<J>()); 
```

__Note:__ Using multiple exports on one implementation type will register the same factory for each export,
that means the same `I` and `J` singleton for exported singleton. 

#### InheritedExport

Allows to mark interface or base type as a service type once, and consider all the implementations as exported.
```
#!c#
    [InheritedExport]
    public interface I {}

    [InheritedExport("xyz")]
    public interface J {}

    // exported as service I
    class A : I {}

    // exported as service I and service J with key "xyz"
    class B : I, J {}

    // composition root
    container.RegisterExports(typeof(A), typeof(B));
    
    var j = container.Resolve<I>();
    var j = container.Resolve<J>(serviceKey: "xyz");
```

### DryIoc.Attributes

Adds extended set of features supported by DryIoc
 
#### ExportEx

The difference from the normal MEF `ExportAttribute`, the extended DryIoc `ExportExAttribute` supports:

- `ContractKey` of arbitrary type instead of string only `ContractName`
- `IfAlreadyExported` option mapped to `DryIoc.IfAlreadyRegistered`

For example, to ensure _register-once_ functionality you can export with `IfAlreadyExported.Keep` option:
```
#!c#
    [ExportEx(typeof(IService), IfAlreadyExported=IfAlreadyExported.Keep)]
    public class InOneFile : IService {} 

    [ExportEx(typeof(IService), IfAlreadyExported=IfAlreadyExported.Keep)]
    public class InAnotherFile : IService {} 
```

To export with enumeration key:
```
#!c#
    public enum CommandKey { Add, Delete }

    [ExportEx(CommandKey.Add, typeof(ICommandHandler))]
    public class AddCommandHandler : ICommandHandler {} 

    [ExportEx(CommandKey.Delete, typeof(ICommandHandler))]
    public class DeleteCommandHandler : ICommandHandler {} 
```

#### ExportMany

`ExportMany` maps directly to DryIoc [RegisterMany](..\RegisterResolve#markdown-header-registermany) method.

It allows to omit specifying `typeof(IService)` contract, because it will be firgure out automatically by `ExportMany`:
```
#!c#
    public interface IService {}

    // automagically discovers the interface for registration 
    [ExportMany]
    class ServiceImpl : IService {}

    var c = new Container().WithMefAttributedModel();
    c.RegisterExports(typeof(ServiceImpl));

    c.Resolve<IService>();
```

Additionally `ExportMany` provides facilities to:

- Exclude some types from registration via `Except` property 
- Allow `NonPublic` types for registration
```
#!c#
    // Exports X, IA and IC, but not IB

    [ExportMany(NonPublic=true, Except=new[] { typeof(IB) })]
    class X : IA, IB, IC {}
```


## CreationPolicy and Reuse

__By default MEF treat all exports as Singletons__: It means that if you do not specify `PartCreationPolicy` attribute for the exported type, it will be registered as Singleton.

To register Transient  service you need to specify `PartCreationPolicy(CreationPolicy.NonShared)` attribute:
```
#!C#
   
    [Export] // exports Singleton
    public class A {}

    [Export] // exports Transient
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class B {}

    [Export] // again Singleton but specified explicitly
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class C {}
```

DryIoc converts MEF `CreationPolicy` to its own `IReuse` as following:

- CreationPolicy.Shared -> Reuse.Singleton
- CreationPolicy.NonShared -> Reuse.Transient
- CreationPolicy.Any -> Reuse.Singleton

Alternatively __DryIocAttributes__ library introduces its own Reuse attributes to support whole set of DryIoc reuses:

- TransientReuseAttribute -> Reuse.Transient
- SingletonReuseAttribute -> Reuse.Singleton
- CurrentScopeReuseAttribute -> Reuse.InCurrentScope. Supports the Scope name as attribute parameter.
- WebRequestReuseAttribute -> Reuse.InWebRequest. Inherits from the CurrentScopeReuseAttribute.

Example:
```
#!c#
    [Export, TransientReuse]
    class A {}

    [Export, WebRequestReuse]
    class B { public B(A a) {} }

    [Export, CurrentScopeReuse("my-scope-name")]
    class C { public C(A a) {}}
```


## Importing

### MEF attributes

#### Import

All properties of `ImportAttribute` are supported by DryIoc: 

- `ContractName`, is mapped to Service Key
- `ContractType`, is mapped to [Required Service Type](..\RequiredServiceType)
- `AllowDefault`, if set is mapped to `IfUnresolved.ReturnDefault`, otherwise is `IfUnresolved.Throw`

Example:
```
#!c#
    [Export("some-key", typeof(IA))]
    public class A : IA {}

    [Export]
    public class B 
    {
        public IA A { get; private set; }

        public B([Import("some-key", AllowDefault=true)]IA a) { A = a; }
    }

    // in composition root
    var container = new Container().WithMefAttributedModel();
    container.RegisterExports(typeof(A), typeof(B));
    var b = container.Resolve<B>();
    Assert.IsNotNull(b.A);
```

__Note:__ Again, you can just replace `RegisterExports` with the normal `Register`, and remove all exports, 
and example will still work.

### DryIoc.Attributes

### ImportEx

Inherits from `ImportAttribute` and adds ability to specify required service key of arbitrary type via `ContractKey` 
instead of string `ImportAttribite.ContractName`.
```
#!c#
    public enum Speed { Fast, Slow }

    [ExportMany(ContractKey=Speed.Fast)]
    public class A : I {}

    [ExportMany(ContractKey=Speed.Slow)]
    public class B : I {}

    [Export]
    public class B 
    {
        public B([ImportWithKey(Speed.Fast)]I i) { /* will import i as A */ }
    }
```

#### ImportExternal

Imports the specified service normally if the service is registered. 
But in case the service is not registered, attribute will __exports the service in-place for registration__ with provided implementation info.

This is useful for ad-hoc registration of types from not controlled libraries.
```
#!c#
    // third-party MEF ignorant library 
    class AwesomeService : IService {}

    // My code
    [Export]
    public class NativeClient
    {
        [ImportExternal(typeof(IService), contractType: typeof(AwesomeService)), SingletonReuse]
        public IService Service { get; set; }
    }
```

In this example `AwesomeService` is not exported and not aware of MEF composition in your code.
But I can export the service in place of import, __without need for introducing the exporting proxy__.

In addition `ImportExternal` allows to specify:

- Constructor to be used for registration via `ConstructorSignature`
- Associated export `Metadata`


## Metadata support

### WithMetadata

Have two goals:

- Allows to specify metadata object associated with exported implementation.
- Specifies the required metadata object for imported part, similar to MEF2 `RequiredMetadataAttribute`.

```
#!c#
    [ExportMany][WithMetadata("a")]
    public class X : I {}

    [ExportMany][WithMetadata("b")]
    public class Y : I {}

    [Export]
    public class Client 
    {
        public Client([WithMetadata("b")]I it) { /* will import it as Y */}
    }
```

## Exporting disposable transient

The attributes described below correspond to the specific [DryIoc registration options](..\ReuseAndScopes#markdown-header-disposable-transient).

By default exporting transient service implementing `IDisposable` will throw the exception,
until the container-wide rule is set `Rules.WithoutThrowOnRegisteringDisposableTransient`.

### AllowDisposableTransient

To prevent the exception for specific export you can mark it with `AllowDisposableTransientAttribute`:

```
#!c#
    [Export][AllowDisposableTransient]
    public class Foo : IDisposable {}

    var c = new Container().WithMefAttributedModel();

    // will throws until the export is marked with AllowDisposableTransient
    c.RegisterExports(typeof(Foo));

    using (var scope = c.OpenScope())
    {
        var foo = c.Resolve<Foo>();
        
        // disposing is the client responsibility
        foo.Dispose();
    }
```

__Note:__ Container still won't be tracking the disposable transient, and disposing it is consumer responsiblity. 
To enable tracking use `TrackDisposableTransientAttribute`.


### TrackDisposableTransient

Exported disposable transient marked with this attributed will be tracked by container and disposed on disposing the tracking scope.
The attribute corresponds to DryIoc registration option [trackDisposableTransient](..\ReuseAndScopes#markdown-header-disposable-transient)

```
#!c#
    [Export][TrackDisposableTransient]
    public class Foo : IDisposable {}

    var c = new Container().WithMefAttributedModel();

    // will throws until the export is marked with AllowDisposableTransient
    c.RegisterExports(typeof(Foo));

    using (var scope = c.OpenScope())
    {
        var foo = c.Resolve<Foo>();

    }   // foo will be disposed automatically by Container on tracking scope disposal
```

## Decorators and Wrappers

### AsDecorator



### AsWrapper



## Other export options

### PreventDisposalAttribute

Sets the registration option [preventDisposal](..\ReuseAndScopes#markdown-header-prevent-disposal-of-reused-service).

### WeaklyReferencedAttribute

Set the registration option [weaklyReferenced](..\ReuseAndScopes#markdown-header-weakly-referenced-reused-service).

### AsResolutionCall

Set the registration option [asResolutionCall](..\RulesAndDefaultConventions#markdown-header-injecting-dependency-asresolutioncall).
