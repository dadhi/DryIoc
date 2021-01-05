/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Kinds of Child Container


- [Kinds of Child Container](#kinds-of-child-container)
  - [No child containers](#no-child-containers)
  - [CreateChild](#createchild)
  - [Facade](#facade)
  - [With different Rules and ScopeContext](#with-different-rules-and-scopecontext)
  - [Without Cache](#without-cache)
  - [Without Singletons](#without-singletons)
  - [With registrations copy](#with-registrations-copy)
  - [With no more registration allowed](#with-no-more-registration-allowed)


## No child containers 

DryIoc has no "usual" notion of child - the container which is have the link to the "parent" and will look for the
registrations in parent once it did not found its own registration.

Instead, DryIoc has a number of APIs to address specific related scenarios 
taking advantage of Container immutable state with very fast `O(1)` copy snapshots.

To create a kind of child container from the existing one, 
you may use one of the extension `With(out)..` methods based on the `IContainer.With` method.

The signature of `IContainer.With` describes what can be changed:
```cs
IContainer With(IResolverContext parent, 
    Rules rules, 
    IScopeContext scopeContext,
    RegistrySharing registrySharing, 
    IScope singletonScope, 
    IScope currentScope, 
    IsRegistryChangePermitted? isRegistryChangePermitted);
```

- `rules` are described in details [here](RulesAndDefaultConventions.md#Rules-per-Container)
- `scopeContext` is described [here](ReuseAndScopes.md#scopecontext) 
- `registrySharing` specifies how or whether to reuse the parent registry and cache and has the values: `Share, CloneButKeepCache, CloneAndDropCache`
- `singletonScope` and `currentScope` may be reused ot cloned with the `Clone(bool withDisposables)` method
- `isRegistryChangePermitted` is self-explanatory and may have the following values: `Permitted, Error, Ignored`

__Note__: `OpenScope` is another way to create a new container from the existing one as explained in details [here](ReuseAndScopes.md#incurrentscope).


## CreateChild

**Note:** The method is the recent addition in v4.7.0

I was hesitating to add the method with such name for the long time because the users expected the different things from the "child" container as the default behavior. 

But collecting the requirements over time I decided to add the method to help with the basic needs and provide the example of how to 
use the `With(...)` method to brew your own "child" container.

Here the whole method:

```cs md*/
using DryIoc;
using NUnit.Framework;

public static partial class ContainerTools
{
    public static IContainer CreateChild(this IContainer container, 
        IfAlreadyRegistered? ifAlreadyRegistered = null, Rules newRules = null, bool withDisposables = false)
    {
        var rules = newRules != null && newRules != container.Rules ? newRules : container.Rules;
        return container.With(
            container.Parent,
            ifAlreadyRegistered == null ? rules : rules.WithDefaultIfAlreadyRegistered(ifAlreadyRegistered.Value),
            container.ScopeContext,
            RegistrySharing.CloneAndDropCache,
            container.SingletonScope.Clone(withDisposables),
            container.CurrentScope ?.Clone(withDisposables));
    }
}

/*md
```

The result container will have the following traits:

- It has all parent registrations copied, so that the registrations added or removed in the child are not affecting the parent. By default child will use the parent `IfAlreadyRegistered` policy but you may specify `IfAlreadyRegistered.Replace` to "shadow" the parent registrations
- It has an access to the scoped services and singletons already created by parent.
- It can be disposed without affecting the parent, disposing the child will dispose only the scoped services and singletons created in the child and not in the parent (can be opt-out with `withDisposables` parameter).
- Its creation has O(1) cost - it is cheap thanks to the fast immutable collections cloning.


## Facade

`CreateFacade` is based on `CreateChild` with addition of rules to mark all the registration with the special service key and
prefer the key over the default one for the resolutions:
```cs md*/

public static partial class ContainerTools
{
    public const string FacadeKey = "@facade";
    public static Rules WithFacadeRules(this Rules rules, string facadeKey = FacadeKey) =>
        rules.WithDefaultRegistrationServiceKey(facadeKey)
             .WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(facadeKey));
}

/*md
```

Let's look into example:

```cs md*/

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

    [Test]public void Facade_for_tests()
    {
        var container = new Container();

        container.Register<IService, ProdService>();
        container.Register<Client>();

        var testFacade = container.CreateFacade();
        testFacade.Register<IService, TestService>();

        var client = testFacade.Resolve<Client>();
        Assert.IsInstanceOf<TestService>(client.Service);
    }
} /*md
```


## With different Rules and ScopeContext

As it said above you may provide the new `rules` and `scopeContext` using the `With` method.

Setting rules is a very common thing, so there is a dedicated `With` overload for this:
```cs
IContainer With(this IContainer container, 
    Func<Rules, Rules> configure = null, 
    IScopeContext scopeContext = null)
```

The important and maybe not as much clear point is what happens with the parent registry in the new container.
The answer is that __the registry is copied__ and the __cache is dropped__. The cache is dropped because
the new rules may lead to the resolving the new services in the child container different from the already
resolved services in the parent. Therefore, we need to drop (invalidate) the cache to stop serving the
wrong results.

The copied (cloned) registry means that the new registration made into the child container won't appear in the parent,
and vice versa. The reason is not only the isolation of the parent from the changes in the child but also there are
rules that affect how registrations are done, e.g. `DefaultRegistrationServiceKey`. 

## Without Cache

Cache in DryIoc usually means the expressions and delegates created and stored in the container while resolving the services.

The reason for removing the cache may be the changing or removing the service registration after the fact - 
when the resolutions were already made and the things were cached. 

```cs md*/
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
} /*md
```


## Without Singletons

To remove resolved singleton instances from the container:
```cs md*/
class Without_singletons
{
    public class S { }
    [Test]public void Example()
    {
        IContainer container = new Container();
        container.Register<S>(Reuse.Singleton);
        var s = container.Resolve<S>();

        var container2 = container.WithoutSingletonsAndCache();
        var s2 = container2.Resolve<S>();

        Assert.AreNotSame(s, s2);
    }
}/*md
```

The method will clone the container registrations but will drop the cache.


## With registrations copy

`WithRegistrationsCopy` will create a container clone (child) where the new registration will be isolated from the parent
and the vice versa.

```cs md*/
class With_registrations_copy
{
    class A { }
    class B { public B(A a) {} }

    [Test]public void Example()
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
/*md
```

Again, the cloning here is the fast `O(1)` operation.

By default, the cache is dropped but you may pass the optional argument `preserveCache: true` to keep the cache.
For instance in the example above we are just adding a new registration without replacing anything, 
so the cache from the parent will proceed to be valid and useful.


## With no more registration allowed

[Explained in detail here](FaqAutofacMigration#separate-build-stage)

md*/
