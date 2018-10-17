/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Wrappers

[recursive dependency]: ErrorDetectionAndResolution#markdown-header-recursivedependencydetected

[TOC]

Wrapper in DryIoc is some useful data structure which operates on registered service or services.. 

- Wrapper may be either an open-generic type with generic argument identifying service type to wrap. Examples are `Func<TService>`, `Lazy<TService>`, `IEnumerable<TService>`. 
- Or non-generic type, in that case a wrapped service is identified with [RequiredServiceType](RequiredServiceType): e.g. `LambdaExpression`.

__Note:__ Open-generic wrapper with multiple type arguments may wrap one service type only, e.g. `Func<TArg0, TArg1, TService>` wraps `TService`. 
Wrapping a multiple services in one wrapper is not supported.

More info:

- Wrappers are composable through nesting: e.g. `IEnumerable<Lazy<TService>>`.
- A similar concept in Autofac is [relationship types](http://docs.autofac.org/en/latest/resolve/relationships.html).
- DryIoc supports both predefined and [user-defined wrappers](Wrappers#markdown-header-user-defined-wrappers).
- Explicitly registering a wrapper type (e.g. `Func<>` or `Lazy<>`) as a normal service overrides corresponding wrapper registration.
- The actual difference between normal open-generic service and wrapper is [explained here](Wrappers#markdown-header-user-defined-wrappers).

Wrapper example:
```cs md*/
using System;
using System.Collections.Generic;
using DryIoc;
using NUnit.Framework;

class Wrapper_example 
{
    [Test]
    public void Example()
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
}/*md
```


## Predefined wrappers

Predefined wrappers are always available, you don't need to `Register` them, but may `Unregister` then if you want.

Predefined wrapper types were selected to be DryIoc agnostic, to keep your business logic as _POCO_ as possible without 
depending on specific IoC library. It means all of wrapper types except `DryIoc.Meta<,>` are available without DryIoc. 

__Note:__ `DryIoc.Meta<,>` type may just help to migrate from Autofac but you may use `System.Tuple<,>` instead.


### Lazy of A

- Provides instance of `A` created on first access.
- Internally implemented as a call to a container Resolve: `r => new Lazy(() => r.Resolve<A>())`, therefore `A` may not be yet available when `Lazy` is injected.
- Permits a [recursive dependency] in resolution graph, because uses a `Resolve` to postpone to getting an actual service value.


### Func of A

- Delegates creation of `A` to user code.
- By default, injected as inline service creation: `new B(() => new A())`. Therefore `A` should be available at the moment of `Func<A>` resolution.
- By default, does not permit [recursive dependency].
- Alternatively, inline `A` creation may be changed to a `Resolve` call:

    `container.Register<A>(setup: Setup.With(asResolutionCall: true));`

    - This way `Func<A>` will be injected as: `new B(() => r.Resolver.Resolve<A>())`    
    - Permits [recursive dependency] in resolution graph.

### Really "lazy" Lazy and Func

The important thing about `Lazy` and `Func` is that the wrapped dependency should be registered into container when injecting a wrappers.
```cs md*/
class Lazy_and_Func_require_services_to_be_registered
{
    [Test]
    public void Example()
    {
        var container = new Container();

        // Throws cause `A` is not registered
        Assert.Throws<ContainerException>(() => 
            container.Resolve<Lazy<A>>());
    }

    class A { }
} /*md
```

This was done intentionally to be able to construct as much of object graph as possible to verify it correctness.
The default DryIoc laziness of `Func` and `Lazy` is about postponing a creation of service value, and not about 
postponing a registration.

There is still a possibility to postpone a service registration:

- One is via [`RegisterPlaceholder`](RegisterResolve#markdown-header-registerplaceholder).
- Another one is globally, via `Rules.WithFuncAndLazyWithoutRegistration()`, see below:

```cs md*/
class Func_works_without_registration
{
    [Test]
    public void Example()
    {
        var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());
        var getA = container.Resolve<Func<A>>();

        // later register an `A`
        container.Register<A>();
        var a = getA();
        Assert.IsNotNull(a);
    }

    class A { }
} /*md
```

### Func of A with parameters

Delegates creation of service to the user and allows to supply subset of dependencies. The rest of dependencies are injected by container as usual. 

More details:

- It may be viewed as an override of normally injected dependencies with custom ones, provided via arguments. 
- Or as "currying" of service creation constructor or method.
- Sometimes Func with parameters is used to pass primitive values, e.g. `string`, `int`, etc.
- Always injected as inline creation expression: `new B((dep1, dep2) => new A(dep1, new D(), dep2))`
- [recursive dependency] is permitted.


Provided arguments are matched with parameters by type and in passed order. 
If matched argument is not found then the argument will be injected by container, as `D` in the example above. 
The arguments order may differ from parameters order.

What if provided arguments are of the same type? DryIoc will track already used arguments and will skip them in subsequent match. 
Means that `dep1` and `dep2` from the example may be a string and still both used.

__Note:__ In case when passed argument is not used then DryIoc will proceed to match it for nested dependencies (if any):
```cs
new B(arg => new A(new D(arg)))
```

When passed argument was not used it is maybe a mistake but may be is not. It was a hard choice, but DryIoc will ignore this for now.
```cs md*/
class Passed_argument_was_not_used
{
    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<A>();

        // Can be used, the argument will be ignored
        var getA = container.Resolve<Func<string, A>>();

        Assert.IsNotNull(getA("ignore me"));
    }

    class A { }
}/*md
```

There is an other caveat related to reuse. You may say that passing a different argument to create a service likely may produce a different service.
But what happens when service is registered with non-transient reuse? 
By default DryIoc will use the first call to `Func` to create a service and ignore the rest. 
```cs md*/
class Func_with_args_and_reuse
{
    [Test]
    public void Example()
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
}/*md
```

If you don't like it this way, you may use a rule `Rules.WithIgnoringReuseForFuncWithArgs()`
```cs md*/
class Func_with_args_with_rule_ignoring_reuse
{
    [Test]
    public void Example()
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
}/*md
```


### KeyValuePair of Service Key and A

Wraps a registered service key together with the resolved service. 
May be used to filter service based on its key. Usually used together with Func or Lazy wrapper and nested inside collection: `IEnumerable<KeyValuePair<string, Func<A>>>`.

More on the `KeyValuePair` usage is [here](RegisterResolve#markdown-header-resolving-as-keyvaluepair-wrapper).


### Meta or Tuple of A with Metadata

Packs together resolved service A and associated metadata object. A bit similar to `KeyValuePair` but with metadata instead of key. 
You may think of the metadata the same way as of `Attribute` defined for the implementation type. 
Metadata may be provided with attributes when using [MefAttributedModel](Extensions/MefAttributedModel). 
```cs md*/
class Providing_metadata
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<A>(setup: Setup.With(metadataOrFuncOfMetadata: "XYZ"));

        var a1 = container.Resolve<Meta<A, string>>();
        var a2 = container.Resolve<Tuple<A, string>>(); // is the same thing

        Assert.AreEqual("XYZ", a1.Metadata);
        Assert.AreEqual("XYZ", a2.Item2);
    }
} /*md
```

__Note:__ You may choose to use `System.Tuple<,>` over `DryIoc.Meta<,>` because former does not require DryIoc reference. 

`Meta<,>` may be a good choice if you are migrating from [Autofac](http://docs.autofac.org/en/latest/resolve/relationships.html#metadata-interrogation-meta-b-meta-b-x), 
or if you want to register `Tuple<,>` with the different meaning.

If no metadata provided in `setup`, then the resolving as `Meta` will throw a `ContainerException` with corresponding message and code.
In the example above registered and resolved metadata types are the same. DryIoc will throw exception in case they are not assignable, 
e.g. when registered as `string` but resolving with `int` metadata type. Resolving as `Meta<A, object>` is always fine because string is object.

Metadata maybe also used for collection filtering. When nested in collection wrapper, and the metadata is not registered or not assignable, 
then no exception will be thrown. Instead the service item will be filtered out from collection (and collection may be empty as the result):
```cs md*/
class Filtering_based_on_metadata
{
    [Test]
    public void Example()
    {
        var container = new Container();
        var items = container.Resolve<Meta<A, int>[]>();
        Assert.IsEmpty(items);
    }
}/*md
```

__Note:__ Filtering works the same way for `KeyValuePair<TKey, TService>`

Wrappers maybe combined further with `Func` or `Lazy`, or even with `KeyValuePair` to provide a filtered collection of lazy services:
```cs
container.Resolve<Meta<string, Func<int, A>>[]>();
```

#### Dictionary Metadata

Metadata value can be a non-primitive object or a `IDictionary<string, object>`. 
In latter case you may `Resolve` by providing the type of any `object` value in a dictionary:

```cs md*/
class Resolve_value_out_of_metadata_dictionary
{
    [Test]
    public void Example()
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
}/*md
```


### IEnumerable or array of A

- Returns all registered service implementations to the user. The actual returned expression is array initializer:

        new B(new A[] { new AImpl1(), new AImpl2(), new AImpl3() })

    __Note:__ Expression is generated only once using all found services and will not change if new service is added afterwards, or existing one updated or removed. You may use LazyEnumerable explained below to address this limitation. IContainer.ResolveMany method may provide results as fixed array or as LazyEnumerable based on passed option. Default is lazy option.

- Due the fact that .NET Array implements number of collection interfaces: IEnumerable<>, IList<>, ICollection<>, IReadOnlyList<>, IReadOnlyCollection<>, DryIoc allows to inject these interfaces. So you may consider them as a wrappers with the same behavior.

- Resolved services are ordered in registration order for default services and without specific order for keyed services. Despite being ordered, the default services may be mixed up with keyed services.

- The fixed array nature implies that all services will be created when wrapper is resolved or injected. But may be you need filter only some services (or inspect service count) and do not want to create the rest. Nested Func or Lazy will help in this case:

        var aas = container.Resolve<IList≤Func<A>>>();
        if (aas.Count == 1) { }

- DryIoc supports filtering of result services when using nested KeyValuePair or Meta wrappers. Basically when nested wrapper is not resolved because of not compatible Key or Metadata type, then the item will be excluded from array. Combining with Func it looks like:

        container.Resolve<KeyValuePair<MyKeyEnum, Func<A>>>();

- If specific service implementation is not resolvable it will not be in result collection. If all implementations are not resolvable then collection will be empty and no exception will be thrown. By not resolvable I mean that registration exist but still could not be resolved due missing dependency or other reason.

#### Open-generics

If container has registered both closed and open-generic implementation of service, then the both services will be included into result collection (_it may be obvious but I want to set clear expectations on this topic_):
```
#!c#
    container.Register<Foo<int>, IntBar>();
    container.Register(typeof(IFoo<>), typeof(Bar<>));
    
    var foos = container.Resolve<Foo<int>[]>();
    Assert.AreEqual(2, foos.Length); // includes both IntBar and Bar<int>
```


#### Contravariant generics

DryIoc by default will include [contravariant](https://msdn.microsoft.com/en-us/library/dd799517(v=vs.110).aspx) generic services into resolved collection.

Example:
```
#!c#
    interface IHandler<in TEvent> {} // contra-variant interface
    
    class MoveEvent {}
    class MoveAbroadEvent : MoveEvent {}

    class MoveHandler : IHandler<MoveEvent> {}
    class MoveAbroadHandler : IHandler<MoveAbroadEvent> {}

    // Using batch registration for convenience:
    container.RegisterMany(new[] { typeof(MoveHandler<>), typeof(MoveAbroadHandler) });

    // Resolve:
    var moveHandlers = container.Resolve<IEnumerable<IHandler<MoveEvent>>>();
    Assert.AreEqual(2, moveHandlers.Count()); // handlers of both Move and MoveAbroad events

    var moveAbroadHandlers = container.Resolve<IEnumerable<IHandler<MoveAbroadEvent>>>();
    Assert.AreEqual(1, moveAbroadHandlers.Count());
```

In this example IHandler<MoveAbroadEvent> is compatible with IHandler<MoveEvent> due variance rules, and therefore included into result `moveHandlers`.

It is possible to disable this behavior per container:

```
#!c#
     var container = new Container(rules => rules
        .WithoutVariantGenericTypesInResolvedCollection());

    // ... the same setup

    var moveHandlers = container.Resolve<IEnumerable<IHandler<MoveEvent>>>();
    Assert.AreEqual(1, moveHandlers.Count()); // exactly one MoveHandler is resolved
```


#### Composite Pattern support

Collection wrapper by default supports [Composite pattern](http://en.wikipedia.org/wiki/Composite_pattern). Composite pattern is observed when service implementation depends on collection of other implementations of the same service. For example:

    class Composite : A
    {
        public Composite(A[] aas) {}
    }

    class A1 : A {}
    class A2 : A {}

In this example we expect that _aas_ in Composite will be composed of A1 and A2 objects, but not of composite itself despite the fact Composite is also an A. Including composite into _aas_ will produce infinite recursion in construction of object graph. Therefore Composite pattern may be looked as a way to avoid such recursion by excluding composite from its dependencies. DryIoc does exactly that:

    container.Register<A, Composite>();
    container.Register<A, A1>();
    container.Register<A, A2>();
    
    container.Resolve<IEnumerable<A>>();

The Resolve will return three instances of A, where first will be composite with only two instances of A: of A1 and A2.


### LazyEnumerable of A

`LazyEnumerable<>` is different from `IEnumerable<>` wrapper in a way that it wraps `ResolveMany` call instead of initialized array expression. When enumerating it will call `ResolveMany` and will return up-to-date services registered in Container. By comparison array wrapper always returns fixed set of services.

```
#!c#
    container.Register<A, A1>();
    var lazyAList = container.Resolve<LazyEnumerable<A>>();
    // at that point not even A1 is resolved
    
    var aList = lazyAList.ToArray(); // A1 is resolved and aList contains it

    container.Register<A, A2>();
    var lazyAList = container.Resolve<LazyEnumerable<A>>();
    aList = lazyAList.ToArray(); // aList now contains A1 and A2 instances
```

`LazyEnumerable<>` implements `IEumerable<>` so you can inject the interface by specifying `LazyEnumerable<>` as required service type. It allows you to override default array injection per dependency:

```
#!c#
    public class AListHandler { public AListHandler(IEnumerable<A> aList) { } }
    
    // Register handler by specifying its dependency as LazyEnumerable<>:
    container.Register(Made.Of(() => new AListHandler(Arg.Of<LazyEnumerable<A>>())));
```

__Note:__ The important thing in dependency specification above, is that your code remains POCO depending on `IEnumerable<>` abstraction and separated from implementation provided at registration time. It also keeps code container-agnostic without introducing DryIoc infrastructure into your business logic.

Another option is to specify `LazyEnumerable<>` implementation for `IEnumerable<>` as a rule per Container:

```
#!c#
    var container = new Container(rules => 
        rules.WithResolveIEnumerableAsLazyEnumerable());
```


### LambdaExpression

Allows to get actual [ExpressionTree](https://msdn.microsoft.com/en-us/library/bb397951.aspx) composed by Container to resolve specific service. 

- It may be used either for diagnostics - to check if Container creates the service in expected way.
- In code generation scenarios - by converting expression to C# code with something like [ExpressionToCode](https://github.com/EamonNerbonne/ExpressionToCode) library. It may be done even at compile-time.
- To understand how DryIoc works internally.

Example:

    container.Register<IService, Service>();

    var expr = container.Resolve<LambdaExpression>(typeof(IService));

    // the result expr may look as following:
    // (object[] state, DryIoc.IResolverContext r, DryIoc.IScope scope) => new Service()

The actual type of returned `LambdaExpression` is `Expression<DryIoc.FactoryDelegate>` which has signature as in example.

__Note:__ Resolving as `LambdaExpression` does not create actual service.

## Nested wrappers

Wrappers may be nested to provide combined functionality. Common use-case is to resolve collection of registered services as `Func` or `Lazy`, then filter and create what you need.

Examples: 

As collection of `Func`:

    container.Resolve<IEnumerable<Func<IA>>>();

With registered keys:
    
    container.Resolve<IEnumerable<KeyValuePair<object, Func<IA>>>>()
        .Where(x => Filter(x.Key))  // filter based on key
        .Select(x => x.Value());    // create IA by invoking Func

With typed metadata:

	container.Resolve<IEnumerable<Meta<MyMetadata, Func<IA>>>>();

Other combinations:

    container.Resolve<IEnumerable<KeyValuePair<object, Meta<Lazy<IA>, MyMetadata>>>>();
    container.Resolve<Func<Arg1, Arg2, IA>[]>();
    container.Resolve<Meta<Func<Arg1, Arg2, IA>, object>>();
    // etc.

## User-defined wrappers

To register your own wrapper just specify setup parameter as `Setup.Wrapper` or `Setup.WrapperWith`:
```
#!c#

    public interface ICmd { }
    public class X : ICmd { }
    public class Y : ICmd { }

    // Here the wrapper!
    public class MenuItem<T> where T : ICmd { }

    // Register
    container.Register<ICmd, X>();
    container.Register<ICmd, Y>();

    // Register wrapper!
    container.Register(typeof(MenuItem<>), setup: Setup.Wrapper);

    var items = container.Resolve<MenuItem<ICmd>[]>();
    Assert.AreEqual(2, items.Length);
```

__Note:__ The main difference between wrapper and non-wrapper is how they are treated by ResolveMany and collection wrappers:

- When `MenuItem` registered normally - array will contain single item as result - because of single `MenuItem` registration.
- When `MenuItem` registered as wrapper it is treated specially by collection resolution: resolver will try to find all wrapped types first (`ICmd`), and then wrap each found type in `MenuItem`.

If open-generic wrapper has more than one type argument (e.g. `Meta<A, Metadata>`) you need to specify wrapped argument index: `setup: Setup.WrapperWith(0)`.

You may register non-generic wrapper. In this case when resolved you should identify wrapped service with [Required Service Type](RequiredServiceType).

Example of non-generic wrapper:
```
#!c#
    
    public class MyWrapper { public MyWrapper(IService service) { } }

    container.Register<IService, MyService>();
    container.Register<MyWrapper>(setup: Setup.Wrapper);

    container.Resolve<MyWrapper[]>(requiredServiceType: typeof(IService));
```

When injecting `MyWrapper` as dependency you may specify required service type for the dependency:
```
#!c#
    public class UseMyWrapper { public UseMyWrapper(MyWrapper wr) { } }

    // Using Made expression for registration
    container.Register<UseMyWrapper>(
        Made.Of(() => new UseMyWrapper(Arg.Of<MyWrapper, IService>())));
```
md*/
