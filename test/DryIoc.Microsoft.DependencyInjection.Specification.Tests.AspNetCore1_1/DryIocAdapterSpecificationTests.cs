using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace DryIoc.Microsoft.DependencyInjection.Specification.Tests
{
    public class DryIocAdapterSpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return new Container().WithDependencyInjectionAdapter(serviceCollection).Resolve<IServiceProvider>();
        }

        internal class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection, IList<ServiceDescriptor>, ICollection<ServiceDescriptor>, IEnumerable<ServiceDescriptor>
        {
        }
    }
}
