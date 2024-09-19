/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Select Constructor or Factory Method


- [Select Constructor or Factory Method](#select-constructor-or-factory-method)
  - [Multiple constructors](#multiple-constructors)
  - [Selecting constructor with resolvable parameters](#selecting-constructor-with-resolvable-parameters)
  - [Factory Method instead of Constructor](#factory-method-instead-of-constructor)
    - [Using static factory method](#using-static-factory-method)
    - [Select the constructor or factory method based on the condition](#select-the-constructor-or-factory-method-based-on-the-condition)
    - [Using instance factory method](#using-instance-factory-method)
  - [Property/Field as Factory Method](#propertyfield-as-factory-method)
  - [Open-generic Factory Method](#open-generic-factory-method)
  - [Export Factory Method with DryIoc.MefAttributedModel](#export-factory-method-with-dryiocmefattributedmodel)


## Multiple constructors

By default DryIoc expects implementation to have only single constructor.
This constructor will be used for Constructor Injection of parameter dependencies.

Default constructor means no dependencies.

If class has multiple constructors the default behavior is to throw corresponding `ContainerException`.
To avoid the exception you may specify what constructor to use while registering.

Given the class with the two constructors:
md*/
//md{ usings ...
//md```cs
namespace DryIoc.Docs;
using DryIoc;
using DryIocAttributes;
using DryIoc.MefAttributedModel;
using System.ComponentModel.Composition;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
// ReSharper disable UnusedVariable
//md```
//md}

//md```cs
public interface IDependency { }
public class Dep : IDependency { }
public class Foo
{
    public IDependency Dep { get; }
    public Foo(IDependency dep) => Dep = dep;
}
/*md
```

There are multiple ways to select constructor:

- The preferable way is strongly typed specification with [Expression Tree](https://msdn.microsoft.com/en-us/library/bb397951.aspx) expression:

```cs md*/
public class Register_strongly_typed_service_with_expression
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<IDependency, Dep>();
        c.Register<Foo>(made: Made.Of(() => new Foo(Arg.Of<IDependency>())));
        Assert.IsNotNull(c.Resolve<Foo>());
    }
}
/*md
```

**Note:** The code `() => new Foo(Arg.Of<IDependency>()` is just a specification expression and won't be executed.

`Arg` class provides static methods to specify injected dependency details as explained here: [Specify Dependency or Primitive Value Injection](SpecifyDependencyOrPrimitiveValueInjection).

- Another way is using Reflection:

```cs md*/
public class Register_with_reflection
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<IDependency, Dep>();
        c.Register<Foo>(made: Made.Of(typeof(Foo).GetConstructor(new[] { typeof(IDependency) })));
        Assert.IsNotNull(c.Resolve<Foo>());
    }
}
/*md
```

__Note:__ When registering open-generic the reflection is the only way:

```cs md*/
public interface IDependency<T> { }
public class Foo<T>
{
    public IDependency<T> Dep { get; }
    public Foo(IDependency<T> dep) => Dep = dep;
}

public class Dep<T> : IDependency<T> { }

public class Register_open_generics_with_reflection
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<IDependency<int>, Dep<int>>();
        c.Register(typeof(Foo<>), made: Made.Of(typeof(Foo<>).GetConstructors()[0]));
        Assert.IsNotNull(c.Resolve<Foo<int>>());
    }
}
/*md
```


## Selecting constructor with resolvable parameters

DryIoc supports selecting of constructor with all resolvable parameters. The process is peeking constructor with
maximum number of parameters first and trying to resolve them. If some parameter is not resolved, then container will proceed to next constructor with less
parameters. If no constructors resolved successfully and there is no default constructor container will throw meaningful exception.
Succeeded it will use the constructor for service resolution.

The rule may be used:

- Per service registration (preferable to pin-point problematic service but stay deterministic for rest of registrations):

```cs md*/
public class Register_with_automatic_constructor_selection
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<IDependency, Dep>();
        c.Register<Foo>(made: FactoryMethod.ConstructorWithResolvableArguments);
        Assert.IsNotNull(c.Resolve<Foo>());
    }
}
/*md
```


- For the entire Container:

```cs md*/
public class Register_with_automatic_constructor_selection_for_entire_container
{
    [Test]
    public void Example()
    {
        var c = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments));
        c.Register<IDependency, Dep>();
        c.Register<Foo>(); // no need to specify how to select constructor
        Assert.IsNotNull(c.Resolve<Foo>());
    }
}
/*md
```

Registration level rule will override container rule if both are present.


## Factory Method instead of Constructor

Some designs may require to use static or instance method for creating service, e.g. `OpenSession`.
This way API provider may additionally configure or initialize service before returning it to client.

DryIoc directly supports static or instance factory methods. Container will inject dependencies into method parameters the
same way as for constructors.

__Note:__ You may also consider using `RegisterDelegate<TDependencies..., TService>` method to register factory method. This version of method does not have problems of `RegisterDelegate<IResolverContext, TService>` of being service-locator. Read more [here](RegisterResolve.md#the-cure---registerdelegate-with-the-dependency-parameters).


### Using static factory method

```cs md*/
public class Register_with_static_factory_method
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<Repo>();
        c.Register<IFoo>(made: Made.Of(() => FooFactory.CreateFoo(Arg.Of<Repo>())));
        Assert.IsNotNull(c.Resolve<IFoo>());
    }

    public static class FooFactory
    {
        public static IFoo CreateFoo(Repo repo)
        {
            var foo = new FooBar();
            repo.Add(foo);
            return foo;
        }
    }

    public interface IFoo { }
    public class FooBar : IFoo { }
    public class Repo
    {
        public void Add(IFoo foo) { }
    }
}
/*md
```

### Select the constructor or factory method based on the condition

Using `Made.Of` you may specify the condition to select the constructor or factory method based on the context 
of the resolution or injection of the service. 

**Note:** I advice to explore the overloads of the `Made.Of` and `FactoryMethod.Of` methods to see what's possible
in the current DryIoc version.

```cs md*/
public class Select_factory_method_based_on_condition
{
    [Test]
    public void Example()
    {
        var c = new Container();

        c.Register<Consumer>();
        c.Register<Consumer>(serviceKey: "special");
        c.RegisterInstance(new FooNameProvider("foo"));

        c.Register<IFoo>(made: Made.Of(req =>
        {
            // Use factory method of Consumer to produce foo if Foo is injected as dependency into any parent service,
            // resolved or injected with "special" service key.
            if (req.Parent.Any(p => "special".Equals(p.ServiceKey)))
                return typeof(Consumer).GetMethod(nameof(Consumer.GetMyFoo), BindingFlags.Public | BindingFlags.Static);

            // Use default constructor if IFoo is directly resolved and not injected as dependency (a resolution root)
            if (req.IsResolutionRoot)
                return typeof(Foo).GetConstructors().Where(c => c.GetParameters().Length == 0).FirstOrDefault();

            // Use constructor with the NameProvider dependency for the rest of cases
            return typeof(Foo).GetConstructors()
                .Where(c => c.GetParameters().Any(p => p.ParameterType == typeof(FooNameProvider)))
                .FirstOrDefault();
        }));

        var foo = c.Resolve<IFoo>();
        Assert.AreEqual("default foo", foo.Name);

        var specialConsumer = c.Resolve<Consumer>(serviceKey: "special");
        Assert.AreEqual("special foo", specialConsumer.Foo.Name);

        var normalConsumer = c.Resolve<Consumer>();
        Assert.AreEqual("foo", normalConsumer.Foo.Name);
    }

    public interface IFoo
    {
        string Name { get; }
    }

    public class Foo : IFoo
    {
        public string Name { get; internal set; }
        public Foo(FooNameProvider np) => Name = np.FooName;
        public Foo() => Name = "default foo";
    }

    public class FooNameProvider
    {
        public readonly string FooName;
        public FooNameProvider(string fooName) => FooName = fooName;
    }

    public class Consumer
    {
        public static IFoo GetMyFoo() => new Foo { Name = "special foo" };

        public IFoo Foo { get; }
        public Consumer(IFoo foo) => Foo = foo;
    }
}
/*md
```

### Using instance factory method

```cs md*/
public class Register_with_instance_factory_method
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<IFooFactory, FooFactory>(Reuse.Singleton);
        c.Register<IDependency, Dep>();
        c.Register<Repo>();
        c.Register<IFoo>(made: Made.Of(r => ServiceInfo.Of<IFooFactory>(), f => f.CreateFoo(Arg.Of<Repo>())));
        Assert.IsNotNull(c.Resolve<IFoo>());
    }

    public interface IFooFactory
    {
        IFoo CreateFoo(Repo repo);
    }
    public class FooFactory : IFooFactory
    {
        public FooFactory(IDependency dep) { }

        public IFoo CreateFoo(Repo repo)
        {
            var foo = new FooBar();
            repo.Add(foo);
            return foo;
        }
    }

    public interface IFoo { }
    public class FooBar : IFoo { }
    public class Repo
    {
        public void Add(IFoo foo) { }
    }
}
/*md
```

With instance factory methods you can use chain of factories if necessary.


## Property/Field as Factory Method

If DryIoc supports factory methods then why not support Properties and Fields?

Here we are:

```cs md*/
public class Register_with_instance_property
{
    [Test]
    public void Example()
    {
        var c = new Container();
        c.Register<Repo>();
        c.Register<FooFactory>(Reuse.Singleton);
        c.Register<IFoo>(made: Made.Of(r => ServiceInfo.Of<FooFactory>(), f => f.Foo));
        Assert.IsNotNull(c.Resolve<IFoo>());
    }

    public class FooFactory
    {
        public IFoo Foo { get; private set; }
        public FooFactory(Repo repo) { Foo = new Foo(repo); }
    }
    public interface IFoo { }
    public class Foo : IFoo
    {
        public Foo(Repo repo) { }
    }
    public class Repo { }
}
/*md
```


## Open-generic Factory Method

DryIoc supports open-generic methods (and properties/fields) defined in the open-generic classes. 
The level of support is the same as for [OpenGenerics](OpenGenerics). 
That means the Container is capable to match repeated, recurring, position-swapped, etc. generic type parameters with service type arguments. 
The generic parameter constraints are supported too.

Example:

```cs md*/
public class Register_open_generics
{
    [Test]
    public void Example()
    {
        var container = new Container();

        container.Register<Foo>();
        container.Register(typeof(Factory<>));
        container.Register(typeof(IService<,>),
            made: Made.Of(typeof(Factory<>).GetSingleMethodOrNull("Create"), ServiceInfo.Of(typeof(Factory<>))));

        Assert.IsNotNull(container.Resolve<IService<Foo, string>>());
    }

    public interface IService<A, B>
    {
        void Initialize(A a);
    }
    public class ServiceImpl<A, B> : IService<A, B>
    {
        public void Initialize(A a) { }
    }

    public class Foo { }

    [Export]
    public class Factory<A>
    {
        [Export]
        public IService<A, B> Create<B>(A a)
        {
            var service = new ServiceImpl<A, B>();
            service.Initialize(a);
            return service;
        }
    }
}
/*md
```

## Export Factory Method with DryIoc.MefAttributedModel

DryIoc provides the [DryIoc.MefAttributedModel](Extensions/MefAttributedModel) extension which enables the use of MEF `Export` and `Import` attributes for registrations and injections 
which may help to register the open-generics. Look for the use of the `Export` attribute and for the `AsDecorator` (how simple is this).
```cs md*/
public class Register_open_generics_with_MefAttributedModel_extension
{
    [Test]
    public void Example()
    {
        var container = new Container().WithMefAttributedModel();

        container.RegisterExports(typeof(Factory<>), typeof(Foo), typeof(FooDecorator));

        Assert.IsNotNull(container.Resolve<IService<Foo, string>>());
    }

    public interface IService<A, B>
    {
        void Initialize(A a);
    }
    public class ServiceImpl<A, B> : IService<A, B>
    {
        public void Initialize(A a) { }
    }

    [Export]
    public class Foo { }

    [Export, AsDecorator]
    public class FooDecorator : Foo
    {
        public FooDecorator(Foo f) { }
    }

    [Export]
    public class Factory<A>
    {
        [Export]
        public IService<A, B> Create<B>(A a)
        {
            var service = new ServiceImpl<A, B>();
            service.Initialize(a);
            return service;
        }
    }
}
/*md
```
md*/