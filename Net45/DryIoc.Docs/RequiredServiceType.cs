/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Required Service Type

[TOC]

## Overview

Required service type identifies the registered service type when resolving or injecting things, when
the type is different from the resolution service type. 

Better illustrated with examples.

### Required service type is implemented by resolved type 

```cd md*/
using DryIoc;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

class Required_service_type_is_implemented_by_resolution_type
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
} /*md
```
    
Where `Foo` is required (registered) service type and `IFoo` is a resolved type.

Resolve will throw an exception if `Foo` does not implement `IFoo`.

__Note:__ Required service type always refers to __service type__ and not to __implementation type__. 

### Required service type identifies a wrapped service type when resolving a [Wrapper](Wrappers)

```cd md*/
class Service_type_for_a_wrapper
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
}/*md
```

We are resolving collection of objects `IEnumerable<object>` where an object required to ba a `Foo`. 

Works with nested wrappers as well, e.g. `IEnumerable<Func<object>>`

### Specify to use open-generic type in case you have both closed and open-generic registrations

```cs md*/
class Select_to_use_and_open_generic_type 
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
} /*md
```

To specify required service type for injected dependency you may use `Made.Of` expression:

    container.Register<Bar>(Made.Of(() => new Bar(Arg.Of<IDisposable, IFoo>()));

Where `Bar` expects `IDisposable` and we are injecting `IFoo` required service type.


## Adapts external code

Given the service: `container.Register<IFoo, Foo>()`
and `GenericHandler` class in external library that expects `object` dependency:
```
#!c#
    public class GenericHandler 
    {
        public readonly object Target,
        public class GenericHandler(object target)
        {
            Target = target;
        }
    }
```

How to configure `GenericHandler` to use `IFoo`?

Let's try `RegisterDelegate` as generally available technique in may IoC Containers: 
```
#!c#
    container.RegisterDelegate(r => new GenericHandler(r.Resolve<IFoo>()));
```

Seems fine, but what if `GenericHandler` has many more dependencies which are also available from container. Then we need to specify Resolve for all of them.

But the main point is the delegate registration (though powerful) is the "black box" for container and may lead to problems when used wrong: 

- __Memory leaks by capturing variable into delegate closure and keeping them for container lifetime.__
- __Container is unable to see what's inside delegate. Which makes it hard to find type mismatches or diagnose other potential problems.__

Let's use required service type:
```
#!c#
    contaner.Register(Made.Of(() => new GenericHandler(Arg.Of<IFoo>()));
    contaner.Register<IFoo, Foo>();

    // and resolution work fine
    var service = c.Resolve<DynamicService>();
    Assert.That(service.FooObject, Is.InstanceOf<IFoo>());
```

Here we are using `Made.Of` specification expression saying to use required service type `IFoo` as argument.

__Made.Of looks similar to RegisterDelegate but actually not a delegate but [ExpressionTree](https://msdn.microsoft.com/en-us/library/bb397951.aspx) analyzed by Container to get registration information.__

Another example is just resolving some service as base or interface type:
```
#!c#
    public class A {}
    public class B : Log4net_logger.A {}

    // Given registered B:
    container.Register<B>();

    // Resolve B as A:
    A a = container.Resolve<A>(requiredServiceType: typeof(B));
    // or just
    a = container.Resolve<A>(typeof(B));
```


## Works with Wrappers

More interesting is using required service type together with [Wrappers](Wrappers).

Basically required service type will be propagated inside wrapper the same way as service key. That's allow us to specify __wrapped__ service type.

Let's imaging that `GenericHandler` expects `Lazy<object>` in constructor:
```
#!c#
    public class GenericHandler 
    {
        public object Target { get { return _target.Value; }}
    
        public class DynamicService(Lazy<object> target)
        {
            _target = target;
        }
    
        private readonly Lazy<object> _target;
    }

    // Change registration as following:
    contaner.Register(Made.Of(() => new GenericHandler(Arg.Of<Lazy<object>, IFoo>()));
    contaner.Register<IFoo, Foo>();

    // Resolution remain the same
    var handler = c.Resolve<GenericHandler>();
    Assert.IsInstanceOf<IFoo>(handler.Target));
```

Here DryIoc will look for `IFoo` instead of `object` inside `Lazy<>` wrapper.


## Works with IEnumerable and collection wrappers

[IEnumerable and the rest of supported collection types]((Wrappers#markdown-header-ienumerable-or-array-of-a)) are also Wrappers, so you may expect required service type to work with them too:
```
#!c#
    container.Register<IDigit, One>();
    container.Register<IDigit, Two>();
    
    // Get all digits as objects:
    container.Resolve<IEnumerable<object>>(typeof(IDigit));
```

__Note:__ Examples with Lazy and IEnumerable give the same vibe as [Variance for open-generic types in .NET 4.0 and higher](http://msdn.microsoft.com/en-us/library/dd799517%28v=vs.110%29.aspx) and DryIoc is supporting this functionality starting from .NET 3.5.

md*/
