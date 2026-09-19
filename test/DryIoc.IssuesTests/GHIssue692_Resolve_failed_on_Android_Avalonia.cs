using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue692_Resolve_failed_on_Android_Avalonia : ITest
{
    public int Run()
    {
        ResolveMany_should_propagate_exception_from_throwing_singleton();
        ResolveMany_should_propagate_exception_from_throwing_singleton_second_time();
        ResolveMany_should_propagate_exception_WithoutUseInterpretation();
        return 3;
    }

    public interface IService { }

    public class ServiceOk : IService { }

    public class ServiceThrowing : IService
    {
        public ServiceThrowing() => throw new InvalidOperationException("Constructor throws");
    }

    [Test]
    public void ResolveMany_should_propagate_exception_from_throwing_singleton()
    {
        var container = new Container();
        container.Register<IService, ServiceOk>(Reuse.Singleton);
        container.Register<IService, ServiceThrowing>(Reuse.Singleton);

        // The original exception from ServiceThrowing constructor should be propagated, not a MissingMethodException
        Assert.Throws<InvalidOperationException>(() =>
            container.Resolve<IEnumerable<IService>>());
    }

    [Test]
    public void ResolveMany_should_propagate_exception_from_throwing_singleton_second_time()
    {
        var container = new Container();
        container.Register<IService, ServiceOk>(Reuse.Singleton);
        container.Register<IService, ServiceThrowing>(Reuse.Singleton);

        Assert.Throws<InvalidOperationException>(() =>
            container.Resolve<IEnumerable<IService>>());

        // Second resolve should also propagate the original exception
        Assert.Throws<InvalidOperationException>(() =>
            container.Resolve<IEnumerable<IService>>());
    }

    [Test]
    public void ResolveMany_should_propagate_exception_WithoutUseInterpretation()
    {
        var container = new Container(Rules.Default.WithoutUseInterpretation());
        container.Register<IService, ServiceOk>(Reuse.Singleton);
        container.Register<IService, ServiceThrowing>(Reuse.Singleton);

        Assert.Throws<InvalidOperationException>(() =>
            container.Resolve<IEnumerable<IService>>());

        Assert.Throws<InvalidOperationException>(() =>
            container.Resolve<IEnumerable<IService>>());
    }
}
