/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Decorators


- [Decorators](#decorators)
  - [Overview](#overview)
  - [General use-case](#general-use-case)
  - [Decorator of service registered with serviceKey](#decorator-of-service-registered-with-servicekey)
    - [Using Setup.DecoratorOf](#using-setupdecoratorof)
  - [Nested Decorators](#nested-decorators)
    - [Decorators Order](#decorators-order)
  - [Open-generic decorators](#open-generic-decorators)
  - [Decorator of generic T](#decorator-of-generic-t)
  - [Decorator Reuse](#decorator-reuse)
    - [UseDecorateeReuse](#usedecorateereuse)
  - [Decorator of Wrapped Service](#decorator-of-wrapped-service)
  - [Decorator of Wrapper](#decorator-of-wrapper)
  - [Decorator as Initializer](#decorator-as-initializer)
  - [Decorator as Interceptor with Castle DynamicProxy](#decorator-as-interceptor-with-castle-dynamicproxy)
  - [Doing the interesting things with decorators](#doing-the-interesting-things-with-decorators)
    - [Reusing the scoped service from the parent scope](#reusing-the-scoped-service-from-the-parent-scope)
    - [Using the Decorator directly for the complex initialization](#using-the-decorator-directly-for-the-complex-initialization)


[FactoryMethod]:SelectConstructorOrFactoryMethod.md#factory-method-instead-of-constructor

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
- Decorator may decorate service wrapped in `Func`, `Lazy` and the rest of [Wrappers](Wrappers.md). Nesting is supported as well.
- Decorator registered with a [FactoryMethod] may be used to add functionality around decorated service creation, 
aka __Initializer__.
- Combining decorator reuse and factory method registration, decorator may provide additional action on service dispose, aka __Disposer__.
- With Factory Method and decorator condition, it is possible to register decorator of generic `T` service type. This opens possibility for more generic use-cases, e.g. EventAggregator with attributed subscribe.
- Combining Decorator with library like _Castle.DynamicProxy_ enables Interception and AOP support.


## General use-case

We start by defining the `IHandler` which we will decorate adding the logging capabilities:
md*/
//md{ usings ...
//md```cs
namespace DryIoc.Docs;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DryIoc;
using DryIoc.FastExpressionCompiler.LightExpression; // light alternative to the System.Linq.Expressions
// ReSharper disable UnusedParameter.Local
//md```
//md}

//md```cs
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

public interface ILogger
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

public class Decorator_with_logger
{
    [Test]
    public void Example()
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

## Decorator of service registered with serviceKey

In most cases you only need to add `setup: Setup.Decorator` parameter. 
The rest of registration options are available for decorators as for the normal services.
Except for the `serviceKey`, which is not supported. The `serviceKey` is not available because 
really you don't want to register decorator with the `serviceKey`, instead you want decorator
to decorate service registered with specific `serviceKey`.

To apply decorator for service registered with `serviceKey` you need to specify a setup condition or
use a `Decorator.Of` method.

```cs md*/
public class Decorator_of_service_with_serviceKey
{
    [Test]
    public void Example_with_condition()
    {
        var container = new Container();

        container.Register<ILogger, InMemoryLogger>();
        container.Register<IHandler, FooHandler>(serviceKey: "foo");

        container.Register<IHandler, LoggingHandlerDecorator>(
            setup: Setup.DecoratorWith(condition: request => "foo".Equals(request.ServiceKey)));

        var foo = container.Resolve<IHandler>("foo");
        Assert.IsInstanceOf<LoggingHandlerDecorator>(foo);
    }

    [Test]
    public void Example_with_DecoratorOf()
    {
        var container = new Container();

        container.Register<ILogger, InMemoryLogger>();
        container.Register<IHandler, FooHandler>(serviceKey: "foo");

        container.Register<IHandler, LoggingHandlerDecorator>(
            setup: Setup.DecoratorOf(decorateeServiceKey: "foo")); // a condition in disguise

        var foo = container.Resolve<IHandler>("foo");
        Assert.IsInstanceOf<LoggingHandlerDecorator>(foo);
    }
}/*md
```

### Using Setup.DecoratorOf

`DecoratorOf` is just a `DecoratorWith(condition: ...)` wrapped in a 
more simple API to specify a decorated service key or/and type. 

```cs md*/
public class Decorator_of_service_with_key_and_type
{
    class BooHandler : IHandler
    {
        public bool IsHandled { get; private set; }
        public void Handle() => IsHandled = true;
    }

    [Test]
    public void Example_with_DecoratorOf_type_and_key()
    {
        var container = new Container();

        container.Register<ILogger, InMemoryLogger>();
        container.Register<IHandler, FooHandler>(serviceKey: "foo");
        container.Register<IHandler, BooHandler>(serviceKey: "boo");

        // Decorating `Boo` with key "boo"
        container.Register<IHandler, LoggingHandlerDecorator>(
            setup: Setup.DecoratorOf<BooHandler>(decorateeServiceKey: "boo"));

        var boo = container.Resolve<IHandler>("boo");
        Assert.IsInstanceOf<LoggingHandlerDecorator>(boo);
    }

    [Test]
    public void Example_with_DecoratorOf_type()
    {
        var container = new Container();

        container.Register<ILogger, InMemoryLogger>();
        container.Register<IHandler, FooHandler>();
        container.Register<IHandler, BooHandler>(serviceKey: "boo");

        // Decorating `Boo`
        container.Register<IHandler, LoggingHandlerDecorator>(
            setup: Setup.DecoratorOf<BooHandler>());

        var boo = container.Resolve<IHandler>("boo");
        Assert.IsInstanceOf<LoggingHandlerDecorator>(boo);
    }
}/*md
```


## Nested Decorators

DryIoc supports decorator nesting. 
**The first registered** decorator will wrap the actual service, 
 and the subsequent decorators will wrap the already decorated objects.

```cs md*/
class S { }
class D1 : S { public D1(S s) { } }
class D2 : S { public D2(S s) { } }

public class Nested_decorators
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
        StringAssert.Contains("new D2(new D1(new S()))", expr.ToString());
    }
} /*md
```

### Decorators Order

The order of decorator nesting may be explicitly specified with `order` setup option:
```cs md*/
public class Nested_decorators_order
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
        StringAssert.Contains("new D1(new D2(new S()))", expr.ToString());
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
class S<T> { }
class D1<T> : S<T> { }
class D2<T> : S<T> { }
class DStr : S<string> { }

public class Open_generic_decorators
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

public class Decorator_of_generic_T_type
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
__affecting the resolution performance__. 

To make decorator more targeted, we can provide the setup condition:

```cs md*/
public class Decorator_of_generic_T_type_with_condition
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

__Note__: If no reuse specified for decorator, it means the decorator is of default container reuse `container.Rules.DefaultReuse` 
(which is `Reuse.Transient` by default).

```cs md*/
public class Decorator_reuse
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
}/*md
```

### UseDecorateeReuse

This setup option allows decorator to match the decorated service reuse whatever it might be. 
It is a good "default" option when you don't want to adjust decorated service reuse, but just want it to follow along. 
```cs md*/
public class Decoratee_reuse
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
}/*md
```


## Decorator of Wrapped Service

Decorator may be applied to the [wrapped](Wrappers.md) service in order to provide laziness, create decorated service on demand, proxy-ing, etc.
```cs md*/
class ACall
{
    public virtual void Call() { }
}

class LazyCallDecorator : ACall
{
    public Lazy<ACall> A;
    public LazyCallDecorator(Lazy<ACall> a) { A = a; }

    // Creates an object only when calling it.
    public override void Call() => A.Value.Call();
}

public class Decorator_of_wrapped_service
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
}/*md
```

Decorators of wrappers may be nested the same way as decorators of normal services:
```cs md*/
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

public class Nesting_decorators_of_wrapped_service
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
}/*md
```


## Decorator of Wrapper

DryIoc supports decorating of wrappers directly to adjust corresponding wrapper behavior, to add new functionality, to apply filtering, etc.

Consider the decorating of [collection wrapper](Wrappers.md#ienumerable-or-array-of-a).
Let`s say we want to change the default collection behavior and exclude keyed services from the result:

```cs md*/
public interface I { }
public class A : I { }
public class B : I { }
public class C : I { }

public class Collection_wrapper_of_non_keyed_and_keyed_services
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
        CollectionAssert.AreEqual(new[] { typeof(A), typeof(B), typeof(C) },
            iis.Select(i => i.GetType()));
    }
} /*md
```

To exclude keyed service `C` we may define the decorator for `IEnumerable<I>`, 
which accepts all instances and filters out the keyed things:

```cs md*/
public class Decorator_of_wrapper
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
}/*md
```

__Note:__ By decorating the `IEnumerable<T>` we are automatically decorating the array of `T[]` as well.


## Decorator as Initializer

When registering decorator with [FactoryMethod], it is possible to __decorate an initialization pipeline of a service__. 

It means that decorator factory method accepts injected decorated service, 
invokes some initialization logic on the decorated instance, 
and returns this (or another) instance.

```cs md*/
public class Decorator_as_initializer
{
    public class Foo
    {
        public string Message { get; set; }
    }

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
} /*md
```

Here `DecorateFooWithGreet` is a static method just for the demonstration. 
It also may be a non-static and include additional dependencies to be injected by Container, 
check the [FactoryMethod] for more details.

DryIoc has a dedicated [`RegisterInitializer`](RegisterResolve.md#registerinitializer) method,
which is a decorator in disguise.

Moreover, to complement the `RegisterInitializer` there is also a [`RegisterDisposer`](RegisterResolve.md#registerdisposer).


## Decorator as Interceptor with Castle DynamicProxy

[Explained here](Interception.md#decorator-with-castle-dynamicproxy)


## Doing interesting things with decorators

### Reusing a scoped service from the parent scope

Originated from the issue [#333](https://github.com/dadhi/DryIoc/issues/333).

Let's imagine that we have a scoped service and number of the nested scopes,
and we want the already created service from the parent scope instead of creating the new one in the child scope:

```cs md*/
public class Reusing_a_scoped_service_from_the_parent_scope
{
    [Test]
    public void Example()
    {
        var c = new Container();

        // Generic wrapper to extract the FactoryID, we need it to lookup the scope for the created service
        c.Register(typeof(FactoryInfo<>),
           new ExpressionFactory(
               req =>
               {
                   var wrapperType = req.ServiceType;
                   var serviceType = wrapperType.GetGenericArguments()[0];
                   var factory = req.Container.ResolveFactory(req.Push(serviceType));
                   return Expression.New(wrapperType.SingleConstructor(), Expression.Constant(factory.FactoryID));
               },
               setup: Setup.Wrapper));

        // Generic decorator applied to all Scoped services (see condition)
        c.Register<object>(
            made: Made.Of(req => GetType().SingleMethod(nameof(GetFromParentOrCurrent)).MakeGenericMethod(req.ServiceType)),
            setup: Setup.DecoratorWith(condition: req => req.Reuse == Reuse.Scoped));

        // Application code
        c.Register<A>(Reuse.Scoped);

        var s = c.OpenScope();
        var a = s.Resolve<A>();

        var ss = s.OpenScope();
        var aa = ss.Resolve<A>();
        Assert.AreSame(aa, a);

        // health check that the other services are not affected
        c.Register<B>(Reuse.Singleton);
        Assert.IsNotNull(c.Resolve<B>());
    }

    // Application services
    class A { }
    class B { }

    public class FactoryInfo<T>
    {
        public int Id;
        public FactoryInfo(int id) => Id = id;
    }

    public static T GetFromParentOrCurrent<T>(Func<IResolverContext, T> fd, FactoryInfo<T> fi, IResolverContext r)
    {
        var id = fi.Id;
        for (var s = r.CurrentScope; s != null; s = s.Parent)
            if (s.TryGet(out var res, id))
                return (T)res;
        return fd(r);
    }
} /*md
```

### Using the Decorator directly for the complex initialization

When you have a complex initialization scenario at your hands where `RegisterInitializer` behavior does not fit, 
you may remember that `RegisterInitializer` is just a Decorator in disguise. So we may use it directly.

Especially, because the features of DryIoc are greatly composable - you can decorate wrapped services, 
inject the additional services (either into the decorator constructor or into the method or into both!), etc., etc.

Let's imagine that we need a transient `DbContext` which should be consumed lazily, 
but requires a one-time initialization logic (say a database migration) based on the config provided. 

Btw, this is the real case from the Gitter discussion with the user.

```cs md*/
public class Using_the_Decorator_directly_for_the_complex_initialization
{
    [Test]
    public void Example()
    {
        var c = new Container();

        var config = new Config();
        c.RegisterInstance(config);

        c.Register<DbContext>();
        c.Register<InitDbContext>(Reuse.Singleton, serviceKey: "initializer"); // serviceKey is optional, here it basically hides the service unless you know its key
        c.Register<DbContext>(setup: Setup.Decorator,
            made: Made.Of(_ => ServiceInfo.Of<InitDbContext>(serviceKey: "initializer"),
            f => f.Init(Arg.Of<DbContext>())));

        var ctx0 = c.Resolve<Lazy<DbContext>>();
        var ctx1 = c.Resolve<Lazy<DbContext>>();

        var initializer = c.Resolve<InitDbContext>("initializer");

        Assert.IsFalse(initializer.Initialized);

        Assert.IsTrue(ctx0.Value.Migrated);
        Assert.IsTrue(initializer.Initialized);

        Assert.AreNotSame(ctx0.Value, ctx1.Value);
    }

    // DI infrastructure, but it does not need any knowledge of DryIoc - it may perfectly work and be tested without DryIoc.
    class InitDbContext
    {
        Config _config;
        public bool Initialized;
        public InitDbContext(Config config) { _config = config; }
        public DbContext Init(DbContext ctx)
        {
            if (!Initialized)
            {
                if (_config.ShouldMigrateDatabase())
                    ctx.MigrateDatabase();
                Initialized = true;
            }
            return ctx;
        }
    }

    // Application services
    class DbContext
    {
        public bool Migrated;
        public void MigrateDatabase() { Migrated = true; }
    }

    class Config
    {
        public bool ShouldMigrateDatabase() => true;
    }
} /*md
```

md*/