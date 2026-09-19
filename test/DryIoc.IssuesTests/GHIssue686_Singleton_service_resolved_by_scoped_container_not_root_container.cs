using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue686_Singleton_service_resolved_by_scoped_container_not_root_container : ITest
{
    public int Run()
    {
        Singleton_IResolver_should_be_root_when_first_resolved_via_scope_with_WithoutThrowIfDependencyHasShorterReuseLifespan();
        Singleton_IResolver_should_be_root_when_first_resolved_via_scope_with_default_rules();
        Singleton_IResolver_should_be_root_when_first_resolved_via_root_then_scope();
        Singleton_IContainer_should_be_root_when_first_resolved_via_scope();
        Singleton_lazy_scoped_dependency_via_scope_should_still_work_after_fix();
        return 5;
    }

    public class Service
    {
        public readonly IResolver Resolver;
        public Service(IResolver resolver) => Resolver = resolver;
    }

    public class ServiceWithContainer
    {
        public readonly IContainer Container;
        public ServiceWithContainer(IContainer container) => Container = container;
    }

    public class ServiceWithLazy
    {
        public readonly Lazy<ScopedDep> LazyDep;
        public ServiceWithLazy(Lazy<ScopedDep> lazyDep) => LazyDep = lazyDep;
    }

    public class ScopedDep { }

    [Test]
    public void Singleton_IResolver_should_be_root_when_first_resolved_via_scope_with_WithoutThrowIfDependencyHasShorterReuseLifespan()
    {
        var rules = Rules.Default.WithoutThrowIfDependencyHasShorterReuseLifespan();
        var container = new Container(rules);

        container.Register<Service>(Reuse.Singleton);

        using var scope = container.OpenScope();

        var fromScope = scope.Resolve<Service>();

        // Singleton should always capture the root container as its IResolver, regardless of how it's first resolved
        Assert.AreSame(container, fromScope.Resolver);
    }

    [Test]
    public void Singleton_IResolver_should_be_root_when_first_resolved_via_scope_with_default_rules()
    {
        var container = new Container();

        container.Register<Service>(Reuse.Singleton);

        using var scope = container.OpenScope();

        var fromScope = scope.Resolve<Service>();

        Assert.AreSame(container, fromScope.Resolver);
    }

    [Test]
    public void Singleton_IResolver_should_be_root_when_first_resolved_via_root_then_scope()
    {
        var rules = Rules.Default.WithoutThrowIfDependencyHasShorterReuseLifespan();
        var container = new Container(rules);

        container.Register<Service>(Reuse.Singleton);

        // Resolve from root first
        var fromRoot = container.Resolve<Service>();
        Assert.AreSame(container, fromRoot.Resolver);

        using var scope = container.OpenScope();

        // Then from scope - should return the same singleton with root as resolver
        var fromScope = scope.Resolve<Service>();
        Assert.AreSame(fromRoot, fromScope);
        Assert.AreSame(container, fromScope.Resolver);
    }

    [Test]
    public void Singleton_IContainer_should_be_root_when_first_resolved_via_scope()
    {
        var rules = Rules.Default.WithoutThrowIfDependencyHasShorterReuseLifespan();
        var container = new Container(rules);

        container.Register<ServiceWithContainer>(Reuse.Singleton);

        using var scope = container.OpenScope();

        var fromScope = scope.Resolve<ServiceWithContainer>();

        // Singleton should always capture the root container as its IContainer, regardless of how it's first resolved
        Assert.AreSame(container, fromScope.Container);
    }

    [Test]
    public void Singleton_lazy_scoped_dependency_via_scope_should_still_work_after_fix()
    {
        // This test verifies that GHIssue378 behavior still works correctly after the fix.
        // A singleton with Lazy<ScopedDep> should still be able to resolve the scoped dependency
        // through the scope when the lazy is evaluated.
        var rules = Rules.Default.WithoutThrowIfDependencyHasShorterReuseLifespan();
        var container = new Container(rules);

        container.Register<ServiceWithLazy>(Reuse.Singleton);
        container.Register<ScopedDep>(Reuse.Scoped);

        using var scope = container.OpenScope();

        var service = scope.Resolve<ServiceWithLazy>();

        // The lazy should resolve ScopedDep from the current scope
        Assert.DoesNotThrow(() => { var dep = service.LazyDep.Value; });
    }
}
