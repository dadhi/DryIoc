using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public sealed class GHIssue631_Conditional_registrations : ITest
{
    public int Run()
    {
        // TestWithDefaultScopedToSingleton(); // todo: @fixme @wip the test
        TestWithIdenticalScopes();
        return 2;
    }

    [Test]
    public void TestWithDefaultScopedToSingleton()
    {
        var container = new Container();

        container.Register<IService, MyServiceA>(
            reuse: Reuse.Transient,
            setup: Setup.With(condition: request => request.DirectParent.ServiceType == typeof(ServiceConsumerA))
        );

        // the intention is having a default resolution if previous conditions aren't satisfied !!
        container.Register<IService, MyServiceB>(
            reuse: Reuse.Singleton
        );

        container.Register<ServiceConsumerA>();
        container.Register<ServiceConsumerB>();

        var consumerA = container.Resolve<ServiceConsumerA>();
        var consumerB = container.Resolve<ServiceConsumerB>();

        Assert.NotNull(consumerA);
        Assert.NotNull(consumerB);

        // here is the mismatch from consumerA.Service type !!.
        Assert.IsInstanceOf<MyServiceA>(consumerA.Service);
        Assert.IsInstanceOf<MyServiceB>(consumerB.Service);
    }

    [Test]
    public void TestWithIdenticalScopes()
    {
        var container = new Container();

        container.Register<IService, MyServiceA>(
            reuse: Reuse.Transient,
            setup: Setup.With(condition: request => request.DirectParent.ServiceType == typeof(ServiceConsumerA))
        );

        // the intention is having a default resolution if previous conditions aren't satisfied !!
        container.Register<IService, MyServiceB>(
            reuse: Reuse.Transient
        );

        container.Register<ServiceConsumerA>();
        container.Register<ServiceConsumerB>();

        var consumerA = container.Resolve<ServiceConsumerA>();
        var consumerB = container.Resolve<ServiceConsumerB>();

        Assert.NotNull(consumerA);
        Assert.NotNull(consumerB);

        // those asserts work, so types match !
        Assert.IsInstanceOf<MyServiceA>(consumerA.Service);
        Assert.IsInstanceOf<MyServiceB>(consumerB.Service);
    }

    public class ServiceConsumerA
    {
        public IService Service { get; private set; }
        public ServiceConsumerA(IService service) => Service = service;
    }

    public class ServiceConsumerB
    {
        public IService Service { get; private set; }
        public ServiceConsumerB(IService service) => Service = service;
    }

    public class MyServiceA : IService { }

    public class MyServiceB : IService { }

    public interface IService { }
}