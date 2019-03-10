**Not updated yet for V4**

# Unregister and Resolution Cache

[TOC]

## Unregister

Given setup:

    public class B {}
    public class A 
    {
        public A(B b) {}
    }

    container.Register<A>();
    container.Register<B>();

### Works for not resolved and not injected service

    container.Unregister<A>();
    // and/or container.Unregister<B>();

    container.Resolve<A>(); // "Unable to resolve.." exception

### Works for resolved root

    var a = container.Resolve<A>();

    container.Unregister<A>();

    container.Resolve<A>(); // "Unable to resolve.." exception

### Does not work for injected dependency

    var a = container.Resolve<A>(); // here B is injected in A

    container.Unregister<B>(); // unregister dependency B

    var a1 = container.Resolve<A>(); // Unexpectedly still resolves!

That's because DryIoc is caching creation expressions.

When resolving A there will be two expressions cached:
 
- `new A(new B())`
- and separately `new B()`, to reuse in other services depending on B

This caching leads to the fact, that even if B is unregistered and its cache `new B()` is removed, there is still expression B in A cache `new A(new B())`.

To make the unregister work we need to prevent inlinlining of B into expression of A.

### Make injected dependency re-wireable by registering it asResolutionCall

    container.Register<B>(setup: Setup.With(asResolutionCall: true));
    
    var a = container.Resolve<A>(); // B injected as: new A(r.Resolve<B>())
    container.Unregister<B>();
    var a1 = container.Resolve<A>(); // Expected "Unable to resolve.." exception
    


## Unregister and Reused services

Unregistering will work the same way as for non-reused services. 
__But be aware__ that reused instance won't be removed from the scope and won't be disposed until the scope is disposed.

### Unregistering resolved singleton

    public class B {}
    public class A 
    {
        public A(B b) {}
    }

    container.Register<A>(Reuse.Singleton);
    container.Register<B>();

    var a = container.Resolve<A>();

    container.Unregister<A>();

    Assert.Throws<ContainerException>(() => 
    container.Resolve<A>()); // Will throw "Unable to resolve.." exception

    // Unregistered singleton will be kept for container lifetime 
    // and disposed with the rest of singletons.
    // You may register singleton as Setup.With(weaklyReferenced: true)
    Assert.IsFalse(a.IsDisposed); 

    // After that re-registering and resolving A should return different instance
    container.Register<A>(Reuse.Singleton);

    var a1 = container.Resolve<A>();
    Assert.AreNotSame(a, a1);

### Unregistering injected singleton

Use the same setup `asResolutionCall: true` as a workaround for tne dependency owner cache.

    container.Register<A>();
    container.Register<B>(Reuse.Singleton, setup: Setup.With(asResolutionCall: true));

    var a = container.Resolve<A>();

    container.Unregister<B>();

    Assert.Throws<ContainerException>(() =>
    container.Resolve<A>()); // Will throw "Unable to resolve.." exception

    // Unregistered singleton will be kept for container lifetime 
    // and disposed with the rest of singletons.
    // You may register singleton as Setup.With(weaklyReferenced: true)
    Assert.IsFalse(a.B.IsDisposed);

    // After that re-registering and resolving A should return different instance
    container.Register<B>(Reuse.Singleton);

    var a1 = container.Resolve<A>();
    Assert.AreNotSame(a, a1);