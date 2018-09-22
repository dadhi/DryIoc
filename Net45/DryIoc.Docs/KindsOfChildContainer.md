<!--Auto-generated from .cs file, the edits here will be lost! -->

# Kinds of Child Container

[TOC]

## No child containers 

DryIoc has no "usual" notion of child and parent container.  

Instead, DryIoc has a number of APIs to address specific related scenarios, 
taking advantage of Container immutable state with very fast `O(1)` copy snapshots.

To create a kind of child container from the existing one, 
you may use one of the extension `With..` methods based on the `IContainer.With` method.

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
    public interface IService {}
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

    [Test] public void Facade_for_tests()
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


## Without cache

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

## Without Singletons

To remove resolved singleton instances from the container:

    container = container.WithoutSingletonsAndCache();

It will create copy of container registrations but without cache, because cache may refer to resolved singleton instances.


## With registrations copy

WithRegistrationsCopy allows to register to some basic container and then produce from it more containers with ready-to-go registrations:

    var startingSet = new Container();
startingSet.RegisterMany(new[] { MyAssembly });
    var container = startingSet.WithRegistrationsCopy();
container.Register<More>();
    container.Register<A, NewA>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

Again copy here is fast O(1) operation.

You may resolve from `startingSet` too, all resolved singletons then will be shared with new container.

Cache is not copied by default, that is why it safe to replace registrations.But if you want you may optionally preserve cache for performance reasons.


## With no more registration allowed

[Here is explained in detail](FaqAutofacMigration#markdown-header-separate-build-stage)

