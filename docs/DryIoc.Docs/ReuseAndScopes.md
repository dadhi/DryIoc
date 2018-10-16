<!--Auto-generated from .cs file, the edits here will be lost! -->

# Reuse and Scopes

[TOC]

## What is Reuse?

Reuse (or lifestyle) instructs container to create service once and then return the same instance on every resolve or inject.
Created service becomes shared between its consumers.

One type of reuse is well known in software development as [Singleton](http://en.wikipedia.org/wiki/Singleton_pattern). IoC Containers implement Singleton in a way that make it easy to test and replace.

DryIoc provides following basic types of reuse:

* `Transient`
* `Singleton`
* `Scoped`
* `ScopedOrSingleton`

Variations of basic `Scoped` reuse:

* `ScopedTo(scopeName(s))`
* `ScopedTo<Service>()`
* `ScopedTo<Service>(serviceKey)`
* `InThread`
* `InWebRequest`

Service setup options:

* `Setup.UseParentReuse`
* `DecoratorSetup.UseDecorateeReuse`

You can create your own reuse by implementing `IReuse` interface.

Container uses Scopes ([see below](ReuseAndScopes#markdown-header-what-scope-is)) to 
store resolved services of non-Transient reuse.
Scope implements `IDisposable` and when disposed will dispose reused disposable services. You may prevent service disposal 
via [setup option](ReuseAndScopes#markdown-header-prevent-disposal-of-reused-service).

__Note:__ Service disposal is always taken in the reverse registration order.


## Reuse.Transient

Means no reuse: transient service will be created each time when resolved or injected.
Transient is the default if you omit the `reuse` parameter in registration and don't change `Container.Rules.DefaultReuse`. 

The following two registrations are the same: 

```cs
container.Register<IFoo, Foo>(Reuse.Transient);
container.Register<IFoo, Foo>();
```

### Disposable Transient

When you register transient service implementing `IDisposable` interface it becomes problematic who is responsible for service disposal.
Let's look at two situations to understand why it is a problem:

First case, if you resolve a transient disposable service via `Resolve` method.

__Note__: DryIoc does not support transient disposable registration by default (explained later). You need to use `allowDisposableTransient: true` to setup 
an individual registration or `Rules.WithoutThrowOnRegisteringDisposableTransient()` to allow per container.

```cs 
using System;
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedVariable

class Disposable_transient_as_resolved_service
{
    [Test]
    public void Example()
    {
        var container = new Container();
        // ...or global option
        //var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());

        container.Register<X>(setup: Setup.With(allowDisposableTransient: true));

        // You are in control to dispose, because you have an access to `x`
        var x = container.Resolve<X>();
        x.Dispose();
    }

    class X : IDisposable { public void Dispose() { } }
}
```

In this case, using container is very similar to using a `new` operator. 
You are controlling the resolved service and may decide when it is no longer needed and call `x.Dispose();`.
No problem in this case.

Second case, when the disposable transient is injected as dependency:
```cs 
class Disposable_transient_as_injected_dependency
{
    [Test]
    public void Example()
    {
        // global option
        var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());

        container.Register<XUser>();

        container.Register<X>();
        // ...or individual setup
        //container.Register<X>(setup: Setup.With(allowDisposableTransient: true));

        var user = container.Resolve<XUser>();

        // Now you don't have to `x`, what to do?
        //x.Dispose();
    }

    class X : IDisposable { public void Dispose() { } }
    class XUser { public XUser(X x) { } }
}
```

Here `XUser` just accepts `X` parameter without knowing its reuse, is it shared or not. 
Given the parameter alone it is not enough to decide if `XUser` can dispose `x`, or may be `x` is still used by other consumers.
`XUser` may even don't know that `X` implementation implements `IDisposable`. 

That means the responsibility for Disposing injected dependency should be on injecting side - IoC Container.

In order to control (have an access) transient disposable object, container should somehow track (store) transient disposable object somewhere.

DryIoc provides a way to track a transient disposable object in the current open scope (if any) or in the singleton scope in container.

```cs 
class Tracking_disposable_transient
{
    [Test]
    public void Example()
    {
        // global rule to track
        var container = new Container(rules => rules.WithTrackingDisposableTransients());

        //.. or individual setup option
        //container.Register<X>(setup: Setup.With(trackDisposableTransient: true));
        container.Register<X>();

        container.Register<XUser>();

        XUser user;
        using (var scope = container.OpenScope())
        {
            user = scope.Resolve<XUser>();
        }

        Assert.IsTrue(user.X.IsDisposed);
    }

    class X : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    class XUser
    {
        public readonly X X;
        public XUser(X x) { X = x; }
    }
} 
```

If case, you want for some reason to prevent tracking of specific service, there are couple of options:

Using `preventDisposal` setup option:
```cs
container.Register<Y>(setup: Setup.With(preventDisposal: true));
```

Another way to prevent tracking is wrapping disposable transient in `Func`:
```cs 

class Prevent_disposable_tracking_with_Func
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules.WithTrackingDisposableTransients());

        container.Register<XFactory>();
        container.Register<X>();

        var xf = container.Resolve<XFactory>();
        var x = xf.GetX();
        container.Dispose();

        Assert.IsTrue(xf.X.IsDisposed);
        Assert.IsFalse(x.IsDisposed); // `x` created via injected `Func` is not tracked
    }

    class XFactory
    {
        public readonly Func<X> GetX;
        public readonly X X;

        public XFactory(Func<X> getX, X x)
        {
            GetX = getX;
            X = x;
        }
    }

    class X : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }
}
```


### Different default Reuse instead of Transient

Sometimes you may want to apply another default reuse instead of Transient. 
Possible reasons maybe: to minimize clutter in registrations, 
or to automatically provide reuse preferred to your use-case.

You can achieve this by setting the Container Rules:
```cs 
class Default_reuse_per_container
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules.WithDefaultReuse(Reuse.Scoped));

        container.Register<Abc>();

        using (var scope = container.OpenScope())
        {
            var abc = scope.Resolve<Abc>();
            Assert.AreSame(abc, scope.Resolve<Abc>());
        }
    }

    class Abc { }
} 
```

What if I want a Transient reuse when `DefaultReuse` is different from Transient. In this case you need to 
specify `Reuse.Transient` explicitly: `container.Register<X>(Reuse.Transient)`.


## Reuse.Singleton

The same single instance per Container. Service instance will be created on first resolve or injection and will live until
container is disposed. If instance type is `IDisposable` then it will be disposed together with container.

```cs 
class Singleton_reuse
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<A>(Reuse.Singleton);

        var a = container.Resolve<A>();
        Assert.AreSame(a, container.Resolve<A>());
    }

    class A { }
} 
```

## Reuse.Scoped

`Reuse.Scoped` or specifically its type `CurrentScopeReuse` is the base type for the rest of predefined reuses in DryIoc. 
But before explaining it, let's talk about the notion of Scope.


### What Scope is?

DryIoc uses Scope (`IScope` interface) to implement a [Unit-Of-Work](http://msdn.microsoft.com/en-us/magazine/dd882510.aspx) pattern.

Physically scope is the storage for resolved and injected services registered with non-Transient reuse (except the Disposable Transient). 
Once created, a reused object is stored in the scope internal collection and will live there until the scope is disposed.
In addition, Scope ensures that __service instance will be created only once__ in multi-threading scenarios.

__Note:__ The scope is the only place in DryIoc where `lock`, is used to prevent multiple creation of the same instance. The rest of DryIox is lock-free.

Scope also has `Parent` and `Name` properties (see `ScopedTo(name)` reuse explained below).


### What Current Scope is?

Current scope is created when you call `var scopedContainer = container.OpenScope()`, or when you creating a nested scope 
`var nestedScopedContainer = scopedContainer.OpenScope()`.

The result of this call is `scopedContainer` of type `IResolverContext`. But actually it is a new container, 
which shares all the registrations and cached resolutions with the original container, 
but contains a reference to newly opened scope. 

Resolving a service with `Scoped` reused from `scopedContainer` will store the service instance in the opened scope. 
When you dispose `scopedContainer`, open scope will be disposed as well together with the stored service instance.

```cs 
class Scoped_reuse_register_and_resolve
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Car>(Reuse.Scoped);

        // Resolving scoped service outside of scope throws an exception.
        Assert.Throws<ContainerException>(() => container.Resolve<Car>());

        Car car;
        using (var scopedContainer = container.OpenScope())
        {
            car = scopedContainer.Resolve<Car>();
            car.DriveToMexico();
        }

        // Disposable car will be disposed here together with opened scope in scopedContainer.
        Assert.IsTrue(car.IsDisposed);
    }

    class Car : IDisposable
    {
        public void DriveToMexico() { }

        public void Dispose() => IsDisposed = true;
        public bool IsDisposed { get; private set; }
    }
} 
```

Interesting thing, if you resolve `Car` from container wrapped in `Func<Car>` or `Lazy<Car>`, it won't throw. 
But when you try to use a lazy value in open scope it will throw.

```cs 
class Scoped_reuse_resolve_wrapped_in_Lazy_outside_of_scope
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<Car>(Reuse.Scoped);

        var carFactory = container.Resolve<Lazy<Car>>(); // Does not throw.
        Car car = null;
        using (var scopedContainer = container.OpenScope())
        {
            Assert.Throws<ContainerException>(() => car = carFactory.Value);
        }
    }

    class Car
    {
        public void DriveToMexico() { }
    }
} 
```

It happens because scope is the part of container. When resolving from top container id does not have the opened scope part, 
but laziness of `Lazy` and `Func` prevents throwing, cause we are not accessing scope yet.
When opening scope we are creating another container with the __bound scope__ part. But previously resolved services does not 
aware of the new container and new scope, therefore attempt to get actual value throws an exception.

There is way to support such a lazy scenario, check the next section. 

### ScopeContext

ScopeContext (`IScopeContext` interface) is the __shared__ storage and tracking mechanism for current opened scope and its nested scopes.
By default, `Container` does not have any scope context, and stores a scope directly in itself. When providing with scope context,
container may share the context with scoped containers, making possible lazy resolve outside of the scope and then getting value
when scope is available __in shared scope context__.

```cs 
class Scoped_reuse_resolve_Lazy_with_scope_context
{
    [Test]
    public void Example()
    {
        // The important bit is creating the scope context for container
        var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

        container.Register<Car>(Reuse.Scoped);

        var carFactory = container.Resolve<Lazy<Car>>();

        using (var scopedContainer = container.OpenScope())
        {
            var car = carFactory.Value;
            car.DriveToMexico();
        }
    }

    class Car
    {
        public void DriveToMexico() { }
    }
} 
```

Supported contexts:

- `AsyncExecutionFlowScopeContext` is available in .NET 4.5+ and .NET Standard 1.3+. 
This context tracks open scopes across `await / async` call boundaries. 
Check this [blog post](http://blog.stephencleary.com/2013/04/implicit-async-context-asynclocal.html) for details.
- `ThreadScopeContext` is a thread local context, shares the open scopes in the current thread.
- `HttpContextScopeContext`is available with `DryIoc.Web` extension. It stores the open scopes in `HttpContext`.

You may create your own context by implementing `IScopeContext` interface.

__Note:__ `AsyncExecutionFlowScopeContext` maybe considered a default option, when you don't know what to use.


### Nested scopes

The scopes maybe nested either with or without scope context present.

```cs 
class Nested_scopes_without_scope_context
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<A>(Reuse.Scoped);

        // Three nested scopes
        var s1 = container.OpenScope();
        var s2 = s1.OpenScope();
        var s3 = s2.OpenScope();

        Assert.AreSame(s1.Resolve<A>(), s1.Resolve<A>());
        Assert.AreNotSame(s1.Resolve<A>(), s2.Resolve<A>());
        Assert.AreNotSame(s2.Resolve<A>(), s3.Resolve<A>());
    }

    class A { }
} 
```

Scopes `s1`, `s2`, `s3` form a nested chain where `s1` is parent of `s2` and `s2` is parent of `s3`.

__Note:__ In DryIoc a singleton scope is not a part of scope chain.

In absence of scope context, all the nested scopes are exist and available __independently__, so you may 
resolve from any nested scoped container any time getting a different instances from the different scopes. 

Now we add `ScopeContext` to the mix:

```cs 
class Nested_scopes_with_scope_context
{
    [Test]
    public void Example()
    {
        var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

        container.Register<A>(Reuse.Scoped);

        // Three nested scopes
        var s1 = container.OpenScope();
        var s2 = s1.OpenScope();
        var s3 = s2.OpenScope();

        Assert.AreSame(s1.Resolve<A>(), s1.Resolve<A>());

        // Here is the difference - all the instances of are the same and stored in the deepest scope `s3`
        Assert.AreSame(s1.Resolve<A>(), s2.Resolve<A>());
        Assert.AreSame(s2.Resolve<A>(), s3.Resolve<A>());

        // An `a` will be actually resolved from the `s3` despite that we resolving from the `parent`
        var a = s1.Resolve<A>();

        // We disposed of the `s3` scope, making its parent `s2` a new current scope.
        s3.Dispose();
        Assert.IsTrue(a.IsDisposed);

        // Now a new instances started to be resolved from `s2`
        Assert.AreNotSame(a, s2.Resolve<A>());
        Assert.AreSame(s2.Resolve<A>(), s1.Resolve<A>());
    }

    class A : IDisposable
    {
        public void Dispose() => IsDisposed = true;
        public bool IsDisposed { get; private set; }
    }
} 
```

Scope context associated with `container` has a shared "current" scope property, which references the last / deepest open scope.
The "current" scope is overridden with new scope open, making it an actual current scope to resolve from. 
When current scope is disposed, its parent becomes a new current scope.


## Reuse.ScopedTo(name)

You may tag the scope with the distinct "name" to resolve from the specific scope in nested chain using the 
`Reuse.ScopedTo(object name)` reuse.

It also works both with or without the context scope.

```cs 
class Named_open_scopes_and_scoped_to_name
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Car>(Reuse.ScopedTo("top"));

        using (var s1 = container.OpenScope("top"))
        {
            var car1 = s1.Resolve<Car>();

            using (var s2 = s1.OpenScope())
            {
                var car2 = s2.Resolve<Car>();

                // Cars are the same despite that `car2` is resolved from the nested `s2`,
                // because it was specifically resolved from the matching name scope `s1`
                Assert.AreSame(car2, car1);
            }
        }
    }

    class Car { }
} 
```

When resolving or injecting a service with `ScopeTo(name)` reuse DryIoc will look up starting from the current open scope, 
through chain of its parents until the scope with the name is found or we reached the top scope.

To define a name you may use object of any type with `Equals(object other)` and `GetHashCode()` available. 
`Reuse.ScopedTo(42)` or `Reuse.ScopedTo(Flags.Red)` are the valid names.

__Note:__ When the name is not specified it has a `null` value.

Since v3.0 DryIoc supports specifying multiple names via `Reuse.ScopedTo(params object[] names)`. 
DryIoc will stop when __any__ of the names are equal to the scope name.


### Reuse.InWebRequest and Reuse.InThread

Basically `Reuse.InWebRequest` and `Reuse.InThread` are just a scope reuses:

- `Reuse.InWebRequest == Reuse.ScopedTo(specificName)`.
- `Reuse.InThread == Reuse.Scoped` just a scoped reuse in presence of `ThreadScopeContext`


## Reuse.ScopeTo{TService}(serviceKey)

`ScopeTo<TService>()` reuse defines to use the same dependency instance in specific service object sub-tree. 
The concept is similar to assigning dependency object to the variable and then re-using this variable when creating service object.

```cs 
class Example_of_reusing_dependency_as_variable
{
    Foo Create()
    {
        var sub = new SubDependency();
        return new Foo(sub, new Dependency(sub));
    }

    class Foo
    {
        public Foo(SubDependency sub, Dependency dep) { }
    }

    class Dependency
    {
        public Dependency(SubDependency sub) { }
    }

    class SubDependency { }
} 
```

In terms of DryIoc `SubDependency` has a reuse `Reuse.ScopedTo<Foo>()`.
Here is the full setup:
```cs 
class Scoped_to_service_reuse
{
    [Test]
    public void Example()
    {
        var container = new Container();

        // This is required to mark that `Foo` opens the scope
        container.Register<Foo>(setup: Setup.With(openResolutionScope: true));

        container.Register<Dependency>();
        container.Register<SubDependency>(Reuse.ScopedTo<Foo>());

        var foo = container.Resolve<Foo>();
        Assert.AreSame(foo.Sub, foo.Dep.Sub);
    }

    class Foo
    {
        public SubDependency Sub { get; }
        public Dependency Dep { get; }
        public Foo(SubDependency sub, Dependency dep)
        {
            Sub = sub;
            Dep = dep;
        }
    }

    class Dependency
    {
        public SubDependency Sub { get; }
        public Dependency(SubDependency sub)
        {
            Sub = sub;
        }
    }

    class SubDependency { }
} 
```

Important note, that you need to register `Foo` with `openResolutionScope: true` setup option.

Actually, this requirement explains how things are working for `ScopeTo<Type>()`. It is no different to 
`ScopedTo(object name)` where is `name` is of special type `ResolutionScopeName` composed from 
`typeof(Foo)` and optional service key.

To satisfy the `ScopedTo` reuse we need an open scope somewhere. Here DryIoc will automatically open scope when resolving the `Foo` service.
We may desugar it to something like: 
```cs
var foo container.OpenScope(new ResolutionScopeName(typeof(Foo))).Resolve<Foo>();
```

The code also implies that `Foo` itself will be automatically scoped to its scope. It maybe important if you want to access `Foo` recursively from its dependency.

But with such an automatic scoping we still have a problem: how to dispose the scope.

In order to dispose the scope DryIoc should somehow track the reference to it. This is exactly how it works, automatically created scope will be held by either
current open scope or by singleton scope. When the current scope or container with singletons is disposed - the automatic scope is disposed to.

```cs 
class Scoped_to_service_reuse_with_dispose
{
    [Test]
    public void Example()
    {
        var container = new Container();

        // This is required to mark that `Foo` opens the scope
        container.Register<Foo>(setup: Setup.With(openResolutionScope: true));

        container.Register<Dependency>(Reuse.ScopedTo<Foo>());

        var foo = container.Resolve<Foo>();

        container.Dispose();
        Assert.IsTrue(foo.Dep.IsDisposed);
    }

    class Foo
    {
        public Dependency Dep { get; }
        public Foo(Dependency dep) { Dep = dep; }
    }

    class Dependency : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }
} 
```

__Note:__ There is a similar reuse (lifestyle) available in other IoC libraries, e.g. 
[Autofac InstancePerOwned Lifestyle](http://docs.autofac.org/en/latest/lifetime/instance-scope.html#instance-per-owned) and 
[Castle Winsdor Bound LifeStyle](http://docs.castleproject.org/Default.aspx?Page=LifeStyles&NS=Windsor&AspxAutoDetectCookieSupport=1#Bound_8).


There is more to `Reuse.ScopeTo<T>(object serviceKey = null)`:

- You may provide an optional `serviceKey` to match a service registered with this key.
- You may register to match not only the exact `TService` but its base class or the implemented interface.


## Setup.UseParentReuse

This option allows the dependency to use parent or ancestor service reuse (if it has parents defining a reuse).
In case of all parents are transient or dependency is wrapped in `Func` somewhere in parents chain,
the dependency will be transient at the end.

```cs 
class Use_parent_reuse
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Mercedes>(Reuse.Singleton);
        container.Register<Dodge>();

        container.Register<Wheels>(setup: Setup.With(useParentReuse: true));

        // same Mercedes with the same Wheels
        var m1 = container.Resolve<Mercedes>();
        var m2 = container.Resolve<Mercedes>();
        Assert.AreSame(m1.Wheels, m2.Wheels);

        // different Dodges with the different Wheels
        var d1 = container.Resolve<Dodge>();
        var d2 = container.Resolve<Dodge>();
        Assert.AreNotSame(d1.Wheels, d2.Wheels);
    }

    class Mercedes
    {
        public Wheels Wheels { get; }
        public Mercedes(Wheels wheels)
        {
            Wheels = wheels;
        }
    }

    class Dodge
    {
        public Wheels Wheels { get; }
        public Dodge(Wheels wheels)
        {
            Wheels = wheels;
        }
    }

    private class Wheels { }

}
```

__Note:__ If both `reuse` and `useParentReuse` specified then `reuse` has an upper hand and setup option is ignored.


## Reuse lifespan diagnostics

Lifetime diagnostics helps you to find [Captive Dependency](http://blog.ploeh.dk/2014/06/02/captive-dependency/) 
of service which tends to outlive its parent, making the parent behavior undetermined afterwards.

`IReuse` implementations in DryIoc have associated `Lifespan` property. This is a relative number of how long the reused object lives, 
which allows to detect lifespan mismatches by simply comparing the lifespan values:

Pre-defined DryIoc reuses have following lifespan:

- `Singleton`: 1000. Object lives for lifetime of container.
- `InCurrentScope` family: 100. Object lives until current scope is closed which is less the container lifetime.
- `InResolutionScope` family: 0. Because resolution scope reuse is orthogonal to other reuses - the comparison does not make sense.
- transient services: 0. Does not have the reuse and therefore lifetime.

Example: 
Given the numbers above, when singleton `Car` depends on injected `Wheels` reused in current scope,
the resolution of `Car` will throw exception - because `Wheels` lifespan 100 is less than parent's 1000.
```
#!c#
   var c = new Container();
   
   c.Register<Car>(Reuse.Singleton);
   c.Register<Wheels>(Reuse.InCurrentScope);
   
   using (var scope = c.OpenScope())
       c.Resolve<Car>(); // will throw ContainerException with message:
   
   // Dependency Wheels as parameter "wheels" has shorter Reuse lifespan than its parent: Car.
   // CurrentScopeReuse:100 lifetime is shorter than SingletonReuse:1000.
   // You may turn Off this error with Rules.WithoutThrowIfDepenedencyHasShorterReuseLifespan().
```

The error message is saying how to turn Off this error via rule:
```
#!c#
   var c = new Container(rules => rules.WithoutThrowIfDepenedencyHasShorterReuseLifespan());
   // ...
   var car = c.Resolve<Car>(); // works but may surprise Car with disposed Wheels!
```

__Note:__ Another way to prevent the exception is wrapping reused dependency in a `Func` [wrapper](Wrappers). 
When using `Func` you are actually saying that you want to control or postpone creation of dependency:
```
#!c#
   class Car 
   {
       public Car(Func<Wheels> getWheels) {} // Car is in control when to get new Wheels.
   }
```


## Reuse for externally created objects

Externally created objects may be registered into Container with `RegisterInstance` method. The method accepts _reuse_ parameter:

- By default when no reuse parameter provided, instance will be registered as Singleton. That make sense because singleton lifetime is directly associated with lifetime of container, so living in container instance will have the same lifetime as singleton.

- Instance may be registered with `Reuse.InCurrentScope` only when current scope is available. That way instance will be directly placed in current scope. If no current scope at the moment then exception will be thrown.

- Registering instance with `Reuse.InResolutionScope` is not permitted because resolution scope does not exist when registration is done. Therefor attempt to register will throw exception.

Examples of instance registrations:
```
#!c#
   var service = new Service();

   // Places service into singleton scope
   container.RegisterInstance(service, Reuse.Singleton);
   // the same as above
   container.RegisterInstance(service);

   // Places service into current opened scope
   using (var scope = conitainer.OpenScope())
       container.RegisterInstance(service, Reuse.InCurrentScope);

   // Fails with exception because of no current scope
   container.RegisterInstance(service, Reuse.InCurrentScope);

   // Fails as well
   container.RegisterInstance(service, Reuse.InResolutionScope);
```

If `IDisposable` instance is registered as singleton by default, it also means that it will be disposed when container is disposed. The same is true for the current scope - instance will be disposed together with scope.

You may prevent disposal of the instance by providing [preventDisposal setup](ReuseAndScope#markdown-header-prevent-disposal-of-reused-service).
```
#!c#
   container.RegisterInstance(service, setup: Setup.With(preventDisposal: true));
```

Another option is to store instance as WeakReference:
```
#!c#
   container.RegisterInstance(service, setup: Setup.With(weaklyReferenced: true));
```

__Note:__ If you register instance with `IfAlreadyRegistered.Replace` option, then existing reused instance will be directly replaced by the new one - Container wills keep original factory and cache intact. This approach provides faster performance and less allocations, so the replacing registered instance is cheap.


## Weakly Referenced reused service

You may specify to store reused object as `WeakReference`:
```
#!c#
   container.Register<Service>(Reuse.Singleton, setup: Setup.With(weaklyReferenced: true));
```


## Prevent Disposal of reused service

By default DryIoc will dispose `IDisposable` reused service together with its scope. To prevent that you may register service with as following:
```
#!c#
   container.Register<Service>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
```

__Note:__ `preventDisposal` should be used with weakly referenced service too in order to override default behavior, or weakly referenced `IDisposable` service will be disposed.
```
#!c#
   container.Register<Service>(Reuse.Singleton, 
       setup: Setup.With(preventDisposal: true, weaklyReferenced: true);
```
