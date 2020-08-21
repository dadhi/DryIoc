/*md
<!--Auto-generated from .cs file, the edits here will be lost! -->

# Parameter and Property injection

## Matching the parameter name with the registration service key

### The problem

I am trying to figure out how to get DryIoc to resolve ITest in ExampleClass?
This means the matching of the parameter name to the service key as there are multiple registrations to locate the correct service.

<details>
<summary><code>using...</code></summary>

```cs md*/
using DryIoc;
using NUnit.Framework;
// ReSharper disable UnusedTypeParameter
/*md
```

</details>

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
    public void Example()
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
}
/*md
```

### The solution 

TBD

md*/