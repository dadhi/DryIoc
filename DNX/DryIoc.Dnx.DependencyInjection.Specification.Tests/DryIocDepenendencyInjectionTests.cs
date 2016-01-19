using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace DryIoc.Dnx.DependencyInjection.Specification.Tests
{
    public class DryIocDepenendencyInjectionTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return new Container().GetDryIocServiceProvider(serviceCollection);
        }
    }
}
