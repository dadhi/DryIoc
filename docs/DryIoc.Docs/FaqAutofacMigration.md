# FAQ - Migration from Autofac


- [FAQ - Migration from Autofac](#faq---migration-from-autofac)
  - [Autofac version](#autofac-version)
  - [Separate build stage](#separate-build-stage)
  - [Registration order](#registration-order)
  - [Auto-activated components](#auto-activated-components)
  - [IStartable](#istartable)
  - [Register AsImplementedInterfaces](#register-asimplementedinterfaces)
  - [Owned instances](#owned-instances)
  - [Using constructor with most resolvable parameters](#using-constructor-with-most-resolvable-parameters)
  - [Modules](#modules)
  - [IRegistrationSource and the dynamic registrations](#iregistrationsource-and-the-dynamic-registrations)


## Autofac version

Relevant to __Autofac v3.5.2__


## Separate build stage

By default DryIoc does not have separate build stage - you may resolve and then add new registrations at any time.

Autofac on the other hand has `ContainerBuilder` which accumulate registrations and then produce `Container` to resolve from. 

The __pros__ of DryIoc approach is to provide more flexibility and less API obstacles in using the container, especially when you need register later after resolve.

The __cons__ may be openness for later/external registrations that may override your initial setup. So you need to be careful when sharing container with plugins or other third-parties.

Other __cons__ may be to be aware when container is built, so to hook some additional logic to this event: e.g. [Auto-activated components](FaqAutofacMigration#markdown-header-auto-activated-).

__The question is:__ Does the Autofac provides the first __cons__ by guarding container from later changes? It seems no, because you may create new `ContainerBuilder`, put new registrations into it, and then update/mutate initial container.

On the other hand DryIoc provides the way to produce resolution-only container to share with third-parties:
```cs
    var resolutionOnlyContainer = container.WithNoMoreRegistrationAllowed();
    resolutionOnlyContainer.Register<A>(); // will throw ContainerException
```

or you may ignore later registrations:
```cs
    var resolutionOnlyContainer = container.WithNoMoreRegistrationAllowed(ignoreInsteadOfThrow: true);
    resolutionOnlyContainer.Register<A>(); // ignores registration - does nothing
```


## Registration order

Sometimes you need to get container registrations in determined order.
DryIoc has the way to get service registrations without actually resolving them:
```cs
    IEnumerable<ServiceRegistrationInfo> registrations = container.GetServiceRegistrations();
    foreach (var r in registrations) { /*...*/ }
```

`ServiceRegistrationInfo` contains: 

- `ServiceType` 
- `OptionalServiceKey` is `null` for single default registration, arbitrary object for keyed registration, and `DefaultKey` object for multiple defaults.
- `Factory` holds implementation details (__may be the same when single implementation registered with multiple services__). Includes:
    - `ImplementationType`, may be null
    - `Reuse`, e.g. `SingletonReuse`
    - `Setup`, e.g. `Setup.Metadata`
- `FactoryRegistrationOrder` is relative number identifying the order of Factory registration (__may be the same for multiple services__)

By default the return order is undetermined (internally ordered by Type hash code + Service Key hash code). The solution is to use LINQ `OrderBy` method:
```cs
    var registrations = container.GetServiceRegistrations()
        .OrderBy(r => r.FactoryRegistrationOrder);
    
    foreach (var r in registrations) { /*...*/ }
```


## Auto-activated components

[This Autofac feature](http://docs.autofac.org/en/latest/lifetime/startup.html#auto-activated-components) 
allows to automatically resolve and create specific service when Container is built. DryIoc does not have separate build stage. 
That means at any time you can get service registrations, filter specific services and resolve them to activate. 
For instance, let's mark activate-able services with Metadata and then create them as following:
```cs
    // Defining Metadata in extensible way:
    public abstract class Metadata
    {
        public class AutoActivated : Metadata
        {
            public static readonly AutoActivated It = new AutoActivated();
        }
    }
    
    // Configure registrations:
    container.Register<ISpecific, Foo>(setup: Setup.With(Metadata.AutoActivated.It));
    container.Register<INormal, Bar>();
    
    // Resolve to activate:
    var ignored = container.GetServiceRegistrations()
        .Where(r => r.Factory.Setup.Metadata is Metadata.AutoActivated)
        .OrderBy(r => r.FactoryRegistrationOrder)
        .GroupBy(r => r.FactoryRegistrationOrder, (f, r) => r.First())
        .Select(r => container.Resolve(r.ServiceType, r.OptionalServiceKey));
```

__Note:__ `GroupBy` required to activate single implementation (Factory) only once, because  single implementation may be registered with multiple services.

Metadata provides generic way to filter services. Alternatively you may go with convention, for instance auto-activate Singletons: `.Where(r => r.Factory.Reuse is SingletonReuse)`.


## IStartable

[Autofac Startable components](http://docs.autofac.org/en/latest/lifetime/startup.html#startable-components) are the same __auto-activated__ services with additional `Start` action executed on activation (and never on normal resolve). It is pretty easy to modify above example to filter `IStartable` interface instead of (or in addition to) Metadata:
```cs
    var ignored = container.GetServiceRegistrations()
        .Where(r => (r.Factory.ImplementationType ?? r.ServiceType).IsAssignableTo(typeof(IStartable)))
        .OrderBy(r => r.FactoryRegistrationOrder)
        .GroupBy(r => r.FactoryRegistrationOrder, (f, r) => r.First())
        .Select(r => ((IStartable)container.Resolve(r.ServiceType, r.OptionalServiceKey)).Start());
```


## Register AsImplementedInterfaces

Autofac has conventional method to register type implemented interfaces as service types. 

DryIoc provides a set of `RegisterMany` methods to register multiple service types for possibly multiple implementation types or assemblies. To achieve Autofac behavior you need to say:
```cs
    container.RegisterMany<FooBar>(serviceTypeCondition: type => type.IsInterface);
```

Condition will keep interface service types and skip _public_ base types and source type itself. 

__Note:__ General purpose interfaces like `IDisposable` will not be registered in any case.

To register _non-public_ service types modify previous example:
```cs
    container.RegisterMany<FooBar>(
        nonPublicServiceTypes: true,
        serviceTypeCondition: type => type.IsInterface);
```


## Owned instances

[The feature](http://docs.autofac.org/en/latest/advanced/owned-instances.html) implicitly opens scope for `Owned<TService>`. 
Disposing owned service will dispose the scope and also dispose its disposable dependencies. 
Using `Owned` is closely related to the fact that Autofac tracks `IDisposable` services even if they are Transient (`InstancePerDependency`).

Meanwhile DryIoc [does not track disposable transients by default](ReuseAndScopes#markdown-header-disposabletransient).
Instead DryIoc will throw exception on registering disposable transient.

To allow such registrations you need to explicitly use `allowDisposableTransient` setup option 
or use the container rule `WithoutThrowOnRegisteringDisposableTransient()`.
This option is similar to Autofac registration as `ExternallyOwned()`. 
That way you do not need to use `Owned` to control disposal.

To match default Autofac disposable tracking you need to use DryIoc `trackDisposableTransient` registration setup option
or specify the container rule `WithTrackingDisposableTransients()`.
Then instead of `Owned` in DryIoc, just wrap dependency in `Func`. This way the tracking will be prevented.
Using `Func` has a slight advantage over `Owned` because `Func` is not container specific type and does not
require DryIoc reference.

If you want the Autofac [InstancePerOwned](http://docs.autofac.org/en/stable/lifetime/instance-scope.html#instance-per-owned) 
to [reuse service in object sub-graph](ReuseAndScopes#markdown-header-reuseinresolutionscopeof) use:
```cs
    container.Register<SomeService>(setup: Setup.With(openResolutionScope: true));
```

To dispose dependencies instead of `Owned` just define `IDisposable` parameter and it will be automatically injected with current Resolution Scope.

Similar setups in Autofac and DryIoc:

In Autofac:
```cs
    class SomeService 
    {
        public SomeService(Dependency d) {}
    }
    
    class SomeClient : IDisposable
    {
        // Owned will open scope and provide access for its disposal
        public SomeClient(Owned<SomeService> owned) { _owned = owned; }
    
        // Will dispose scoped dependency and nested dependencies
        public void Dispose() { _owned.Dispose(); } 
    }
    
    // configure:
    builder.RegisterType<NestedDependency>().InstancePerOwned<SomeService>();
    builder.RegisterType<Dependency>().InstancePerOwned<SomeService>();
    builder.RegisterType<SomeService>();
    builder.RegisterType<SomeClient>();
```

In DryIoc:

```cs
    class SomeService 
    {
        public SomeService(Dependency d) {}
    }
    
    class SomeClient : IDisposable
    {
        // No need in Owned wrapper - Scope will be injected automatically
        public SomeClient(SomeService s, IDisposable scope) { _scope = scope; }
    
        // Will dispose scoped dependency and nested dependencies
        public void Dispose() { _scope.Dispose(); } 
    }
    
    // configure:
    container.Register<NestedDependency>(Reuse.InResolutionScopeOf<SomeService>());
    container.Register<Dependency>(Reuse.InResolutionScopeOf<SomeService>());
    container.Register<SomeService>(setup: Setup.With(openResolutionScope: true));
    container.Register<SomeClient>();
```


## Using constructor with most resolvable parameters

Autofac by default will use constructor with most resolvable parameters. So the `A` will be successfully resolved in example below:
```cs
    public class B {}
    public class C {}
    
    public class A
    {
        public A(B b) {}
        public A(C c) {}
    }
    
    // configure:
    var builder = new ContainerBuilder();
    builder.RegisterType<A>();
    builder.RegisterType<B>();
    // C is not registered.
    var container = builder.Build();
    
    container.Resolve<A>();
```

DryIoc on the other hand will expect type to have a single constructor. 
In case of multiple constructors available DryIoc will throw `ContainerException` with corresponding message. 
This behavior was selected as default, because it is more deterministic - you always know the way of instantiating your service.

But you may enable the Autofac behavior in DryIoc:

- per whole Container: 
```cs
    var container = new Container(rules => 
        rules.With(FactoryMethod.ConstructorWithResolvableArguments));
```

- per individual registration:

```cs
    var container = new Container();
    container.Register<A>(made: FactoryMethod.ConstructorWithResolvableArguments);
```

If you enable the feature per Container but provide some parameter specs on registration level, they will completely override the Container level setting:
```cs
    public class B {}
    public class C {}
    
    public class A
    {
        public bool IsCreatedWithB { get; private set; }
        public bool IsCreatedWithC { get; private set; }
    
        public A(B b) { IsCreatedWithB = true; }
        public A(C c) { IsCreatedWithC = true; }
    }
    
    // configure:
    var container = new Container(rules => 
    	rules.With(FactoryMethod.ConstructorWithResolvableArguments));
    
    container.Register(Made.Of(() => new A(Arg.Of<C>(IfUnresolved.ReturnDefault))));
    container.Register<B>();
    // No C registered
    var a = container.Resolve<A>();
    Assert.IsTrue(a.IsCreatedWithC); // Because registration level setting override container's
```


## Modules

Here the docs describing [Autofac Modules feature](http://docs.autofac.org/en/latest/configuration/modules.html).

They major responsibility of Module to be the unit of configuration and registration. Actually the Module plays the role of [Facade](https://en.wikipedia.org/wiki/Facade_pattern) to hide some related registrations behind.

Here is the example of module definition and usage in Autofac:
```cs
    // Here is the AModule
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<A>().SingleInstance();
        }
    }

    // And using it
    var builder = new ContainerBuilder();

    builder.RegisterModule<AModule>(); // registers module with some registrations inside
    builder.RegisterType<B>();

    var container = builder.Build();

    var a = container.Resolve<A>();
    Assert.IsInstanceOf<B>(a.B);
```

And here the equivalent in DryIoc without use of any additional abstractions except for `IModule` interface for conformity:
```cs
    public interface IModule
    {
        // Here we are using registration role of DryIoc Container for the builder
        void Load(IRegistrator builder);
    }

    public class DryIocModule : IModule
    {
        public void Load(IRegistrator builder)
        {
            builder.Register<BB>(Reuse.Singleton);
        }
    }

    // And the use is straightforward
    var container = new Container();

    container.RegisterMany<AModule>();
    container.Register<B>();

    // Resolve all registered modules and call Load on them
    foreach (var module in container.ResolveMany<IModule>())
        module.Load(container);

    var a = container.Resolve<A>();
    Assert.IsInstanceOf<B>(a.B);
```

## IRegistrationSource and the dynamic registrations

[The related case](https://github.com/dadhi/DryIoc/issues/143)

Example of the dynamic registration of the types started with the `Asp` prefix:

```cs
    IEnumerable<DynamicRegistration> GetAspTypes(Type serviceType, object serviceKey)
    {
        if (serviceType.Namespace.StartsWith("ASP", true, CultureInfo.InvariantCultureIgnoreCase))
            return new[] { new DynamicRegistration(new ReflectionFactory(serviceType), IfAlreadyRegistered.Keep) };
        return null;
    }

    container = container.With(rules => rules.WithDynamicRegistrations(GetAspTypes));
```
