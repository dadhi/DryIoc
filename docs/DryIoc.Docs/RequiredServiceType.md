<!--Auto-generated from .cs file, the edits here will be lost! -->

# Required Service Type


- [Required Service Type](#required-service-type)
  - [Overview](#overview)
    - [Required service type is implemented by resolved type](#required-service-type-is-implemented-by-resolved-type)
    - [Required service type identifies a wrapped service type when resolving a Wrapper](#required-service-type-identifies-a-wrapped-service-type-when-resolving-a-wrapper)
    - [Specify to use open-generic type in case you have both closed and open-generic registrations](#specify-to-use-open-generic-type-in-case-you-have-both-closed-and-open-generic-registrations)
  - [Adapts external code](#adapts-external-code)
  - [Works with Wrappers](#works-with-wrappers)
  - [Works with IEnumerable and collection wrappers](#works-with-ienumerable-and-collection-wrappers)


## Overview

Required service type identifies the registered service type when resolving or injecting things, when
the type is different from the resolution service type. 

Better illustrated with examples.

### Required service type is implemented by resolved type 

```cs 
namespace DryIoc.Docs;
using DryIoc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class Required_service_type_is_implemented_by_resolution_type
{
    interface IFoo { }
    class Foo : IFoo { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<Foo>();
        var f = container.Resolve<IFoo>(requiredServiceType: typeof(Foo));
        Assert.IsInstanceOf<Foo>(f);
    }
} 
```
    
Where `Foo` is required (registered) service type and `IFoo` is a resolved type.

Resolve will throw an exception if `Foo` does not implement `IFoo`.

__Note:__ Required service type always refers to __service type__ and not to __implementation type__. 

### Required service type identifies a wrapped service type when resolving a [Wrapper](Wrappers)

```cs 
public class Service_type_for_a_wrapper
{
    class Foo { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<Foo>();

        var fooObjects = container.Resolve<IEnumerable<object>>(typeof(Foo));

        Assert.IsInstanceOf<Foo>(fooObjects.Single());
    }
}
```

We are resolving collection of objects `IEnumerable<object>` where an object required to ba a `Foo`. 

Works with nested wrappers as well, e.g. `IEnumerable<Func<object>>`

### Specify to use open-generic type in case you have both closed and open-generic registrations

```cs 
public class Select_to_use_and_open_generic_type 
{
    interface IFoo<T> { }
    class FooInt : IFoo<int> { }
    class Foo<T> : IFoo<T> { }

    [Test] public void Example() 
    {
        var container = new Container();

        container.Register<IFoo<int>, FooInt>();
        container.Register(typeof(IFoo<>), typeof(Foo<>));

        var f = container.Resolve<IFoo<int>>();
        Assert.IsInstanceOf<FooInt>(f);

        // using required service type to resolve Foo<int>
        var f2 = container.Resolve<IFoo<int>>(typeof(IFoo<>));
        Assert.IsInstanceOf<Foo<int>>(f2);

        // AGAIN, Important that required service type should specify 
        // a service type and not the implementation type.
        // The below Resolve won't work cause the `Foo<>` is implementation type.
        var f3 = container.Resolve<IFoo<int>>(typeof(Foo<>), IfUnresolved.ReturnDefault);
        Assert.IsNull(f3);
    }
} 
```

To specify required service type for injected dependency you may use `Made.Of` expression:

    container.Register<Bar>(Made.Of(() => new Bar(Arg.Of<IDisposable, IFoo>()));

Where `Bar` expects `IDisposable` and we are injecting `IFoo` required service type.


## Adapts external code

Given the service registration: `container.Register<IFoo, Foo>()`
and `GenericHandler` class in external library that expects an `object` dependency:
```cs 
class GenericHandler
{
    public readonly object Target;
    public GenericHandler(object target)
    {
        Target = target;
    }
} 
```

How to configure `GenericHandler` to use `IFoo` for the `object` dependency?

First, let's use a `RegisterDelegate` as a generally available technique in many IoC Containers: 
```cs 
public class Using_register_delegate_to_adapt_service_type
{
    interface IFoo { }
    class Foo : IFoo { }

    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<IFoo, Foo>();
        container.RegisterDelegate(r => new GenericHandler(r.Resolve<IFoo>()));

        var handler = container.Resolve<GenericHandler>();
        Assert.IsInstanceOf<Foo>(handler.Target);
    }
} 
```

Seems fine, but what if `GenericHandler` has many more dependencies which are also available from container, 
then we need to specify Resolve for all of them.

But the main point is the delegate registration (though powerful) is the "black box" for the container, 
and may lead to problems when used wrong:

- Memory leaks by capturing variable into delegate closure and keeping them for container lifetime.
- Container is unable to see what's inside delegate, which makes it hard to find a lifestyle mismatch or diagnose other problems.

Let's use required service type:
```cs 
public class Required_service_type_to_adapt_the_object_dependency
{
    interface IFoo { }
    class Foo : IFoo { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register(Made.Of(() => new GenericHandler(Arg.Of<IFoo>())));
        container.Register<IFoo, Foo>();

        var handler = container.Resolve<GenericHandler>();
        Assert.IsInstanceOf<IFoo>(handler.Target);
    }
} 
```

Here, we are using `Made.Of` specification expression saying to use required service type `IFoo` as argument.

`Made.Of` looks a very similar to RegisterDelegate but actually its argument is not a delegate, but an 
[ExpressionTree](https://msdn.microsoft.com/en-us/library/bb397951.aspx) parsed by Container to get a 
registration information. The `Made.Of<T>(...)` is similar to `Register<T>(...)` where 
DryIoc retrieves the information about `T` and do its "magic", but with the `Made.Of<T>` it is
possible to provide statically checked expression, which won't compile if provided wrong.


## Works with Wrappers

Another interesting case is using required service type with [Wrappers](Wrappers).

Basically, required service type will be propagated inside wrapper the same way as service key. 
That's allow us to specify a target service type inside a wrapper.

Imaging, the `GenericHandler` expects a `Lazy<object>` instead of an `object` in its constructor:
```cs nd*/
public class Required_service_type_with_wrapper
{
    public class GenericHandler
    {
        public object Target => _target.Value;

        public GenericHandler(Lazy<object> target)
        {
            _target = target;
        }

        private readonly Lazy<object> _target;
    }

    interface IFoo { }
    class Foo : IFoo { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<IFoo, Foo>();
        container.Register(Made.Of(() => new GenericHandler(Arg.Of<Lazy<object>, IFoo>())));

        // Resolution remain the same
        var handler = container.Resolve<GenericHandler>();
        Assert.IsInstanceOf<IFoo>(handler.Target);
    }
} 
```

Here in `Arg.Of<Lazy<object>, IFoo>()` DryIoc will look for required service type `IFoo` instead of `object` inside the `Lazy<>` wrapper.


## Works with IEnumerable and collection wrappers

[IEnumerable and the rest of supported collection types]((Wrappers.md#ienumerable-or-array-of-a)) are also Wrappers, 
so you may expect required service type to work with them too:
```cs 
public class Required_service_type_in_collection
{
    interface IDigit { }
    class One : IDigit { }
    class Two : IDigit { }

    [Test]
    public void Example()
    {
        var container = new Container();
        container.Register<IDigit, One>();
        container.Register<IDigit, Two>();

        // Get all digits as objects:
        container.Resolve<IEnumerable<object>>(requiredServiceType: typeof(IDigit));
    }
} 
```

__Note:__ Examples with Lazy and IEnumerable give the same vibe as 
[Variance for open-generic types in .NET 4.0 and higher](http://msdn.microsoft.com/en-us/library/dd799517%28v=vs.110%29.aspx) 
and DryIoc is supporting this functionality starting from .NET 3.5.
