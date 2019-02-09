using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

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
    }
}
