/*md
<!--Auto-generate from .cs file, the edits here will be lost! -->

## Auto-mocking with a mocking library

For examples below we need to add:
```cs md*/
using NUnit.Framework;
using DryIoc;
using NSubstitute;

using System.Collections.Concurrent;
/*md
```

## Auto-mocking with NSubstitute

Auto-mocking here means that we can setup container to return mocks for unregistred services. 
In example below we will use [NSubstitute](http://nsubstitute.github.io/) library for creating mocks, 
but you can use something else, e.g. Fake-it-easy or Moq.

Let's define some interfaces and classes:
```cs md*/
public interface INotImplementedService { }

public class SomeConsumer
{
    public INotImplementedService Service { get; }
    public SomeConsumer(INotImplementedService service) { Service = service; }
}
/*md
```

Let's test a `SomeConsumer` by mocking its `Service` dependency:
```md*/
class NSubstituteExample
{
    // Let's define a method to configure our container with auto-mocking of interfaces or abstract classes
    private IContainer WithAutoMocking(IContainer container) => 
        container.With(rules => rules.WithUnknownServiceResolvers(request =>
        {
            var serviceType = request.ServiceType;
            if (!serviceType.IsAbstract) // Mock interface or abstract class only.
                return null; 
            return new ReflectionFactory(made: Made.Of(() => Substitute.For(new[] { serviceType }, null)));
        }));

    [Test] public void Mock_via_unknown_service_resolvers()
    {
        var container = WithAutoMocking(new Container());

        container.Register<SomeConsumer>();

        var consumer = container.Resolve<SomeConsumer>();

        Assert.IsInstanceOf<INotImplementedService>(consumer.Service);
    }
}
/*md
```

With above example there is still the problem though - the factory will be created each time when non-registered service is requested.
It may be also problematic if we want the mock to be a _singleton_. Let's fix it by caching the factory in the dictionary:
```md*/
class NSubstituteExample_with_singleton_mocks
{
    public class OtherConsumer
    {
        public INotImplementedService Service { get; }
        public OtherConsumer(INotImplementedService service) { Service = service; }
    }

    readonly ConcurrentDictionary<System.Type, ReflectionFactory> _mockFactories = 
        new ConcurrentDictionary<System.Type, ReflectionFactory>();

    // Let's define a method to configure our container with auto-mocking of interfaces or abstract classes.
    // Optional `reuse` parameter will allow to specify a mock reuse.
    private IContainer WithAutoMocking(IContainer container, IReuse reuse = null) =>
        container.With(rules => rules.WithUnknownServiceResolvers(request =>
        {
            var serviceType = request.ServiceType;
            if (!serviceType.IsAbstract) // Mock interface or abstract class only.
                return null;
            return _mockFactories.GetOrAdd(serviceType, 
                type => new ReflectionFactory(
                    reuse: reuse,
                    made: Made.Of(() => Substitute.For(new[] { type }, null))));
        }));


    [Test] public void Mock_via_unknown_service_resolvers()
    {
        var container = WithAutoMocking(new Container(), Reuse.Singleton);

        container.Register<SomeConsumer>();
        container.Register<OtherConsumer>();

        var consumer1 = container.Resolve<SomeConsumer>();
        var consumer2 = container.Resolve<OtherConsumer>();

        // Verify that `Service` dependency is indeed a singleton in a different consumers
        Assert.AreSame(consumer1.Service, consumer2.Service);
    }
}
/*md
```

## Auto-mocking with 


md*/
