/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Open-generics

[TOC]

## Registering open-generic service

Registering open-generic is no different from the non-generic service. 
The only limitation is imposed by C# itself - it is impossible to specify type statically, you need to use `typeof`.
```cs md*/
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedTypeParameter

class Register_open_generic
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
/*md
```

The rest of API is identical. Here the variants:
```cs md*/
class Open_generic_registrations
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
}/*md
```

When resolving a single service a registered closed service has a priority over the corresponding open-generic service implementation:
```cs md*/
class Closed_is_preferred_over_open_generic
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
/*md
```

## Matching type arguments constraints

DryIoc will evaluate type argument constraints when resolving open-generic. Let's review specific cases where it may be useful

### Filter services in collection based on constraints

Example:
```
#!c#
    public interface I<T> { }
    public A<T> : I<T> where T : IDisposable { }
    public B<T> : I<T> { }
    
    container.RegisterMany(new[] { typeof(A<>), typeof(B<>) });
    
    var items = container.Resolve<IEnumerable<I<string>>>();
    
    // The only result item will be of type B<> 
    // An A<> was filtered out because string is matching to IDisposable constraint.
    Assert.IsInstanceOf<B<string>>(items.Single()); 
```


### Fill-in type arguments from constraints

Example:

```
#!c#
    public interface ICommandHandler<TCommand> { }
    public class SpecialEntity { }
    public class UpdateCommand<TEntity> { }
    
    public class UpdateCommandHandler<TEntity, TCommand> : ICommandHandler<TCommand>
        where TEntity : SpecialEntity
        where TCommand : UpdateCommand<TEntity> { }
    
    public class MyEntity : SpecialEntity { }
    
    [Test]
    public void Can_fill_in_type_argument_from_constraint()
    {
        var container = new Container();
        container.Register(typeof(ICommandHandler<>), typeof(UpdateCommandHandler<,>));
    
        var handler = container.Resolve<ICommandHandler<UpdateCommand<MyEntity>>>();
    
        Assert.IsInstanceOf<UpdateCommandHandler<MyEntity, UpdateCommand<MyEntity>>>(handler);
    }
```

In this example DryIoc is smart enough to use `MyEntity` as `UpdateCommandHandler` first type argument, given rules provided by constraints.

__Note:__ This example is not so uncommon in a modern world, e.g. [MediatR](https://github.com/jbogard/MediatR).


## Generic variance when resolving many services

When resolving many or collection of generic types DryIoc will include variance-compatible types. By example:

    public interface IHandler<out TEvent> {} // covariant handler

    public class A {}
    public class B : A {}

    public class AHandler : IHandler<A> {}
    public class BHandler : IHandler<B> {}

    // register handlers
    container.Register<IHandler<A>, AHandler>();
    container.Register<IHandler<B>, BHandler>();

    // get all handlers of A
    var ahandlers = container.ResolveMany<IHandler<A>>();

    // Result contains both AHandler and BHandler, 
    // because IHandler<B> is assignable to IHandler<A> due variance rules
    Assert.AreEqual(2, ahandlers.Count());

    // Result contains only BHandler
    var bhandlers = container.ResolveMany<IHandler<B>>();
    Assert.AreEqual(1, bhandlers.Count());

This rule is enabled by default. To turn it off:

    var container = new Container(rules =>
        rules.WithoutVariantGenericTypesInResolvedCollection());

    // the same setup ...
    // but result contains AHandler only
    var ahandlers = container.ResolveMany<IHandler<A>>();
    Assert.AreEqual(1, ahandlers.Count());
md*/
