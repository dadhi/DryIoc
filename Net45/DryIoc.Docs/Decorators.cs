/*md
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

We start with defining the `IHandler` which we will decorate adding the logging capabilities:

```cs md*/
using DryIoc;

using NUnit.Framework;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionToCodeLib;
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
/*md
```

In most cases you only need to add `setup: Setup.Decorator` parameter. 
The rest of registration options are available for decorators.
Except for the `serviceKey` which is not supported. 
To apply decorator for service registered with `serviceKey` you need to specify a setup condition or
use a `Decorator.Of` method.

```cs md*/
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
/*md
```

__Note:__ In the example above we are registering the decorator twice. 
It is OK because DryIoc supports decorator nesting. Read on to the next section for details.

## Nested Decorators

DryIoc supports decorator nesting. 
**The first registered** decorator will wrap the actual service,
 * and the subsequent decorators will wrap the already decorated objects.

```cs md*/
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
} /*md
```

### Decorators Order

The order of decorator nesting may be explicitly specified with `order` setup option:
```cs md*/
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
} /*md
```

The decorators without defined `order` have an implicit `order` value of `0`. Decorators with identical `order` are ordered by registration.
You can specify `-1`, `-2`, etc. order to insert decorator closer to decorated service.

__Note:__ The order does not prefer a specific decorator type, e.g. concrete, open-generic, or decorator of any generic `T` type. All decorators are ordered based on the same rules.


## Open-generic decorators

Decorators may be open-generic and registered to wrap open-generic services. 

```cs md*/
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
}/*md
```

## Decorator of generic T

Decorator may be registered using a [FactoryMethod]. This brings an interesting question: what if the factory method is open-generic and returns a generic type `T`:
```cs md*/
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
}/*md
```

Now we will use method `Decorate` to register decorator applied to any `T` service. 
For this we need to register a decorator of type `object`, cause `T` does not make sense outside of its defined method.
```cs md*/
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
}/*md
```

The "problem" of `object` decorators that they will be applied to all services,
__affecting the resolution performance__. To make decorator more targeted, we can provide the setup condition.

Adding condition to apply decorator only to `IStartable` services:

```cs md*/
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
}/*md
```

## Decorator Reuse

Decorator may have its own reuse the same way as a normal service. 
This way you may add a context-based reuse to already registered service just by applying the decorator.

__Note__: If no reuse specified for decorator, it means the decorator is of default container reuse `container.Rules.DefaultReuse` (if not changed it is a `Reuse.Transient`).

```csharp
container.Register<A>();

// singleton decorator effectively makes the decorated service a singleton too 
container.Register<A, D>(Reuse.Singleton, setup: Setup.Decorator);

var a1 = container.Resolve<A>();
var a2 = container.Resolve<A>();
Assert.AreSame(a1, a2);
```


### UseDecorateeReuse

This setup option allows decorator to match the decorated service reuse whatever it might be. 
It is a good "default" option when you don't want to adjust decorated service reuse, but just want it to follow along. 
```csharp

container.Register<A>(Reuse.Singleton);
container.Register<A, D>(setup: Setup.DecoratorWith(useDecorateeReuse: true));

var a1 = container.Resolve<A>();
var a2 = container.Resolve<A>();
Assert.AreSame(a1, a2); // because of decorated service reuse.
```


## Decorator of Wrapped Service

Decorator may be applied to [wrapped](Wrappers) service in order to provide laziness, create decorated service on demand, proxy-ing, etc.
```csharp
class A { virtual void Call() {} }
class LazyADecorator : A
{ 
    public Lazy<A> _a;
    public LazyADecorator(Lazy<A> a) { _a = a; }
    
    public override void Call() 
    {
        // gets A value lazily on demand:
        _a.Value.Call();
    }
}

container.Register<A>();

// register as usual
container.Register<A, LazyADecorator>(setup: Setup.Decorator);

var a = container.Resolve<A>(); // A is not created.
a.Call(); // A is created and called.
```

Decorators of wrappers may be nested the same way as for normal services:
```csharp
public class AsLazy : A
{
    public AsLazy(Lazy<A> a) { A = a; }
}

public class AsFunc : A
{
    public AsLazy(Func<A> a) { A = a; }
}

container.Register<A>();
container.Register<AsLazy, A>(setup: Setup.Decorator);
container.Register<AsFunc, A>(setup: Setup.Decorator);

var a = container.Resolve<A>() // wrapped in AsFunc
a = ((AsFunc)a).A();           // wrapped in AsLazy
a = ((AsLazy)a).A.Value;       // actual decorated A
```


## Decorator of Wrapper

DryIoc supports decorating of wrappers directly to adjust corresponding wrapper behavior, to add new functionality, to apply filtering, etc.

Consider the decorating of [collection wrapper](Wrappers#markdown-header-ienumerable-or-array-of-a).
Let`s say we want to change the default collection behavior and exclude keyed services from the result:

```csharp
container.Register<I, A>();
container.Register<I, B>();
container.Register<I, C>(serviceKey: "test");

// by default will return instances of A, B, C
var iis = container.Resolve<IEnumerable<I>>();
```

To exclude keyed service `C` we may define the decorator for `IEnumerable<I>`, which accepts all instances and filter out the keyed things:

```csharp
public static class Decorators
{
    public static IEnumerable<I> FilterOutKeyed(IEnumerable<KeyValuePair<object, I>> all) =>
        all.Where(kv => kv.Key is DryIoc.DefaultKey).Select(kv => kv.Value);
}

// a collection decorator
container.Register<IEnumerable<I>>(
    Made.Of(() => Decorators.FilterOutKeyed(Arg.Of<IEnumerable<KeyValuePair<object, I>>())), 
    setup: Setup.Decorator);
```

__Note:__ By decorating the `IEnumerable<T>` we automatically decorating the array of `T[]` as well.

## Decorator as Initializer

When registering decorator with [FactoryMethod], it is possible to __decorate the creation of a service__. 

It means that decorator factory method accepts injected decorated service, 
invokes some initialization logic on the decorated instance, 
and returns this (or another) instance.

```csharp
public class Foo : IFoo 
{
    public string Message { get; set; }
}

public static class FooMiddleware 
{
    public static IFoo Greet(IFoo foo) 
    {
        foo.Message = "Hello, " + (foo.Message ?? "");
        return foo;
    }
}

container.Register<IFoo, Foo>();
container.Register<IFoo>(
    Made.Of(() => FooMiddleware.Greet(Arg.Of<IFoo>())),
    setup: Setup.Decorator)

var foo = container.Resolve<IFoo>();
StringAssert.Contains("Hello", foo.Message);
```

Here `FooMiddleware.Greet` is a static method just for the demonstration. 
It also may be a non-static and include additional dependencies to be injected by Container, check the [FactoryMethod] docs for more details.

DryIoc has a dedicated `RegisterInitializer` [method](RegisterResolve#markdown-header-registerinitializer) which is a decorator in disguise. Here is the implementation of register initializer to illustrate the idea:

```csharp
// Step 1:
// Define the decorator factory which accepts user initialize action.
// The actual decorator method is Decorate which just invokes stored action and 
// returns decorated service instance.
internal sealed class InitializerFactory<TTarget>
{
    private readonly Action<TTarget, IResolver> _initialize;

    public InitializerFactory(Action<TTarget, IResolver> initialize)
    {
        _initialize = initialize;
    }

    public TService Decorate<TService>(TService service, IResolver resolver)
        where TService : TTarget
    {
        _initialize(service, resolver);
        return service;
    }
}

// Step 2: Register the decorator factory and decorator produced by Decorate method.
// unique key to bind decorator factory and decorator registrations
var decoratorFactoryKey = new object();

// register decorator factory and identify it with the key
registrator.RegisterDelegate(
    _ => new InitializerFactory<TTarget>(initialize),
    serviceKey: decoratorFactoryKey);

// decorator condition to make decorator applied only to TTarget service type.
Func<RequestInfo, bool> decoratorCondition = r => 
    (r.ImplementationType ?? r.GetActualServiceType()).IsAssignableTo(typeof(TTarget));

// register decorator with Decorate method of factory identified with the key
registrator.Register<object>(
    made: Made.Of(request => FactoryMethod.Of(
        typeof(InitializerFactory<TTarget>).GetSingleMethodOrNull("Decorate").MakeGenericMethod(request.ServiceType),
        ServiceInfo.Of<InitializerFactory<TTarget>>(serviceKey: decoratorFactoryKey))),
    setup: Setup.DecoratorWith(decoratorCondition, useDecorateeReuse: true));
```

The example illustrations use of decorator of generic type `T`, and use of instance factory method, as well as `useDecorateeReuse`.


## Decorator as Disposer

DryIoc supports a [dispose action](RegisterResolve#markdown-header-registerdisposer) for the service that does not implement as `IDisposable` (or a custom action for `IDisposable`).

As you may predict. a disposer is also implemented as decorator:

```csharp
// Step 1:
// Define decorator factory to store dispose action 
// and actual decorator method "TrackForDispose"
internal sealed class Disposer<T> : IDisposable
{
    private readonly Action<T> _dispose;
    private int _state;
    private const int Tracked = 1, Disposed = 2;
    private T _item;

    public Disposer(Action<T> dispose)
    {
        _dispose = dispose.ThrowIfNull();
    }

    public T TrackForDispose(T item)
    {
        if (Interlocked.CompareExchange(ref _state, Tracked, 0) != 0)
            Throw.It(Error.Of("Something is {0} already."), _state == Tracked ? " tracked" : "disposed");
        _item = item;
        return item;
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _state, Disposed, Tracked) != Tracked)
            return;
        var item = _item;
        if (item != null)
        {
            _dispose(item);
            _item = default(T);
        }
    }
}

// Step 2:
// Register factory and decorator via factory method
var disposerKey = new object();

registrator.RegisterDelegate(_ => new Disposer<TService>(dispose), 
    serviceKey: disposerKey,
    setup: Setup.With(useParentReuse: true));

registrator.Register(Made.Of(
    r => ServiceInfo.Of<Disposer<TService>>(serviceKey: disposerKey),
    f => f.TrackForDispose(Arg.Of<TService>())),
    setup: Setup.DecoratorWith(condition, useDecorateeReuse: true));
```

The example illustrates use of instance factory method for registering decorator, `useDecorateeReuse`, and `useParentReuse` for the factory.


## Decorator as Interceptor with Castle DynamicProxy

[Explained here](Interception#markdown-header-decorator-with-castle-dynamicproxy)
md*/
