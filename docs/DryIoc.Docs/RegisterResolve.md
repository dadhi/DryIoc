<!--Auto-generated from .cs file, the edits here will be lost! -->

# Register and Resolve

[TOC]

## DryIoc Glossary

We will use the following definitions:

- __Resolution Root__ is a service object resolved from container by calling `Resolve` method, 
e.g. in `var client = container.Resolve<IClient>();` `client` is a Resolution Root.
- __Injected Dependency__ is an argument of type `IService` passed (injected) to the constructor of `SomeClient`.
    It will be automatically created by container and injected into the client constructor when resolving `IClient` Resolution Root. 
    When injecting dependency you do not need to call `container.Resolve` for the dependency itself.
    _If not said otherwise Resolution and Injection will be used in the same sense of retrieving object from container._

    - __Constructor Injection__: Injecting dependencies into constructor parameters. 
        __It is a preferable way__ because all dependencies are visible in a single place - a constructor. 
        If constructor parameters are too many, this is an indication of [God object anti-pattern](http://en.wikipedia.org/wiki/God_object) 
        and violation of [Single Responsibility principle](http://en.wikipedia.org/wiki/Single_responsibility_principle). 
        That means, time to split your class into multiple cohesive units.

    - __Property/Field Injection__: Injecting dependencies as properties and fields of the class. Because properties may
        be set not only in constructor but in any code at any time, it is harder to track and conclude about all class
        dependencies. __Therefore, use the Property Injection as last-resort and strife for Constructor Injection as much as possible.__ 

    DryIoc supports both Constructor and Property/Field injection.

* __Implementation Type__ is an actual type used for service creation, e.g. `SomeClient` and `SomeService`. 
    In DryIoc Implementation Type may be open-generic: `OtherService<>`.

* __Service Type__ is usually an abstract or an interface type using for service location and dependency injection, e.g. `IClient` and `IService`.
    Service Type should be assignable from Implementation Type. It is possible to have multiple Service Types for single Implementation Type as it may
    implement multiple interfaces. In addition you can use Implementation Type itself as Service Type, so the one type is used for injection and creation. 
    In DryIoc Service Type may be open-generic: `IService<>`.


## Registration API

DryIoc supports registration vid `Register..` methods with the provided mapping of service and implementation types:
```cs 

using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc;
using NUnit.Framework;

class Register_service_with_implementation_types
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<IClient, SomeClient>();
        container.Register<IService, SomeService>();
    }
} 
```

__Note:__ The order of registrations is not important __here__, 
but may be important for other cases, like [Decorators](Decorators) or Collection [Wrappers](Wrappers).

If you don't know the type in compile-time you can specify a run-time `Type` instead:
```cs 
class Register_service_with_implementation_runtime_types
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register(typeof(IClient), typeof(SomeClient));
        container.Register(typeof(IService), typeof(SomeService));
    }
}
```

Container will check if the service type is assignable to the implementation type and will throw exception otherwise.

You can register open-generic as well:
```cs 
class Register_open_generic_service_with_implementation_runtime_types
{
    interface IService<T> { }
    class SomeService<T> : IService<T> { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register(typeof(IService<>), typeof(SomeService<>));

        var s = container.Resolve<IService<string>>();
        Assert.IsInstanceOf<SomeService<string>>(s);
    }
}
```

An implementation type may play the role of service type:
```cs 
class Register_implementation_as_service_type
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<SomeService>();

        // or via run-time type
        container.Register(typeof(SomeService));
    }
}
```


## Registering as Singleton

The registrations above are simple because they don't take into account a service lifetime, 
that means they are Transient - the new service created each time it is resolved or injected. 

What if I want service to be a Singleton. Singleton means that the same service instance is used during container lifetime. 

__Note:__ Usually, the container lifetime will match your application lifetime. 

DryIoc supports Singleton with a concept of [Reuse and Scopes](ReuseAndScopes). 

Example of singleton service registration:

```cs 
class Singleton_service_registration
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<IClient, SomeClient>();

        // making Service dependency a singleton
        container.Register<IService, SomeService>(Reuse.Singleton);

        // consuming part is still the same, win!
        var one = container.Resolve<IClient>();
        var two = container.Resolve<IClient>();

        Assert.AreSame(two.Service, one.Service);
    }
} 
```

Static singletons are considered an anti-pattern, because they hard to replace, for instance in tests.
Here, when using DI / IoC Container, we may change `IService` to `SomeTestService` on registration side, without need to change 
anything on consuming side. 


## Registering multiple implementations

DryIoc supports two kinds of multiple implementations of the same service: default and keyed registration with a `serviceKey`. 


### Default registrations

For multiple default implementations write Register as usual:

```cs 
class Multiple_default_registrations
{
    internal interface ICommand { }
    internal class GetCommand : ICommand { }
    internal class SetCommand : ICommand { }
    internal class DeleteCommand : ICommand { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<ICommand, GetCommand>();
        container.Register<ICommand, SetCommand>();
        container.Register<ICommand, DeleteCommand>();

        Assert.Throws<ContainerException>(() =>
            container.Resolve<ICommand>()); // Huh, what command did you mean?

        var commands = container.Resolve<IEnumerable<ICommand>>();
        Assert.AreEqual(3, commands.Count());

        // There is also a dedicated ResolveMany method:
        var commands2 = container.ResolveMany<ICommand>();
        Assert.AreEqual(3, commands2.Count());
    }
}
```

There are three different `ICommand` implementations registered. 
By default there is no way to distinguish between them, therefore just resolving an `ICommand` will lead to error.
So you need to resolve a collection (`IEnumerable<ICommand>`) of commands.

__Note:__ Additionally to `IEnumerable<T>`, DryIoc supports resolving an array `T[]` 
or any interface implemented by array, e.g. `IList<T>`, `ICollection<T>`, `IReadOnlyList<T>`, `IReadOnlyCollection<T>`. 
Check the documentation for collection [Wrappers](Wrappers). 

For convenience DryIoc has dedicated `ResolveMany` method with couple of optional settings. 

Internally to store multiple default registrations, DryIoc uses special key type `DefaultKey`. 
It has an interesting property `DefaultKey.RegistrationOrder`. More on keyed registrations below.

__Note:__ When resolving collection of multiple defaults, it always ordered in registration order.

There are also a couple of ways to select a specific registration and avoid an exception in `Resolve<ICommand>`:

- Using [condition](SpecifyDependencyAndPrimitiveValues#markdown-header-registering-with-condition): 
`container.Register<ICommand, GetCommand>(setup: Setup.With(condition: req => req.IsResolutionRoot))` 
and for the rest of registrations to specify opposite condition, e.g. `condition: r => !r.IsResolutionRoot`.
- Using specific metadata type (`CommandId` enum) and resolving as `Meta<,>` wrapper: 
`container.Register<ICommand, GetCommand>(setup: Setup.With(metadata: CommandId.Get));` 
and then resolving as `container.Resolve<IEnumerable<Meta<ICommand, CommandId>>>().Where(m => m.Metadata == CommandId.Get))`
- Using [reuse bound to specific parent scope](ReuseAndScopes#markdown-header-reuseinresolutionscopeof) 
or to [named scope](ReuseAndScopes#markdown-header-reuseincurrentnamedscope-and-reuseinthread).
- Registering with `serviceKey`.


### Keyed registrations

To identify specific service implementation you may provide Service Key in registration. Service Key is value of arbitrary type which implements `Object.GetHashCode` and `Object.Equals`. Usual choice is enum type, string, or number.
```cs 
class Multiple_keyed_registrations
{
    internal interface ICommand { }
    internal class GetCommand : ICommand { }
    internal class SetCommand : ICommand { }
    internal class DeleteCommand : ICommand { }

    enum CommandId { Get, Set, Delete }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<ICommand, GetCommand>(serviceKey: CommandId.Get);
        container.Register<ICommand, SetCommand>(serviceKey: CommandId.Set);
        container.Register<ICommand, DeleteCommand>(serviceKey: CommandId.Delete);

        // then specify the required key on resolve
        var setCommand = container.Resolve<ICommand>(serviceKey: CommandId.Set);
        Assert.IsInstanceOf<SetCommand>(setCommand);

        // get array of all commands, don't matter the key
        var commands = container.Resolve<ICommand[]>();
        Assert.AreEqual(3, commands.Length);

        // you may select a specific behavior of ResolveMany:
        var commands2 = (ICommand[])container.ResolveMany<ICommand>(behavior: ResolveManyBehavior.AsFixedArray);
        Assert.AreEqual(3, commands2.Length);
    }
} 
```

There is also a way to get the commands with their respective keys. 
It may be especially useful in combination with `Lazy` or `Func` [wrappers](Wrappers), e.g. to built a menu or navigation in UI.

```cs 
class Resolve_commands_with_keys
{
    internal interface ICommand { }
    internal class GetCommand : ICommand { }
    internal class SetCommand : ICommand { }
    internal class DeleteCommand : ICommand { }

    enum CommandId { Get, Set, Delete }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<ICommand, GetCommand>(serviceKey: CommandId.Get);
        container.Register<ICommand, SetCommand>(serviceKey: CommandId.Set);
        container.Register<ICommand, DeleteCommand>(serviceKey: CommandId.Delete);

        var commands = container.Resolve<KeyValuePair<CommandId, ICommand>[]>();
        Assert.AreEqual(CommandId.Get, commands[0].Key);

        // or as Lazy 
        var lazyCommands = container.Resolve<KeyValuePair<CommandId, Lazy<ICommand>>[]>();
        Assert.AreEqual(CommandId.Set, lazyCommands[1].Key);

        // or as Func
        var commandFactories = container.Resolve<KeyValuePair<CommandId, Func<ICommand>>[]>();
        Assert.AreEqual(CommandId.Delete, commandFactories[2].Key);
    }
}
```

### Resolving as KeyValuePair wrapper

DryIoc supports resolving of services registered with corresponding service key via `KeyValuePair<KeyType, ServiceType>` wrapper.
It works both for default and keyed registrations, because a default registration internally identified with the special `DefaultKey` type. 

__Note:__ When resolving a collection of services with service keys of different types, you need to specify `object` as `TKey` in a pair.

```cs 
class Resolving_service_with_key_as_KeyValuePair
{
    interface I { }
    class X : I { }
    class Y : I { }
    class Z : I { }

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<I, X>();
        container.Register<I, Y>();
        container.Register<I, Z>(serviceKey: "z");

        // use an `object` key to get all of registrations
        var items = container.Resolve<KeyValuePair<object, I>[]>();

        // the keys of resolved items
        CollectionAssert.AreEqual(
            new object[] { DefaultKey.Of(0), DefaultKey.Of(1), "z" },
            items.Select(x => x.Key));

        // you may get only services with default keys
        var defaultItems = container.Resolve<KeyValuePair<DefaultKey, I>[]>();
        Assert.AreEqual(2, defaultItems.Length);

        // or the one with string key
        // you may get only services with default keys
        var z = container.Resolve<KeyValuePair<string, I>[]>().Single().Value;
        Assert.IsInstanceOf<Z>(z);
    }
}
```

You see, that you can filter registrations based on the `serviceKey` type.

__Note:__ DryIoc does not guaranty return order for non-default keyed registrations. 
So in example above it is not guarantied that `"z"` registration will be the last, because it was registered the last.


In case of multiple string keys you will get expected exception: _"Unable to resolve multiple ..."_.


## IsRegistered

Sometimes you need to find, if a service is already registered. 
`IsRegistered` method allows you to find, that specific service, decorator, or wrapper is registered into the Container.

Additionally, `IsRegistered` allows you to provide a `condition` to test registered implementation `Factory` of the service. 
For instance, you may test for specific `Reuse`, or `Metadata`, or `Setup`.

To find if `MyService` is registered as `Singleton`:
```cs 
class IsRegistered_examples
{
    class MyService { }

    [Test]
    public void Example()
    {
        var c = new Container();

        // not registered yet
        Assert.IsFalse(c.IsRegistered<MyService>(condition: factory => factory.Reuse is SingletonReuse));

        c.Register<MyService>();

        // registered, but not a singleton
        Assert.IsTrue(c.IsRegistered<MyService>());
        Assert.IsFalse(c.IsRegistered<MyService>(condition: factory => factory.Reuse is SingletonReuse));

        c.Register<MyService>(Reuse.Singleton);

        // found a singleton
        Assert.IsTrue(c.IsRegistered<MyService>(condition: factory => factory.Reuse is SingletonReuse));
    }
}
```

In above case we are looking for any default or keyed registration. 
If you need to find service with specific key then specify the key:
```cs 
class IsRegistered_with_key_examples
{
    class MyService { }

    [Test]
    public void Example()
    {
        var c = new Container();

        c.Register<MyService>(serviceKey: "the key");
        Assert.IsTrue(c.IsRegistered<MyService>(serviceKey: "the key"));

        // check that, there is no default registration
        Assert.IsFalse(c.IsRegistered<MyService>(serviceKey: DefaultKey.Value));

        // Note, when key is not provided it will find any registered service
        Assert.IsTrue(c.IsRegistered<MyService>());

        c.Register<MyService>();

        // Now found registered default service
        Assert.IsTrue(c.IsRegistered<MyService>(serviceKey: DefaultKey.Value));
    }
}
```

By default `IsRegistered` will look only for __services__, not for [decorators](Decorators) or [wrappers](Wrappers). 
To look for them you need to specify corresponding FactoryType:
```cs 

class IsRegistered_for_wrapper_or_decorators
{
    class Owned<T> { }

    [Test]
    public void Example()
    {
        var c = new Container();
        Assert.IsFalse(c.IsRegistered(typeof(Owned<>), factoryType: FactoryType.Wrapper));

        c.Register(typeof(Owned<>), setup: Setup.Wrapper);
        Assert.IsTrue(c.IsRegistered(typeof(Owned<>), factoryType: FactoryType.Wrapper));
    }
}
```

__Important:__ `IsRegistered` does not check if service is actually resolvable. 
For instance, if some of its dependencies are not registered. To check for resolvability use Resolve with `IfUnresolved.ReturnDefault`:

```cs 
class Check_if_resolvable
{
    class MyService { }

    [Test]
    public void Example()
    {
        var c = new Container();
        Assert.IsFalse(c.Resolve<MyService>(ifUnresolved: IfUnresolved.ReturnDefault) != null);

        c.Register<MyService>();
        Assert.IsTrue(c.Resolve<MyService>(ifUnresolved: IfUnresolved.ReturnDefault) != null);

        // If you want just to check service resolvability without actually creating service then resolve it as Func wrapper.
        Assert.IsTrue(c.Resolve<Func<MyService>>(ifUnresolved: IfUnresolved.ReturnDefault) != null);
    }
}
```

### Implicit ways to know that service is registered

There are other ways to get existing registration, that may fit to your needs.
The basic one is to get all Container registrations and find the one you need.

```cs 
class Get_specific_registration
{
    class MyService { }

    [Test]
    public void Example()
    {
        var c = new Container();

        c.Register<MyService>(Reuse.Scoped, serviceKey: "foo");

        var serviceRegistration = c.GetServiceRegistrations()
            .FirstOrDefault(r => Equals(r.OptionalServiceKey, "foo") && r.Factory.Reuse == Reuse.Scoped);

        Assert.AreEqual(typeof(MyService), serviceRegistration.ImplementationType);
    }
}
```

`GetServiceRegistrations()` method will enumerate all service registrations in container. 


## RegisterMany

`RegisterMany` method allows to register single or multiple implementations of multiple services. 
In addition, it may automatically deduce service types from given implementation types (or assemblies of implementation types).

`RegisterMany` helps to automate batch registrations and registrations from assemblies.

```cs 
public class RegisterMany_examples
{
    public interface X { }
    public interface Y { }

    public class A : X, Y { }

    public class B : X, IDisposable
    {
        public void Dispose() { }
    }

    public static A CreateA() => new A();

    [Test]
    public void Example()
    {
        // Allows to register B which implements IDisposable as Transient, which is default `RegisterMany` reuse.
        var container = new Container(rules => rules
            .WithTrackingDisposableTransients()); 

        // Registers X, Y and A itself with A implementation 
        container.RegisterMany<A>();

        // Registers only X and Y, but not A itself
        container.RegisterMany<A>(serviceTypeCondition: type => type.IsInterface);

        // X, Y, A are sharing the same singleton
        container.RegisterMany<A>(Reuse.Singleton);
        Assert.AreSame(container.Resolve<X>(), container.Resolve<Y>());

        // Registers X, Y with A and X with B
        // IDisposable is too general to be considered as a service type, 
        // see the full list of excluded types after example below.
        container.RegisterMany(
            new[] { typeof(A), typeof(B) },
            serviceTypeCondition: type => type.IsInterface);

        // Registers only X with A and X with B
        container.RegisterMany(
            new[] { typeof(A), typeof(B) },
            serviceTypeCondition: type => type == typeof(X));

        // The same as above if A and B in the same assembly.
        // Plus registers the rest of the types from assembly of A.
        container.RegisterMany(new[] { typeof(A).Assembly }, type => type == typeof(X));

        // Made.Of expression is supported too
        container.RegisterMany(Made.Of(() => CreateA()));

        // Explicit about what services to register
        container.RegisterMany(new[] { typeof(X), typeof(Y) }, typeof(A));

        // Provides full control to you
        container.RegisterMany(new[] { typeof(A).Assembly },
            getServiceTypes: implType => implType.GetImplementedServiceTypes(),
            getImplFactory: implType => new ReflectionFactory(implType,
                reuse: implType.IsAssignableTo<IDisposable>() ? Reuse.Scoped : Reuse.Transient,
                made: FactoryMethod.ConstructorWithResolvableArguments));
    }
} 
```

DryIoc does not consider some types as a service type for `RegisterMany` method. 
You may examine the excluded types by looking at `Registrator.ExcludedGeneralPurposeServiceTypes` property.
The value may depend on your .NET framework.

```cs 
class RegisterMany_excludes_these_types
{
    [Test]
    public void Example()
    {
        var excludedTypes = Registrator.ExcludedGeneralPurposeServiceTypes;
        Assert.IsNotEmpty(excludedTypes);
    }
}
```

The excluded types are:

  - `System.IDisposable`
  - `System.ValueType`
  - `System.ICloneable`
  - `System.IEquatable`
  - `System.IComparable`
  - `System.Runtime.Serialization.ISerializable`
  - `System.Collections.IStructuralEquatable`
  - `System.Collections.IEnumerable`
  - `System.Collections.IList`
  - `System.Collections.ICollection`

__Note:__ If you really need to register something from the list, you may register it with standalone `Register` method.


## RegisterMapping

`RegisterMapping` allows to map new service to already registered service and its implementation. 

For example you have a singleton implementation which can be accessed via two different facades / services:
```cs 
class Register_mapping
{
    public interface I { }
    public interface J { }
    class S : I, J { } // implements both I and J

    [Test]
    public void Example()
    {
        var container = new Container();

        // Register I as a singleton S
        container.Register<I, S>(Reuse.Singleton);

        // Map J to I, and therefore map J to the same singleton S
        container.RegisterMapping<J, I>();

        Assert.AreSame(container.Resolve<I>(), container.Resolve<J>());
    }
}
```

This same result maybe achieved via `RegisterMany`:

```cs 
class Register_mapping_with_RegisterMany
{
    public interface I { }
    public interface J { }
    class S : I, J { } // implements both I and J

    [Test]
    public void Example()
    {
        var container = new Container();

        // Multiple interfaces refer to a single implementation
        container.RegisterMany(new[] { typeof(I), typeof(J) }, typeof(S), Reuse.Singleton);
        Assert.AreSame(container.Resolve<I>(), container.Resolve<J>());

        Assert.AreSame(container.Resolve<I>(), container.Resolve<J>());
    }
}
```

The possible advantage of `RegisterMapping` would be that you can decide on mapping for already existing registration.

Performance considered it should be the same.


## RegisterDelegate

You can register any custom delegate as your service factory via `RegisterDelegate`:

```cs 
class Register_delegate
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.RegisterDelegate<IService>(resolverContext => new CheerfulService { Greetings = "Hey!" });

        var x = container.Resolve<IService>();
        Assert.AreEqual("Hey!", ((CheerfulService)x).Greetings);
    }

    internal class CheerfulService : IService
    {
        public string Greetings { get; set; }
    }
}
```

The `IResolverContext resolverContext` delegate parameter was not used in example above. 
Actually, it could be used to resolve any additional dependencies required for service creation and initialization:

```cs 
class Register_delegate_with_resolved_dependencies
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.RegisterDelegate(_ => new GreetingsProvider { Greetings = "Mya" });
        container.RegisterDelegate<IService>(resolverContext =>
            new CheerfulService(resolverContext.Resolve<GreetingsProvider>()));

        var x = container.Resolve<IService>();
        Assert.AreEqual("Mya", ((CheerfulService)x).Greetings);
    }

    class GreetingsProvider
    {
        public string Greetings { get; set; }
    }

    class CheerfulService : IService
    {
        public string Greetings => _greetingsProvider.Greetings;

        public CheerfulService(GreetingsProvider greetingsProvider)
        {
            _greetingsProvider = greetingsProvider;
        }

        private readonly GreetingsProvider _greetingsProvider;
    }
}
```

Though powerful, registering delegate may lead to the problems:

1. Memory leaks by capturing variables into delegate closure and keeping them for a container lifetime.
2. Delegate is the black box for Container - which makes hard to find type mismatches or diagnose other potential problems.

Therefore, try to use it only as a last resort. DryIoc has plenty of tools to cover for custom delegate in more effective way. 
The ultimate alternative would be a [FactoryMethod](ConstructorSelection).

Another thing, that delegate is usually hard to use when types are not known in the compile time. 
Given `type = typeof(Foo)` it is impossible to write `new type();`.
To enable such use-case DryIoc allow to register delegate with runtime known type:

```cs 
class Register_delegate_returning_object
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.RegisterDelegate(typeof(IService), r => Activator.CreateInstance(typeof(Foo)));

        var x = container.Resolve<IService>();
        Assert.IsInstanceOf<Foo>(x);
    }

    class Foo : IService { }
}
 ```


### Will not detect recursive dependencies

When using normal typed registration DryIoc will detect [recursive dependencies](ErrorDetectionAndResolution#markdown-header-RecursiveDependencyDetected). 

But when using the delegate registration DryIoc is unable to analyze what dependencies are used inside delegate. 
That is another reason to avoid `RegisterDelegate` whatsoever:
```cs
public class A { public A(B b) {} } // A requires B
public class B { public B(A a) {} } // B requires A

container.Register<A>();
container.RegisterDelegate<B>(r => new B(r.Resolve<A>()));

container.Resolve<A>(); // Fails with StackOverflowException
```

To catch the problem with recursive dependency, replace `RegisterDelegate` with `container.Register<B>()`.


## RegisterInstance

Basically `RegisterInstance` method will supply an already created external instance to the container to use it for dependency injection.

```cs 
class Register_instance_example
{
    [Test]
    public void Example()
    {
        var container = new Container();

        var a = new A();
        container.RegisterInstance(a);
        container.Register<B>();

        var b = container.Resolve<B>();
        Assert.AreSame(a, b.A);
    }

    class A { }
    class B
    {
        public readonly A A;
        public B(A a) { A = a; }
    }
} 
```

Ok, now we have a pre-created instance in the registry, but what if I need a different instances when opening a new scope.
Say I want to put `RequestMessage` object into ASP request scope. A request message has a different value in different requests.

In that case you we can use method `Use` to put instance directly into the current scope skipping the registration ceremony.

__Caution:__ the instance put via `Use` does not support `serviceKey`, [Wrappers](Wrappers), and [Decorators](Decorators), 
but instead it provides nice performance gains and less memory allocations.

```cs 

class Example_of_scoped_and_singleton_instance
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<B>();

        using (var scope = container.OpenScope())
        {
            var a = new A();
            scope.Use(a); // injected into current open scope

            var b = scope.Resolve<B>(); // will inject `a`
            Assert.AreSame(a, b.A);
        }

        var anotherA = new A();
        container.Use(anotherA); // injected into singleton scope

        var anotherB = container.Resolve<B>(); // will inject `anotherA`
        Assert.AreSame(anotherA, anotherB.A);
    }

    class A { }
    class B
    {
        public readonly A A;
        public B(A a) { A = a; }
    }
}
```

__Note:__ The same way you may put an instance into the singletons scope if no current scope is open.

In previous examples we did not specify the service type, but you can specify it for used instance 
and moreover, you can provide run-time **service type** for the `object` instance:

```cs 
class Typed_instance
{
    public void Example()
    {
        var container = new Container();

        // compile-time known type
        var a = new A();
        container.Use<ISomeService>(a);

        // run-time known type
        object aa = a;
        container.Use(typeof(ISomeService), aa);
    }

    interface ISomeService {}
    class A : ISomeService {}
} 
```


## RegisterInitializer

Initializer is an action to be invoked on created service before returning it from resolve method, 
or before injecting it as dependency. 

__Note:__ Underneath Initializer is registered as [Decorator](Decorators).

Let's say we want to log the creation of our service:
```cs 
class Register_initializer
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<Logger>(Reuse.Singleton);
        container.Register<IService, MyService>();

        container.RegisterInitializer<IService>(
            (service, resolver) => resolver.Resolve<Logger>().Log("created"));

        container.Resolve<IService>();
        Assert.AreEqual("created", container.Resolve<Logger>().LogData[0]);
    }

    class Logger
    {
        public readonly List<string> LogData = new List<string>();
        public void Log(string s) => LogData.Add(s);
    }

    interface IService {}
    class MyService: IService {}
}
```

OK, this works for specific service `IService`. What if I want to log creation of _any_ resolved or injected service.

```cs 
class Register_initializer_for_any_object
{
    [Test]
    public void Example()
    {
        var container = new Container();
        var loggerKey = "logger";
        container.Register<Logger>(Reuse.Singleton, serviceKey: loggerKey);
        container.Register<IService, MyService>();

        container.RegisterInitializer<object>(
            (anyObj, resolver) => resolver.Resolve<Logger>(loggerKey).Log("created object"),
            condition: request => !loggerKey.Equals(request.ServiceKey));

        container.Resolve<IService>();
        Assert.AreEqual("created object", container.Resolve<Logger>(loggerKey).LogData[0]);
    }

    class Logger
    {
        public readonly List<string> LogData = new List<string>();
        public void Log(string s) => LogData.Add(s);
    }

    interface IService { }
    class MyService : IService { }
}
```

Initializer maybe registered for an `object` service, which means _any_ service type.

__Note:__ Invoking initializer for any object means, that it will be invoked for the `Logger` itself causing a `StackOverflowException`. 
To avoid this, logger is registered with a `serviceKey` and excluded from initializer action via `condition` parameter.


The `TTarget` of `RegisterInitializer<TTarget>()` maybe an any type implemented by registered service type, but not by implementation type.

For instance, to register logger for disposable services:
```cs
container.RegisterInitializer<IDisposable>(
    (disposable, resolver) => resolver.Resolve<Logger>().Log("resolved disposable " + disposable));
```

## RegisterPlaceholder

Sometimes, you maybe not yet decided on the service implementation or maybe you want to provide it at later time.
In this case you may use a `RegisterPlaceholder<IService>()`, then later do `Register<IService, Foo>(ifAlreadyRegistered:IfAlreadyRegistered.Replace)`.

```cs 
class Register_placeholder
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.RegisterPlaceholder<IService>();
        var getService = container.Resolve<Func<IService>>();

        // Throws because service is just a placeholder and does not have implementation registered yet
        IService service;
        Assert.Throws<ContainerException>(() => service = getService());

        // Replace placeholder with a real implementation
        container.Register<IService, Foo>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        service = getService();
        Assert.IsInstanceOf<Foo>(service);
    }

    interface IService { }
    class Foo : IService { }
}
```

## RegisterDisposer

The normal way to cleanup things is to implement `IDisposable` interface. 
If disposable service was registered with `Reuse`, and reuse scope is disposed, then the service will be disposed as well.
For instance, registered singleton will be disposed when container (and therefore singleton scope) is disposed.

But what if service for some reason does not implement `IDisposable` 
but still wants to invoke "release logic" when goes out of the scope.

`RegisterDisposer` addresses such a need. 

__Note:__ Internally it is just a [Decorator](Decorators) which creates "companion" disposable object
with specified _release_ action. When scope is disposed then the companion is disposed too invoking the action.
```cs 
class Register_disposer
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<FileAbstraction>(Reuse.Singleton);
        container.RegisterDisposer<FileAbstraction>(f => f.CloseFile());

        var file = container.Resolve<FileAbstraction>();
        file.ReadWriteStuff();

        container.Dispose(); // will call disposer action for FileUser singleton
        Assert.IsTrue(file.IsClosed);
    }

    internal class FileAbstraction
    {
        public bool IsClosed { get; private set; }
        public void CloseFile() => IsClosed = true;

        public void ReadWriteStuff() { }
    }
}

```
The same as for `RegisterInitializer`, the disposer _target type_ maybe any type implemented by registered service type 
(but not the implementation type).
```cs 
class Register_disposer_for_many_services
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<X>(Reuse.Singleton);
        container.Register<Y>(Reuse.Singleton);
        container.RegisterDisposer<IClosable>(closable => closable.Close());

        var x = container.Resolve<X>();
        var y = container.Resolve<Y>();
        container.Dispose();

        Assert.IsTrue(x.IsClosed);
        Assert.IsTrue(y.IsClosed);
    }

    interface IClosable
    {
        bool IsClosed { get; } 
        void Close();
    }
    class X : IClosable
    {
        public bool IsClosed { get; private set; }
        public void Close() => IsClosed = true;
    }
    class Y : IClosable
    {
        public bool IsClosed { get; private set; }
        public void Close() => IsClosed = true;
    }
} 
```

Additionally, you may provide condition parameter to select target service. 
For instance to filter out services already implementing `IDisposable`:
```cs
container.RegisterDisposer<IClosable>(closable => closable.Close(),
    condition: request => !request.GetKnownImplementationOrServiceType().IsAssignableTo<IDisposable>());
```

__NOTE__: The `RegisterDisposer` is just for adapt / compensate absence of `IDisposable` interface in third-party code. 
If you in control, please implement `IDisposable` interface instead - it will cleanly state the intent without any DI tool involved.
