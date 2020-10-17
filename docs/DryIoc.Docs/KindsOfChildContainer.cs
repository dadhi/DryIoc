/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Kinds of Child Container


- [Kinds of Child Container](#kinds-of-child-container)
  - [No child containers](#no-child-containers)
  - [Facade](#facade)
  - [With different Rules and ScopeContext](#with-different-rules-and-scopecontext)
  - [Without Cache](#without-cache)
  - [Without Singletons](#without-singletons)
  - [With registrations copy](#with-registrations-copy)
  - [With no more registration allowed](#with-no-more-registration-allowed)
  - [Scenarios from the actual Users](#scenarios-from-the-actual-users)
    - [The child container for each test disposed at end of the test without disposing the parent](#the-child-container-for-each-test-disposed-at-end-of-the-test-without-disposing-the-parent)


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

- `rules` are described in details [here](RulesAndDefaultConventions#Rules-per-Container)
- `scopeContext` and `singletonScope` are described [here](ReuseAndScopes#scopecontext) 

`RegistrySharing` is the `enum` to specify how to re-use the parent registry:
```cs
public enum RegistrySharing { Share, CloneButKeepCache, CloneAndDropCache }
```

The enum member names are self-explanatory.

__Note__: `OpenScope` is another way to create a new container from existing one, but a bit different from `With`.
It is explained in details [here](ReuseAndScopes#incurrentscope).


## Facade

Facade is a new container which allows to have __a new separate registrations__ from the parent container,
making them override the default resolutions of the parent. To make it more concrete, think of example where 
you need to replace the `prod` service in tests with `test` service or mock. 

```cs md*/
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

Actually, `CreateFacade` does not do anything magic. It uses a `With` method to create a new container with
a new default `serviceKey` and set a rule to prefer this `serviceKey` over default:

```cs md*/
static class CreateFacade_implementation 
{
    public const string FacadeKey = "@facade";

    public static IContainer CreateFacade_example(this IContainer container, string facadeKey = FacadeKey) =>
        container.With(rules => rules
            .WithDefaultRegistrationServiceKey(facadeKey)
            .WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(facadeKey)));
}
/*md
```

__Note:__ In case the `CreateFacade` does no meet your use-case, you may always go one level deeper in API and
select your set of rules and arguments for the `With` method.


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


## Scenarios from the actual Users

### The child container for each test disposed at end of the test without disposing the parent

[The related case](https://github.com/dadhi/DryIoc/issues/269)

```cs md*/

class Child_container_per_test_disposed_at_the_end_without_disposing_the_parent
{
    [Test] public void Child_lifecycle_should_be_independent_of_parent_lifecycle()
    {
        var parent = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());
        
        parent.Register<IService, Service>(Reuse.Singleton);

        var child = CreateChildContainer(parent);

        // child can override parent registrations and parent is unchanged
        child.Use<IService>(new TestService());

        Assert.IsInstanceOf<TestService>(child.Resolve<Concrete>().Service);
        Assert.IsInstanceOf<Service>(parent.Resolve<Concrete>().Service);
        
        child.Dispose();
        Assert.IsTrue(child.IsDisposed);

        // when child is disposed parent is unaffected
        Assert.IsFalse(parent.IsDisposed);
        Assert.IsInstanceOf<Service>(parent.Resolve<Concrete>().Service);
    }

    private static IContainer CreateChildContainer(Container parent) => 
        parent.With(
            parent.Rules,
            parent.ScopeContext,
            RegistrySharing.CloneAndDropCache,
            parent.SingletonScope.Clone());

    public interface IService { }
    public class Service : IService { }
    public class TestService : IService { }
    public class Concrete
    {
        public IService Service { get; }
        public Concrete(IService service) => Service = service;
    }
}

/*md
```
md*/
