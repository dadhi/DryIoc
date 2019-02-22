using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification;
using NUnit.Framework;

// uncomment when I want to copy some test here for testing.
//using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
//using Xunit;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    public class DryIocAdapterSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection services) => DryIocAdapter.Create(services);

        internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
        {
        }

        public static object[] LifetimeCombinations =
        {
          new object[] { false, ServiceLifetime.Singleton, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Singleton, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Singleton, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Transient, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Transient, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Transient, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Scoped, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Scoped, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { false, ServiceLifetime.Scoped, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Singleton, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Singleton, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Singleton, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Transient, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Transient, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Transient, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Scoped, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Scoped, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { true, ServiceLifetime.Scoped, ServiceLifetime.Scoped, typeof(ServiceB) },
        };

        [Test, TestCaseSource(nameof(LifetimeCombinations))]
        public void Resolve_single_service_with_multiple_registrations_should_resolve_the_same_way_as_microsoft_di(bool usingScope, ServiceLifetime firstLifetime, ServiceLifetime secondLifetime, Type expectedResolveType)
        {
            // arrange
            var collection = new ServiceCollection();
            collection.Add(ServiceDescriptor.Describe(typeof(IService), typeof(ServiceA), firstLifetime));
            collection.Add(ServiceDescriptor.Describe(typeof(IService), typeof(ServiceB), secondLifetime));

            IServiceProvider msProvider = collection.BuildServiceProvider();
            IServiceProvider dryiocProvider = new Container().WithDependencyInjectionAdapter(collection).BuildServiceProvider();

            if (usingScope)
            {
              msProvider = msProvider.CreateScope().ServiceProvider;
              dryiocProvider = dryiocProvider.CreateScope().ServiceProvider;
            }

            // act
            var msService = msProvider.GetRequiredService<IService>();
            var dryiocService = dryiocProvider.GetRequiredService<IService>();

            // assert
            Assert.IsInstanceOf(expectedResolveType, msService, "Microsoft changed the implementation");
            Assert.IsInstanceOf(expectedResolveType, dryiocService, "DryIoc resolves the requested type different than microsofts di implementation");
        }

        public interface IService
        {
        }

        public class ServiceA : IService
        {
        }

        public class ServiceB : IService
        {
        }
    }
}
