/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Rules and Default Conventions

[TOC]

## General Approach

- DryIoc strives to be as deterministic as possible. 
By default in uncertain case it will rather throw exception than hinder the case with convention - 
because of "never fail silently" and "be smart but not smarter than you". 

__Note:__ DryIoc does leading towards the Best Practices and the Pit-Of-Success by providing a more simple, less noisy, 
configured by default API, rather than a opinionated feature prohibition.

- DryIoc designed to be unobtrusive, with ability to start using DI with legacy code and 3rd party libraries. 
It has small footprint and no particular requirements for consumer code.

- DryIoc provides sensible defaults but allows to override them. 
Based on defaults nature they may be overridden per container(s) with `Rules` or per service with optional parameters and `Setup` object. 
Rules per container(s) mean that `Rules` object may be created once for your typical needs and then reused in many containers.


## Resolution order

DryIoc follows a certain order when looking for the service to resolve:

1. Look for concrete type registration. For generic service it means looking for closed-generic registration.
2. If not found and service is generic: Look for open-generic service registration.
3. If not found: Look for [Wrapper](Wrappers) of requested type.
4. If not found: Resolve from Fallback Containers if any.
5. If not resolved: Resolve with Unknown Service Resolver rules if any.

Additionally after the service is found, DryIoc will look for any matching [Decorators](Decorators).


## Multiple services

### Registering multiple default services

- DryIoc allows to register multiple default (without key) services. 
Actually, multiple services mean multiple implementations of the single service type:
```cs md*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DryIoc;
using NUnit.Framework;

class Register_many_implementation
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<ICommand, Copy>();
        container.Register<ICommand, Paste>();

        var commands = container.Resolve<ICommand[]>();
        Assert.AreEqual(2, commands.Length);
    }

    interface ICommand {}
    class Copy : ICommand { }
    class Paste : ICommand { }
} /*md
```

This default behavior is specified with the default value of `ifAlreadyRegistered` optional parameter in Register method. 
The equivalent to the previous code would be: 
```cs md*/
class Register_many_implementation_with_default_if_already_registered_behavior
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<ICommand, Copy>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
        container.Register<ICommand, Paste>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

        var commands = container.Resolve<ICommand[]>();
        Assert.AreEqual(2, commands.Length);
    }

    interface ICommand { }
    class Copy : ICommand { }
    class Paste : ICommand { }
} /*md
```

The other options are `Throw`, `Keep`, `Replace`, and `AppendNewImplementation`:

- `Throw` will throw exception on second registration for the same service type.
- `Keep` will preserve all previous registrations for the service type, basically it turns method Register to Register Once.
- `Replace` will replace all previous service registrations with the new one. 
- `AppendNewImplementation` will add the the new implementation for the service, 
otherwise (if implementation is the same) it will be kept intact. This option is for having a single same implementation of the service.

__Note:__ The result of `Replace` may be NOT what you expect if the first service was already resolved and cached by DryIoc. 
To ensure `Replace` to work after `Resolve` you should __specifically register for it__.

Registering with `asResolutionCall` `setup` makes consumers depend on the call to `Resolve` instead of 
inline service creation (which is not replaceable):

```cs md*/
class Register_with_ifAlreadyReplaced_option
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<ICommand, Copy>(setup: Setup.With(asResolutionCall: true));
        container.Register<CopyMenu>();

        // now resolve the menu with `Copy` command
        var menu = container.Resolve<CopyMenu>();
        Assert.IsInstanceOf<Copy>(menu.Command);

        container.Register<ICommand, FastCopy>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

        // resolve a new menu
        menu = container.Resolve<CopyMenu>();
        Assert.IsInstanceOf<FastCopy>(menu.Command);
    }

    interface ICommand { }
    class Copy : ICommand { }
    class FastCopy : ICommand { }
    class CopyMenu
    {
        public ICommand Command { get; }
        public CopyMenu(ICommand command) { Command = command; }
    }
}/*md
```


### Resolving from multiple default services

- DryIoc does not use any smart policy to select from multiple defaults. 
Sometimes multiple registrations are due error, sometimes you want to get latest. 
But container most be defensive, deterministic and fail fast. Therefore when resolving single service from 
multiple available it will throw `ContainerException` with message 
`"Unable to select from multiple default <registration list> ..."`.

You may alter the container `Rules` how to select a service from multiple available. 
For instance, to select the latest registration:
```cs md*/
class Select_last_registered_service
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules
            .WithFactorySelector(Rules.SelectLastRegisteredFactory()));

        container.Register<ILetter, A>();
        container.Register<ILetter, B>();

        var letter = container.Resolve<ILetter>(); // will return `B` without exception
        Assert.IsInstanceOf<B>(letter);
    }

    interface ILetter {}
    class A : ILetter {}
    class B : ILetter {}
} /*md
```


## Injecting dependency asResolutionCall

By default DryIoc is injecting dependency by directly putting dependency creation expression into dependency holder constructor 
(or property, field, factory method).

```cs md*/
class AsResolutionCall_setup
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<IDep, Dep>();
        container.Register<Holder>();

        var expr = container.Resolve<LambdaExpression>(typeof(Holder));

        // To make resolve possible, DryIoc constructs and then calls the following delegate (pseudo-code)
        Assert.AreEqual("r => new Holder(new Dep())", expr.ToString());
    }

    interface IDep { }
    class Dep : IDep { }
    class Holder
    {
        public Holder(IDep dep) { }
    }
} /*md
```

Pretty straightforward, it will be more complicated for reused dependency, 
but the `new Dep()` still be there, embedded into the holder expression. 

Such approach also exposes the reason why is the recursive creation of `Dep` is not possible, 
the expression would've been infinite: 
```cs
    new Dep(new Dep(new Dep(... // to infinity 
```

But sometimes the recursion or generally the "dynamic" resolution of dependency is required. 
For instance when injecting a [Lazy](Wrappers#markdown-header-lazy-of-a) we can expect the recursion to be possible.

Given approach with embedded expression:
```cs
    public class Holder 
    {
        public Holder(Lazy<IDep> dep) {}
    }

    // r => new Holder(new Lazy(() => new Dep()))
```

Therefore DryIoc supports a special factory setup option `asResolutionCall`. 
`Lazy` wrapper has this option enabled by default. 

To enable it to for normal dependency you need to change the `Dep` registration to: 
```cs
container.Register<IDep, Dep>(setup: Setup.With(asResolutionCall: true))
```

Instead of inlined expression DryIoc will embed the nested call to `Resolve` method, 
which black boxes the dependency creation for its holder. Such approach will make the recursion 
generally possible for `Lazy` and `Func` wrappers.


## Implicit registration selection based on scope

By default DryIoc will implicitly filter out a service that does not have matching scope.

```cs md*/
class Filter_out_service_that_do_not_have_a_matching_scope
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<I, A>(Reuse.ScopedTo("a"));
        container.Register<I, B>(Reuse.ScopedTo("b"));

        using (var scope = container.OpenScope("b"))
        {
            // will skip a registration of `A` because the open scope has different name
            var b = scope.Resolve<I>();
            Assert.IsInstanceOf<B>(b);
        }
    }

    interface I { }
    class A : I { }
    class B : I { }
} /*md
```

__Note:__ This way you may identify a service based on its reuse rather than having a specific service key.

A usual, the rule could be turned off:
```cs md*/
class Turning_matching_scope_filtering_Off
{
    [Test]
    public void Example() 
    {
        var container = new Container(rules => rules
            .WithoutImplicitCheckForReuseMatchingScope());

        container.Register<I, A>(Reuse.ScopedTo("a"));
        container.Register<I, B>(Reuse.ScopedTo("b"));

        using (var scope = container.OpenScope("b"))
        { 
            // Throws an exception because of the multiple `I` registrations found
            Assert.Throws<ContainerException>(() => scope.Resolve<I>());
        }
    }

    interface I { }
    class A : I { }
    class B : I { }
} /*md
```

__Note:__ When both `Singleton` and `Transient` services are registered, DryIoc will prefer the `Singleton` service. 

In case you are using `Rules.WithFactorySelector(Rules.SelectLastRegisteredFactory())` and wandering how it relates to implicit reuse-based service selection,
here is the example:
```cs md*/
class Select_last_registered_factory_with_implicit_scope_selection
{
    [Test]
    public void Example()
    {
        IContainer container = new Container();

        container.Register<I, A>(Reuse.Singleton);
        container.Register<I, B>(Reuse.Transient);

        var i = container.Resolve<I>();
        Assert.IsInstanceOf<A>(i); // Singleton is implicitly preferred over Transient

        container = container.With(rules => rules
            .WithFactorySelector(Rules.SelectLastRegisteredFactory()));

        i = container.Resolve<I>();
        Assert.IsInstanceOf<B>(i); // Transient is used because the explicit rule is in work
    }

    interface I { }
    class A : I { }
    class B : I { }
} /*md
```


## Implicitly available services
 
DryIoc automatically without registration can resolve or inject: `IResolverContext`, `IResolver`, `IRegistrator`, `IContainer`, 
and `IServiceProvider` (for the platforms where `IServiceProvider` is available).

Note: `IResolverContext` implements `IResolver` and additionally provides the **access to the current scope** if any and allows 
to open a nested scope, it also implements `IDisposable` to dispose of the current scope.


### Container interfaces

Normally using container directly in your services indicates a "code-smell" and referred as a 
[Service Locator anti-pattern](http://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/).

There are still number of cases when it may be useful. For instance, integration with other libraries.

DryIoc makes `Container` interfaces automatically available (injected) because registering them manually may be tricky, 
especially in presence of the scope.

```cs md*/
class Automatically_injected_container_interfaces
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<X>(Reuse.Scoped);

        using (var scope = container.OpenScope())
        {
            var x = scope.Resolve<X>();

            // the expected behavior is that `x.Resolver` will be the one from scope, not from container
            Assert.AreSame(scope, x.Resolver);
        }
    }

    class X
    {
        public readonly IResolver Resolver;
        public X(IResolver resolver) { Resolver = resolver; }
    }
} /*md
```

Given the example you can see that registering a `Container` object will not get you a scope for the scoped services.

The right way to register container interfaces manually with the correct scoping behavior may be this:
```cs md*/
class Registering_container_interfaces_by_hand
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.RegisterDelegate<IResolver>(resolver => resolver, 
            setup: Setup.With(allowDisposableTransient: true));

        container.RegisterDelegate<IRegistrator>(resolver => (IRegistrator)resolver, 
            setup: Setup.With(allowDisposableTransient: true));

        container.RegisterDelegate<IContainer>(resolver => (IContainer)resolver, 
            setup: Setup.With(allowDisposableTransient: true));

        // etc.
    }

    [Test]
    public void Example_injecting_all_container_interfaces_without_registering_them()
    {
        var container = new Container();

        container.Register<User>();

        var x = container.Resolve<User>();

        Assert.AreSame(container, x.Container);
        Assert.AreSame(container, x.Registrator);
        Assert.AreSame(container, x.ResolverContext);
        Assert.AreSame(container, x.Resolver);
        Assert.AreSame(container, x.ServiceProvider);

        using (var scope = container.OpenScope())
        {
            var y = scope.Resolve<User>();
            Assert.AreSame(scope, y.Container);
            Assert.AreSame(scope, y.Registrator);
            Assert.AreSame(scope, y.ResolverContext);
            Assert.AreSame(scope, y.Resolver);
            Assert.AreSame(scope, y.ServiceProvider);
        }
    }

    public class User
    {
        public IServiceProvider ServiceProvider { get; }
        public IResolver Resolver { get; }
        public IResolverContext ResolverContext { get; }
        public IRegistrator Registrator { get; }
        public IContainer Container { get; }

        public User(IServiceProvider serviceProvider, IResolver resolver, IResolverContext resolverContext,
            IRegistrator registrator, IContainer container)
        {
            ServiceProvider = serviceProvider;
            Resolver = resolver;
            ResolverContext = resolverContext;
            Registrator = registrator;
            Container = container;
        }
    }
} /*md
```

The above registrations look rather complex and leaky with all this casting in place.

There is much easy and error-prone to make these registrations always available, which DryIoc jus does for you. 


## Default constructor selection

By default DryIoc expects from the registered type to have a __single public constructor__. 

If no or multiple constructors available it will throw the corresponding exception. 
The reason for this is to be as deterministic as possible and to prevent the hard-to-find errors.

But the default behavior may be opt-out in the following ways:

1. You may specify predefined `FactoryMethod.ConstructorWithResolvableArguments` rule to be used for the registration or per Container. 
The rule will work if multiple constructors available, and will select the constructor with maximum number of parameters where each parameter is successfully resolved from container.

```cs md*/
[TestFixture] public class Constructor_with_resolvable_arguments
{
    [Test] public void Example()
    {
        // Enabling the rule for container
        var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments));
        container.Register<A>();
        container.RegisterInstance(new C());

        var a = container.Resolve<A>();
        Assert.IsNotNull(a.C);

        // Enabling the rule per registration
        container = new Container();
        container.Register<A>(made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments));
        container.Register<B>();

        a = container.Resolve<A>();
        Assert.IsNotNull(a.B);
    }

    public class A
    {
        public B B { get; }
        public A(B b)
        {
            B = b;
        }

        public C C { get; }
        public A(C c)
        {
            C = c;
        }
    }

    public class B { }
    public class C { }
}
/*md
```

2. You may specify to use a specific constructor, or even the static or the instance method, the field or the property for producing the service. 
The preferred way to do it is using the `Made.Of` expression specification:
```cs md*/
[TestFixture] class Using_specific_ctor_or_factory_method
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<A>(Made.Of(() => new A(Arg.Of<B>(), Arg.Of<C>("someKey"))));
        container.Register<B>();
        container.Register<C>(serviceKey: "someKey");

        var a = container.Resolve<A>();
    }

    public class A
    {
        public A(B b) { }
        public A(C c) { }
        public A(B b, C c) { }
    }

    public class B { }
    public class C { }
}
/*md
```

[More examples are here](SelectConstructorOrFactoryMethod).


## Unresolved parameters and properties

By default DryIoc will throw exception if constructor or method parameter is not resolved, 
__but will not throw in case of property or field, or in case of optional parameter__ 
(yes, DryIoc supports optional parameters with the default values just fine).

This is because "normally" the constructor parameters specify required dependencies. 
On the other hand, writable properties usually specify dependencies that may be not set when object is created, set by third-party, or not set at all.

As usual, you may override default behavior to throw an exception for unresolved property, or set the default value for unresolved parameter:
```cs md*/
[TestFixture]
class Specify_how_to_treat_unresolved_parameter_or_property
{
    [Test]public void Example()
    {
        var container = new Container();
        
        container.Register<A>(Made.Of(() =>
            new A(Arg.Of<B>(IfUnresolved.ReturnDefault),  // return default even if parameter is not optional with the default value
                  Arg.Of<C>(IfUnresolved.Throw))          // throw for the optional parameter
                  {
                      P = Arg.Of<P>(IfUnresolved.Throw)   // throw for the unresolved property instead of the default silence
                  }));

        Assert.Throws<ContainerException>(() => container.Resolve<A>());
    }

    public class A
    {
        public P P { get; set; }
        public A(B b, C c = null) { }
    }

    public class B { }
    public class C { }
    public class P { }
} /*md
```


## Rules per Container

### FactorySelector

__Note:__ In DryIoc _Factory_ is the unit of registration. Speaking of factory selection we are speaking of the registration selection.

Allows to override default factory selection, especially when we have multiple registered default factories.

DryIoc has two predefined rules that you can use instead of [default policy](RulesAndDefaultConventions#markdown-header-resolving-from-multiple-default-services):

- `Rules.SelectLastRegisteredFactory` - explained [here](RulesAndDefaultConventions#markdown-header-resolving-from-multiple-default-services).
- `Rules.SelectKeyedOverDefaultFactory(serviceKey)` - to prefer registration with the specific key over the default. 
    
For example you may register some dependencies to be available only inside opened scope like this:
```cs md*/
[TestFixture] public class Using_factory_selector_to_change_the_default_preferred_service
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<I, A>();
        container.Register<I, B>(serviceKey: "scoped");

        using (var scope = container
            .With(r => r.WithFactorySelector(Rules.SelectKeyedOverDefaultFactory("scoped")))
            .OpenScope())
        {
            // Resolve will return B instead of A despite the absence of "scoped" key in Resolve
            var x = scope.Resolve<I>();
            Assert.IsInstanceOf<B>(x);
        }
    }
}
/*md
```


### FactoryMethod, Parameters and Properties selector

This way you may specify how to select constructor, parameters and properties. 

For instance [MefAttributedModel](Extensions/MefAttributedModel) uses these rules to instruct DryIoc to select constructor and properties 
marked with `ImportingConstructor` and `Import` attributes.

In addition you can use the rule to [select constructor with all resolvable parameters](RulesAndDefaultConventions#markdown-header-default-constructor-selection).


### UnknownServiceResolvers

The rules is used as a last resort / fallback resolution strategy when no registration is found.

You may use this rule to implement on-demand registrations, or automatic concrete types registrations, etc.


#### AutoFallbackDynamicRegistrations

DryIoc provides predefined rule `WithDynamicRegistrations` and `WithDynamicRegistrationsAsFallback` to register additional services 
from the provided list of types or assemblies:
```cs md*/
[TestFixture] public class Auto_register_unknown_service
{
    [Test] public void Example()
    {
        var implTypes = new []{ typeof(A), typeof(B) };

        var container = new Container(rules =>
            rules.WithDynamicRegistrationsAsFallback(
                Rules.AutoFallbackDynamicRegistrations((_, __) => implTypes)));

        var b = container.Resolve<B>();
        Assert.IsNotNull(b.A);
    }

    public class A { }
    public class B
    {
        public A A { get; }
        public B(A a) { A = a; }
    }
}
/*md
```


#### WithConcreteTypeDynamicRegistrations

The rule to "automatically" resolve concrete (non-interface, non-abstract) types without registering them in container.

Using the rule:
```cs md*/
[TestFixture]
public class Auto_concrete_dynamic_type_registrations
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules
            .WithConcreteTypeDynamicRegistrations((serviceType, serviceKey) => true, Reuse.Singleton));

        container.Register<ICar, FastCar>();
        // but no Driver registration!

        // Driver is created by container
        var car = container.Resolve<ICar>();
        Assert.IsNotNull(car.Driver);
    }

    class Driver { }

    interface ICar
    {
        Driver Driver { get; }
    }

    class FastCar : ICar
    {
        public Driver Driver { get; }
        public FastCar(Driver driver) { Driver = driver; }
    }
}
/*md
 ```


### Fallback Containers

[Explained in "Child" containers](KindsOfChildContainer#markdown-header-facade).


### ThrowIfDependencyHasShorterReuseLifespan

__This rule is enabled by default__ and instructs container to throw exception when injecting dependency with shorter lifespan 
than dependency holder. 

What does it mean?

- In DryIoc services with `Reuse.Singleton` have a longest lifespan equal to lifespan of container itself.
- Then services with `Reuse.Scoped` and siblings which live no longer than singletons.
- Transient services do not have a lifespan, so the rule is not applied for them.

From implementation point of view `Lifespan` is the property defined in `IReuse` interface, 
and the respective implementations define relative lifespan values for the property:

- `SingletonReuse.Lifespan`    is `1000`
- `CurrentScopeReuse.Lifespan` is `100`
- `TransientReuse.Lifespan`    is `0`

When defining your own Reuse you may take advantage of the rule by defining specific Lifespan number.

Example:
```cs md*/
[TestFixture] public class Throw_if_dependency_has_a_shorter_lifetime
{
    [Test] public void Example()
    {
        var container = new Container(); // enabled by default

        container.Register<A>(Reuse.Singleton);
        container.Register<B>(Reuse.InCurrentScope);

        using (var scope = container.OpenScope())
        {
            var ex = Assert.Throws<ContainerException>(() => scope.Resolve<A>());

            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.ErrorName);
        }
    }

    class A { public A(B b) { } }
    class B { }
}
/*md
```

You may disable the rule for Container, so lifespan mismatch will not throw exception.
```cs md*/
[TestFixture] public class Disable_captive_dependency_exception
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules
            .WithoutThrowIfDependencyHasShorterReuseLifespan());

        container.Register<A>(Reuse.Singleton);
        container.Register<B>(Reuse.InCurrentScope);

        A a; 
        using (var scope1 = container.OpenScope())
            a = scope1.Resolve<A>(); // OK

        using (var scope2 = container.OpenScope())
        {
            a = scope2.Resolve<A>(); // OK, 

            // But `a` still holds `B` from the `scope1` that's why it is called a Captive Dependency
            Assert.AreNotSame(a.B, scope2.Resolve<B>());
        }
    }

    class A
    {
        public B B { get; }
        public A(B b) { B = b; }
    }
    class B { }
}

/*md
```

__Note:__ Another way to skip the captive dependency check is to wrap dependency in `Func`. 
Using `Func` means that client is in charge of creating the dependency whenever its needed, 
so the check does not make sense.

```cs md*/
[TestFixture] public class Wrap_captive_dependency_in_Func
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<A>(Reuse.Singleton);
        container.Register<B>(Reuse.InCurrentScope);

        container.Resolve<A>(); // works, an `A` will decide when to create `B`.
    }

    public class B { }
    public class A { public A(Func<B> getB) { } }
}
/*md
```


### ThrowOnRegisteringDisposableTransient

DryIoc does not track disposable transients by default as described [here](ReuseAndScopes#markdown-header-disposable-transient).

That means you may register Transient `IDisposable` and forgot to dispose it, thinking that Container will do this for you.

To prevent the possible memory leaks due the ignored `Dispose`, DryIoc by default will throw the exception on registering 
transient implementing `IDisposable` interface:
```cs md*/
[TestFixture] public class Register_disposable_transient
{
    [Test] public void Example()
    {
        var container = new Container();

        var ex = Assert.Throws<ContainerException>(() => container.Register<MyDisposableService>()); // Throws exception!
        Assert.AreSame(Error.NameOf(Error.RegisteredDisposableTransientWontBeDisposedByContainer), ex.ErrorName);

        container.Register<MyDisposableService>(Reuse.Scoped); // OK
    }

    public class MyDisposableService : IDisposable
    {
        public void Dispose() { }
    }
}
/*md
```

If you disagree, you may silence this exception per registration or per Container:
```cs
md*/
[TestFixture]public class Silence_registering_disposable_transient_exception
{
    [Test]public void Example()
    {
        var container = new Container();

        // silence per registration:
        container.Register<MyDisposableService>(setup: Setup.With(allowDisposableTransient: true)); // OK

        // silence per container:
        container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());
        container.Register<MyDisposableService>(); // OK
    }

    public class MyDisposableService : IDisposable
    {
        public void Dispose() { }
    }
}
/*md
```

### WithTrackingDisposableTransient

In detail is described [here](ReuseAndScopes#markdown-header-disposabletransient).


### WithDefaultReuseInsteadOfTransient

Allows to specify different default Reuse per Container as described [here in Reuse and Scopes](ReuseAndScopes#markdown-header-different-default-reuse-instead-of-transient).


### WithDefaultIfAlreadyRegistered

Allows to specify registration option per Container which is different from default `IfAlreadyRegistered.AppendNonKeyed`. 

For instance I want my container to follow the "Register Once" registration semantics:
```cs md*/
[TestFixture] public class Default_IfAlreadyRegistered
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules
            .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Keep));

        container.Register<I, A>();
        container.Register<I, B>();

        var i = container.Resolve<I>();

        Assert.IsInstanceOf<A>(i); // the first registration will be kept, and the second one is ignored
    }
}
/*md
```

Another interesting use is to make a "Collection registration explicit". 
Following the previous example I can explicitly specify `IfAlreadyRegistered.AppendNonKeyed` for individual registrations 
to be added to collection:
```cs md*/
[TestFixture]
public class Default_IfAlreadyRegistered_AppendNotKeyed
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules
            .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Keep));

        container.Register<I, A>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
        container.Register<I, B>(ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

        var ii = container.Resolve<IEnumerable<I>>();
        Assert.AreEqual(2, ii.Count());
    }
}
/*md
```


### WithoutImplicitCheckForReuseMatchingScope 

This rule turns Off the default [Implicit registration selection based on scope](RulesAndDefaultConventions#markdown-header-implicit-registration-selection-based-on-scope).


### ResolveIEnumerableAsLazyEnumerable

[Explained in Wrappers](Wrappers#markdown-header-lazyenumerable-of-a).


### VariantGenericTypesInResolvedCollection

[Explained in Wrappers](Wrappers#markdown-header-contravariant-generics).

md*/
