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
```csharp
class X 
{ 
    X(Y y) {}
}

// registering X but forget to register Y
container.Register<X>(); 

// do resolve
container.Resolve<X>();

/* ContainerException message:

Unable to resolve MyNamespace.Y as parameter "y"
    in MyNamespace.X.
Where no service registrations found
    and no dynamic registrations found in 0 of Rules.DynamicServiceProviders
    and nothing found in 0 of Rules.UnknownServiceResolvers
*/
```


### UnableToResolveFromRegisteredServices

If in previous example, `Y` class is registered with key, then DryIoc will list available registrations and provide an additional information about container state:
```csharp
container.Register<X>();
container.Register<Y>(serviceKey: "special");

container.Resolve<X>();

/* ContainerException message:

Unable to resolve MyNamespace.Y as parameter "y"
    in MyNamespace.X
from container
with normal and dynamic registrations:
    ("special", {ID=27, MyNamespace.Y})
*/
```

Let's see a bit different situation when you have registered a scoped service, but there was no scope opened.

```csharp
container.Register<X>();
container.Register<Y>(Reuse.Scoped);
    
container.Resolve<X>();
    
/* ContainerException message:

No current scope available. Probably you are registering to, or resolving from outside of scope. 
Current resolver context is: container without scope
*/
```


## RecursiveDependencyDetected

The problem says that you dependencies form a (infinite) cycle in object graph. 
Better illustrated with code:
```csharp
class A 
{
    public A(B b) { } // A requires B
}

class B 
{
    public B(A a) { } // and B requires A
}
```

Straightforward approach of creating `A` with `new` will fail:
```csharp
new A(new B(new A // Infinite loop!
``` 

Fails for Container as well.

__Note:__ Recursive dependency usually points to design problem. That's why some languages check and prohibit it at compile-time, e.g. F#.

DryIoc will throw `ContainerException` with `Error.RecursiveDependencyDetected` when resolving either `A` or `B`:
```csharp
container.Register<A>();
container.Register<B>();

container.Resolve<A>(); // throws

/*
ContainerException message:

Recursive dependency is detected when resolving
MyNamespace.A as parameter "a" <--recursive
    in MyNamespace.B as parameter "b"
    in MyNamespace.A <--recursive.
*/
```

`<--recursive` markings identify exact points in object graph when recursion is introduced.


### How to allow recursive dependency

In some case recursive dependency is what you wont, usually inside `Lazy` or `Func` wrapper:
```csharp
class Parent 
{
    public(Child child) {}
}

class Child 
{
    public(Lazy<Parent> parentAccess) { /* stores lazy parentAccess for future use */ }
}

Parent parent = null;
var parentAccess = new Lazy<Parent>(() => parent);
parent = new Parent(new Child(parentAccess)); 
// Only after that point parentAccess will return initialized parent.
```

By the way, DryIoc natively supports  `Lazy`  and `Func` [wrappers](Wrappers.md):
```csharp
container.Register<Child>();
container.Register<Parent>();

var parent = container.Resolve<Parent>(); // works just fine
```


## Service Registrations Diagnostics

DryIoc provides a way to examine potential errors in Container registrations __prior to the actual service resolution__ via `Validate` method overloads. The method finds all or selected registrations (except the open-generics), tries to "resolve" them and catches the errors.

__Note:__ `Validate` does not actually create any service object, neither affects container state.

```csharp
// Given following setup:
public class RequiredDependency {}
public class MyService { public MyService(RequiredDependency d) {} }

// Let's assume we forgot to register RequiredDependency
var container = new Container();
container.Register<MyService>();

// Find what's missing
var errors = container.Validate();
Assert.AreEqual(1, errors.Length);
```

`errors` is the collection of key-value pairs of `ServiceRegistrationInfo` and `ContainerException`. In this example above the error will contain:

key:
```
MyNamespace.MyService registered as factory {ID=14, ImplType=MyNamespace.MyService}
```

value:
```
DryIoc.ContainerException: Unable to resolve MyNamespace.RequiredDependency as parameter "d"
    in MyNamespace.MyService.  
Where no service registrations found  
    and no dynamic registrations found in 0 of Rules.DynamicServiceProviders
    and nothing found in 0 of Rules.UnknownServiceResolvers
```

`Validate` allows to specify the registrations to resolve via either predicate `Func<ServiceRegistrationInfo, bool>` or via exact collection of service roots `ServiceInfo[]`. 


### Validate is ignorant version of GenerateResolutionExpressions

Internally `Validate` method delegates to another public method `GenerateResolutionExpressions` which collects errors and __successful resolution expressions__. The latter are just ignored by `Validate`.

Generated resolution expressions are expression-trees `Expression<DryIoc.FactoryDelegate>` and maybe examined, or even compiled to actual delegates and used for __container-less service resolution__.

__Note:__ [DryIocZero](Companions/DryIocZero) package uses the `GenerateResolutionExpressions` to generate factory delegates at compile-time.
