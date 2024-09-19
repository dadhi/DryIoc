<!--Auto-generated from .cs file, the edits here will be lost! -->

# Reuse and Scopes


- [Reuse and Scopes](#reuse-and-scopes)
  - [What is Reuse?](#what-is-reuse)
  - [Reuse.Transient](#reusetransient)
    - [Disposable Transient](#disposable-transient)
    - [Different default Reuse instead of Transient](#different-default-reuse-instead-of-transient)
  - [Reuse.Singleton](#reusesingleton)
  - [Reuse.Scoped](#reusescoped)
    - [What Scope is?](#what-scope-is)
    - [What Current Scope is?](#what-current-scope-is)
    - [ScopeContext](#scopecontext)
    - [Nested scopes](#nested-scopes)
  - [Reuse.ScopedTo(name)](#reusescopedtoname)
    - [Reuse.InWebRequest and Reuse.InThread](#reuseinwebrequest-and-reuseinthread)
  - [Reuse.ScopedTo service type](#reusescopedto-service-type)
    - [Opening resolution scope per dependency](#opening-resolution-scope-per-dependency)
    - [Disposing of resolution scope](#disposing-of-resolution-scope)
      - [Automatic scope disposal](#automatic-scope-disposal)
      - [Own the resolution scope disposal](#own-the-resolution-scope-disposal)
  - [Setup.UseParentReuse](#setupuseparentreuse)
  - [Reuse lifespan diagnostics](#reuse-lifespan-diagnostics)
  - [Weakly Referenced reused service](#weakly-referenced-reused-service)
  - [Prevent Disposal of reused service](#prevent-disposal-of-reused-service)


## What is Reuse?

Reuse (or lifestyle) instructs container to create service once and then return the same instance on every resolve or inject.
Created service becomes shared between its consumers.

One type of reuse is well known in software development as [Singleton](http://en.wikipedia.org/wiki/Singleton_pattern). IoC Containers implement Singleton in a way that makes it easy to test and replace.

DryIoc provides following basic types of reuse:

* `Transient`
* `Singleton`
* `Scoped`
* `ScopedOrSingleton`

Variations of basic `Scoped` reuse:

* `ScopedTo(scopeName(s))`
* `ScopedToService<Service>()`
* `ScopedToService<Service>(serviceKey)`

Service setup options:

* `Setup.UseParentReuse`
* `DecoratorSetup.UseDecorateeReuse`

You can create your own reuse by implementing `IReuse` interface.

Container uses Scopes ([see below](ReuseAndScopes.md#what-scope-is)) to 
store resolved services of non-Transient reuse.
Scope implements `IDisposable` and when disposed will dispose reused disposable services. You may prevent service disposal 
via [setup option](ReuseAndScopes.md#prevent-disposal-of-reused-service).

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

The first case, if you resolve a transient disposable service via `Resolve` method.

__Note__: DryIoc does not support transient disposable registration by default (explained later). You need to use `allowDisposableTransient: true` to setup 
an individual registration or `Rules.WithoutThrowOnRegisteringDisposableTransient()` to allow per container.

```cs 
namespace DryIoc.Docs;
using System;
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedVariable

public class Disposable_transient_as_resolved_service
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

In this case, using the container is very similar to using a `new` operator. 
You are controlling the resolved service and may decide when it is no longer needed and call `x.Dispose();`.
No problem in this case.

The second case, when the disposable transient is injected as a dependency:
```cs 
public class Disposable_transient_as_injected_dependency
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
Given the parameter alone it is not enough to decide if `XUser` can dispose `x`, or maybe `x` is still used by other consumers.
`XUser` may even don't know that `X` implementation implements `IDisposable`. 

That means the responsibility for Disposing injected dependency should be on injecting side - IoC Container.

In order to control (have an access) transient disposable object, the container should somehow track (store) transient disposable object somewhere.

DryIoc provides a way to track a transient disposable object in the current open scope (if any) or in the singleton scope in the container.

```cs 
public class Tracking_disposable_transient
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

If for some reason you want to prevent tracking of specific service, there are a couple of options:

Using `preventDisposal` setup option:
```cs
container.Register<Y>(setup: Setup.With(preventDisposal: true));
```

Another way to prevent tracking is wrapping disposable transient in `Func`:
```cs 

public class Prevent_disposable_tracking_with_Func
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
Possible reasons may be: to minimize clutter in registrations, 
or to automatically provide reuse preferred to your use-case.

You can achieve this by setting the Container Rules:
```cs 
public class Default_reuse_per_container
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

What if I want a Transient reuse when `DefaultReuse` is different from Transient. In this case, you need to specify `Reuse.Transient` explicitly: `container.Register<X>(Reuse.Transient)`.


## Reuse.Singleton

The same single instance per Container. Service instance will be created on first resolve or injection and will live until the container is disposed. If instance type is `IDisposable` then it will be disposed together with the container.

```cs 
public class Singleton_reuse
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

`Reuse.Scoped` or specifically, its type `CurrentScopeReuse` is the base type for the rest of predefined reuses in DryIoc. 
But before explaining it, let's talk about the notion of Scope.


### What Scope is?

DryIoc uses Scope (`IScope` interface) to implement a [Unit-Of-Work](http://msdn.microsoft.com/en-us/magazine/dd882510.aspx) pattern.

Physically scope is the storage for resolved and injected services registered with non-Transient reuse (except the Disposable Transient). 
Once created, a reused object is stored in the scope internal collection and will live there until the scope is disposed.
In addition, Scope ensures that __service instance will be created only once__ in multi-threading scenarios.


Scope also has `Parent` and `Name` properties, see the `ScopedTo(name)` reuse explained below.


### What Current Scope is?

Current scope is created when you call `var scopedContainer = container.OpenScope()`, or when you creating a nested scope 
`var nestedScopedContainer = scopedContainer.OpenScope()`.

The result of this call is `scopedContainer` of type `IResolverContext`. But actually it is a new container, 
which shares all the registrations and cached resolutions with the original container, 
but contains a reference to newly opened scope. 

Resolving a service with `Scoped` reused from `scopedContainer` will store the service instance in the opened scope. 
When you dispose `scopedContainer`, the open scope will be disposed as well together with the stored service instance.

```cs 
public class Scoped_reuse_register_and_resolve
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

An interesting thing, if you resolve `Car` from container wrapped in `Func<Car>` or `Lazy<Car>`, it won't throw. 
But when you try to use a lazy value in open scope it will throw.

```cs 
public class Scoped_reuse_resolve_wrapped_in_Lazy_outside_of_scope
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

It happens because a scope is the part of the container. When resolving from top container id does not have the opened scope part, 
but the laziness of `Lazy` and `Func` prevents throwing, cause we are not accessing scope yet.
When opening scope we are creating another container with the __bound scope__ part. But previously resolved services does not 
aware of the new container and new scope, therefore attempt to get actual value throws an exception.

There is a way to support such a lazy scenario, check the next section. 

### ScopeContext

ScopeContext (`IScopeContext` interface) is the __shared__ storage and tracking mechanism for the current opened scope and its nested scopes.
By default, `Container` does not have any scope context and stores a scope directly in itself. When provided with the scope context,
a container may share the context with scoped containers, making possible lazy resolve outside of the scope and then getting value
when the scope is available __in shared scope context__.

```cs 
public class Scoped_reuse_resolve_Lazy_with_scope_context
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

__Note:__ `AsyncExecutionFlowScopeContext` may be considered a default option when you don't know what to use.


### Nested scopes

The scopes may be nested either with or without scope context present.

```cs 
public class Nested_scopes_without_scope_context
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

__Note:__ In DryIoc a singleton scope is not a part of the scope chain.

In absence of scope context, all the nested scopes exist and available __independently__, so you may resolve from any nested scoped container any time to get different instances from the different scopes. 

Now we add `ScopeContext` to the mix:

```cs 
public class Nested_scopes_with_scope_context
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
When the current scope is disposed, its parent becomes a new current scope.


## Reuse.ScopedTo(name)

You may tag the scope with the distinct "name" to resolve from the specific scope in the nested chain using the 
`Reuse.ScopedTo(object name)` reuse.

It also works both with or without the context scope.

```cs 
public class Named_open_scopes_and_scoped_to_name
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

When resolving or injecting a service with `ScopedTo(name)` reuse DryIoc will look up starting from the current open scope, 
through the chain of its parents until the scope with the name is found or we reached the top scope.

To define a name you may use object of any type with `Equals(object other)` and `GetHashCode()` available. 
`Reuse.ScopedTo(42)` or `Reuse.ScopedTo(Flags.Red)` are the valid names.

__Note:__ When the name is not specified it has a `null` value.

Since v3.0 DryIoc supports specifying multiple names via `Reuse.ScopedTo(params object[] names)`. 
DryIoc will stop when __any__ of the names are equal to the scope name.


### Reuse.InWebRequest and Reuse.InThread

Basically `Reuse.InWebRequest` and `Reuse.InThread` are just a scope reuses:

- `Reuse.InWebRequest == Reuse.ScopedTo(specificName)`.
- `Reuse.InThread == Reuse.Scoped` just a scoped reuse in presence of `ThreadScopeContext`


## Reuse.ScopedTo service type

**Note:** `ScopedToService` methods are replacing the `ScopedTo` for the service type and the optional service key. The reason is the clash of overloading between the `ScopedTo(Type)` and the `ScopedTo(object)`, so the `ScopedToService(Type)` is added.

`ScopedToService<TService>(object serviceKey = null)` and `ScopedToService(Type serviceType, object serviceKey = null)` 
define the reuse of the same dependency value in the service sub-graph.
The concept is similar to the assigning the dependency value to the variable and then passing (re-using) this variable 
inside the service and its nested dependencies.

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

In the example above `SubDependency` has a reuse `Reuse.ScopedToService<Foo>()`:

```cs 
public class Scoped_to_service_reuse
{
    [Test] public void Example()
    {
        var container = new Container();

        // `openResolutionScope` option is required to open the scope for the `Foo`, read the sub-section below to see why.
        container.Register<Foo>(setup: Setup.With(openResolutionScope: true));

        container.Register<Dependency>();
        container.Register<SubDependency>(Reuse.ScopedToService<Foo>());

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

### Opening resolution scope per dependency

In the example above the setup with `openResolutionScope: true` explains how things are working for the `ScopedToService<Type>()`. 
The method is a thin layer over `ScopedTo(object name)` where the `name` is the type `ResolutionScopeName` wrapping together the `typeof(Foo)` and optional service key.

Next thing is to open scope to satisfy the `ScopedToService` reuse. 

Using the setup with `openResolutionScope: true` we are instructing DryIoc to automatically open the scope when resolving the `Foo` service.

The line `container.Register<Foo>(setup: Setup.With(openResolutionScope: true));` will result in the following resolved object-graph: 

```cs
var foo = container.OpenScope(new ResolutionScopeName(typeof(Foo))).Resolve<Foo>();
```

**Note:** The code tells that `Foo` itself will be scoped to its scope.


The de-sugared code also tells that the scope may be opened manually without the special setup. It may be useful for testing purposes.

```cs 
public class Emulating_openResolutionScope_setup
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Foo>(Reuse.ScopedToService<Foo>()); // huh, scope to itself explicitly - not needed / implied when using `openResolutionScope: true`  
        container.Register<Dependency>();
        container.Register<SubDependency>(Reuse.ScopedToService<Foo>());

        var scopeNameForFoo = ResolutionScopeName.Of<Foo>();
        using var fooScope = container.OpenScope(scopeNameForFoo);

        var foo = fooScope.Resolve<Foo>();
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


### Disposing of resolution scope

#### Automatic scope disposal

Having such an implicit opened scope poses a problem - how to dispose the scope?

In order to dispose the scope DryIoc or the user code (explained later) 
should track the reference to it otherwise we have an undisposed dangling scope - **which is the bad thing to have**. 

Therefore to avoid dangling scope the resolution scope will be automatically tracked by either the parent scope or by the singleton scope. 
When the parent scope or container with singletons is disposed - the resolutions scope is disposed too.

```cs 
public class Scoped_to_service_reuse_with_dispose
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Foo>(setup: Setup.With(openResolutionScope: true));

        container.Register<Dependency>(Reuse.ScopedToService<Foo>());

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


#### Own the resolution scope disposal

The scope tracking is not ideal because it postpones the disposal until parent container or parent scope is disposed
But we may want to dispose it sooner, e.g. together with the service that opened it.

You may control the disposing of the resolution scope by injecting its `IResolverContext` ([automatically provided by Container](RulesAndDefaultConventions.md#implicitly-available-services)) 
and then dispose it manually.

```cs 
public class Own_the_resolution_scope_disposal
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules
            .WithTrackingDisposableTransients() // we need this to allow disposable transient Foo
        );

        container.Register<Foo>(setup: Setup.With(openResolutionScope: true));

        container.Register<Dependency>(Reuse.ScopedToService<Foo>());

        var foo = container.Resolve<Foo>();
        
        // Disposing the foo will dispose its scope and its scoped dependencies down the tree
        foo.Dispose(); 

        Assert.IsTrue(foo.Dep.IsDisposed);
    }

    class Foo : IDisposable
    {
        public Dependency Dep { get; }
        private readonly IResolverContext _scope;
        public Foo(Dependency dep, IResolverContext scope) 
        { 
            Dep = dep;
            _scope = scope;
        }

        public void Dispose() => _scope.Dispose();
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


There is more to `Reuse.ScopedToService<T>(object serviceKey = null)`:

- You may provide an optional `serviceKey` to match a service registered with this key.
- You may match not only the exact `TService` but its base class or the implemented interface.


## Setup.UseParentReuse

This option allows the dependency to use its parent or ancestor service reuse. In case parent or ancestor is 
Transient then it will be skipped until the first non-Transient ancestor found. If all ancestors are Transient or dependency is wrapped in `Func` somewhere in ancestor chain, the dependency will be Transient at the end.

**Note:** If ancestors do not have assigned reuse (it is `null`) then the `Rules.DefaultReuse` will be used instead
and the above rule is applied.

```cs 
public class Use_parent_reuse
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

Lifespan diagnostics will help you to find a [Captive Dependencies](http://blog.ploeh.dk/2014/06/02/captive-dependency/) 
of a service which tends to outlive its parent, making the parent behavior undetermined afterwards.

`IReuse` implementations in DryIoc have an associated `Lifespan` property. This is a relative `int` number of how long the reused object lives, 
which allows detecting lifespan mismatches by simply comparing the lifespan values.

DryIoc reuses have the following lifespans:

- `Singleton`: 1000. Object lives for a lifetime of the container.
- `Scoped(To)`: 100. Object lives until the current scope is closed, which is less the container lifetime.
- `Transient`: 0. Indicate an absence of reuse and therefore the absence of a lifetime.

```cs 
public class Reuse_lifespan_mismatch_detection
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Mercedes>(Reuse.Singleton);
        container.Register<Wheels>(Reuse.Scoped);

        using (var scope = container.OpenScope())
        {
            // Throws an exception with captive dependency detected:
            // dependency Scoped lifespan is less than parent Singleton lifespan 
            Assert.Throws<ContainerException>(() => scope.Resolve<Mercedes>());
        }
    }

    class Mercedes { public Mercedes(Wheels wheels) { } }
    class Wheels { }
}
```

If you really want (say temporary) you may suppress the error via 

The error message is saying how to turn Off this error via rule `Rules.WithoutThrowIfDependencyHasShorterReuseLifespan()`
```cs 
public class Reuse_lifespan_mismatch_error_suppress
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());

        container.Register<Mercedes>(Reuse.Singleton);
        container.Register<Wheels>(Reuse.Scoped);

        Mercedes car;
        using (var scope = container.OpenScope())
        {
            car = scope.Resolve<Mercedes>();
        }

        // Here the singleton is still exist and valid but its `Wheels` scoped dependency is disposed
        Assert.IsTrue(car.Wheels.IsDisposed);
    }

    class Mercedes
    {
        public Mercedes(Wheels wheels) { Wheels = wheels; }
        public Wheels Wheels { get; }
    }

    class Wheels : IDisposable
    {
        public void Dispose() => IsDisposed = true;
        public bool IsDisposed { get; private set; }
    }
}
```

**Note:** The `Transient` reuse as stated above is not a "real" reuse (because it will be recreated every time its used), 
so you won't get the captive dependency exceptions for the `Transient` dependency inside the `Scoped` or `Singleton` services by default. 
But wou may to disagree with the `Rules.WithThrowIfScopedOrSingletonHasTransientDependency`.

Another way to prevent the exception is wrapping a shorter reused dependency in a `Func` or `Lazy` [wrapper](Wrappers). 
The user may decide to delay the creation of the dependency via `Lazy` or create multiple dependency values via `Func` 
and be fully in control of their lifetime.
```cs 
public class Avoiding_reuse_lifespan_mismatch_for_Func_or_Lazy_dependency
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Mercedes>(Reuse.Singleton);
        container.Register<Wheels>(Reuse.Scoped);

        using (var scope = container.OpenScope())
        {
            var car = scope.Resolve<Mercedes>();
            var wheels = car.GetWheels();
        }
    }

    class Mercedes
    {
        public Mercedes(Func<Wheels> getWheels) { GetWheels = getWheels; }
        public readonly Func<Wheels> GetWheels;
    }

    class Wheels { }
}
```


## Weakly Referenced reused service

You may specify to store a reused object as `WeakReference`:
```cs
container.Register<Service>(Reuse.Singleton, setup: Setup.With(weaklyReferenced: true));
```


## Prevent Disposal of reused service

By default, DryIoc will dispose `IDisposable` reused service together with its scope. To prevent that you may register service with following:
```cs
container.Register<Service>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));
```
