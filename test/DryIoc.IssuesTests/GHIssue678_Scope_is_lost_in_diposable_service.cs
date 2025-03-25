using NUnit.Framework;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue678_Scope_is_lost_in_diposable_service : ITest
{
    public int Run()
    {
        Original_case();
        return 1;
    }

    [Test]
    public void Original_case()
    {
        IServiceProvider serviceProvider = null;

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                serviceProvider = new Container(
                    // NOTE: You don't need to specify the rules here, 
                    // because this rule is the part of `WithDependencyInjectionAdapter` configuration 
                    //
                    // rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments)
                    )
                    .WithDependencyInjectionAdapter(services)
                    // NOTE: you need this line (for now) to obtain the Container from the DryIocServiceProvider 
                    // in the DryIoc.MS.DI v8.0.0-preview-04. 
                    // I will add the new ConfigureServiceProvider overload for the DryIocServiceProvider into the later version, 
                    // so you don't need to change your code.
                    .Container
                    .ConfigureServiceProvider<CompositionRoot>();
            })
            .Build();

        using var scope = serviceProvider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ServiceA>();
        service.Do();
    }

    public class CompositionRoot
    {
        public CompositionRoot(IRegistrator registrator)
        {
            registrator.Register<SomeValueContext>(reuse: Reuse.Scoped);
            registrator.Register<Strategy>();
            registrator.Register<ServiceC<Strategy>>();
            registrator.Register<ServiceB>();
            registrator.Register<ServiceA>();
        }
    }

    public class SomeValueContext
    {
        public string Value { get; set; }
    }

    public class Strategy
    {
        public SomeValueContext ContextFromConstructor { get; }
        public SomeValueContext ContextResolveWithInjectedResolver { get; }
        public Strategy(SomeValueContext context, IResolver resolver)
        {
            ContextFromConstructor = context;
            ContextResolveWithInjectedResolver = resolver.Resolve<SomeValueContext>();
        }
    }

    public class ServiceC<TContext> : IDisposable
    {
        public Strategy Strategy { get; }
        public SomeValueContext Contexto { get; }
        public ServiceC(Strategy strategy, SomeValueContext contexto)
        {
            Strategy = strategy;
            Contexto = contexto;
        }

        public void Dispose() { }
    }

    public class ServiceB
    {
        private readonly IResolverContext _resolver;
        private readonly SomeValueContext _contexto;
        public ServiceB(IResolverContext resolver, SomeValueContext contexto)
        {
            _resolver = resolver;
            _contexto = contexto;
        }

        public void Do()
        {
            using var scope = _resolver.OpenScope();
            scope.Use(_contexto);

            var context = scope.Resolve<SomeValueContext>();
            Assert.AreEqual("value", context.Value);

            var strategy = scope.Resolve<Strategy>();
            Assert.AreEqual("value", strategy.ContextResolveWithInjectedResolver.Value);
            Assert.AreEqual("value", strategy.ContextFromConstructor.Value);

            // The problem here is because container uses cached expression for `ServiceC<Strategy>` 
            // which is contain dependency of `SomeValueContext` created via `GetOrAddViaFactoryDelegate`,
            // which is in turn does not checs the Used items!!!
            // todo: @wip @fixme
            using var serviceC = scope.Resolve<ServiceC<Strategy>>();
            Assert.AreEqual("value", serviceC.Strategy.ContextResolveWithInjectedResolver.Value);
            Assert.AreEqual("value", serviceC.Strategy.ContextFromConstructor.Value);
            Assert.AreEqual("value", serviceC.Contexto.Value);
        }
    }

    public class ServiceA
    {
        private readonly IResolver _resolver;
        private readonly SomeValueContext _contexto;
        private readonly ServiceB _serviceB;

        public ServiceA(IResolver resolver, SomeValueContext contexto, ServiceB serviceB)
        {
            _resolver = resolver;
            _contexto = contexto;
            _serviceB = serviceB;
        }

        public void Do()
        {
            // This resolution causes the issue, because it caches the expression with the dependency for creating the `SomeValueContext`
            // using var serviceC = _resolver.Resolve<ServiceC<Strategy>>();

            _contexto.Value = "value";

            _serviceB.Do();
        }
    }
}
