# Register and Resolve

[TOC]

## DryIoc Glossary

Given example setup:
```
#!c#
    public interface IService { }
    
    public class SomeService : IService { }
    
    public interface IClient { IService Service { get; } }
    
    public class SomeClient : IClient
    {
        public IService Service { get; private set; }
        public SomeClient(IService service) { Service = service; }
    }
```

- __Resolution Root__ is the service object resolved from container by calling `Resolve` method, 
e.g. in `var client = container.Resolve<IClient>();` `client` is Resolution Root.
- __Injected Dependency__ is argument of type `IService` passed (injected) to constructor of `SomeClient`.
    It will be automatically created by container and injected into client constructor when resolving `IClient` Resolution Root. 
    When injecting dependency you do not need to call container directly, so no need to store container object for that.
    _If not said otherwise Resolution and Injection will be used in the same sense of retrieving object from container._

    - __Constructor Injection__: Injecting dependencies in constructor parameters. 
        __It is preferable way__ because all dependencies are visible in single place - constructor. 
        If constructor parameters are too many, that is indication of [God object anti-pattern](http://en.wikipedia.org/wiki/God_object) 
        and violation of [Single Responsibility principle](http://en.wikipedia.org/wiki/Single_responsibility_principle). 
        Time to split your class into multiple cohesive units.

    - __Property/Field Injection__: Injecting dependencies as properties and fields of the class. Because properties may
        be set not only in constructor but in any code at any time, it is harder to track and conclude about all class
        dependencies. __Therefore use it only as last-resort and strife for Constructor Injection as much as possible.__ 

    DryIoc supports both Constructor and Property/Field injection.

* __Implementation Type__ is actual type used for service creation, e.g. `SomeClient` and `SomeService`. 
    In DryIoc Implementation Type may be open-generic: `OtherService<>`.

* __Service Type__ is usually abstract or interface type using for service location and dependency injection, e.g. `IClient` and `IService`.
    Service Type should be assignable from Implementation Type. It is possible to have multiple Service Types for single Implementation Type as it may
    implement multiple interfaces. In addition you can use Implementation Type itself as Service Type, so the one type is used for injection and creation. 
    In DryIoc Service Type may be open-generic: `IService<>`.


## Registration API

For example above DryIoc supports registration with provided implementation and service types:
```
    #!c#
    container.Register<IClient, SomeClient>();
    container.Register<IService, SomeService>();
```

__Note:__ The order of registration is not important.

If you don't know types in compile-time you can specify run-time `Type` objects:
```
#!c#
    container.Register(typeof(IClient), typeof(SomeClient));
    container.Register(typeof(IService), typeof(SomeService));
```

Container will check if service type is assignable to implementation type and will throw exception otherwise.

You can register open-generic the same way:

`container.Register(typeof(IService<>), typeof(OtherService<>));`

Implementation type may be used as service type itself:
```
#!c#
    container.Register<SomeClient>();
    container.Register(typeof(SomeClient));
```


## Registering as Singleton

The setup above is simple. What if `IService` is Singleton? Singleton means
that the same `IService` instance should be reused during container lifetime - usually it means the same an Application lifetime. 

DryIoc supports Singletons via concept of [Reuse and Scopes](ReuseAndScopes). 

Example of singleton service registration:

```
#!c#
    c.Register<IClient, SomeClient>();
    c.Register<IService, SomeService>(Reuse.Singleton);
    
    // consuming part is still the same, win!
    IClient client = c.Resolve<IClient>();
    
    var anotherClient = c.Resolve<IClient>();
    Assert.AreSame(anotherClient.Service, client.Service);
```

It is a simple task to replace `IService` implementation with `FasterService` or `TestMockService` without changing the resolution code. 


## Registering multiple implementations

DryIoc supports two kinds of multiple implementations of the same service: default and keyed registrations with Service Key. 


### Default registrations

For multiple default implementations write Register as usual:

```
#!c#
    container.Register<ICommand, GetCommand>();
    container.Register<ICommand, SetCommand>();
    container.Register<ICommand, DeleteCommand>();
```

Here three different `ICommand` implementations registered. By default there is no way to distinguish between them and resolving `ICommand` will lead to error: 

_"Expecting single default registration of ICommand but found many ..."_

But you may Resolve all three services as Collection:

```
#!c#
    var commands = container.Resolve<IEnumerable<ICommand>>();
    Assert.AreEqual(3, commands.Count());
```

__Note:__ Additionally to `IEnumerable<T>` DryIoc supports resolution as array (`T[]`) and any interface implemented by array, e.g. `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`. 

For convenience DryIoc has dedicated `ResolveMany` method with couple of optional settings: 

```
#!c#
    IEnumerable<ICommand> commands = container.ResolveMany<ICommand>();
```

Internally to store multiple default registrations DryIoc uses special key type `DefaultKey`. It has single `int` property `DefaultKey.RegistrationOrder`. More on keyed registrations below.

__Note:__ When resolving collection of multiple defaults they always ordered in registration order.

There are still couple of ways to select specific registration:

- Using [condition](SpecifyDependencyAndPrimitiveValues#markdown-header-registering-with-condition): `container.Register<ICommand, GetCommand>(setup: Setup.With(condition: r => r.IsCompositionRoot))` and for the rest of registrations specify opposite, e.g. `condition: r => !r.IsCompositionRoot`.
- Using specific metadata type and resolving as `Meta<,>` wrapper: `container.Register<ICommand, GetCommand>(setup: Setup.With(metadata: CommandId.Get));` and then resolving as `container.Resolve<IEnumerable<Meta<ICommand, CommandId>>>().Where(m => m.Metadata == CommandId.Get))`
- Using [reuse bound to specific parent](ReuseAndScopes#markdown-header-reuseinresolutionscopeof) or to [named current scope](ReuseAndScopes#markdown-header-reuseincurrentnamedscope-and-reuseinthread).


### Keyed registrations

To identify specific service implementation you may provide Service Key in registration. Service Key is value of arbitrary type which implements `Object.GetHashCode` and `Object.Equals`. Usual choice is enum type, string, or number.
```
#!c#
    container.Register<ICommand, GetCommand>(serviceKey: CommandId.Get);
    container.Register<ICommand, SetCommand>(serviceKey: CommandId.Set);
    container.Register<ICommand, DeleteCommand>(serviceKey: CommandId.Del);
```

And then resolve using registration key:

```
#!c#
    var setCommand = container.Resolve<ICommand>(serviceKey: CommandId.Set);
```

Or get all commands:
```
#!c#
    var commands = container.Resolve<ICommand[]>();
``` 

Or get all commands with corresponding registration key:

```
#!c#
    var commands = container.Resolve<KeyValuePair<ICommandId, ICommand>[]>();
    
    // or as Lazy 
    var lazyCommands = container.Resolve<KeyValuePair<ICommandId, Lazy<ICommand>>[]>();
    
    // or as Func
    var commandFactories = container.Resolve<KeyValuePair<ICommandId, Func<ICommand>>[]>();
```

### Resolving as KeyValuePair wrapper

DryIoc supports resolving service with corresponding service key as `KeyValuePair<KeyType, ServiceType>` wrapper.

It works both for default and keyed registrations, because default registrations internally stored with `DefaultKey`. To get all registrations you need to specify `object` as `TKey`:
```
#!c#
    container.Register<I, X>();
    container.Register<I, Y>();
    container.Register<I, Z>(serviceKey: "z");
    
    var items = container.Resolve<KeyValuePair<object, I>[]>();
    
    Assert.AreEqual(DefaultKey.Of(0), items[0].Key);
    Assert.AreEqual(DefaultKey.Of(1), items[1].Key);
    Assert.AreEqual("z", items[2].Key);
```

__Note:__ DryIoc does not guaranty return order for non-default keyed registrations. So in example above it is not guarantied that _"z"_ registration will be last because it was registered the last.

Resolving as `KeyValuePair` has ability to filter services based on provided `TKey` type. For instance the way default registrations only is:

```
#!c#
    // will resolve only X and Y but not Z
    var items = container.Resolve<KeyValuePair<DefaultKey, I>[]>();
```

Using filtering ability you may resolve single keyed _"z"_:

```
#!c#
    // check that array is not used here:
    var z = container.Resolve<KeyValuePair<string, I>>();
```

In case of multiple string keys you will get expected exception: _"Unable to resolve multiple ..."_.


## IsRegistered

Sometimes you need to find if service is already registered. IsRegistered allows to find that specified service, decorator, or wrapper was registered into Container.

It also allows to provide condition to test registered implementation Factory for the service. For instance you may test for specific Reuse, or Metadata, or Setup.

To find if MyService is registered as Singleton:

    container.IsRegistered<MyService>(
        condition: factory => factory.Reuse is SingletonReuse);

In above case we are lookingfor any default or keyed registration. If you need to find service with specific key then specify the key:

    container.IsRegistered<MyService>(serviceKey: "my string key");

    // to check for default registration
    container.IsRegistered<MyService>(serviceKey: DefaultKey.Value);

By default IsRegistered will look only for registered services, not for decorators or wrappers. To look for them you need to specify corresponding FactoryType:

    container.IsRegistered(typeof(MyOwned<>), factoryType: FactoryType.Wrapper);

__Note:__ IsRegistered does not check if service is actually resolvable. For instance, if some of its dependencies are not registered. To check for resolvability use Resolve with IfUnresolved.ReturnDefault:

    var isResolvable = container.Resolve<MyService>(ifUnresolved: IfUnresolved.ReturnDefault) != null;

If you want just to check service resolvability without actually creating service then resolve it as Func wrapper.

    var isResolvable = container.Resolve<Func<MyService>>(ifUnresolved: IfUnresolved.ReturnDefault) != null;

### Implicit ways to know that service is registered

There are other ways to get existing registration that may fit to your needs:

- GetServiceRegistrations() will enumerate all service registrations in container. For instance to find all services registered with specific key and reuse:

        container.GetServiceRegistrations()
            .Where(r => myKey.Equals(r.OptionalServiceKey) && r.Factory.Reuse is CurrentScopeReuse);

- Register with ifAlreadyRegistered option:

        container.Register<MyService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);


## RegisterMany

`RegisterMany` allows to register single or multiple implementations of multiple services. In addition it may automatically deduce service types from given implementation types (or assemblies of implementation types).

`RegisterMany` may help you to automate batch registrations and registrations from assemblies.

Examples:
```
#!c#
    public interface X {}
    public interface Y {}

    public class A : X, Y {}
    public class B : X, IDisposable {}
    
    
    // Registers X, Y and A itself with A implementation 
    container.RegisterMany<A>();
    
    
    // Registers only X and Y, but not A itself
    container.RegisterMany<A>(serviceTypeCondition: type => type.IsInterface);
    
    
    // X, Y, A are sharing the same singleton
    container.RegisterMany<A>(Reuse.Singleton)
    Assert.AreSame(container.Resolve<X>(), container.Resolve<Y>());
    
    
    // Registers X, Y with A and X with B
    // IDisposable is too general to be considered as a service type, 
    // see the full list of excluded types after example below.
    container.RegisterMany(new[] { typeof(A), typeof(B) },
        serviceTypeCondition: type => type.IsInterface);
    
    
    // Registers only X with A and X with B
    container.RegisterMany(new[] { typeof(A), typeof(B) },
        serviceTypeCondition: type => type == typeof(X));
    
    
    // The same as above if A and B in the same assembly.
    // Plus registers the rest of the types from assembly of A.
    container.RegisterMany(new[] { typeof(A).Assembly }, 
        serviceTypeCondition: type => type == typeof(X));
    
    
    // Everything in assembly of A including internal types
    container.RegisterMany(new[] { typeof(A).Assembly }, nonPublicServiceTypes: true);
        
    
    // Made.Of expression is supported too
    container.RegisterMany(Made.Of(() => MyMethodReturnsA()));
    
    
    // Explicit about what services to register
    container.RegisterMany(new[] { typeof(X), typeof(Y) }, typeof(A));
    
    
    // Provides full control to you
    container.RegisterMany(new[] { typeof(A).Assembly },
        action: (registrator, serviceTypes, implType) =>
        {
            var reuse = implType.IsAssignableTo(typeof(IDisposable)))
                ? Reuse.InResolutionScope
                : Reuse.Transient;
            registrator.RegisterMany(serviceTypes, implType, reuse);
        });
```

DryIoc does not consider some types as a service type (__for RegisterMany method__). Here is the full list of excluded types:

  - System.IDisposable
  - System.ValueType
  - System.ICloneable
  - System.IEquatable
  - System.IComparable
  - System.Runtime.Serialization.ISerializable
  - System.Collections.IStructuralEquatable
  - System.Collections.IEnumerable
  - System.Collections.IList
  - System.Collections.ICollection

__Note:__ But if you really need this, you may register the type with `Register` method.


## RegisterMapping

`RegisterMapping` allows to map new service to already registered service and its implementation. 

For example you have a singleton implementation which can be accessed via two different facades / services:

    public interface I {}
    public interface J {}
    class S : I, J {} // implements both I and J
    
    // register singleton as I
    container.Register<I, S>(Reuse.Singleton);

    // map J to I and therefore to the same S singleton
    container.RegisterMapping<J, I>();

    Assert.AreSame(container.Resolve<I>(), container.Resolve<J>());

This feature may be viewed as complementary to `RegisterMany`:

    container.RegisterMany(new[] { typeof(I), typeof(J) }, typeof(S), Reuse.Singleton);
    Assert.AreSame(container.Resolve<I>(), container.Resolve<J>());

The difference is that you just map to existing registration.
Performance considered it should be the same.


## RegisterDelegate

You can register any custom delegate as your service factory:

```
#!c#
    container.RegisterDelegate<IService>(r => new CheerfulService() { Greetings = "Hey!" });
```

`r` parameter allows to access `IResolver` side of Container, it could be used to resolve any additional dependencies required for service creation and initialization:

```
#!c#
    container.RegisterDelegate<IService>(
        r => new CheerfulService(r.Resolver.Resolve<IGreetingsProvider>());
```

Though powerful, registering delegate may lead to the problems:

- __Memory leaks by capturing variables into delegate closure and keeping them for a container lifetime.__
- __Delegate is the black box for Container - which makes hard to find type mismatches or diagnose other potential problems.__

Therefore, try to use it only as last resort. DryIoc has plenty of tools to cover for custom delegate in more effective way. The ultimate alternative would be [Factory Methods](ConstructorSelection).

Another thing that delegate usually is hard to use when types and not known in the compile time. Given `type = typeof(Foo)` it is impossible to write `new type();`.
To enable such use-case DryIoc allow to register delegate with runtime known type:

```
#!c#
    container.RegisterDelegate(someTypeOrInterface, 
        r => MyCustomReflectionMagicToCreate(someType));
```

### Will not detect recursive dependencies

When using the normal typed registration DryIoc will detect [recursive dependencies](ErrorDetectionAndResolution#markdown-header-RecursiveDependencyDetected). 

But when using delegate registration DryIoc is unable to analyze what dependencies are used inside delegate. That is another reason to avoid `RegisterDelegate` whatsoever:

    public class A { public A(B b) {} } // A requires B
    public class B { public B(A a) {} } // B requires A
    
    container.Register<A>();
    container.RegisterDelegate<B>(r => new B(r.Resolve<A>()));

    container.Resolve<A>(); // Fails with StackOverflowException

    // To catch the problem do container.Register<B>() instead


## UseInstance

**Caution:** Please avoid using `UseInstance` if not absolutely necessary: 
when it is impossible to utilize DryIoc container for creating the instance for you. 
The only case I can imagine, when the value provided by some external context, e.g. `HttpContext` in ASP.NET.

`UseInstance` method supplies the already created external instance to container to use for injection in resolved services.

Example:

    var a = new A();
    container.UseInstance(a);

    // has a constructor with A parameter
    container.Register<B>();

    // will inject used instance of `a` as a parameter when creating B
    container.Resolve<B>(); 

Why it is called `UseInstance` instead of `RegisterInstance`?

The main container goal is to _create_ the services using registered Type or Delegate.
In case of _pre-created_ external instance there is nothing to create - 
container should just _use instance_ for injection.

When you call the `UseInstance` the instance will be **directly put into Open scope or Singleton scope* 
based on whether the container is scoped (returned from `OpenScope` call) or not.
In addition, the scoped and sington instances may coexist with each other.

Example of scoped and singleton instance:

    var container = new Container();
    container.Register<B>();

    using (var scope = container.OpenScope())
    {
        var a = new A();
        scope.UseInstance(a); // Scoped

        scope.Resolve<B>(); // will inject `a`
    }

    var anotherA = new A();
    container.UseInstance(anotherA); // Singleton
    container.Resolve<B>(); // will inject `anotherA`

In previous examples we did not specify the service type, but you can specify it for used instance 
and moreover, you can provide run-time **service type** for the `object` instance:

    // compile-time known type
    container.UseInstance<ISomeService>(a);

    // run-time known type
    object aa = a;
    container.UseInstance(typeof(ISomeService), aa);

You may also provide **service key** to distinguish used instances:

    container.UseInstance(new A()); 
    container.UseInstance(new A(special), serviceKey: "specialOne");

`UseInstance` method will replace (override) the previous registrations done on the same level: container or scope.

    container.UseInstance(new A(1)); 
    container.UseInstance(new A(2)); // will replace the A(1) instance.

**Note:** The DryIoc version prior 3.0 also contains `RegisterInstance` method. 
The method is obsolete and its usages should be replaced with `UseInstance`.


## RegisterInitializer

Initializer is the action to be invoked on created service before returning it from resolve method, 
or before injecting it as dependency. 

__Note:__ Underneath Initializer is registered as [Decorator](Decorators).

Let's say we want to log the creation of our service:

    container.Register<ILogger, MyLogger>();
    container.Register<IService, MyService>();
    container.RegisterInitializer<IService>(
        (service, resolver) => resolver.Resolve<ILogger>().LogInfo("resolved: " + service));

OK, this works for specific service `IService`. What if I want to log creation of _any_ resolved or injected service.

Initializer may be registered for `object` target, which means _any_ service type.

    container.RegisterInitializer<object>(
        (serviceObj, resolver) => resolver.Resolve<ILogger>().LogInfo("resolved: " + serviceObj));

__Note:__ Invoking intializer for any object will affect performance of each resolution and injection.

It would be helpful to limit initializer to specific cases. 
It may be done via optional condition parameter passed to `RegisterInitializer`.

Let's change example to log only Controller dependencies creation:

    container.RegisterInitializer<object>(
        (serviceObj, resolver) => resolver.Resolve<ILogger>().LogInfo("created: " + serviceObj),
        condition: request => request.Parent.ServiceType.Name.EndsWith("Controller"));

Important thing that `TTarget` of Initializer may not correspond to actual registered _Service Type_.
It may be any type implemented by registered _Service Type_ or _Implementation Type_.

To register logger for disposable services:

    container.RegisterInitializer<IDisposable>(
        (disposable, resolver) => resolver.Resolve<ILogger>().LogInfo("resolved disposable: " + disposable));


## RegisterDisposer

The normal way to cleanup things is to implement `IDisposable` interface. 
If disposable service was registered with `Reuse`, and reuse scope is disposed, then the service will be disposed as well.
For instance, registered singleton will be disposed when container (and therefore singletons scope) is disposed.

But what if service for some reason does not implement `IDisposable` 
but still wants to invoke __release logic__ when goes out of scope.

`RegisterDisposer` addresses such a need. 

__Note:__ Internally it is just a [Decorator](Decorators) which creates companion disposable object
with specified _release_ action. When scope is disposed the conpanion is disposed too and invokes the action.

    container.Register<FileUser>(Reuse.Singleton);
    container.RegisterDisposer<FileUser>((fileUser, resolver) => fileUser.CloseFile());

    container.Resolve<FileUser>().DoUsefulStuff();

    container.Dispose() // will call disposer action for FileUser singleton

The same as for `RegisterInitializer` the disposer _target type_ is any type implemented by registered service or its implementation.

    class X : IClosable {}
    class Y : IClosable {}
    
    container.Register<X>();
    container.Register<Y>();

    container.RegisterDisposer<IClosable>((closable, resolver) => closable.Close());

Additionally you may provide condition parameter to select target service. 
For instance to filter out services already implementing `IDisposable`:

    container.RegisterDisposer<IClosable>((closable, resolver) => closable.Close(),
        condition: request => !typeof(IDisposable).IsAssignableFrom(request.ImplementationType ?? request.ServiceType));
