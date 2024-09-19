/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

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
  - [Complete example of matching the parameter name to the service key](#complete-example-of-matching-the-parameter-name-to-the-service-key)
    - [The problem](#the-problem)
    - [The solution](#the-solution)


## Service Type

The minimal part of service and dependency resolution specification is Service Type. It allows container to find corresponding service registration:
md*/
//md{ usings ...
//md```cs
namespace DryIoc.Docs;
using System;
using NUnit.Framework;
using DryIoc;
// ReSharper disable UnusedVariable
//md```
//md}

//md```cs 
public class Resolving_with_a_service_type 
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IDependency, Dependency>();
        container.Register<Foo>();

        // elsewhere
        container.Resolve<Foo>();
    }

    public interface IDependency {}
    public class Dependency : IDependency {}
    public class Foo { public Foo(IDependency dependency) { /*...*/} }
} 
/*md
```

In example above container will use parameter type of `dependency` as Service Type
to find corresponding registered `IDependency` service.

In addition to Service Type you may provide:

- Required Service Type
- Service Key
- Policy to handle Unresolved dependency
- Default value for Unresolved dependency
- Custom primitive value for dependency

## [Required Service Type](RequiredServiceType)

## Service Key

Helps to identify service to be used for specific resolution.
Given you registered multiple services of the same Service Type, Service Key provides the easiest way to find specific service. 

First let's see what happens when Service Key specification is omitted (using `Foo` from  above):
```cs md*/
public class Fail_to_resolve_from_the_multiple_registered_services 
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IDependency, XDependency>();
        container.Register<IDependency, YDependency>();
        container.Register<Foo>();

        var ex = Assert.Throws<ContainerException>(() =>
        container.Resolve<Foo>());

        Assert.AreEqual(Error.NameOf(Error.ExpectedSingleDefaultFactory), ex.ErrorName);
    }

    public interface IDependency {}
    public class XDependency : IDependency {}
    public class YDependency : IDependency {}
    public class Foo { public Foo(IDependency dependency) { /*...*/} }
} 
/*md
```

Resolution of `Foo` will fail with the exception `"Expecting a single default registration of IDependency but found many ..."`

Now let's make it work with the `enum` Service Key:

```cs md*/
public class Using_the_enum_service_key
{    
    [Test] public void Example()
    {
        var container = new Container();
    
        container.Register<IDependency, XDependency>(serviceKey: SomeKind.In);
        container.Register<IDependency, YDependency>(serviceKey: SomeKind.Out);
        
        // changing Foo registration to inject the dependency with the specific service key
        container.Register<Foo>(made: 
            Made.Of(() => new Foo(Arg.Of<IDependency>(SomeKind.In))));

        var foo = container.Resolve<Foo>();
        Assert.IsInstanceOf<XDependency>(foo.Dependency);
    }

    public enum SomeKind { In, Out }

    public interface IDependency {}
    public class XDependency : IDependency {}
    public class YDependency : IDependency {}
    public class Foo 
    {
        public IDependency Dependency;
        public Foo(IDependency dependency) => Dependency = dependency;
    }
} 
/*md
```

Only the registration part was changed - resolution remained the same. Which is great for  extensibility and testability.

__Note:__ Service Key may be of any type as long as type implements `object.Equals` and `object.GetHashCode` methods. You may use strings as well, but __strings are more fragile for refactoring and do not statically checked by compiler__. So using `enum` is more preferable.

We have used `Arg` class for specifying Service Key (explained later in details). Alternatively you may use the `Parameters` or `PropertiesAndFields` classes:

```cs md*/
public class Using_the_enum_service_key_and_parameter_specification
{    
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<IDependency, XDependency>(serviceKey: SomeKind.In);
        container.Register<IDependency, YDependency>(serviceKey: SomeKind.Out);
        
        // using the paremeter specification
        container.Register<Foo>(made: 
            Parameters.Of.Type<IDependency>(serviceKey: SomeKind.In));

        var foo = container.Resolve<Foo>();
        Assert.IsInstanceOf<XDependency>(foo.Dependency);
    }

    public enum SomeKind { In, Out }

    public interface IDependency {}
    public class XDependency : IDependency {}
    public class YDependency : IDependency {}
    public class Foo 
    {
        public IDependency Dependency;
        public Foo(IDependency dependency) => Dependency = dependency;
    }
} 
/*md
```

__Note:__ Using the `Parameters` is less refactoring friendly and therefore more error-prone comparing to constructor with `Arg` expression. Latter is statically checked and won't even compile if `Foo` constructor does not contain the dependency.


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
```cs md*/
public class Specifying_IfUnresolved_for_the_parameter
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Foo>(made: Made.Of(() => 
            new Foo(Arg.Of<IDependency>(IfUnresolved.ReturnDefault))));

        var foo = container.Resolve<Foo>(ifUnresolved: IfUnresolved.Throw);
        // IfUnresolved.Throw is the default so the alternative is just a
        foo = container.Resolve<Foo>();

        Assert.IsNotNull(foo);
        Assert.IsNull(foo.Dependency);
    }

    public class Foo 
    { 
        public IDependency Dependency { get; }
        public Foo(IDependency dependency) => Dependency = dependency;
    }
} 
/*md
```

Specify `IfUnresolved.Throw` for the property or the field dependency to override returning null by default:
```cs md*/
public class Specifying_IfUnresolved_for_the_property
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Bar>(made: Made.Of(() =>
            new Bar() { Dependency = Arg.Of<IDependency>(IfUnresolved.Throw) }));

        var ex = Assert.Throws<ContainerException>(() =>
            container.Resolve<Bar>());
        Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), ex.ErrorName);

        // compare it to the default behavior:

        container.Register<Bar>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        var bar = container.Resolve<Bar>();
        Assert.IsNotNull(bar);
        Assert.IsNull(bar.Dependency);
    }

    public class Bar 
    { 
        public IDependency Dependency { get; set; }
    }
}
/*md
```

### Default value for Unresolved dependency

Primitive default value may be specified in case of `IfUnresolved.ReturnDefault`:
```cs md*/
public class Specifying_the_default_value_for_the_unresolved_parameter
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Foo>(
            made: Parameters.Of.Name("answer", ifUnresolved: IfUnresolved.ReturnDefault, defaultValue: 42));

        var foo = container.Resolve<Foo>();
        Assert.AreEqual(42, foo.Answer);
    }

    public class Foo 
    { 
        public int Answer; 
        public Foo(int answer) => Answer = answer;
    }
}
/*md
```

__Note:__ DryIoc supports only primitive custom values: numbers, strings, enums are OK - but it is not possible to specify arbitrary object. So the only supported default value for `IDependency` is `null`.

### Optional arguments

DryIoc respects the [Optional Arguments](https://msdn.microsoft.com/en-us/library/dd264739.aspx) in
the constructors and the factory methods. Basically it is the application of the `IfUnresolved.ReturnDefault` option for the parameter dependency with the use of the provided default parameter value. No need to specify anything in addition:

```cs md*/
public class Respecting_the_csharp_optional_arguments
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Foo>();

        var foo = container.Resolve<Foo>();

        Assert.IsNull(foo.Dependency);
        Assert.AreEqual(42, foo.Answer);
    }

    public class Foo 
    { 
        public IDependency Dependency;
        public int Answer;
        public Foo(IDependency dependency = null, int answer = 42)
        {
            Dependency = dependency;
            Answer     = answer;
        }
    }
}
/*md
```

## Injecting value of primitive type

An overview of all possible ways of injecting primitive value using `String` parameter as example:
```cs md*/
public class Injecting_the_value_of_a_primitive_type
{
    public class Foo
    {
        public string Name;
        public Foo(string name) => Name = name;
    }

    // There many ways of injecting the `name`:

    // 1) Register the string object
    [Test] public void Example_via_RegisterInstance()
    {
        var c = new Container();

        c.Register<Foo>();
        c.RegisterInstance("my string");

        Assert.AreEqual("my string", c.Resolve<Foo>().Name);
    }

    // 2) Register the string and identify it with the service key
    [Test] public void Example_via_RegisterInstance_and_ServiceKey()
    {
        var c = new Container();

        c.Register<Foo>(made: Parameters.Of.Type<string>(serviceKey: "someSetting"));
        c.RegisterInstance("my string", serviceKey: "someSetting");

        Assert.AreEqual("my string", c.Resolve<Foo>().Name);
    }

    // 3) Register string with the key and a Foo with the strongly typed constructor specification
    [Test] public void Example_via_strongly_typed_spec()
    {
        var c = new Container();

        c.Register<Foo>(Made.Of(() => new Foo(Arg.Of<string>("someSetting"))));
        c.RegisterInstance("my string", serviceKey: "someSetting");

        Assert.AreEqual("my string", c.Resolve<Foo>().Name);
    }

    // 4) Specify the custom value as argument for Foo constructor
    [Test] public void Example_via_strongly_typed_spec_and_direct_argument_spec()
    {
        var c = new Container();

        c.Register<Foo>(Made.Of(() => new Foo(Arg.Index<string>(0)), argValues: _ => "my string"));

        Assert.AreEqual("my string", c.Resolve<Foo>().Name);
    }
     
    // 5) Use the old-school black-boxy delegate with the RegisterDelegate for the Foo
    [Test] public void Example_via_RegisterDelegate()
    {
        var c = new Container();

        c.RegisterDelegate<Foo>(() => new Foo("my string"));

        Assert.AreEqual("my string", c.Resolve<Foo>().Name);
    }
}
/*md
```


## Custom value for dependency

DryIoc supports the injecting of custom (non-registered) values as a parameter, property, or field. But using the _constant_ value is not very interesting, so let's look at the case when the value depends on the  object graph context. It is the common pattern to pass the holder Type as a parameter when utilizing the "Logger" object. Check example below:
```cs md*/
public class Injecting_the_custom_value_depending_on_context
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<User>();
        container.Register<ILogger, Logger>(made: 
            Parameters.Of.Type<Type>(req => req.Parent.ImplementationType));

        var user = container.Resolve<User>();
        Assert.AreEqual(typeof(User), user.Logger.ContextType);
    }

    public interface ILogger
    {
        Type ContextType { get; }
    }

    public class Logger : ILogger 
    {
        public Type ContextType { get; }
        public Logger(Type type) => ContextType = type;
    }
    
    public class User 
    {
        public ILogger Logger { get; }
        public User(ILogger logger) => Logger = logger;
    }
}
/*md
```

In addition to `Parameters.Of.Type` there are corresponding methods `Name` and `Details` in `Parameters` and `PropertiesAndFields` classes.

Another possibility is specifying value when registering with Factory Method like in this [example](ExamplesContextBasedResolution).

__Note:__ DryIoc supports only primitive custom values: numbers, strings, enums are fine. But it is not possible to use arbitrary object.


## Registering with Condition

Sometimes dependency may depend on concrete injection position in the object graph. You may need the one type of `ILogger` for one service and the another one for another service. You may handle it by registering two loggers with the different Service Key. Another way is to address problem directly by setting up the resolution condition:
```cs md*/
public class Injecting_the_custom_value_with_condition_setup
{
    [Test] public void Example()
    {
        var container = new Container();
        container.Register<IBigFish, Tuna>();

        container.Register<ILogger, FileLogger>(
            setup: Setup.With(condition: request => request.Parent.ServiceType.IsAssignableTo<ISmallFish>()));
        
        container.Register<ILogger, DbLogger>(
            setup: Setup.With(condition: request => request.Parent.ServiceType.IsAssignableTo<IBigFish>()));

        var fish = container.Resolve<IBigFish>();
        Assert.IsInstanceOf<DbLogger>((fish as Tuna).Logger);
    }

    public interface ISmallFish {}
    public interface IBigFish {}

    public class Tuna : IBigFish
    {
        public ILogger Logger;
        public Tuna(ILogger logger) => Logger = logger;
    }

    public interface ILogger {}
    public class FileLogger : ILogger {}
    public class DbLogger : ILogger {}
}
/*md
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
```cs md*/
public class Full_spec_with_reflection
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IFoo, Foo>(made: Made.Of(
            r => FactoryMethod.Of(typeof(Foo).GetConstructorOrNull(args: new[] { typeof(IDependency) })),
            parameters: Parameters.Of.Type<IDependency>(requiredServiceType: typeof(TestDependency)),
            propertiesAndFields: PropertiesAndFields.Auto));
    }

    public interface IFoo {}
    public class Foo : IFoo {}
    public interface IDependency {}
    public class TestDependency : IDependency {}
}
/*md
```

As you can see, the `factoryMethod` selector uses reflection to select the constructor.

But the Reflection-based spec is rarely needed because the `Made` has an option for the strongly-typed expression spec:

```cs md*/
public class The_spec_with_strongly_typed_Made
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<IFoo, Foo>(made: Made.Of(() => 
            new Foo(Arg.Of<TestDependency>())
        ));
    }

    public interface IFoo {}
    public class Foo : IFoo 
    {
        public Foo(IDependency dependency) {}
    }
    public interface IDependency {}
    public class TestDependency : IDependency {}
}
/*md
```

__Note:__ A strongly-typed expression for factory method also defines spec for parameters and properties/fields in statically checked way so that the separate definitions are not needed - [more details here](SelectConstructorOrFactoryMethod).

If you omit either part in first example: `factoryMethod`, `parameters`, or `propertiesAndFields` then
the default convention will be applied for the omitted part.

To define the `parameters` and `propertiesAndFields` parts DryIoc provides the corresponding `Parameters` and `PropertiesAndFields` static classes. They may be defined in a more simple way as following:

```cs md*/
public class The_spec_Parameters
{
    [Test] public void Example()
    {
        var container = new Container();

        // note that the dependency registered with the implementation type, but Foo requires the interface...
        container.Register<TestDependency>();

        // so let's specify this with the `requiredServiceType`
        container.Register<IFoo, Foo>(
            made: Parameters.Of.Type<IDependency>(requiredServiceType: typeof(TestDependency)));
    }

    public interface IFoo {}
    public class Foo : IFoo 
    {
        public Foo(IDependency dependency) {}
    }
    public interface IDependency {}
    public class TestDependency : IDependency {}
}
/*md
```

__Note:__ The `Parameters` and `PropertiesAndFields` are implicitly convertible to the `Made` class, so use whatever is more convenient for you.

To specify multiple parameters or properties/fields just chain the `Type`, `Name`, and the more low-level `Details` extension methods:
```cs md*/
public class The_spec_chain
{
    [Test] public void Example()
    {
        var container = new Container();

        container.Register<Foo>(
            made: Parameters.Of

                .Details((req, paramInfo) => paramInfo.ParameterType.IsAssignableTo<IDisposable>()
                    ? ServiceDetails.Of(IfUnresolved.ReturnDefault)
                    : null) // the `null` means to use the default parameter resolution

                .Name("parameter2", serviceKey: "p2")

                .Type<IDependency>(serviceKey: SomeKind.In));
    }

    public enum SomeKind { In, Out }

    public class Foo 
    {
        public Foo(IDisposableResource parameter1, int parameter2, IDependency parameter3) {}
    }

    public interface IDisposableResource : IDisposable {}
    public interface IDependency {}
}
/*md
```

## Default conventions

DryIoc uses following default conventions if you are not specifying resolution details:

- Constructor and factory method parameters are injected based on Service Type equal to the `ParameterType` with `IfUnresolved.Throw` policy.
- __If not explicitly specified Properties and Fields are not injected at all__. If the `PropertiesAndFields.Auto` specified - then all assignable and non primitive properties and fields are injected with `IfUnresolved.ReturnDefault` policy.
- Primitive parameter and property/field types are treated the same as a normal service types: e.g. DryIoc does not forbid registering of `bool` or `int` services.


## Complete example of matching the parameter name to the service key

### The problem

I am trying to figure out how to get DryIoc to resolve ITest in ExampleClass?
This means the matching of the parameter name to the service key as there are multiple registrations to locate the correct service.

```cs md*/
public class Match_the_parameter_name_to_the_service_key
{
    public interface ITest { }
    public class A : ITest { }
    public class B : ITest { }
    public class ExampleClass
    {
        public ExampleClass(ITest a, ITest b) {}
    }

    [Test]
    public void Problem()
    {
        var container = new Container();
        
        container.Register<ITest, A>(serviceKey: "a");
        container.Register<ITest, B>(serviceKey: "b");

        container.Register<ExampleClass>();


        var ex = Assert.Throws<ContainerException>(() =>
        container.Resolve<ExampleClass>());

        // Throws the 'Unable to resolve ITest as parameter "a"'
        Assert.AreEqual(Error.NameOf(Error.UnableToResolveFromRegisteredServices), ex.ErrorName);
    }

/*md
```

### The solution 

Use `Parameters.Of`

```cs md*/
    [Test]
    public void Solution()
    { 
        var c = new Container();
        c.Register<ITest, A>(serviceKey: "a");
        c.Register<ITest, B>(serviceKey: "b");

        c.Register<ExampleClass>(made:
            Made.Of(parameters: Parameters.Of
                .Name("a", serviceKey: "a")
                .Name("b", serviceKey: "b")));

        var example = c.Resolve<ExampleClass>();
        Assert.IsNotNull(example);
    }
/*md
```
You may also omit the `Made.Of(parameters:` because `ParameterSelector` returned by `Parameters.Of` is implicitly convertible to `Made`:

```cs md*/
    [Test]
    public void Solution_drop_MadeOf_part()
    { 
        var c = new Container();
        c.Register<ITest, A>(serviceKey: "a");
        c.Register<ITest, B>(serviceKey: "b");

        c.Register<ExampleClass>(made: Parameters.Of // drop Made.Of
            .Name("a", serviceKey: "a")
            .Name("b", serviceKey: "b"));

        Assert.IsNotNull(c.Resolve<ExampleClass>());
    }
/*md
```

**Note:** You may chain single or multiple parameter selectors for all or some of the parameters producing the final selector. If some parameter from a constructor is omitted then it will have the default rules applied.

**Btw,** If you need to specify arguments by type use `.Type` instead of `.Name`.

You may apply more generic matching of the parameter name to service key without explicitly listing the parameters, but it will be more fragile given you will add non-keyed parameter later:

```cs md*/
    [Test]
    public void Solution_matching_all_registration_parameters()
    { 
        var c = new Container();
        c.Register<ITest, A>(serviceKey: "a");
        c.Register<ITest, B>(serviceKey: "b");

        c.Register<ExampleClass>(made: Parameters.Of.Details(
            (req, parInfo) => ServiceDetails.Of(serviceKey: parInfo.Name)));

        Assert.IsNotNull(c.Resolve<ExampleClass>());
    }
/*md
```

Another type-safe option is directly specifying the constructor via delegate expression (`Linq.Expressions.Expression<T>`) describing its positional arguments - this option will inform you with compilation error when the constructor is changed:

```cs md*/
    [Test]
    public void Solution_with_strongly_typed_parameters()
    { 
        var c = new Container();
        c.Register<ITest, A>(serviceKey: "a");
        c.Register<ITest, B>(serviceKey: "b");

        c.Register<ExampleClass>(made: Made.Of(() =>
            new ExampleClass(
                Arg.Of<ITest>("a"),
                Arg.Of<ITest>("b"))));

        Assert.IsNotNull(c.Resolve<ExampleClass>());
    }
/*md
```

The above ways applied on the specific registration, but the same may be done on Container level using Rules:

```cs md*/
    [Test]
    public void Solution_with_the_rule_applied_on_container_level()
    { 
        var c = new Container(rules =>
            rules.With(parameters:
                Parameters.Of.Details(
                    (req, parInfo) => req.ServiceType == typeof(ExampleClass) 
                        ? ServiceDetails.Of(serviceKey: parInfo.Name) 
                        : null)));

        c.Register<ITest, A>(serviceKey: "a");
        c.Register<ITest, B>(serviceKey: "b");
        c.Register<ExampleClass>();

        Assert.IsNotNull(c.Resolve<ExampleClass>());
    }
}
/*md
```
md*/
