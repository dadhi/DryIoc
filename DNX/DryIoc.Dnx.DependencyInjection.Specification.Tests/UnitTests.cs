using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DryIoc.Dnx.DependencyInjection.Specification.Tests
{
    public class UnitTests
    {
        [Fact]
        public void Can_create_transient_service_registered_as_delegate()
        {
            var services = new ServiceCollection();

            services.AddTransient<A>();
            services.AddTransient(provider => new B(provider.GetRequiredService<A>()));

            var container = new Container().WithDependencyInjectionAdapter();
            container.Populate(services);

            var serviceProvider = container.Resolve<IServiceProvider>();
            var b = serviceProvider.GetRequiredService<B>();
            Assert.NotNull(b);
        }

        [Fact]
        public void Can_create_scoped_service_registered_as_delegate_Without_explicit_opened_scope()
        {
            var services = new ServiceCollection();

            services.AddTransient<A>();
            services.AddScoped(provider => new B(provider.GetRequiredService<A>()));

            var container = new Container().WithDependencyInjectionAdapter();
            container.Populate(services);

            var serviceProvider = container.Resolve<IServiceProvider>();
            var b = serviceProvider.GetRequiredService<B>();
            Assert.NotNull(b);
        }

        [Fact]
        public void Scoped_services_resolved_from_nested_scope_are_different_from_the_outer_scope()
        {
            var services = new ServiceCollection();
            services.AddScoped<A>();

            var serviceProvider = GetDryIocServiceProvider(services);

            A outerA;
            A nestedA1;
            A nestedA2;

            using (var outerScope = serviceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                outerA = outerScope.ServiceProvider.GetRequiredService<A>();
                using (var nestedScope = outerScope.ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
                {
                    nestedA1 = nestedScope.ServiceProvider.GetRequiredService<A>();
                    nestedA2 = nestedScope.ServiceProvider.GetRequiredService<A>();
                }
            }

            Assert.Same(nestedA2, nestedA1);
            Assert.NotSame(outerA, nestedA1);
        }

        private static IServiceProvider GetDryIocServiceProvider(ServiceCollection services, IScopeContext scopeContext = null)
        {
            var container = new Container(scopeContext: scopeContext).WithDependencyInjectionAdapter();
            container.Populate(services);
            return container.Resolve<IServiceProvider>();
        }


        public class A { }

        public class B
        {
            public A A { get; private set; }

            public B(A a)
            {
                A = a;
            }
        }
    }
}
