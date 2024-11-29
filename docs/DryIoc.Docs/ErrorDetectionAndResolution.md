<!--Auto-generated from .cs file, the edits here will be lost! -->

# Error Detection and Resolution


- [Error Detection and Resolution](#error-detection-and-resolution)
  - [Overview](#overview)
  - [DryIoc exceptions](#dryioc-exceptions)
  - [Unable to resolve](#unable-to-resolve)
    - [UnableToResolveUnknownService](#unabletoresolveunknownservice)
    - [UnableToResolveFromRegisteredServices](#unabletoresolvefromregisteredservices)
  - [RecursiveDependencyDetected](#recursivedependencydetected)
    - [How to allow recursive dependency](#how-to-allow-recursive-dependency)
  - [Service Registrations Diagnostics](#service-registrations-diagnostics)
  - [Using Validate to check for Captive Dependency](#using-validate-to-check-for-captive-dependency)


## Overview

DryIoc motto is:

- Be as deterministic as possible, but provide a reasonable defaults.
- Try to detect errors as early as possible, better at compile-time, otherwise better when registering than when resolving things.
- Never fail silently and provide information about problem, context, and the possible fix. 


## DryIoc exceptions

When something goes wrong then DryIoc will throw a `ContainerException`. It is derived from `InvalidOperationException`. It also used for checking an input arguments instead of `ArgumentException` - this way you know that container is culprit, not the other code.

`ContainerException` has two properties: 

- `Message` display info about problem cause, context and possible fix.
- `Error` provides error-code to test and filter corresponding error case.

All DryIoc errors with their `Error` and `Message` are listed in `DryIoc.Error` class. If in doubt, you may look in this class on what DryIoc is capable to detect and the level of information it provides.


## Unable to resolve

A very common problem when you forgot to register required service or dependency, or Container was unable to use existing registrations for specific reason.


### UnableToResolveUnknownService

For instance if no registration exist for the service type - nor keyed nor default, then the error will be:
```cs 
namespace DryIoc.Docs;
using System;
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedParameter.Local

public class Unable_to_resolve_unknown_service
{
    public class Y { }
    public class X { public X(Y y) { } }

    [Test]
    public void Example()
    {
        var container = new Container();

        // registering X but forget to register Y
        container.Register<X>();

        // the resolve will throw
        var ex = Assert.Throws<ContainerException>(() =>
            container.Resolve<X>());

        Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), ex.ErrorName);

        // ex.Message:
        // Unable to resolve Y as parameter "y"
        //   in X #27
        //   from Container without Scope
        //   Where no service registrations found
        // and no dynamic registrations found in 0 of Rules.DynamicServiceProviders
        //   and nothing found in 0 of Rules.UnknownServiceResolvers
    }
}
```


### UnableToResolveFromRegisteredServices

If in previous example, `Y` class is registered with key, 
then DryIoc will list available registrations and provide an additional information about container state:
```cs 

public class Unable_to_resolve_from_registered_services
{
    public class Y { }
    public class X { public X(Y y) { } }

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<X>();
        container.Register<Y>(serviceKey: "special");

        var ex = Assert.Throws<ContainerException>(() => 
            container.Resolve<X>());

        Assert.AreEqual(Error.NameOf(Error.UnableToResolveFromRegisteredServices), ex.ErrorName);

        // ex.Message:
        // Unable to resolve Y as parameter "y"
        //   in X #27
        //   from Container without Scope
        //   with normal and dynamic registrations:
        // ("special", { FactoryID = 28, ImplType = Y})
    }
}
```

Let's see a bit different situation when you have registered a scoped service, but there was no scope opened.

```cs 
public class No_current_scope_available
{
    public class Y { }
    public class X { public X(Y y) { } }

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<X>();
        container.Register<Y>(Reuse.Scoped);

        var ex = Assert.Throws<ContainerException>(() =>
            container.Resolve<X>());

        Assert.AreEqual(Error.NameOf(Error.NoCurrentScope), ex.ErrorName);

        // ex.Message:
        // No current scope is available: probably you are registering to, or resolving from outside of the scope.
        // Current resolver context is: container without scope.
    }
}
```

## RecursiveDependencyDetected

The problem says that you dependencies form a (infinite) cycle in object graph. 
Better illustrated with code:
``` 
class Recursive_dependencies
{
    class A
    {
        public A(B b) { } // A requires B
    }

    class B
    {
        public B(A a) { } // B requires A
    }
}
```

Straightforward approach of creating `A` with a `new` will fail:
```cs
new A(new B(new A // Infinite loop!
``` 
The same will fail for container as well.

__Note:__ Recursive dependency usually points to a design problem. That's why some languages prohibit it at compile-time, e.g. F#.

DryIoc will throw `ContainerException` with `Error.RecursiveDependencyDetected` when resolving either `A` or `B`:
```cs 

public class Recursive_dependency_detected
{
    class A { public A(B b) { } }
    class B { public B(A a) { } }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<A>();
        container.Register<B>();

        var ex = Assert.Throws<ContainerException>(() =>
            container.Resolve<A>());

        Assert.AreEqual(Error.NameOf(Error.RecursiveDependencyDetected), ex.ErrorName);

        // contains recursive twice
        StringAssert.Contains(@"<--recursive", ex.Message);

        // ex.Message example: """
        // code: Error.RecursiveDependencyDetected;
        // message: Recursive dependency is detected when resolving
        // A as parameter "a" <--recursive
        //  in B as parameter "b" FactoryID=28
        //  in A FactoryID=27 <--recursive
        // from container without scope.
        // """
    }
}
```

`<--recursive` identify exact points in object graph when recursion is introduced.


### How to allow recursive dependency

In some case recursive dependency is what you want, usually inside `Lazy` or `Func` with [caveat](Wrappers.md#really-lazy-lazy-and-func):
```cs 

public class Allow_a_recursive_dependencies
{
    class Parent
    {
        public Parent(Child child) {}
    }

    class Child
    {
        public Child(Lazy<Parent> lazyParent) {}
    }

    [Test]
    public void Example()
    {
        Parent parent = null;
        parent = new Parent(new Child(new Lazy<Parent>(() => parent)));
    }
}
```

By the way, DryIoc natively supports  `Lazy`  and `Func` [wrappers](Wrappers.md):
```cs 
public class Allow_recursive_dependency_in_DryIoc
{
    class Child
    {
        public Parent Parent => _lazyParent.Value;
        public Child(Lazy<Parent> lazyParent) { _lazyParent = lazyParent; }
        private readonly Lazy<Parent> _lazyParent;
    }

    class Parent
    {
        public Child Child { get; }
        public Parent(Child child) { Child = child; }
    }

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Child>();

        // note the singleton reuse, so that container resolves and injects the same instance of Parent 
        container.Register<Parent>(Reuse.Singleton);

        var parent = container.Resolve<Parent>(); // works just fine
        Assert.AreSame(parent, parent.Child.Parent);
    }
}
```


## Service Registrations Diagnostics

DryIoc provides a way to examine potential errors in Container registrations __prior to the actual service resolution__ via `Validate` method overloads. 
The method finds all or selected registrations (except for the open-generics), tries to "resolve" them and catches the errors.

__Note:__ `Validate` does not actually create any service object, neither affects container state (internally it clones the container with modified rules to guide the validation)

```cs 
public class Registrations_diagnostics
{
    public class RequiredDependency { }
    public class MyService { public MyService(RequiredDependency dependency) { } }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<MyService>();

        // Let's assume we forgot to register a RequiredDependency
        //container.Register<RequiredDependency>();

       // Find what's missing
       var errors = container.Validate();
       Assert.AreEqual(1, errors.Length);
       Assert.AreEqual(nameof(MyService), errors[0].Key.ServiceType.Name);
    }
}
```

`errors` is the collection of key-value pairs of `ServiceRegistrationInfo` and `ContainerException`. 
In the example above the error will contain:

Key:
```
MyService
```

Value of `ContainerException` will have a message similar to this:
```
code: Error.UnableToResolveUnknownService
message: Unable to resolve RequiredDependency as parameter "dependency"
  in MyService #27
  from container without scope
Where no service registrations found
  and no dynamic registrations found in 0 of Rules.DynamicServiceProviders
  and nothing found in 0 of Rules.UnknownServiceResolvers
```

`Validate` allows to specify the registrations to resolve via predicate `Func<ServiceRegistrationInfo, bool>` 
or via exact collection of service roots `ServiceInfo[]`. 

## Using Validate to check for Captive Dependency

Captive Dependency in DI means the use of service with shorter lifespan inside a service with longer lifespan, e.g.
when a Scoped dependency is injected into Singleton. The problem here is that dependency with shorter livespan may be 
requested from the longer lived consumer when the dependency is already dead - and now you are in uncharted territory.

```cs 
public class Validate_CaptiveDependency_example
{
    [Test]
    public void Scoped_in_a_Singleton_should_be_reported_by_Validate()
    {
        var container = new Container();
        container.Register<Foo>(Reuse.Scoped);
        container.Register<Bar>(Reuse.Singleton);
        container.Register<Buz>(Reuse.Scoped); // here is the problem!

        var errors = container.Validate(ServiceInfo.Of<Foo>());

        Assert.AreEqual(1, errors.Length);
        var error = errors[0].Value;
        Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), error.ErrorName);

        /* Exception message:
        code: Error.DependencyHasShorterReuseLifespan; 
        message: Dependency Buz as parameter "buz" (IsSingletonOrDependencyOfSingleton) with reuse Scoped {Lifespan=100} has a shorter lifespan than its parent's Singleton Bar as parameter "bar" FactoryID=145 (IsSingletonOrDependencyOfSingleton)
            in resolution root Scoped Foo FactoryID=144
            from container without scope
            with Rules with {UsedForValidation} and without {ImplicitCheckForReuseMatchingScope, EagerCachingSingletonForFasterAccess} with DependencyCountInLambdaToSplitBigObjectGraph=2147483647
        If you know what you're doing you may disable this error with the rule `new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan())`.
        */
    }

    public class Foo
    {
        public Foo(Bar bar) {}
    }
    
    public class Bar
    {
        public Bar(Buz buz) {}
    }

    public class Buz { }
}
```


