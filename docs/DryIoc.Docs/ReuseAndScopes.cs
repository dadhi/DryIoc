/*md
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

```cs md*/
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

    class X : IDisposable { public void Dispose() {} }
}
/*md
```

In this case, using container is very similar to using a `new` operator. 
You are controlling the resolved service and may decide when it is no longer needed and call `x.Dispose();`.
No problem in this case.

Second case, when the disposable transient is injected as dependency:
```cs md*/
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
/*md
```

Here `XUser` just accepts `X` parameter without knowing its reuse, is it shared or not. 
Given the parameter alone it is not enough to decide if `XUser` can dispose `x`, or may be `x` is still used by other consumers.
`XUser` may even don't know that `X` implementation implements `IDisposable`. 

That means the responsibility for Disposing injected dependency should be on injecting side - IoC Container.

In order to control (have an access) transient disposable object, container should somehow track (store) transient disposable object somewhere.

DryIoc provides a way to track a transient disposable object in the current open scope (if any) or in the singleton scope in container.

```cs md*/
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
} /*md
```

If case, you want for some reason to prevent tracking of specific service, there are couple of options:

Using `preventDisposal` setup option:
```cs
container.Register<Y>(setup: Setup.With(preventDisposal: true));
```

Another way to prevent tracking is wrapping disposable transient in `Func`:
```cs md*/

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
/*md
```


### Different default Reuse instead of Transient

Sometimes you may want to apply another default reuse instead of Transient. 
Possible reasons maybe: to minimize clutter in registrations, 
or to automatically provide reuse preferred to your use-case.

You can achieve this by setting the Container Rules:
```cs md*/
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
} /*md
```

What if I want a Transient reuse when `DefaultReuse` is different from Transient. In this case you need to 
specify `Reuse.Transient` explicitly: `container.Register<X>(Reuse.Transient)`.


## Reuse.Singleton

The same single instance per Container. Service instance will be created on first resolve or injection and will live until
container is disposed. If instance type is `IDisposable` then it will be disposed together with container.

```cs md*/
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
} /*md
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

```cs md*/
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
} /*md
```

Interesting thing, if you resolve `Car` from container wrapped in `Func<Car>` or `Lazy<Car>`, it won't throw. 
But when you try to use a lazy value in open scope it will throw.

```cs md*/
class Scoped_reuse_resolve_wrapped_in_Func
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
} /*md
```

It happens because scope is the part of container. When resolving from top container id does not have the opened scope part, 
but laziness of `Lazy` and `Func` prevents throwing, cause we are not accessing scope yet.
When opening scope we are creating another container with the __bound scope__ part. But previously resolved services does not 
aware of the new container and new scope, therefore attempt to get actual value throws an exception.

There is way to support such a lazy scenario, check the next section. 

### ScopeContext

ScopeContext is the storage and tracking mechanism for current opened scope and its nested scopes:
```
#!c#
   var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

   // Three nested scopes in one branch
   s1 = c.OpenScope(); s2 = s1.OpenScope(); s3 = s2.OpenScope();
       
   // One nested scope in another branch
   t1 = c.OpenScope();
```

Result scopes `s1`, `s2`, `s3`and `t1` form a tree.

Given example from previous section, when you call `carFactory()` from the first thread, the created car will be stored in `s3`,
and when called from the second thread, car will be stored in `t1`;

Then if you dispose `s3`, context will start tracking `s2` as a current scope of first thread (there is no car in it yet.).
When you dispose all `s3`, `s2`, `s1` then no current scope in ScopeContext for the first thread, and car factory will
throw as we saw in first example.

__By default container does no have associated scope context__ and opened scope will be bound to container itself.

To specify scope context when creating container: 
```
#!c#
   var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
```

To change context when you have an existing container:
```
#!c#
   var containerForTest = c.With(scopeContext: new MyTestScopeContext());
```

Supported contexts:

- `AsyncExecutionFlowScopeContext` - defined only for .NET 4.5 and higher. This context tracks opened scope across await/async boundaries using `System.Runtime.Remoting.Messaging.CallContext`. Check this [blog post](http://blog.stephencleary.com/2013/04/implicit-async-context-asynclocal.html) for details.
- `ThreadScopeContext` - aka Thread Local, opened scope will be bound to current thread.
- `HttpContextScopeContext` - is available with __DryIoc.Web__ extension. It binds opened scope to `HttpContext`.

You may create your own context by implementing `IScopeContext` interface.


### ScopeContext is important for ASP.NET Web applications

Tracking or binding the scopes to some context, e.g. `HttpContext` or `CallContext`, allow to reuse services per Request. Check `Reuse.InWebRequest` section  below for more details.

### OpenScope without ScopeContext

`OpenScope` in container without context (__which is default__) creates new scope associated with the container. 
You may call `OpenScope` multiple times producing new independent scopes existing in parallel.
```
#!c#
   var container = new Container();
   container.Register<Blah>(Reuse.InCurrentScope);
   
   var scope1 = container.OpenScope();
   var scope2 = container.OpenScope();
   
   var blah1 = scope1.Resolve<Blah>();
   var blah2 = scope2.Resolve<Blah>();
   
   Assert.AreSame(blah1, scope1.Resolve<Blah>());
   Assert.AreNotSame(blah1, blah2);
   
   scope1.Dispose();
   scope2.Dispose();
```

__Note:__ `OpenScope` creates scope tree bound to corresponding container tree. 
Calling `OpenScope` with context produces chain of current scopes with latest (current) scope stored in the context.


### Implicit Open Scope

DryIoc has an optional rule do automatically open scope when container is created. 
The rule is only valid for non-ambient scopes - it means for container without ScopeContext.

__Note:__ The important thing, that implicitly open scope will be disposed when the container is disposed, 
so that both scoped and singleton services will be disposed.

```
#!c#
   var container = new Container(rules => rules.WithImplicitRootOpenScope());
   
   container.Register<X>(Reuse.InCurrentScope);
   container.Register<Y>(Reuse.Singleton);

   var x = container.Resolve<X>(); // works without OpenScope
   var y = container.Resolve<Y>();

   container.Dispose(); // both x and y are disposed here.
```

Given implicitly open scope calling `OpenScope` again will create second level nested scope. 
This nested scope should be disposed normally via `scope.Dispose()`;


## Reuse.InCurrentNamedScope and Reuse.InThread

Scopes may be nested. Let's review what happens when we open one scope from another:
```
#!c#
   container.Register<Car>(Reuse.InCurrentScope);
   
   using (var s1 = container.OpenScope()) // creates scope s1, s1 becomes ScopeContext.CurrentScope
   {
       var car1 = s1.Resolve<Car>();      // creates new Car and stores it into CurrentScope (s1)
   
       using (var s2 = s1.OpenScope())    // creates scope s2, s1 becomes s2.Parent, s2 becomes ScopeContext.CurrentScope
       {
           var car2 = s2.Resolve<Car>();  // creates new Car and stores it into CurrentScope s2
   
           Assert.AreNotSame(car2, car1); // car2 and car1 are different because residing in different scopes: s2 and s1
   
       }                                  // disposes s2 together with car2, s2.Parent (s1) becomes ScopeContext.CurrentScope again
   
       var car3 = s1.Resolve<Car>();      // returns existing Car (car1) in CurrentScope (s1)
   
       Assert.AreSame(car3, car1);        // car3 and car1 are the same
   
   }                                      // disposes s1 together with car1, CurrentScope becomes null
   
   container.Resolve<Car>();              // throws ContainerException as there is no CurrentScope
```

`IScope.Parent` is used to track scope nesting.

__Note:__ If you want to get `car1` in any nested scope instead of creating the new Car, 
use same name for `OpenScope(name)` and for `Reuse.InCurrentNamedScope(name)`.
Name will identify required scope in nested scopes stack:
```
#!c#
   container.Register<Car>(Reuse.InCurrentNamedScope("top"));
   
   using (var s1 = container.OpenScope("top")) // creates scope s1 with Name="top"
   {
       var car1 = s1.Resolve<Car>();           // looks up nested scope chain for Name=="top", found s1,
                                               // creates new Car and stores it into s1
   
       using (var s2 = s1.OpenScope())         // creates scope s2 without Name
       {
           var car2 = s2.Resolve<Car>();       // looks up nested scope chain for Name=="top", found s1 again,       
                                               // returns existing car1 from s1
   
           Assert.AreSame(car2, car1);         // car2 and car1 are the same
       }
   }
```

To define Name you may use object of any type with overridden method `Equals`: `Reuse.InCurrentNamedScope(42)` - `42` is valid Name.

__Note:__ By default if no Name specified in first `c.OpenScope()` DryIoc will set Name to container's `ScopeContext.RootScopeName`.

To make previous example work without "top" name:
```
#!c#
   container.Register<Car>(Reuse.InCurrentNamedScope(container.ScopeContext.RootScopeName));
   using (var s1 = container.OpenScope()) // creates scope s1 with Name=container.ScopeContext.RootScopeName
   // the rest ...
```

All supported scope contexts have different `IScopeContext.RootScopeName`.
For convenience this name also available statically from ScopeContext type: `ThreadScopeContext.ROOT_SCOPE_NAME`;

So we are close to understanding what is `Reuse.InThread` and `Reuse.InWebRequest`.
Code is better than thousands words:
```
#!c#
   Reuse.InThread = Reuse.InCurrentNamedScope(ThreadScopeContext.ROOT_SCOPE_NAME);
```

It is clear that `InThread` is just in reuse in current scope with special predefined name.

__Note:__ Reuse itself is independent from specific scope context - you may change scope context to another, for instance `AsyncExecutionFlowScopeContext`, without changing reuse in registrations. As long as you `OpenScope` with predefined name everything will work as expected.

## Reuse.InWebRequest

Similar to `InThread` `InWebRequest` is just reuse in scope with special predefined name.
That's it. It is defined as following:

```
#!c#
   Reuse.InWebRequest = Reuse.InCurrentNamedScope(Reuse.WebRequestScopeName);
```

ASP.NET extensions are using `InWebRequest` paired with corresponding ScopeContext:

- `HttpContextScopeContext` for Web Forms and MVC
- `AsyncExecutionFlowScopeContext` for WebApi

__Note:__ In tests you may change scope context without changing reuse, e.g. change `HttpContextScopeContext` to `ThreadScopeContext`:

```
#!c#
   testContainer = webContainer.With(scopeContext: new ThreadScopeContext());
```

__Note:__ If you want to emulate Request Begin/End in test just `OpenScope` with corresponding name: `using (var scope = testContainer.OpenScope(Reuse.WebRequestScopeName)) { }`.

No need to touch reuse in registrations, everything still works.



## Reuse.InResolutionScope

The same instance per Resolution Root, which means the same instance inside `Resolve` method call. 
It is similar to assigning resolved service to variable and then reusing this variable during service creation.

So the manual code:
```
#!c#
   Foo Create()
   {
       var log = new Log();
       return new Foo(log, new Dependency(new SubDependency(log), log))
   }
```

Translates to container setup:
```
#!c#
   container.Register<Log>(Reuse.InResolutionScope);
   container.Register<Foo>(); 
   container.Register<Dependency>(); 
   container.Register<SubDependency>();
   
   // create Foo
   var foo = container.Resolve<Foo>();
   Assert.AreTheSame(foo.Log, foo.Dependency.Log);
```

What if `Log` is `IDisposable`? How it could be disposed?

Container does track current `ResolutionScope` but it may be injected as `IDisposable` as one of service dependencies.
So the service can dispose the scope when the time comes.

```
#!c#
   public class Foo : IDisposable
   {
       public Foo(Log log, Dependency dep, IDisposable scope) {}
       
       public void Dispose()
       {
           _scope.Dispose(); // Will dispose resolution scope together with Log instance
       }
}
```

__Note:__ When [disposable transients tracking](ReuseAndScopes#markdown-header-disposable-transient) is turned On then resolution scope
`IDisposable` dependency will be disposed automatically, as a normal disposable transient.



## Reuse.InResolutionScopeOf

Works similar to [Castle Winsdor Bound LifeStyle](http://docs.castleproject.org/Default.aspx?Page=LifeStyles&NS=Windsor&AspxAutoDetectCookieSupport=1#Bound_8) or
to [Autofac InstancePerOwned Lifestyle](http://docs.autofac.org/en/latest/lifetime/instance-scope.html#instance-per-owned).

Service is reused in specified resolution sub-graph. For instance we want to share `Log` instance inside `XViewModel` object and its dependencies. 
It means that in another `XViewModel` there will be another `Log` instance.
```
#!c#
   public class Presentation 
   {
       public XViewModel Area1, Area2;
       public Presentation(XViewModel area1, XViewModel area2) 
       { 
           Area1 = area1; 
           Area2 = area2; 
       }
   }
   
   public class XViewModel 
   {
       public YViewModel SubArea; public Log Log;
       public XViewModel(YViewModel subArea, Log log) { SubArea = subArea; Log = log; }
   }
   
   public class YViewModel 
   {
       public Log Log;
       public YViewModel(Log log) { Log = log; }
   }
   
   // Container setup:
   c.Register<Presentation>(); c.Register<YViewModel>();
   
   c.Register<XViewModel>(setup: Setup.With(openResolutionScope: true));
   c.Register<Log>(Reuse.InResolutionScopeOf<XViewModel>());
   
   var p = c.Resolve<Presentation>();
   Assert.AreSame(p.Area1.Log, p.Area1.SubArea.Log);
   Assert.AreNotSame(p.Area1.Log, p.Area2.Log);
```

If no matching scope found, container will throw exception because __never fail silently until it explicitly said__.

### How it works?

- When you are calling `container.Resolve` the top-level resolution scope is created - actually it is created on first access for performance reasons.

- If service creation expression contains nested Resolve call: 

   `(state, r, scope) => new Client(r.Resolver.Resolve<Service>(..., scope)`

   (_you can setup it with_ `c.Register<Service>(setup: Setup.With(openResolutionScope: true))`)

   The new scope is created inside new `Resolve` with top-level scope set as `IScope.Parent`. 
   And the same happens for further nested resolves.
   The result is hierarchy of resolution scopes.

- When nested scope is created, it also captures resolved service Type and service Key (_if specified_).
   In example above: `Service` is service Type. This information is used to find matching scope in hierarchy if 
   you register as following: 
   `c.Register<ServiceDependency>(Reuse.InResolutionScopeOf<Service>(serviceKey: optionalKey));`

   __Note:__ 

 - You may register to match not only with exact `Service` type but with it base class/interfaces: `c.Register<ServiceDependency>(Reuse.InResolutionScopeOf<ServiceBaseClass>())`.

 - If both Type and Key specified then they both should match the scope. But for Type only or Key only, it is enough to match specified option, even if scope has both options set up. So you are in control of strategy of scope selection.

- `Reuse.InResolutionScopeOf<T>(key = null, outermost: false)` has additional option `outermost`: it commands to lookup for outermost matched ancestor instead of nearest/closest one.




## Setup.UseParentReuse

This option allows dependency to use parent or ancestor reuse, if it has reused parents.
In case if all parents are transient or dependency is wrapped in `Func` somewhere in parents chain,
then the dependency itself will be transient.

```
#!c#
   class Mercedes
   {
       public Mercedes(Wheels wheels) {}
   }

   class Car 
   {
       public Car(Wheels wheels) {}
   }

   container.Register<Mercedes>(Reuse.Singleton);
   container.Register<Car>();
   container.Register<Wheels>(setup: Setup.With(useParentReuse: true));

   // same Mercedes with the same Wheels
   var m1 = container.Resolve<Mercedes>();
   var m2 = container.Resolve<Mercedes>();

   // different Cars with different Wheels
   var c1 = container.Resolve<Car>();
   var c2 = container.Resolve<Car>();
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
md*/
