<!--Auto-generated from .cs file, the edits here will be lost! -->

[recursive dependency]: ErrorDetectionAndResolution.md#recursivedependencydetected

# Wrappers


- [Wrappers](#wrappers)
  - [Overview](#overview)
  - [Predefined wrappers](#predefined-wrappers)
    - [Lazy of A](#lazy-of-a)
    - [Func of A](#func-of-a)
    - [Really "lazy" Lazy and Func](#really-lazy-lazy-and-func)
    - [Func of A with parameters](#func-of-a-with-parameters)
      - [Func with single argument to resolve service by key](#func-with-single-argument-to-resolve-service-by-key)
    - [KeyValuePair of Service Key and A](#keyvaluepair-of-service-key-and-a)
    - [Meta or Tuple of A with Metadata](#meta-or-tuple-of-a-with-metadata)
      - [Dictionary Metadata](#dictionary-metadata)
    - [IEnumerable or array of A](#ienumerable-or-array-of-a)
      - [Custom order of items in the collection wrapper](#custom-order-of-items-in-the-collection-wrapper)
      - [Open-generics](#open-generics)
      - [Co-variant generics](#co-variant-generics)
      - [Composite Pattern support](#composite-pattern-support)
    - [LazyEnumerable of A](#lazyenumerable-of-a)
    - [IDictionary of services A and their keys](#idictionary-of-services-a-and-their-keys)
    - [LambdaExpression](#lambdaexpression)
      - [DryIoc is not a magic](#dryioc-is-not-a-magic)
  - [Nested wrappers](#nested-wrappers)
  - [User-defined wrappers](#user-defined-wrappers)


## Overview

Wrapper in DryIoc is some useful data structure which operates on registered service or services. 

- A wrapper may be either an open-generic type with generic argument identifying service type to wrap. Examples are `Func<TService>`, `Lazy<TService>`, `IEnumerable<TService>`. 
- Or non-generic type, in that case a wrapped service is identified with [RequiredServiceType](RequiredServiceType): e.g. `LambdaExpression`.

__Note:__ Open-generic wrapper with multiple type arguments may wrap one service type only, e.g. `Func<TArg0, TArg1, TService>` wraps `TService`. 
Wrapping multiple services in one wrapper is not supported.

More info:

- Wrappers are composable through nesting: e.g. `IEnumerable<Lazy<TService>>`.
- A similar concept in Autofac is [relationship types](http://docs.autofac.org/en/latest/resolve/relationships.html).
- DryIoc supports both predefined and [user-defined wrappers](Wrappers.md#user-defined-wrappers).
- Explicitly registering a wrapper type (e.g. `Func<>` or `Lazy<>`) as a normal service overrides corresponding wrapper registration.
- The actual difference between normal open-generic service and wrapper is [explained here](Wrappers.md#user-defined-wrappers).

Wrapper example:
```cs 
namespace DryIoc.Docs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedVariable

public class Wrapper_example
{
    [Test] public void Example()
    {
        var container = new Container();

        // The actual service registrations
        container.Register<A>();
        container.Register<B>();

        // Lazy is available without registration!
        container.Resolve<B>();
    }

    class A { }
    class B
    {
        public B(Lazy<A> a) { }
    }
}
```


## Predefined wrappers

Predefined wrappers are always available, you don't need to `Register` them, but may `Unregister` then if you want.

Predefined wrapper types were selected to be DryIoc agnostic, to keep your business logic as _POCO_ as possible without depending on specific IoC library. It means all of the wrapper types except `DryIoc.Meta<,>` are available without DryIoc. 

__Note:__ `DryIoc.Meta<,>` type may just help to migrate from Autofac but you may use `System.Tuple<,>` instead.


### Lazy of A

- Provides an instance of `A` created on first access.
- Internally implemented as a call to a container Resolve: `r => new Lazy(() => r.Resolve<A>())`, therefore `A` may not be yet available when `Lazy` is injected.
- Permits [recursive dependency] in resolution graph, because uses the `Resolve` call for service creation postponing its dependency check.


### Func of A

- Delegates creation of `A` to the user code.
- By default injected as inline service creation: `new B(() => new A())`. Therefore `A` should be available at the moment of `Func<A>` resolution.
- By default does not permit [recursive dependency].
- Alternatively, inline `A` creation may be changed to a `Resolve` call:

    `container.Register<A>(setup: Setup.With(asResolutionCall: true));`

    - This way `Func<A>` will be injected as: `new B(() => r.Resolver.Resolve<A>())`
    - Permits [recursive dependency] in resolution graph.

### Really "lazy" Lazy and Func

The important thing about `Lazy` and `Func` is that the wrapped dependency should be registered into the container when injecting wrappers.
```cs 
public class Lazy_and_Func_require_services_to_be_registered
{
    [Test] public void Example()
    {
        var container = new Container();

        // Throws cause `A` is not registered
        Assert.Throws<ContainerException>(() =>
            container.Resolve<Lazy<A>>());
    }

    class A { }
} 
```

This was done intentionally to be able to construct as much of the object graph as possible to verify its correctness.
The default DryIoc laziness of `Func` and `Lazy` is about postponing a creation of service value and not about 
postponing a registration.

There is still a possibility to postpone a service registration:

- One is via [`RegisterPlaceholder`](RegisterResolve#registerplaceholder).
- Another one is globally, via `Rules.WithFuncAndLazyWithoutRegistration()`, see below:

```cs 
public class Func_works_without_registration
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());
        var getA = container.Resolve<Func<A>>();

        // later register an `A`
        container.Register<A>();
        var a = getA();
        Assert.IsNotNull(a);
    }

    class A { }
} 
```

**Note:** Using `WithFuncAndLazyWithoutRegistration` will also permit the [recursive dependency].


### Func of A with parameters

Delegates creation of service to the user and allows supplying a subset of dependencies. The rest of dependencies are injected by the container as usual. 

More details:

- It may be viewed as an override of normally injected dependencies with custom ones, provided via arguments. 
- Or as "currying" of service creation constructor or method.
- Sometimes Func with parameters is used to pass primitive values, e.g. `string`, `int`, etc.
- Injected as an inline creation expression: `new B((dep1, dep2) => new A(dep1, new D(), dep2))`
- Permits the [recursive dependency].


Provided arguments are matched with parameters by type and in passed order. 
If the matched argument is not found then the argument will be injected by the container. 
The arguments order may differ from parameters order.

What if provided arguments are of the same type? DryIoc will track already used arguments and will skip them in the subsequent match. 
Means that `dep1` and `dep2` from the example may be a string and still both used.

__Note:__ In case when the passed argument is not used then DryIoc will proceed to match it for nested dependencies (if any):
```cs
new B(arg => new A(new D(arg)))
```

When the passed argument was not used it is maybe a mistake but maybe is not. It was a hard choice, but DryIoc will ignore this for now.
```cs 
public class Passed_argument_was_not_used
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<A>();

        // Can be used, the argument will be ignored
        var getA = container.Resolve<Func<string, A>>();

        Assert.IsNotNull(getA("ignore me"));
    }

    class A { }
}
```

There is another caveat related to reuse. You may say that passing a different argument to create a service likely may produce a different service.
But what happens when service is registered with non-transient reuse? 
By default, DryIoc will use the first call to `Func` to create a service and ignore the rest. 
```cs 
public class Func_with_args_and_reuse
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<A>(Reuse.Singleton);

        // Can be used, the argument will be ignored
        var getA = container.Resolve<Func<string, A>>();

        Assert.AreEqual("Hi, Alpha", getA("Hi, Alpha").Greeting);

        // The result still be the same as before, so the second invocation and its arguments are ignored
        Assert.AreEqual("Hi, Alpha", getA("Hi, Beta").Greeting);
    }

    class A
    {
        public string Greeting { get; }
        public A(string greeting) { Greeting = greeting; }
    }
}
```

If you don't like it this way, you may use a rule `Rules.WithIgnoringReuseForFuncWithArgs()`
```cs 
public class Func_with_args_with_rule_ignoring_reuse
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules.WithIgnoringReuseForFuncWithArgs());
        container.Register<A>(Reuse.Singleton);

        // Can be used, the argument will be ignored
        var getA = container.Resolve<Func<string, A>>();

        Assert.AreEqual("Hi, Alpha", getA("Hi, Alpha").Greeting);

        // That's expected now.
        Assert.AreEqual("Hi, Beta", getA("Hi, Beta").Greeting);
    }

    class A
    {
        public string Greeting { get; }
        public A(string greeting) { Greeting = greeting; }
    }
}
```

#### Func with single argument to resolve service by key

In some IoC libraries `Func<string, IService>` has a special meaning 
where string argument represents a service key to access `IService` by key. 

This is not true in DryIoc. 
First, because it more general concept of fulfilling the dependency though the passed argument,
and no one prevent the dependency to be type of `string`.
Second, the service keys in DryIoc may be of arbitrary types not restricted to strings, e.g. `int`, `Guid`, enums, etc.

But DryIoc is powerful enough to emulate this feature without much hassle:

```cs 
public class Func_with_single_argument_to_resolve_service_by_key
{
    [Test] 
    public void Example()
    {
        var container = new Container();

        container.Register<IService, ServiceA>(serviceKey: "A");
        container.Register<IService, ServiceB>(serviceKey: "B");
        container.Register<Consumer>();

        // The magic is not: 
        // DryIoc will inject the IDictionary wrapper of services by key and a string key passed as function argument
        container.RegisterDelegate<IDictionary<string, IService>, string, IService>((services, key) => services[key]);

        var getConsumer = container.Resolve<Func<string, Consumer>>();

        Assert.IsInstanceOf<ServiceA>(getConsumer("A").Service);

        Assert.IsInstanceOf<ServiceB>(getConsumer("B").Service);
    }

    interface IService {}
    class ServiceA : IService {}
    class ServiceB : IService {}
    class Consumer 
    {
        public readonly IService Service;
        public Consumer(IService service) => Service = service;
    }
}
```



### KeyValuePair of Service Key and A

Wraps a registered service key together with the resolved service. 
May be used to filter service based on its key. Usually used together with Func or Lazy wrapper and nested inside collection: `IEnumerable<KeyValuePair<string, Func<A>>>`.

More on the `KeyValuePair` usage is [here](RegisterResolve#resolving-as-keyvaluepair-wrapper).


### Meta or Tuple of A with Metadata

Packs together resolved service A and associated metadata object. A bit similar to `KeyValuePair` but with metadata instead of the key. 
You may think of the metadata the same way as of `Attribute` defined for the implementation type. 
Metadata may be provided with attributes when using [MefAttributedModel](Extensions/MefAttributedModel). 
```cs 
public class Providing_metadata
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<A>(setup: Setup.With(metadataOrFuncOfMetadata: "XYZ"));

        var a1 = container.Resolve<Meta<A, string>>();
        var a2 = container.Resolve<Tuple<A, string>>(); // is the same thing

        Assert.AreEqual("XYZ", a1.Metadata);
        Assert.AreEqual("XYZ", a2.Item2);
    }
} 
```

__Note:__ You may choose to use `System.Tuple<,>` over `DryIoc.Meta<,>` because former does not require the DryIoc reference. 

`Meta<,>` may be a good choice if you are migrating from [Autofac](http://docs.autofac.org/en/latest/resolve/relationships.html#metadata-interrogation-meta-b-meta-b-x), 
or if you want to register `Tuple<,>` with the different meaning.

If no metadata provided in `setup`, then the resolving as `Meta` will throw a `ContainerException` with corresponding message and code.
In the example above registered and resolved metadata types are the same. DryIoc will throw an exception in case they are not assignable, 
e.g. when registered as `string` but resolving with `int` metadata type. Resolving as `Meta<A, object>` is always fine because a string is an object.

Metadata may be also used for collection filtering. When nesting in collection wrapper, and when the metadata is not registered or not assignable, then no exception will be thrown. Instead, a service item will be filtered out from the collection (and collection may be empty as the result):
```cs 
public class Filtering_based_on_metadata
{
    [Test] public void Example()
    {
        var container = new Container();
        var items = container.Resolve<Meta<A, int>[]>();
        Assert.IsEmpty(items);
    }
}
```

__Note:__ Filtering works the same way for `KeyValuePair<TKey, TService>`

Wrappers may be combined further with `Func` or `Lazy`, or even with `KeyValuePair` to provide a filtered collection of lazy services:
```cs
container.Resolve<Meta<string, Func<int, A>>[]>();
```

#### Dictionary Metadata

Metadata value can be a non-primitive object or an `IDictionary<string, object>`. 
In latter case you may `Resolve` by providing the type of any `object` value in a dictionary:

```cs 
public class Resolve_value_out_of_metadata_dictionary
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<A>(setup: Setup.With(
            new Dictionary<string, object>
            {
                { "color", "red" },
                { "quantity", 15 }
            }));

        container.Register<A, B>(setup: Setup.With(
            new Dictionary<string, object>
            {
                { "color", "red" },
                { "special", true }
            }));

        // we have `A` and `B`
        var items = container.Resolve<Meta<A, IDictionary<string, object>>[]>();
        Assert.AreEqual(2, items.Length);

        // here we have only `A` cause its metadata contains an `int` metadata value
        var itemQuantities = container.Resolve<Meta<A, int>[]>();
        Assert.AreEqual(1, itemQuantities.Length);
        Assert.AreEqual(15, itemQuantities[0].Metadata);
    }

    class A { }
    class B : A { }
}
```


### IEnumerable or array of A

Injects a collection of registered service values to the consumer. 
The collection basically a fully initialized "fixed" array:
```cs
var items = new B(new A[] { new Impl1(), new Impl2(), new Impl3() });
```

The result array expression is generated __only once__ using all found services and will not change if new service is added afterward, or the existing one is updated or removed. 
You may use `LazyEnumerable<T>` explained below to address this limitation.

__Note:__ Another way to use a `LazyEnumerable<T>` when resolving `IEnumerable<T>` is to specify it globally via `Rules.WithResolveIEnumerableAsLazyEnumerable`.

There is an alternate way to `Resolve` rather than inject a collection: 
via `IResolver.ResolveMany` method. `ResolveMany` may provide results as a fixed array or as a 
`LazyEnumerable` if you pass optional `ResolveManyBehavior.AsLazyEnumerable` argument, the **default value is the `ResolveManyBehavior.AsFixedArray`**.

Due to the fact that .NET `Array` implements a number of collection interfaces: 
`IEnumerable<>`, `IList<>`, `ICollection<>`, `IReadOnlyList<>`, `IReadOnlyCollection<>`, 
DryIoc allows injecting these interfaces. So you may consider them as the same collection wrapper.

Resolved services are ordered in registration order for default services, 
and without the specific order for the keyed services. 
Despite being ordered, the default services may be mixed up with keyed services.

The fixed array nature implies that all services will be created when the wrapper is resolved or injected. 
But maybe you need filter only some services (or inspect service count) and do not want to create the rest. 
Nested `Func` or `Lazy` will help in this case:
```cs 
public class Collection_of_Lazy_things
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<A>();

        var items = container.Resolve<IReadOnlyList<Func<A>>>();
        if (items.Count == 1)
        {
            // Creating an `A` on demand 
            var a = items[0].Invoke();
        }
    }

    class A { }
}
```

DryIoc supports filtering of result services when using nested `KeyValuePair` or `Meta` wrappers. 
Basically when the nested wrapper is not resolved because of not compatible Key or Metadata type, 
then the item will be excluded from the array. 

When service is not resolvable due to the missing (not registered) dependency, 
the collection wrapper will throw an exception instead of skipping the item. 
In all other cases, the item will be filtered out of the collection.

```cs 
public class Filtering_not_resolved_services
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<B>();

        // `A` is not registered
        //container.Register<A>();

        Assert.Throws<ContainerException>(() => container.Resolve<B[]>());
    }

    class A { }
    class B
    {
        public B(A a) { }
    }
} 
```

#### Custom order of items in the collection wrapper

To achieve the custom ordering we may use the [Meta or Tuple with Metadata](#meta-or-tuple-of-a-with-metadata) wrapper and the [Decorator](Decorators.md) feature.

```cs 
public class Collection_with_custom_order
{
    public sealed class OrderMetadata // may be an suitable data structure defined by you
    { 
        public readonly int Value;
        public OrderMetadata(int value) => Value = value;
    }

    // The "decorator" method sorting the `I` collection based on the `OrderMetadata` before returning it.
    static IEnumerable<I> SortByOrderMetadata(IEnumerable<Tuple<I, OrderMetadata>> ii) =>
        ii.OrderBy(x => x.Item2.Value).Select(x => x.Item1);

    [Test] public void Example()
    {
        var container = new Container();
        container.Register<I, C>(setup: Setup.With(new OrderMetadata(3)));
        container.Register<I, A>(setup: Setup.With(new OrderMetadata(1)));
        container.Register<I, B>(setup: Setup.With(new OrderMetadata(2)));

        container.RegisterDelegate<IEnumerable<Tuple<I, OrderMetadata>>, IEnumerable<I>>(SortByOrderMetadata, setup: Setup.Decorator);

        var items = container.Resolve<I[]>();

        Assert.IsInstanceOf<A>(items[0]);
        Assert.IsInstanceOf<B>(items[1]);
        Assert.IsInstanceOf<C>(items[2]);
    }
    interface I {}
    class A : I { }
    class B : I { }
    class C : I { }
}
```


#### Open-generics

If you registered both closed and open-generic implementation of the service, 
then both types will be present in the result collection 
(_it may be obvious but I wanted to be clear on this_):

```cs 
public class Both_open_and_closed_generic_included_in_collection
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IFoo<int>, IntFoo>();
        container.Register(typeof(IFoo<>), typeof(Foo<>));

        var items = container.Resolve<IFoo<int>[]>();
        Assert.AreEqual(2, items.Length); // includes both `IntFoo` and `Foo<>`
    }

    interface IFoo<T> { }
    class IntFoo : IFoo<int> { }
    class Foo<T> : IFoo<T> { }
} 
```


#### Co-variant generics

DryIoc by default will include [co-variant](https://msdn.microsoft.com/en-us/library/dd799517(v=vs.110).aspx) 
generic services into resolved collection. 

What does it mean? In C# you may declare a co-variant open-generic interface with `out` modifier: `IHandler<out T>`.
Then if you have a class `B` assignable to `A` (`class B : A {}`), then their handlers are assignable as well 
`IHandler<B> b = null; IHandler<A> a = b;`.

Proceeding to the collections you may have `IEnumerable<IHandler<A>>` with the elements of type `IHandler<A>` and `IHandler<B>`.

```cs 
public class Covariant_generics_collection
{
    [Test] public void Example()
    {
        var container = new Container();

        // Using batch registration for convenience
        container.RegisterMany(
            implTypes: new[] { typeof(MoveHandler), typeof(MoveAbroadHandler) },
            nonPublicServiceTypes: true);

        // Collection will include `IHandler<MoveAbroadEvent>` as well as `IHandler<MoveEvent>`
        var moveHandlers = container.Resolve<IEnumerable<IHandler<MoveEvent>>>();
        Assert.AreEqual(2, moveHandlers.Count());

        // Now we narrow the collection type to `IHandler<MoveAbroadEvent>`, so it will include only one item now
        var moveAbroadHandlers = container.Resolve<IEnumerable<IHandler<MoveAbroadEvent>>>();
        Assert.AreEqual(1, moveAbroadHandlers.Count());
    }

    interface IHandler<out TEvent> { } // contra-variant interface
    class MoveEvent { }
    class MoveHandler : IHandler<MoveEvent> { }
    class MoveAbroadEvent : MoveEvent { }
    class MoveAbroadHandler : IHandler<MoveAbroadEvent> { }
} 
```

If you don't want the co-variant types to be included into collection, you may disable this behavior
via `Rules.WithoutVariantGenericTypesInResolvedCollection()`:

```cs 
public class Covariant_generics_collection_suppressed
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules.WithoutVariantGenericTypesInResolvedCollection());

        // Using batch registration for convenience
        container.RegisterMany(
            implTypes: new[] { typeof(MoveHandler), typeof(MoveAbroadHandler) },
            nonPublicServiceTypes: true);

        // Now it as only one handler
        var moveHandlers = container.Resolve<IEnumerable<IHandler<MoveEvent>>>();
        Assert.AreEqual(1, moveHandlers.Count());
    }

    interface IHandler<out TEvent> { } // contra-variant interface
    class MoveEvent { }
    class MoveHandler : IHandler<MoveEvent> { }
    class MoveAbroadEvent : MoveEvent { }
    class MoveAbroadHandler : IHandler<MoveAbroadEvent> { }
} 
```


#### Composite Pattern support

Collection wrapper by default supports a [Composite pattern](http://en.wikipedia.org/wiki/Composite_pattern). 
Composite pattern is observed when service implementation depends on the collection of other implementations of the same service. 
For example:
```cs 

class Composite_example
{
    class A1 : A {}
    class A2 : A {}
    class Composite : A { public Composite(A[] items) { } }
} 
```

In this example we expect that `items` in Composite will be composed of `A1` and `A2` objects, 
but not of composite itself despite the fact Composite is also an `A`. 
Including composite into `items` will produce infinite recursion in the construction of object graph. 
Therefore, a Composite pattern may be looked like a way to avoid such recursion by excluding composite from its dependencies. 
DryIoc does exactly that:

```cs 
public class DryIoc_composite_pattern
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<A, Composite>();
        container.Register<A, A1>();
        container.Register<A, A2>();

        // Contains all three items including the `Composite`
        var items = container.Resolve<A[]>();
        Assert.AreEqual(3, items.Length);

        // Composite contains `A1` and `A2` instances
        var composite = items.OfType<Composite>().First();
        Assert.AreEqual(2, composite.Items.Length);
    }

    class A1 : A { }
    class A2 : A { }
    class Composite : A
    {
        public A[] Items { get; }
        public Composite(A[] items) { Items = items; }
    }
}
```

__Note:__ If the Composite pattern support was missing in DryIoc you would be getting a "recursive dependency detected" `ContainerException`.

Whether with composite or not, when you try to do `container.Resolve<A>()` the exception will be thrown, 
because multiple implementation are registered. But sometimes you may prefer to resolve a composite if it is registered.

```cs 
public class Prefer_composite_when_resolving_a_single_service
{
    [Test] public void Example()
    {
        var container = new Container();

        // Here is the setup option needed
        container.Register<A, Composite>(setup: Setup.With(preferInSingleServiceResolve: true));
        container.Register<A, A1>();
        container.Register<A, A2>();

        // Contains all three items including the `Composite`
        var item = container.Resolve<A>();
        Assert.IsInstanceOf<Composite>(item);
    }

    class A1 : A { }
    class A2 : A { }
    class Composite : A
    {
        public A[] Items { get; }
        public Composite(A[] items) { Items = items; }
    }
}
```


### LazyEnumerable of A

`LazyEnumerable<T>` is an implementation of `IEnumerable<T>`interface. It is different from `IEnumerable<T>` array wrapper in a way, 
that it wraps a `ResolveMany` call instead of an initialized array expression. 
When enumerating the collection, it will call `IResolver.ResolveMany` and will return up-to-date services registered in the container. 
By comparison, an array wrapper always returns a fixed set of services ignoring whatever was added or removed after resolve.

```cs 
public class LazyEnumerable_example
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<A, A1>();

        // At that point, not even `A1` is resolved
        var items = container.Resolve<LazyEnumerable<A>>();

        // `A1` is resolved
        var materializedItems = items.ToArray();
        Assert.IsInstanceOf<A1>(materializedItems.Single());

        // Adding a new service
        container.Register<A, A2>();
        items = container.Resolve<LazyEnumerable<A>>();
        materializedItems = items.ToArray();

        // Now result collection contains `A1` and `A2`
        Assert.AreEqual(2, materializedItems.Length);
    }

    class A { } 
    class A1 : A { } 
    class A2 : A { } 
}
```

Because `LazyEnumerable<>` implements `IEnumerable<>` interface, you can inject the interface by specifying a `LazyEnumerable<>` 
as the `requiredServiceType`. It allows you to override default array injection per dependency:

```cs 

public class Specify_LazyEnumerable_per_dependency
{
    [Test] public void Example()
    {
        var container = new Container();
        
        // using `Made.Of` expression to specify how to construct a type value
        container.Register(Made.Of(() => new Handler(Arg.Of<LazyEnumerable<A>>())));
    }

    public class Handler { public Handler(IEnumerable<A> items) { } }
} 
```

__Note:__ The important thing in dependency specification above, is that your code remains POCO depending on `IEnumerable<>` abstraction and separated from 
the implementation provided at registration time. It also keeps code container-agnostic without adding DryIoc namespace into the business logic.

Another option is to specify `LazyEnumerable<>` implementation for `IEnumerable<>` globally via rules:

```cs 
public class Specify_to_use_LazyEnumerable_for_all_IEnumerable
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules.WithResolveIEnumerableAsLazyEnumerable());
    }
} 
```


### IDictionary of services A and their keys

The `IDictionary` wrapper provides a convenient way to combine services and their service keys in one collection, 
and allows access to service by key.
Basically it is a wrapper around array of pairs `KeyValuePair<TServiceKey, A>[]` turned into a dictionary.

The `TKey` type in dictionary will filter only keys matched this type only, e.g. for services registered with `string` key
and `int` keys, resolving or injecting `IDictionary<string, A>` will return dictionary with `string` keys only.

You need to specify `object` as key to return all services including the ones without keys.

Note: For service without key you will get the `DefaultKey` value for its key.

```cs 
public class Dictionary_of_services_with_their_keys
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<I, A>(serviceKey: "A");
        container.Register<I, B>(); // no key
        container.Register<I, C>(serviceKey: "C");
        container.Register<I, D>(serviceKey: 42);
        container.Register<Consumer>();

        // inject 2 services with string keys into consumer constructor
        var c = container.Resolve<Consumer>();
        Assert.AreEqual(2, c.Services.Count);
        Assert.IsInstanceOf<A>(c.Services["A"]);
        Assert.IsInstanceOf<C>(c.Services["C"]);

        // resolve single int service as dictionary
        var i = container.Resolve<IDictionary<int, I>>();
        Assert.AreEqual(1, i.Count);
        Assert.IsInstanceOf<D>(i[42]);

        // resolve all services as dictionary with object key, including the B without key
        var all = container.Resolve<IDictionary<object, I>>();
        Assert.AreEqual(4, all.Count);
        Assert.IsInstanceOf<B>(all[DefaultKey.Value]);
    }

    interface I { }
    class A : I { }
    class B : I { }
    class C : I { }
    class D : I { }
    class Consumer 
    {
        public readonly IDictionary<string, I> Services;
        public Consumer(IDictionary<string, I> services) => Services = services;
    }
} 
```

If you want to get services lazily, wrap the value either in `Func` or `Lazy`, e.g. `IDictionary<string, Lazy<I>>`.


### LambdaExpression

Allows getting an actual [ExpressionTree](https://msdn.microsoft.com/en-us/library/bb397951.aspx) composed by container to resolve a service. 

- It may be used either for diagnostics, to check if container creates the service in an expected way.
- In code generation scenarios, by converting the expression to C# code with something like [ExpressionToCode](https://github.com/EamonNerbonne/ExpressionToCode) library. 
It may be done even at compile-time.
- To understand how DryIoc works internally.

```cs 
public class Resolve_expression
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IService, Service>();

        var expr = container.Resolve<LambdaExpression>(typeof(IService));

        StringAssert.Contains("r => new Service()", expr.ToString());

        // The result expression is of type `Expression<Func<IResolverContext, object>>`, like this:
        Expression<Func<IResolverContext, object>> f = (IResolverContext r) => new Service();
    }

    interface IService { }
    class Service : IService { }
}
```

The actual type of returned `LambdaExpression` is `Expression<DryIoc.FactoryDelegate>` which has a signature as in the example.

__Note:__ Resolving as `LambdaExpression` does not create an actual service.


#### DryIoc is not a magic

Examining the resolved `LambdaExpression` you won't see any magic -

DryIoc just automates the generation of the object graph taking lifetime into consideration.

Given that you now have the resolved expression you may even `Compile` (and cache) it yourself :-) doing the last "magical" part.

This opens the additional possibilities:

For instance, below is the example which is "normally" won't work without shared `scopeContext`. 
Specifically we have a singleton holding onto the `Func` of scoped service. Now we are getting the expression out 
of container, compiling it and providing it with the scope (or any other container we want) 
as a `IResolverContext` argument to the compiled factory.

```cs 
public class Swap_container_in_factory_delegate
{
    [Test] public void Example()
    {
        var container = new Container(rules => rules.WithExpressionGenerationSettingsOnly());

        container.Register<A>(Reuse.Singleton);
        container.Register<B>(Reuse.Scoped);

        var expr = container.Resolve<LambdaExpression>(typeof(A));
        var factory = (Func<IResolverContext, object>)expr.Compile();

        using (var scope = container.OpenScope())
        {
            var a1 = (A)factory(scope);
            Assert.AreSame(a1.GetB(), a1.GetB());
        }
    }

    class A
    {
        public Func<B> GetB { get; }
        public A(Func<B> getB) { GetB = getB; }
    }
}
```


## Nested wrappers

Wrappers may be nested to provide combined functionality. 
A common use-case is to resolve the collection of registered services as `Func` or `Lazy`, then filter and create what you need.

Examples: 

As collection of `Func`:
```cs
container.Resolve<IEnumerable<Func<IA>>>();
```

With registered keys:
```cs
container.Resolve<IEnumerable<KeyValuePair<object, Func<IA>>>>()
    .Where(x => Filter(x.Key))  // filter based on key
    .Select(x => x.Value());    // create IA by invoking Func
```

With typed metadata:
```cs
container.Resolve<IEnumerable<Meta<MyMetadata, Func<IA>>>>();
```

Other combinations:
```cs
container.Resolve<IEnumerable<KeyValuePair<object, Meta<Lazy<IA>, MyMetadata>>>>();
container.Resolve<Func<Arg1, Arg2, IA>[]>();
container.Resolve<Meta<Func<Arg1, Arg2, IA>, object>>();
// etc.
```

## User-defined wrappers

To register your own wrapper just specify setup parameter as `Setup.Wrapper` or `Setup.WrapperWith`:
```cs 
public class User_defined_wrappers
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<ICmd, X>();
        container.Register<ICmd, Y>();

        // Register a wrapper
        container.Register(typeof(MenuItem<>), setup: Setup.Wrapper);
        Assert.IsTrue(container.IsRegistered(typeof(MenuItem<>), factoryType: FactoryType.Wrapper));

        var items = container.Resolve<MenuItem<ICmd>[]>();
        Assert.AreEqual(2, items.Length);
    }

    public interface ICmd { }
    public class X : ICmd { }
    public class Y : ICmd { }

    // Here is the wrapper
    public class MenuItem<TCmd> where TCmd : ICmd { }
}
```

The main difference between wrapper and non-wrapper is how they are treated by `ResolveMany` and collection wrappers:

- When `MenuItem` registered normally, array will contain single item as a result, because of a single `MenuItem` registration.
- When `MenuItem` registered as a wrapper, it is treated in a special way: resolver will try to find all wrapped types first (`ICmd`), 
and then wrap each found type in a `MenuItem`.

If open-generic wrapper has more than one type argument (e.g. `Meta<A, Metadata>`) 
you need to specify wrapped argument index: `setup: Setup.WrapperWith(0)`.

You may register a non-generic wrapper. In this case when resolved, 
you should identify wrapped service with [Required Service Type](RequiredServiceType).

Example of the non-generic wrapper:
```cs 
public class Non_generic_wrapper
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IService, Foo>();
        container.Register<IService, Bar>();
        container.Register<MyWrapper>(setup: Setup.Wrapper);
        Assert.IsTrue(container.IsRegistered(typeof(MyWrapper), factoryType: FactoryType.Wrapper));

        var items = container.Resolve<MyWrapper[]>(requiredServiceType: typeof(IService));
        Assert.AreEqual(2, items.Length);
    }

    // The same should work with the closed-generic wrapper
    [Test] public void Example_with_closed_generic_wrapper()
    {
        var container = new Container();

        container.Register<IService, Foo>();
        container.Register<IService, Bar>();
        container.Register<MyWrapper<IService>>(setup: Setup.Wrapper);
        Assert.IsTrue(container.IsRegistered(typeof(MyWrapper<IService>), factoryType: FactoryType.Wrapper));

        var items = container.Resolve<MyWrapper<IService>[]>(); // you dont need the `requiredServiceType` here
        Assert.AreEqual(2, items.Length);
    }

    interface IService { }
    class Foo : IService { }
    class Bar : IService { }

    class MyWrapper { public MyWrapper(IService service) { } }

    class MyWrapper<T> { public MyWrapper(T service) { } }
}
```
