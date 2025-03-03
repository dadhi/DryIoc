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
            registrator.Register<Context>(reuse: Reuse.Scoped);
            registrator.Register<Strategy>();
            registrator.Register<ServiceC<Strategy>>();
            registrator.Register<ServiceB>();
            registrator.Register<ServiceA>();
        }
    }

    public class Context
    {
        public string Value { get; set; }
    }

    public class Strategy
    {
        public Strategy(Context context, IResolver resolver)
        {
            var resolvedContext = resolver.Resolve<Context>();
        }
    }

    public class ServiceC<TContext> : IDisposable
    {
        public ServiceC(Strategy strategy, Context context)
        {
        }

        public void Dispose()
        {
        }
    }

    public class ServiceB
    {
        private readonly IContainer _container;
        private readonly Context _context;

        public ServiceB(IContainer container, Context context)
        {
            _container = container;
            _context = context;
        }

        public void Do()
        {
            using var scope = _container.OpenScope();
            scope.Use(_context);

            var context = scope.Resolve<Context>();

            // here context.value is "value"
            var anotherContext = scope.Resolve<Strategy>();

            // here context.value is null
            // here resolvedContext.value is "value" - resolvedContext is resolved from injected IResolver
            using var serviceC = scope.Resolve<ServiceC<Strategy>>();
        }
    }

    public class ServiceA
    {
        private readonly IContainer _container;
        private readonly Context _context;
        private readonly ServiceB _serviceB;

        public ServiceA(IContainer container, Context context, ServiceB serviceB)
        {
            _container = container;
            _context = context;
            _serviceB = serviceB;
        }

        public void Do()
        {
            using var serviceC = _container.Resolve<ServiceC<Strategy>>(); // this cause the issue, please comment this line to resolve problem

            _context.Value = "value";

            _serviceB.Do();
        }
    }
}
