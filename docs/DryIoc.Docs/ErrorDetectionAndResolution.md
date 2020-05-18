<!--Auto-generated from .cs file, the edits here will be lost! -->

# Error Detection and Resolution

[TOC]

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

using System;
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedParameter.Local

class Unable_to_resolve_unknown_service
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

class Unable_to_resolve_from_registered_services
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
class No_current_scope_available
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

class Recursive_dependency_detected
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

        // ex.Message example:

        // code: RecursiveDependencyDetected;
        // message: Recursive dependency is detected when resolving
        // A as parameter "a" <--recursive
        //  in B as parameter "b" FactoryID=28
        //  in A FactoryID=27 <--recursive
        // from container without scope.
    }
}
```

`<--recursive` identify exact points in object graph when recursion is introduced.


### How to allow recursive dependency

In some case recursive dependency is what you wont, usually inside `Lazy` or `Func` wrapper:
```cs 

class Allow_a_recursive_dependencies
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
class Allow_recursive_dependency_in_DryIoc
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

__Note:__ `Validate` does not actually create any service object, neither affects container state.

```cs 
class Registrations_diagnostics
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

Key of `ServiceRegistrationInfo`:
```
MyService
```

Value of `ContainerException` with `Message`:
```
Unable to resolve RequiredDependency as parameter "dependency"
  in MyService #27
  from container without scope
Where no service registrations found
  and no dynamic registrations found in 0 of Rules.DynamicServiceProviders
  and nothing found in 0 of Rules.UnknownServiceResolvers
```

`Validate` allows to specify the registrations to resolve via predicate `Func<ServiceRegistrationInfo, bool>` 
or via exact collection of service roots `ServiceInfo[]`. 


### Validate is ignorant version of GenerateResolutionExpressions

Internally `Validate` method delegates to another public method `GenerateResolutionExpressions` which collects errors and __successful resolution expressions__. 
The latter are just ignored by `Validate`.

Generated resolution expressions are expression-trees `Expression<DryIoc.FactoryDelegate>` and maybe examined, 
or even compiled to actual delegates and used for __container-less service resolution__.

__Note:__ [DryIocZero](Companions/DryIocZero) package uses the `GenerateResolutionExpressions` to generate factory delegates at compile-time.
