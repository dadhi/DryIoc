<!--Auto-generated from .cs file, the edits here will be lost! -->

# Decorators

[TOC]

[FactoryMethod]:SelectConstructorOrFactoryMethod#markdown-header-factory-method-instead-of-constructor

## Overview

Decorator in DryIoc generally represents a
[Decorator Pattern](https://en.wikipedia.org/wiki/Decorator_pattern).
But in conjunction with other DryIoc features, especially with a [FactoryMethod],
the concept may be extended further to cover more scenarios.

DryIoc Decorators support:

- General decorating of service with adding functionality around decorated service calls.
- Applying decorator based on condition.
- Nesting decorators and specifying custom nesting order.
- Open-generic decorators with support of: generic variance, constraints, generic factory methods in generic classes.
- Decorator may have its own Reuse different from decorated service. There is an additional option for decorators to `useDecorateeReuse`.
- Decorator may decorate service wrapped in `Func`, `Lazy` and the rest of [Wrappers](Wrappers). Nesting is supported as well.
- Decorator registered with a [FactoryMethod] may be used to add functionality around decorated service creation, 
aka __Initializer__.
- Combining decorator reuse and factory method registration, decorator may provide additional action on service dispose, aka __Disposer__.
- With Factory Method and decorator condition, it is possible to register decorator of generic `T` service type. This opens possibility for more generic use-cases, e.g. EventAggregator with attributed subscribe.
- Combining Decorator with library like _Castle.DynamicProxy_ enables Interception and AOP support.


## General use-case

We start by defining the `IHandler` which we will decorate adding the logging capabilities:

```cs 
using DryIoc;

using NUnit.Framework;
using ExpressionToCodeLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
// ReSharper disable UnusedParameter.Local

public interface IHandler
{
    bool IsHandled { get; }
    void Handle();
}
public class FooHandler : IHandler 
{
    public bool IsHandled { get; private set; }
    public void Handle() => IsHandled = true;
}

public interface  ILogger
{
    void Log(string line);
}

class InMemoryLogger : ILogger
{
    public readonly List<string> Lines = new List<string>();
    public void Log(string line) => Lines.Add(line);
}

public class LoggingHandlerDecorator : IHandler 
{
    private readonly IHandler _handler;
    public readonly ILogger Logger;

    public LoggingHandlerDecorator(IHandler handler, ILogger logger)
    {
        _handler = handler;
        Logger = logger;
    }

    public bool IsHandled => _handler.IsHandled;

    public void Handle()
    {
        Logger.Log("About to handle");
        _handler.Handle();
        Logger.Log("Successfully handled");
    }
}

class Decorator_with_logger
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IHandler, FooHandler>();
        container.Register<ILogger, InMemoryLogger>();

        // decorator is the normal registration with `Decorator` setup 
        container.Register<IHandler, LoggingHandlerDecorator>(setup: Setup.Decorator);

        // resolved handler will be a decorator instance
        var handler = container.Resolve<IHandler>();
        Assert.IsInstanceOf<LoggingHandlerDecorator>(handler);

        handler.Handle();

        CollectionAssert.AreEqual(
            new[] { "About to handle", "Successfully handled" },
            ((handler as LoggingHandlerDecorator)?.Logger as InMemoryLogger)?.Lines);
    }
}
```

In most cases you only need to add `setup: Setup.Decorator` parameter. 
The rest of registration options are available for decorators.
Except for the `serviceKey` which is not supported. 
To apply decorator for service registered with `serviceKey` you need to specify a setup condition or
use a `Decorator.Of` method.

```cs 
class Decorator_of_keyed_service
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<ILogger, InMemoryLogger>();

        container.Register<IHandler, FooHandler>(serviceKey: "foo");

        container.Register<IHandler, LoggingHandlerDecorator>(
            setup: Setup.DecoratorWith(condition: request => "foo".Equals(request.ServiceKey)));

        container.Register<IHandler, LoggingHandlerDecorator>(
            setup: Setup.DecoratorOf(decorateeServiceKey: "foo")); // a condition in disguise

        Assert.IsInstanceOf<LoggingHandlerDecorator>(container.Resolve<IHandler>("foo"));
    }
} 
```

__Note:__ In the example above we are registering the decorator twice. 
It is OK because DryIoc supports decorator nesting. Read on to the next section for details.

## Nested Decorators

DryIoc supports decorator nesting. 
**The first registered** decorator will wrap the actual service, 
 and the subsequent decorators will wrap the already decorated objects.

```cs 
class S {}
class D1 : S { public D1(S s) {} }
class D2 : S { public D2(S s) {} }

class Nested_decorators
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<S>();
        container.Register<S, D1>(setup: Setup.Decorator);
        container.Register<S, D2>(setup: Setup.Decorator);

        var s = container.Resolve<S>();

        // s is created as `new D2(new D1(new S()))`
        Assert.IsInstanceOf<D2>(s);

        // ACTUALLY, you even can see how service is created yourself
        var expr = container.Resolve<LambdaExpression>(typeof(S));
        Assert.AreEqual("r => new D2(new D1(new S()))", ExpressionToCode.ToCode(expr));
    }
} 
```

### Decorators Order

The order of decorator nesting may be explicitly specified with `order` setup option:
```cs 
class Nested_decorators_order
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<S>();
        container.Register<S, D1>(setup: Setup.DecoratorWith(order: 2));
        container.Register<S, D2>(setup: Setup.DecoratorWith(order: 1));

        var s = container.Resolve<S>();
        Assert.IsInstanceOf<D1>(s);

        var expr = container.Resolve<LambdaExpression>(typeof(S));
        Assert.AreEqual("r => new D1(new D2(new S()))", ExpressionToCode.ToCode(expr));
    }
} 
```

The decorators without defined `order` have an implicit `order` value of `0`. Decorators with identical `order` are ordered by registration.
You can specify `-1`, `-2`, etc. order to insert decorator closer to decorated service.

__Note:__ The order does not prefer a specific decorator type, e.g. concrete, open-generic, or decorator of any generic `T` type. All decorators are ordered based on the same rules.


## Open-generic decorators

Decorators may be open-generic and registered to wrap open-generic services. 

```cs 
// constructors are skipped for brevity, they just accept A parameter
class S<T> {}
class D1<T> : S<T> {}
class D2<T> : S<T> {}
class DStr : S<string> {}

class Open_generic_decorators
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register(typeof(S<>));

        container.Register(typeof(S<>), typeof(D1<>), setup: Setup.Decorator);
        container.Register(typeof(S<>), typeof(D2<>), setup: Setup.Decorator);

        // decorator specific to the `S<string>` type
        container.Register<S<string>, DStr>(setup: Setup.Decorator);

        var intS = container.Resolve<S<int>>();
        Assert.IsInstanceOf<D2<int>>(intS); // won't use DStr decorator

        var strS = container.Resolve<S<string>>();
        Assert.IsInstanceOf<DStr>(strS);    // will use DStr decorator
    }
}
```

## Decorator of generic T

Decorator may be registered using a [FactoryMethod]. This brings an interesting question: what if the factory method is open-generic and returns a generic type `T`:
```cs 
public interface IStartable
{
    void Start();
}

public static class DecoratorFactory 
{
    public static T Decorate<T>(T service) where T : IStartable
    {
        service.Start();
        return service;
    }
}
```

Now we will use method `Decorate` to register decorator applied to any `T` service. 
For this we need to register a decorator of type `object`, cause `T` does not make sense outside of its defined method.
```cs 
public class X : IStartable
{
    public bool IsStarted { get; private set; }
    public void Start() => IsStarted = true;
}

class Decorator_of_generic_T_type
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<X>();

        // register with a service type of `object`
        container.Register<object>(
            made: Made.Of(req => typeof(DecoratorFactory)
                .SingleMethod(nameof(DecoratorFactory.Decorate))
                .MakeGenericMethod(req.ServiceType)),
            setup: Setup.Decorator);

        var x = container.Resolve<X>();
        Assert.IsTrue(x.IsStarted);
    }
}
```

The "problem" of `object` decorators that they will be applied to all services,
__affecting the resolution performance__. 

To make decorator more targeted, we can provide the setup condition:

```cs 
class Decorator_of_generic_T_type_with_condition
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<X>();

        var decorateMethod = typeof(DecoratorFactory)
            .SingleMethod(nameof(DecoratorFactory.Decorate));

        container.Register<object>(
            made: Made.Of(r => decorateMethod.MakeGenericMethod(r.ServiceType)),
            setup: Setup.DecoratorWith(r => r.ImplementationType.IsAssignableTo<IStartable>()));

        // or we may simplify the condition via `DecoratorOf` method:
        container.Register<object>(
            made: Made.Of(r => decorateMethod.MakeGenericMethod(r.ServiceType)),
            setup: Setup.DecoratorOf<IStartable>());

        var x = container.Resolve<X>();
        Assert.IsTrue(x.IsStarted);
    }
}
```

## Decorator Reuse

Decorator may have its own reuse the same way as a normal service. 
This way you may add a context-based reuse to already registered service just by applying the decorator.

__Note__: If no reuse specified for decorator, it means the decorator is of default container reuse `container.Rules.DefaultReuse` 
(which is `Reuse.Transient` by default).

```cs 
class Decorator_reuse
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<S>();

        // singleton decorator effectively makes the decorated service a singleton too.
        container.Register<S, D1>(Reuse.Singleton, setup: Setup.Decorator);

        var s1 = container.Resolve<S>();
        var s2 = container.Resolve<S>();
        Assert.AreSame(s1, s2);
    }
}
```

### UseDecorateeReuse

This setup option allows decorator to match the decorated service reuse whatever it might be. 
It is a good "default" option when you don't want to adjust decorated service reuse, but just want it to follow along. 
```cs 
class Decoratee_reuse
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<S>(Reuse.Singleton);
        container.Register<S, D1>(setup: Setup.DecoratorWith(useDecorateeReuse: true));

        var s1 = container.Resolve<S>();
        var s2 = container.Resolve<S>();
        Assert.AreSame(s1, s2); // because of decoratee service reuse.
    }
}
```


## Decorator of Wrapped Service

Decorator may be applied to the [wrapped](Wrappers) service in order to provide laziness, create decorated service on demand, proxy-ing, etc.
```cs 
class ACall
{
    public virtual void Call() {}
}

class LazyCallDecorator : ACall
{ 
    public Lazy<ACall> A;
    public LazyCallDecorator(Lazy<ACall> a) { A = a; }
    
    // Creates an object only when calling it.
    public override void Call() => A.Value.Call();
}

class Decorator_of_wrapped_service
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<ACall>();
        container.Register<ACall, LazyCallDecorator>(setup: Setup.Decorator);

        var a = container.Resolve<ACall>(); // A is not created.
        a.Call();                           // A is created and called.
    }
}
```

Decorators of wrappers may be nested the same way as decorators of normal services:
```cs 
class DLazy : S
{
    public Lazy<S> S { get; }
    public DLazy(Lazy<S> s) { S = s; }
}

class DFunc : S
{
    public Func<S> S { get; }
    public DFunc(Func<S> s) { S = s; }
}

class Nesting_decorators_of_wrapped_service
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<S>();
        container.Register<S, DLazy>(setup: Setup.Decorator);
        container.Register<S, DFunc>(setup: Setup.Decorator);

        var s = container.Resolve<S>(); // `s` is wrapped in Func with DFunc decorator

        s = ((DFunc)s).S();             // then its wrapped in Lazy with DLazy decorator
        s = ((DLazy)s).S.Value;         // an actual decorated `S` object
        Assert.IsInstanceOf<S>(s);
    }
}
```


## Decorator of Wrapper

DryIoc supports decorating of wrappers directly to adjust corresponding wrapper behavior, to add new functionality, to apply filtering, etc.

Consider the decorating of [collection wrapper](Wrappers#markdown-header-ienumerable-or-array-of-a).
Let`s say we want to change the default collection behavior and exclude keyed services from the result:

```cs 
public interface I { }
public class A : I { }
public class B : I { }
public class C : I { }

class Collection_wrapper_of_non_keyed_and_keyed_services
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<I, A>();
        container.Register<I, B>();
        container.Register<I, C>(serviceKey: "test");

        // by default collection will contain instances of all registered types
        var iis = container.Resolve<IEnumerable<I>>();
        CollectionAssert.AreEqual(new[]{ typeof(A), typeof(B), typeof(C) },
            iis.Select(i => i.GetType()));
    }
} 
```

To exclude keyed service `C` we may define the decorator for `IEnumerable<I>`, 
which accepts all instances and filters out the keyed things:

```cs 
class Decorator_of_wrapper
{
    public static IEnumerable<I> GetNoneKeyed(IEnumerable<KeyValuePair<object, I>> all) =>
        all.Where(kv => kv.Key is DefaultKey).Select(kv => kv.Value);

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<I, A>();
        container.Register<I, B>();
        container.Register<I, C>(serviceKey: "test");

        // a collection decorator to filter out keyed services, a `C` with `"test"` key
        container.Register<IEnumerable<I>>(
            Made.Of(() => GetNoneKeyed(Arg.Of<IEnumerable<KeyValuePair<object, I>>>())),
            setup: Setup.Decorator);

        var iis = container.Resolve<IEnumerable<I>>();
        CollectionAssert.AreEqual(new[] { typeof(A), typeof(B) },
            iis.Select(i => i.GetType()));
    }
}
```

__Note:__ By decorating the `IEnumerable<T>` we are automatically decorating the array of `T[]` as well.


## Decorator as Initializer

When registering decorator with [FactoryMethod], it is possible to __decorate an initialization pipeline of a service__. 

It means that decorator factory method accepts injected decorated service, 
invokes some initialization logic on the decorated instance, 
and returns this (or another) instance.

```cs 

public class Foo 
{
    public string Message { get; set; }
}

class Decorator_as_initializer
{
    public static Foo DecorateFooWithGreet(Foo foo)
    {
        foo.Message = "Hello, " + (foo.Message ?? "");
        return foo;
    }

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Foo>();
        container.Register<Foo>(
            Made.Of(() => DecorateFooWithGreet(Arg.Of<Foo>())),
            setup: Setup.Decorator);

        var foo = container.Resolve<Foo>();
        StringAssert.Contains("Hello", foo.Message);
    }
} 
```

Here `DecorateFooWithGreet` is a static method just for the demonstration. 
It also may be a non-static and include additional dependencies to be injected by Container, 
check the [FactoryMethod] for more details.

DryIoc has a dedicated [`RegisterInitializer`](RegisterResolve#markdown-header-registerinitializer) method,
which is a decorator in disguise.

Moreover, to complement the `RegisterInitializer` there is also a [`RegisterDisposer`](RegisterResolve#markdown-header-registerdisposer).


## Decorator as Interceptor with Castle DynamicProxy

[Explained here](Interception#markdown-header-decorator-with-castle-dynamicproxy)
