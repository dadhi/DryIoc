# Specify Dependency or Primitive Value


- [Specify Dependency or Primitive Value](#specify-dependency-or-primitive-value)
  - [Service Type](#service-type)
  - [Required Service Type](#required-service-type)
  - [Service Key](#service-key)
  - [Unresolved service handling](#unresolved-service-handling)
    - [Default value for Unresolved dependency](#default-value-for-unresolved-dependency)
    - [Optional arguments](#optional-arguments)
  - [Injecting value of primitive type](#injecting-value-of-primitive-type)
  - [Custom value for dependency](#custom-value-for-dependency)
  - [Registering with Condition](#registering-with-condition)
  - [Specification API](#specification-api)
  - [Default conventions](#default-conventions)


## Service Type

The minimal part of service and dependency resolution specification is Service Type. It allows container to find corresponding service registration:

```
#!c#

public class Foo { public Foo(IDependency dependency) { /*...*/} }

container.Register<IDependency, Dependency>();
container.Register<Foo>();

// elsewhere
container.Resolve<Foo>();
```

In example above container will use parameter type of `dependency` as Service Type
to find corresponding registered `IDependency` service.

In addition to Service Type you may provide:

- Required Service Type
- Service Key
- Policy to handle Unresolved dependency
- Default value for Unresolved dependency
- Custom primitive value for dependency

## [Required Service Type](https://bitbucket.org/dadhi/dryioc/wiki/RequiredServiceType)

## Service Key

Helps to identify service to be used for specific resolution.
Given you registered multiple services of the same Service Type, Service Key provides the easiest way to find specific service. 

First let's see what happens when Service Key specification is omitted (using `Foo` from  above):
```
#!c#
    container.Register<IDependency, XDependency>();
    container.Register<IDependency, YDependency>();
    container.Register<Foo>();
    
    // elsewhere
    container.Resolve<Foo>();
```

Resolution of `Foo` will fail with exception `"Expecting single default registration of IDependency but found many ..."`

Make it work with `enum` Service Key:
```
#!c#
    public enum DepKind { In, Out }
    
    container.Register<IDependency, XDependency>(serviceKey: DepKind.In);
    container.Register<IDependency, YDependency>(serviceKey: DepKind.Out);
    
    // changing Foo registration to inject specific dependency
    container.Register<Foo>(made: Made.Of(
        () => new Foo(Arg.Of<IDependency>(DepKind.In))));
    
    // elsewhere
    container.Resolve<Foo>();
```

Only the registration part was changed - resolution remained the same. Which is great for  extensibility and testability.

__Note:__ Service Key may be of any type as long as type implements `object.Equals` and `object.GetHashCode` methods. You may use strings as well, but __strings are more fragile for refactoring and do not statically checked by compiler__. So using `enum` is more preferable.

We have used `Arg` class for specifying Service Key (explained later in details). Alternatively you may use `Parameters` or `PropertiesAndFields` classes:
```
#!c#
    container.Register<Foo>(
        made: Parameters.Of.Type<IDependency>(serviceKey: DepKind.In));
```

__Note:__ Using `Parameters` is less refactoring friendly and therefore more error-prone comparing to constructor with `Arg` expression. Latter is statically checked and won't even compile if `Foo` constructor does not contain the dependency.


## Unresolved service handling

_Unresolved_ means that Container unable to resolve either service itself or one of its dependencies. 

Reason for that may differ: 

- Service or its dependency is not registered,
- And there is no fallback `Rules.UnknownServiceResolvers`,
- Or you forget to specify Service Key, or Required Service Type, or Condition.

DryIoc supports two options to handle unresolved service:

- Throw corresponding exception.
- Return default value of service, usually `null`.

__Note:__ Throwing exception is the default option for everything except Properties/Fields. That's default convention because Properties/Fields may be assigned at any time even after service creation, but constructor parameters is something required for creation. 

These options may be specified when calling `Resolve` method:

- `Resolve<IService>(ifUnresolved: IfUnresolved.Throw)` (or just `Resolve<IService>()`)
- `Resolve<IService>(ifUnresolved: IfUnresolved.ReturnDefault)`

Or per dependency when registering service:
```
#!c#
    public class Foo 
    { 
        public IDependency Dependency { get; set; }
        public Foo(IDependency dependency) { Dependency = dependency; } 
    }

    // Use null if IDependency is unresolved
    container.Register<Foo>(made: Made.Of(() => new Foo(Arg.Of<IDependency>(IfUnresolved.ReturnDefault))));
    
    // Should not throw because of above
    var foo = container.Resolve<Foo>(ifUnresolved: IfUnresolved.Throw);
    Assert.IsNull(foo.Dependency);
```

Specify `IfUnresolved.Throw` for property or field dependency to override returning null by default:
```
#!c#
    container.Register<Bar>(made: Made.Of(() => 
        new Bar() { Depenedency = Arg.Of<IDependency>(IfUnresolved.Throw) }));
```

### Default value for Unresolved dependency

Primitive default value may be specified in case of `IfUnresolved.ReturnDefault`:
```
#!c#
    public class Foo { public Foo(int answer) { /*...*/ } }
    
    container.Register<Foo>(
        made: Parameters.Of.Name("answer", ifUnresolved: IfUnresolved.ReturnDefault, defaultValue: 42));
    
```

__Note:__ DryIoc supports only primitive custom values: numbers, strings, enums are OK - but it is not possible to specify arbitrary object. So the only supported default value for `IDependency` is `null`.

### Optional arguments

DryIoc respects [Optional Arguments](https://msdn.microsoft.com/en-us/library/dd264739.aspx) in
Constructors and Factory Methods. Basically it is application of `IfUnresolved.ReturnDefault` option for parameter dependency with use of provided default parameter value. No need to specify anything in addition:
```
#!c#
    public class Foo { public Foo(IDependency dependency = null, int answer = 42) { /*...*/ } }
    
    container.Register<Foo>();
    
    var foo = container.Resolve<Foo>(); // No need to say IfUnresolved.ReturnDefault
    
    Assert.IsNull(foo.Dependency);
    Assert.AreEqual(42, foo.Answer);
```

## Injecting value of primitive type

An overview of all possible ways of injecting primitive value using `String` parameter as example:
```
#!c#
    public class Foo
    {
        public Foo(string name) { }
    }
```

and the ways of injecting `name`:
```
#!c#

    var c = new Container();

    // 1) Just register string object
    c.Register<Foo>();
    c.RegisterInstance("my string");

    // 2) Register string and identify it with serviceKey
    c.Register<Foo>(made: Parameters.Of.Type<string>(serviceKey: "someSetting"));
    c.RegisterInstance("my string", serviceKey: "someSetting");

    // 3) Register string with key and Foo with strongly typed constructor specification
    c.Register<Foo>(Made.Of(() => new Foo(Arg.Of<string>("someSetting"))));
    c.RegisterInstance("my string", serviceKey: "someSetting");

    // 4) Specify custom value as argument for Foo constructor
    c.Register<Foo>(Made.Of(() => new Foo(Arg.Index<string>(0)), requestIgnored => "someString"));

    // 5) Use oldschool RegisterDelegate for Foo (but DryIoc will be unable to look inside delegate and help with resolution errors)
    c.RegisterDelegate<Foo>(() => new Foo("someString"));
```


## Custom value for dependency

DryIoc supports injecting Custom (non-registered) primitive value as parameter and property/field dependency. Using constant value is not
very interesting, let's look at case when value depends on object graph itself. It is common pattern to pass
holder Type as parameter when utilizing "Logger" objects. Check example below:
```
#!c#
    public class Logger : ILogger 
    {
        public Logger(Type type) { Type = type; }
    }
    
    public class User 
    {
        public User(ILogger logger) { Logger = logger; }
    }
    
    // register types in container:
    container.Register<User>();
    container.Register<ILogger, Logger>(made: 
        Parameters.Of.Type<Type>(request => request.Parent.ImplementationType));
    
    // resolving User
    var user = container.Resolve<User>();
    Assert.AreEqual(typeof(User), user.Logger.Type)
```

In addition to `Parameters.Of.Type` there are corresponding methods `Name` and `Details` in `Parameters` and `PropertiesAndFields` classes.

Another possibility is specifying value when registering with Factory Method like in this [example](ExamplesContextBasedResolution).

__Note:__ DryIoc supports only primitive custom values: numbers, strings, enums are fine. But it is not possible to use arbitrary object.


## Registering with Condition

Sometimes dependency may depend on concrete injection position in object graph. You may need one type of `ILogger` for one service and another one for another service. You may handle it by registering two loggers with different Service Key. Another way is to address problem directly by setting up resolution condition:
```
#!c#
    contaner.Register<ILogger, FileLogger>(
        setup: Setup.With(condition: request => request.Parent.ServiceType.IsAssignableTo(typeof(ISmallFish))));
    
    contaner.Register<ILogger, DbLogger>(
        setup: Setup.With(condition: request => request.Parent.ServiceType.IsAssignableTo(typeof(IBigFish))));
```

## Specification API

The entry point for specifying service or dependency resolution is `Made` class.
It contains three __optional__ parts:

- `FactoryMethod` may provide full specification including:
    - Constructor, method, property or field to be used for service creation, 
    - And __optionally__ parameters and properties/fields specification.
- `Parameters` is responsible for parameters specification only.
- `PropertiesAndFields` is responsible for properties and fields specification only.

Example of full `Made` specification:
```
#!c#
    container.Register<IFoo, Foo>(made: Made.Of(
        factoryMethod: r => typeof(Foo).GetConstructorOrNull(args: new[] { typeof(IDependency) }),
        parameters: Parameters.Of.Type<IDependency>(requiredServiceType: typeof(TestDependency)),
        propertiesAndFields: PropertiesAndFields.Auto));
```

As you see `factoryMethod` uses reflection to select constructor.

But Reflection based spec is rarely needed because `Made` allows strongly-typed expression spec:
```
#!c#
    container.Register<IFoo, Foo>(made: Made.Of(
        () => new Foo(Arg.Of<TestDependency>());
```

__Note:__ Strongly-typed expression for factory method also defines spec for parameters and properties/fields in statically checked way, so the separate definitions are not needed. [More details on factory method here](https://bitbucket.org/dadhi/dryioc/wiki/SelectConstructorOrFactoryMethod).

If you omit either part in first example: `factoryMethod`, `parameters`, or `propertiesAndFields` then
the default convention will be applied for omitted part.

To define `parameters` and `propertiesAndFields` part DryIoc provides corresponding `Parameters` and `PropertiesAndFields` static classes. They may be defined in more simple way as following:
```
#!c#
    container.Register<IFoo, Foo>(
        made: Parameters.Of.Type<IDependency>(requiredServiceType: typeof(TestDependency));
```

and:
```
#!c#
    container.Register<IFoo, Foo>(made: PropertiesAndFields.Auto));
```

__Note:__ That's possible because `Parameters` and `PropertiesAndFields` are implicitly convertible to `Made` specification. Use whatever is more convenient for you.

To specify multiple parameters or properties/fields just chain `Type`, `Name`, and more generic `Details` extension methods:
```
#!c#
    container.Register<Foo>(
        made: Parameters.Of
            .Details((par, req) => par.ParameterType.IsAssignableTo(typeof(IDisposable)) 
                ? ServiceDetails.Of(ifUnresolved: IfUnresolved.ReturnDefault)
                : null) // null means use default parameter resolution
            .Name("parameter2", serviceKey: "p2")
            .Type<IDependency>(serviceKey: DepKind.In));
```


## Default conventions

DryIoc uses following default conventions if you are not specifying resolution details:

- Constructor and factory method parameters are injected based on Service Type equal to `ParameterType` with `IfUnresolved.Throw` policy.
- __If not explicitly specified Properties and Fields are not injected at all__. If `PropertiesAndFields.Auto` specified - then all assignable and non primitive properties and fields are injected with `IfUnresolved.ReturnDefault` policy.
- Primitive parameter and property/field types are treated the same normal service types: e.g. DryIoc does not forbid registering of `string` services.
