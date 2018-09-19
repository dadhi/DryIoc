/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Kinds of Child Container

[TOC]

## No child containers 

DryIoc has no "usual" notion of child and parent container.  

Instead DryIoc has number of APIs to address specific related scenarios, 
taking advantage of Container immutable state with very fast `O(1)` snapshots.


## With Open Scope

Method `OpenScope` produces a new container and [explained in more details here](ReuseAndScopes#markdown-header-incurrentscope).


## Facade

Facade is a normal container that fall-backs resolution to another container(_parent_) for unresolved services. 
Facade inherits from the parent the `Rules` and the `ScopeContext` and nothing more. Facade has its own Registrations, Cache and Singletons.

Example:
```cs md*/
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
} /*md
```

__Note:__ Because facade is just a normal standalone container it has its own singletons, not shared with parent even if resolved from parent.When you resolve singleton directly from parent and then ask for it from child, it will return another object.

To achieve instance sharing between containers you may use `Reuse.InCurrentScope` instead of Singleton together with `container.OpenScope` ([more info here](https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes)).


## With different Rules and ScopeContext

There is no way to change the rules in-place for existing container.One reason of this design  is simplicity and safety in multi-threaded access, another reason s to prevent possible inconsistency of new rules with already resolved cache.

But you may create new container from old one, by copying its registrations, and without its cache, and with the new Rules:

    var c = new Container();
var newC = c.With(currentRules => ChangeRules(currentRules));

Here first container stays untouched and operates as usual.

New container contains all registrations of the first plus all resolved singletons or scoped services.Because of new rules it may operate differently and produce different services.

__Note:__ Because registry is implemented as immutable structure, copying means just passing its reference without any cost added.Simply put, it is very fast.

Beside the Rules With allows to specify new ScopeContext for the container.Rules and context may be specified together just for convenience of one operation instead of two:


  var newC = c.With(scopeContext: new AsyncExecutionFlowScopeContext());


## Without Cache

Cache in DryIoc usually means Resolution cache consisting of:

- Compiled factory delegates which invoked when you call Resolve.__Factory delegates are static__, they could not reference any state except that provided by parameters.
- Expression trees for creating services and their dependencies.These expressions may be reused when compiling delegates for different services.
- State - items could not be re-created inside expression and therefore should be referenced from expression closure.It may be non primitive metadata objects, or registered custom delegates, or singletons copied to state for optimization.

The reason for removing cache may be changing service registrations.When some dependency was injected into resolved service, dependency expression was cached to save the work for next inject. When you replacing dependency registration, you expect new expression to be used for service.Ultimate tool to ensure fresh service creation is removing the cache.

As usual in DryIoc it is not possible to drop cache in container.You may create new container without cache, but otherwise the same as original:

    container.Resolve<A>();
    container = container.WithoutCache();
    container.Register<A, TestB>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
    container.Resolve<A>(); // now is TestB


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

md*/
