<!--Auto-generated from .cs file, the edits here will be lost! -->

# Kinds of Child Container

[TOC]

## No child containers 

DryIoc has no "usual" notion of child and parent container.  

Instead, DryIoc has a number of APIs to address specific related scenarios, 
taking advantage of Container immutable state with very fast `O(1)` copy snapshots.

To create a kind of child container from the existing one, 
you may use one of the extension `With(out)..` methods based on the `IContainer.With` method.

The signature of `IContainer.With` describes what can be changed:
```cs
IContainer With(Rules rules, IScopeContext scopeContext, RegistrySharing registrySharing, IScope singletonScope);
```

- `rules` are described in details [here](RulesAndDefaultConventions#markdown-header-Rules-per-Container)
- `scopeContext` and `singletonScope` are described [here](ReuseAndScopes#markdown-header-scopecontext) 

`RegistrySharing` is the `enum` to specify how to re-use the parent registry:
```cs
public enum RegistrySharing { Share, CloneButKeepCache, CloneAndDropCache }
```

The enum member names are self-explanatory.

__Note__: `OpenScope` is another way to create a new container from existing one, but a bit different from `With`.
It is explained in details [here](ReuseAndScopes#markdown-header-incurrentscope).


## Facade

Facade is a new container which allows to have __a new separate registrations__ from the parent container,
making them override the default resolutions of the parent. To make it more concrete, think of example where 
you need to replace the `prod` service in tests with `test` service or mock. 

```cs 
using DryIoc;
using NUnit.Framework;

class FacadeExample
{
    public interface IService { }
    public class ProdService : IService { }
    public class TestService : IService { }

    public class Client
    {
        public IService Service { get; }
        public Client(IService service)
        {
            Service = service;
        }
    }

    [Test]
    public void Facade_for_tests()
    {
        var container = new Container();

        container.Register<IService, ProdService>();
        container.Register<Client>();

        var testFacade = container.CreateFacade();
        testFacade.Register<IService, TestService>();

        var client = testFacade.Resolve<Client>();
        Assert.IsInstanceOf<TestService>(client.Service);
    }
} 
```

Actually, `CreateFacade` does not do anything magic. It uses a `With` method to create a new container with
a new default `serviceKey` and set a rule to prefer this `serviceKey` over default:

```cs
public static IContainer CreateFacade(this IContainer container, string facadeKey = FacadeKey) =>
    container.With(rules => rules
        .WithDefaultRegistrationServiceKey(facadeKey)
        .WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(facadeKey)));
```

__Note:__ In case the `CreateFacade` does no meet your use-case, you may always go one level deeper in API and
select your set of rules and arguments for the `With` method.


## With different Rules and ScopeContext

As it said above, you may provide a new `rules` and `scopeContext` using the `With` method.

Setting rules is a very common thing, so there is a dedicated `With` overload for this:
```cs
IContainer With(this IContainer container, 
    Func<Rules, Rules> configure = null, 
    IScopeContext scopeContext = null)
```

The important and may be not clear point, what happens with a parent registry in a new container.
The answer is the __registry is cloned__ and the __cache is dropped__. The cache is dropped, because
the new rules may lead to resolving a new services in a child container, different from the already
resolved services in the parent. Therefore, we need to drop (invalidate) the cache to stop serving the
wrong results.

The cloned registry means that new registration made into child container won't appear in the parent,
and vice versa. The reason is not only an isolation of parent from the changes in child, but also there are
rules that affect how registrations are done, e.g. `DefaultRegistrationServiceKey`. 

## With expression generation

```cs
public static IContainer WithExpressionGeneration(this IContainer container)
```

Will store the expressions built for service resolution. 
This is used in `Validate` and `GenerateResolutionExpressions` methods described 
[here](ErrorDetectionAndResolution-#markdown-header-Service-Registrations-Diagnostics).


## Without Cache

Cache in DryIoc usually means a some artifacts stored while resolving services.

More precisely, the resolution cache contains a set of `FactoryDelegate` compiled from expression trees
to create an actual services.

The reason for removing cache may be changing or removing a service registration after the fact, 
when the resolutions were already made from Container and were cached internally. 

```cd 
class Without_cache
{
    public interface I { }
    public class Prod : I { }
    public class Test : I { }

    public class B
    {
        public I I { get; }
        public B(I i) { I = i; }
    }

    [Test]
    public void Example()
    {
        IContainer container = new Container();
        container.Register<B>();
        container.Register<I, Prod>();
        var b = container.Resolve<B>();
        Assert.IsInstanceOf<Prod>(b.I);

        // now lets replace the I with Test
        container.Register<I, Test>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        b = container.Resolve<B>();
        // As you can see the dependency did no change, it is still `Prod` and not the `Test`
        Assert.IsInstanceOf<Prod>(b.I);

        container = container.WithoutCache();
        b = container.Resolve<B>();
        // And now it is Test
        Assert.IsInstanceOf<Test>(b.I);
    }
} 
```


## Without Singletons

To remove resolved singleton instances from the container:
```cd 
class Without_singletons
{
    public class S { }
    [Test]
    public void Example()
    {
        IContainer container = new Container();
        container.Register<S>(Reuse.Singleton);
        var s = container.Resolve<S>();

        var container2 = container.WithoutSingletonsAndCache();
        var s2 = container2.Resolve<S>();

        Assert.AreNotSame(s, s2);
    }
}
```

The method will clone the container registrations but will drop cache.


## With registrations copy

`WithRegistrationsCopy` will create a container clone (child) where new registration will be isolated from the parent
and vice versa.


class With_registrations_copy
{
    class A { }
    class B { public B(A a) {} }

    [Test]
    public void Example()
    {
        var parent = new Container();
        parent.Register<A>();

        // by default cache will be dropped
        var child = parent.WithRegistrationsCopy(preserveCache: false);
        child.Register<B>();

        // child know about dependency `A` copied from the parent
        var b = child.Resolve<B>();
        Assert.IsNotNull(b);

        // parent does not know about `B`
        var parentB = parent.Resolve<B>(IfUnresolved.ReturnDefaultIfNotRegistered);
        Assert.IsNull(parentB);
    }
}
```

Again, a cloning here is the fast `O(1)` operation.

By default, the cache is dropped, but you may pass optional argument `preserveCache: true` to keep the cache.
For instance in the example above, we atr just adding a new registration without replacing anything, 
so the cache from the parent will work just fine.


## With no more registration allowed

[Explained in detail here](FaqAutofacMigration#markdown-header-separate-build-stage)

