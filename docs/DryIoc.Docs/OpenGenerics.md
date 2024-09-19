<!--Auto-generated from .cs file, the edits here will be lost! -->

# Open-generics


- [Open-generics](#open-generics)
  - [Registering open-generic service](#registering-open-generic-service)
  - [Matching type arguments constraints](#matching-type-arguments-constraints)
    - [Filter services in collection based on constraints](#filter-services-in-collection-based-on-constraints)
    - [Fill-in type arguments from constraints](#fill-in-type-arguments-from-constraints)
  - [Generic variance when resolving many services](#generic-variance-when-resolving-many-services)


## Registering open-generic service

Registering open-generic is no different from the non-generic service. 
The only limitation is imposed by C# itself - it is impossible to specify type statically, you need to use `typeof`.

```cs 
namespace DryIoc.Docs;
using System;
using System.Linq;
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedTypeParameter

public class Register_open_generic
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register(typeof(ICommand<>), typeof(DoSomethingCommand<>));

        var cmd = container.Resolve<ICommand<MyData>>();
        Assert.IsInstanceOf<DoSomethingCommand<MyData>>(cmd);
    }

    interface ICommand<T> { }
    class DoSomethingCommand<T> : ICommand<T> { }
    struct MyData { }
}
```

The rest of API is identical. Here the variants:
```cs 
public class Open_generic_registrations
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register(typeof(Command<>));

        container.Register(typeof(Command<>), Reuse.Singleton);

        container.Register(typeof(ICommand<>), typeof(Command<>), 
            ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation,
            serviceKey: "blah");

        container.Register(typeof(ICommand<>), typeof(LoggingCommand<>), 
            setup: Setup.Decorator);

        // etc.
    }

    interface ICommand<T> { }
    class Command<T> : ICommand<T> { }
    class LoggingCommand<T> : ICommand<T> { }
}
```

When resolving a single service a registered closed service has a priority over the corresponding open-generic service implementation:
```cs 
public class Closed_is_preferred_over_open_generic
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<A<int>, BInt>();
        container.Register(typeof(A<>), typeof(B<>));

        var a = container.Resolve<A<int>>(); // will return `BInt` instead of `B<int>`
        Assert.IsInstanceOf<BInt>(a);

        // Resolving as collection will return both `BInt` instead of `B<int>`
        var items = container.Resolve<A<int>[]>();
        Assert.AreEqual(2, items.Length);
    }

    class A<T> { }
    class BInt : A<int> { }
    class B<T> : A<T> { }
}
```

## Matching type arguments constraints

DryIoc will evaluate type argument constraints when resolving open-generic. Let's review specific cases where it may be useful

### Filter services in collection based on constraints

Example:

```cs 
public class Matching_open_generic_type_constraints
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.RegisterMany(new[] { typeof(A<>), typeof(B<>) }, nonPublicServiceTypes: true);

        var items = container.Resolve<I<string>[]>();

        // The only result item will be of type `B<string>` 
        // An `A<T>` was filtered out because `string` is not matching to a `IDisposable` constraint.
        Assert.AreEqual(1, items.Length);
        Assert.IsInstanceOf<B<string>>(items[0]);
    }

    interface I<T> { }
    class A<T> : I<T> where T : IDisposable { }
    class B<T> : I<T> { }
} 
```


### Fill-in type arguments from constraints

Example:

```cs 
public class Fill_in_type_arguments_from_constraints
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register(typeof(ICommandHandler<>), typeof(UpdateCommandHandler<,>));

        var handler = container.Resolve<ICommandHandler<UpdateCommand<MyEntity>>>();

        Assert.IsInstanceOf<UpdateCommandHandler<MyEntity, UpdateCommand<MyEntity>>>(handler);
    }

    public interface ICommandHandler<TCommand> { }
    public class SpecialEntity { }
    public class UpdateCommand<TEntity> { }

    public class UpdateCommandHandler<TEntity, TCommand> : ICommandHandler<TCommand>
        where TEntity : SpecialEntity
        where TCommand : UpdateCommand<TEntity>
    { }

    public class MyEntity : SpecialEntity { }
} 
```

In this example DryIoc is the smart enough to use `MyEntity` as `UpdateCommandHandler` first type argument, 
given the rules defined by constraints.

__Note:__ This example is not so uncommon in the modern world, say in [MediatR](https://github.com/jbogard/MediatR).


## Generic variance when resolving many services

When resolving the collection of generic types DryIoc will include variance-compatible types:
```cs 
public class Generic_variance_thingy
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<IHandler<A>, AHandler>();
        container.Register<IHandler<B>, BHandler>();

        // get all handlers of A
        var aHandlers = container.ResolveMany<IHandler<A>>();

        // Result contains both `AHandler` and `BHandler`, 
        // because `IHandler<B>` is assignable to `IHandler<A>` due variance rules
        Assert.AreEqual(2, aHandlers.Count());

        // Result contains only `BHandler`
        var bHandlers = container.ResolveMany<IHandler<B>>();
        Assert.AreEqual(1, bHandlers.Count());
    }

    public interface IHandler<out TEvent> { } // covariant handler
    public class A { }
    public class B : A { }
    public class AHandler : IHandler<A> { }
    public class BHandler : IHandler<B> { }
} 
```

This rule is enabled by default, but you can turn it off:
```cs 
public class Turn_off_generic_variance_in_collections
{
    [Test]
    public void Example()
    {
        var container = new Container(rules =>
            rules.WithoutVariantGenericTypesInResolvedCollection());

        container.Register<IHandler<A>, AHandler>();
        container.Register<IHandler<B>, BHandler>();

        // the same setup, but result contains `AHandler` only
        var aHandlers = container.ResolveMany<IHandler<A>>();
        Assert.AreEqual(1, aHandlers.Count());
    }

    public interface IHandler<out TEvent> { } // covariant handler
    public class A { }
    public class B : A { }
    public class AHandler : IHandler<A> { }
    public class BHandler : IHandler<B> { }
} 
```

