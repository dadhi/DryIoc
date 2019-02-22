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
          new object[] { ServiceLifetime.Singleton, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { ServiceLifetime.Singleton, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { ServiceLifetime.Singleton, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { ServiceLifetime.Transient, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { ServiceLifetime.Transient, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { ServiceLifetime.Transient, ServiceLifetime.Scoped, typeof(ServiceB) },
          new object[] { ServiceLifetime.Scoped, ServiceLifetime.Singleton, typeof(ServiceB) },
          new object[] { ServiceLifetime.Scoped, ServiceLifetime.Transient, typeof(ServiceB) },
          new object[] { ServiceLifetime.Scoped, ServiceLifetime.Scoped, typeof(ServiceB) },
        };

        [Test, TestCaseSource(nameof(LifetimeCombinations))]
        public void Resolve_single_service_with_multiple_registrations_should_resolve_the_same_way_as_microsoft_di(ServiceLifetime firstLifetime, ServiceLifetime secondLifetime, Type expectedResolveType)
        {
            // arrange
            var collection = new ServiceCollection();
            collection.Add(ServiceDescriptor.Describe(typeof(IService), typeof(ServiceA), firstLifetime));
            collection.Add(ServiceDescriptor.Describe(typeof(IService), typeof(ServiceB), secondLifetime));

            var msProvider = collection.BuildServiceProvider();
            var dryiocProvider = new Container().WithDependencyInjectionAdapter(collection).BuildServiceProvider();

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
