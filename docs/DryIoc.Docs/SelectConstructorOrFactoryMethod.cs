/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Select Constructor or Factory Method

[TOC]

## Multiple constructors

By default DryIoc expects implementation to have only single constructor.
This constructor will be used for Constructor Injection of parameter dependencies.

Default constructor means no dependencies.

If class has multiple constructors the default behavior is to throw corresponding `ContainerException`.
To avoid the exception you may specify what constructor to use while registering.

Given class with two constructors:
```
#!c#
    public class Foo 
    {
        public Foo() {}
        public Foo(IDependency dep) { Dep = dep; }
    }
```

There are multiple ways to select constructor:

- The preferable way is strongly typed specification with [Expression Tree](https://msdn.microsoft.com/en-us/library/bb397951.aspx) expression:

```
#!c#
c.Register<Foo>(made: Made.Of(() => new Foo(Arg.Of<IDependency>())));
```

__Note:__ Code `() => new Foo(Arg.Of<IDependency>()` is just specification expression and won't be executed.

`Arg` class provides static methods to specify injected dependency details as explained here: [Specify Dependency or Primitive Value Injection](https://bitbucket.org/dadhi/dryioc/wiki/SpecifyDependencyOrPrimitiveValueInjection).

- Another way is using Reflection:
```
#!c#
    c.Register<Foo>(made: Made.Of(typeof(Foo).GetConstructor(new[] { typeof(IDependency) })));
```

__Note:__ When registering open-generic the reflection is the only way:
```
#!c#
    c.Register(typeof<Foo<>>, 
        made: Made.Of(typeof(Foo<>).GetConstructor(new[] { typeof(IDependency<>) })));
```


## Selecting constructor with resolvable parameters

DryIoc supports selecting of constructor with all resolvable parameters. The process is peeking constructor with
maximum number of parameters first and trying to resolve them. If some parameter is not resolved, then container will proceed to next constructor with less
parameters. If no constructors resolved successfully and there is no default constructor container will throw meaningful exception.
Succeeded it will use the constructor for service resolution.

The rule may be used:

- Per service registration (preferable to pin-point problematic service but stay deterministic for rest of registrations):
```
#!c#
    c.Register<Foo>(made: FactoryMethod.ConstructorWithResolvableArguments);
```

- Per whole Container:
```
#!c#
    var c = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments));
    c.Register<Foo>(); // no need to specify how to select constructor
```

Registration level rule will override container rule if both are present.


## Factory Method instead of Constructor

Some designs may require to use static or instance method for creating service, e.g. `OpenSession`.
This way API provider may additionally configure or initialize service before returning it to client.

DryIoc directly supports static or instance factory methods. Container will inject dependencies into method parameters the
same way as for constructors.

__Note:__ Please prefer to use factory method over `RegisterDelegate` to minimize state capturing problems leading to memory leaks and to keep code as container-agnostic as possible.

Using static factory method:
```
#!c#
    public static class FooFactory 
    {
        public static IFoo CreateFoo(IRepo repo)
        {
            var foo = new Foo();
            repo.Add(foo);
            return foo;
        }
    }
    
    // elsewhere
    c.Register<IRepo, Repo>();
    c.Register<IFoo>(made: Made.Of(() => FooFactory.CreateFoo(Arg.Of<IRepo>())));
```

Using instance factory method:
```
#!c#
    public class FooFactory : IFooFactory
    {
    	public FooFactory(FactoryDependency dep) { }
    
        public IFoo CreateFoo(IRepo repo)
        {
            var foo = new Foo();
            repo.Add(foo);
            return foo;
        }
    }
    
    // elsewhere
    c.Register<FactoryDependency>();
    c.Register<IFooFactory, FooFactory>(Reuse.Singleton);
    
    c.Register<IRepo, Repo>();
    c.Register<IFoo>(made: Made.Of(r => ServiceInfo.Of<IFooFactory>(), f => f.CreateFoo(Arg.Of<IRepo>())));
```

With instance factory methods you can use chain of factories if necessary.


## Property/Field as Factory Method

If DryIoc supports factory methods then why not support Properties and Fields?

Here we are:
```
#!c#
    public class FooFactory : IFooFactory
    {
        public IFoo Foo { get; private set; }
        public FooFactory(IRepo repo) { Foo = new Foo(repo); }
    }
    
    // elsewhere
    c.Register<IRepo, Repo>();
    c.Register<IFooFactory, FooFactory>(Reuse.Singleton);
    
    c.Register<IFoo>(made: Made.Of(r => ServiceInfo.Of<IFooFactory>(), f => f.Foo));
```


## Open-generic Factory Method

DryIoc supports open-generic methods (and properties/fields) defined in open-generic classes. The level of support is the same as for [OpenGenerics](OpenGenerics). That means the Container is capable to match repeated, recurring, position-swapped, etc. generic type parameters with service type arguments. Generic parameter constraints are supported too.

Example:
```
#!c#
    [Export, AsFactory]
    public class Factory<A> 
    {
        [Export]
        IService<A, B> Create<B>(A a) 
        {
            var service = new ServiceImpl<A, B>();
            service.Initialize(a);
            return service;
        }
    }

    // With DryIoc MefAttributedModel (Export attributes) the registration is simple
    var container = new Contaner().WithMefAttributedModel();
    container.RegisterExports(typeof(Factory<>));

    // Manual registration is more tedious
    container.Register(typeof(Factory<>));
    container.Register(typeof(IService<,>), 
        made: Made.Of(typeof(Factory<>).GetSingleMethodOrNull("Create"), ServiceInfo.Of(typeof(Factory<>))));

    container.Register<Foo>(); // register required dependency A for Create

    // Then resolve:
    container.Resolve<IService<Foo, string>>();
```


## Using Factory Method as Initializer

It may be more advanced topic, but it is actually quite easy to do DryIoc.
Initializer is method that expects service as input and returns initialized (or may be completely new) service as output. This looks quite similar to [Decorator](http://en.wikipedia.org/wiki/Decorator_pattern) and indeed may be implemented as Decorator:
```
#!c#
    public class Service : IService { }
    
    public class Initializer 
    {
        public IService Init(IService service, IOtherDependency dep, IAnotherOneDep anotherDep) {
            service.Configure(dep, anotherDep, "greetings");
            return service;
        }
    }
    
    // elsewhere
    c.Register<IService, Service>();
    c.Register<Initializer>();
    // register the rest of dependencies ...
    
    c.Register<IService>(made: Made.Of(r => ServiceInfo.Of<Initializer>(), 
        i => i.Init(Arg.Of<IService>(), Arg.Of<IOtherDependency>(), Arg.Of<IAnotherOneDep>())),
        setup: Setup.Decorator); // Important!
    
    // resolve as usual:
    c.Resolve<IService>(); // after creating Service will call Initializer.Init injecting  dependencies.
```


## Easy Factory Method with DryIoc.MefAttributedModel

I wanted to show how easy to specify Factory Method and Initializers without notion of container (fully container-agnostic) using _DryIoc.MefAttributedModel_ extension:
```
#!c#
    [Export, AsFactory]
    public class FooFactory
    {
        public FooFactory(FactoryDependency dep) { }
    
        [Export]
        public IFoo CreateFoo(IRepo repo)
        {
            var foo = new Foo();
            repo.Add(foo);
            return foo;
        }
    
        [Export]
        public IBlah Blah { get; private set; }
    
        [Export, AsDecorator]
        public IBlah ConfigureBlah(IBlah original) { } 
    }
    
    // elsewhere
    var container = new Container(rules => rules.WithMefAttributedModel());
    container.RegisterExports(new[] { appAssembly });
```

md*/