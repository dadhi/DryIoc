<!--Auto-generated from .cs file, the edits here will be lost! -->

# Auto-mocking with a mocking library


- [Auto-mocking with a mocking library](#auto-mocking-with-a-mocking-library)
  - [Auto-mocking with NSubstitute](#auto-mocking-with-nsubstitute)
  - [Auto-mocking with Moq](#auto-mocking-with-moq)


For examples below we need to add:

<details><summary><strong>usings ...</strong></summary>

 ```cs
namespace DryIoc.Docs;
using System;
using System.Collections.Concurrent;
using NUnit.Framework;
using DryIoc;
using NSubstitute;
using Moq;
using static DryIoc.ImTools.ArrayTools;
 ```
</details>



## Auto-mocking with NSubstitute

Auto-mocking here means that we can setup container to return mocks for unregistered services. 
In example below we will use [NSubstitute](http://nsubstitute.github.io/) library for creating mocks, 
but you can use something else, e.g. Fake-it-easy or Moq.

Let's define some interfaces and classes:
```cs 
public interface INotImplementedService { }

public class SomeConsumer
{
    public INotImplementedService Service { get; }
    public SomeConsumer(INotImplementedService service) { Service = service; }
}
```

Let's test a `SomeConsumer` by mocking its `Service` dependency:
```cs 
public class NSubstitute_example
{
    readonly ConcurrentDictionary<System.Type, DynamicRegistration> _mockRegistrations =
        new ConcurrentDictionary<System.Type, DynamicRegistration>();

    // Let's define a method to configure our container with auto-mocking of interfaces or abstract classes.
    // Optional `reuse` parameter will allow to specify a mock reuse.
    private IContainer WithAutoMocking(IContainer container, IReuse reuse = null) =>
        container.With(rules => rules.WithDynamicRegistration((serviceType, serviceKey) =>
        {
            if (!serviceType.IsAbstract) // Mock interface or abstract class only.
                return null;

            if (serviceType.IsGenericTypeDefinition)
                return null; // we cannot mock open-generic types - we need something concrete here

            var d = _mockRegistrations.GetOrAdd(serviceType,
                type => new DynamicRegistration(
                    DelegateFactory.Of(r => Substitute.For(new[] { serviceType }, Empty<object>()), reuse)));

            return new[] { d };
        },
        DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback));

    [Test]
    public void Example()
    {
        var container = WithAutoMocking(new Container());

        container.Register<SomeConsumer>();

        var consumer = container.Resolve<SomeConsumer>();

        Assert.IsInstanceOf<INotImplementedService>(consumer.Service);
    }
}
```

With above example there is still the problem though - the factory will be created each time when non-registered service is requested.
It may be also problematic if we want the mock to be a _singleton_. Let's fix it by caching the factory in the dictionary:
```cs 

public class NSubstitute_example_with_singleton_mocks
{
    readonly ConcurrentDictionary<System.Type, DynamicRegistration> _mockRegistrations =
        new ConcurrentDictionary<System.Type, DynamicRegistration>();

    // Let's define a method to configure our container with auto-mocking of interfaces or abstract classes.
    // Optional `reuse` parameter will allow to specify a mock reuse.
    private IContainer WithAutoMocking(IContainer container, IReuse reuse = null) =>
        container.With(rules => rules.WithDynamicRegistration((serviceType, serviceKey) =>
        {
            if (!serviceType.IsAbstract) // Mock interface or abstract class only.
                return null;

            if (serviceType.IsGenericTypeDefinition)
                return null; // we cannot mock open-generic types - we need something concrete here

            var d = _mockRegistrations.GetOrAdd(serviceType,
                type => new DynamicRegistration(
                    DelegateFactory.Of(r => Substitute.For(new[] { serviceType }, Empty<object>()), reuse)));

            return new[] { d };
        },
        DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback));

    [Test]
    public void Example()
    {
        var container = WithAutoMocking(new Container(), Reuse.Singleton);

        container.Register<SomeConsumer>();
        container.Register<OtherConsumer>();

        var consumer1 = container.Resolve<SomeConsumer>();
        var consumer2 = container.Resolve<OtherConsumer>();

        // Verify that `Service` dependency is indeed a singleton in a different consumers
        Assert.AreSame(consumer1.Service, consumer2.Service);
    }

    public class OtherConsumer
    {
        public INotImplementedService Service { get; }
        public OtherConsumer(INotImplementedService service) { Service = service; }
    }

    [Test]
    public void Example_of_mocking_the_open_generic_dependency()
    {
        var container = WithAutoMocking(new Container(), Reuse.Singleton);

        container.Register<Foo>();

        var consumer = container.Resolve<Foo>();
    }

    public interface IOpenGenericDependency<T> { }
    public class Foo
    {
        public readonly IOpenGenericDependency<int> Dependency;
        public Foo(IOpenGenericDependency<int> dependency) => Dependency = dependency;
    }
}
```

## Auto-mocking with Moq

Let's implement auto-mocking with popular [Moq library](https://github.com/moq/moq).

We will create a testing container which dynamically provides the mock implementation for any interface or abstract class 
and automatically will resolve any concrete class. We will use the `DynamicRegistrationProvider` feature for those and `CreateChild` 
container to detach from the production container but provide the access to its services.

```cs 
public class Moq_example_with_test_container
{
    [Test]
    public void Example()
    {
        var prodContainer = new Container();

        using (var container = CreateTestContainer(prodContainer))
        {
            container.Register<UnitOfWork>(Reuse.Singleton);

            // Arrangements
            const bool expected = true;

            container.Resolve<Mock<IDep>>()
                .Setup(instance => instance.Method())
                .Returns(expected);

            // Get concrete type instance of tested unit 
            // all dependencies are fulfilled with mocked instances
            var unit = container.Resolve<UnitOfWork>();

            // Action
            var actual = unit.InvokeDep();

            // Assertion
            Assert.AreEqual(expected, actual);
            container.Resolve<Mock<IDep>>()
                .Verify(instance => instance.Method());
        }
    }

    public static IContainer CreateTestContainer(IContainer container)
    {
        var c = container.CreateChild(IfAlreadyRegistered.Replace,
            container.Rules.WithDynamicRegistration((serviceType, serviceKey) =>
            {
                // ignore services with non-default key
                if (serviceKey != null)
                    return null;

                if (serviceType == typeof(object))
                    return null;

                // get the Mock object for the abstract class or interface
                if (serviceType.IsInterface || serviceType.IsAbstract)
                {
                    // except for the open-generic ones
                    if (serviceType.IsGenericType && serviceType.IsOpenGeneric())
                        return null;

                    var mockType = typeof(Mock<>).MakeGenericType(serviceType);

                    var mockFactory = DelegateFactory.Of(r => ((Mock)r.Resolve(mockType)).Object, Reuse.Singleton);

                    return new[] { new DynamicRegistration(mockFactory, IfAlreadyRegistered.Keep) };
                }

                // concrete types
                var concreteTypeFactory = serviceType.ToFactory(Reuse.Singleton, FactoryMethod.ConstructorWithResolvableArgumentsIncludingNonPublic);

                return new[] { new DynamicRegistration(concreteTypeFactory) };
            },
            DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback));

        c.Register(typeof(Mock<>), Reuse.Singleton, FactoryMethod.DefaultConstructor());

        return c;
    }

    public interface IDep
    {
        bool Method();
    }

    public class Dep1 : IDep
    {
        public bool Method() => true;
    }

    public class UnitOfWork : IDisposable
    {
        public readonly IDep Dep;
        public UnitOfWork(IDep d) => Dep = d;
        public void Dispose() { }

        public bool InvokeDep() => Dep.Method();
    }
}
```
