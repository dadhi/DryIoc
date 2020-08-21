<!--Auto-generated from .cs file, the edits here will be lost! -->

# Parameter and Property injection

## Matching the parameter name with the registration service key

### The problem

I am trying to figure out how to get DryIoc to resolve ITest in ExampleClass?
This means the matching of the parameter name to the service key as there are multiple registrations to locate the correct service.

<details>
<summary><code>using...</code></summary>

```cs 
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedTypeParameter
```

</details>

```cs 
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

```

### The solution 

Use `Parameters.Of` https://www.fuget.org/packages/DryIoc.dll/4.2.5/lib/netstandard2.0/DryIoc.dll/DryIoc/Parameters

```cs 
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
```
You may also omit the `Made.Of(parameters:` because `ParameterSelector` returned by `Parameters.Of` is implicitly convertible to `Made`:

```cs 
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
```

**Note:** You may chain single or multiple parameter selectors for all or some of the parameters producing the final selector. If some parameter from a constructor is omitted then it will have the default rules applied.

**Btw,** If you need to specify arguments by type use `.Type` instead of `.Name`.

You may apply more generic matching of the parameter name to service key without explicitly listing the parameters, but it will be more fragile given you will add non-keyed parameter later:

```cs 
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
```

Another type-safe option is directly specifying the constructor via delegate expression (`Linq.Expressions.Expression<T>`) describing its positional arguments - this option will inform you with compilation error when the constructor is changed:

```cs 
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
```

The above ways applied on the specific registration, but the same may be done on Container level using Rules:

```cs 
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
```

<details>
<summary><code>closing...<code></summary>
}
</summary>
