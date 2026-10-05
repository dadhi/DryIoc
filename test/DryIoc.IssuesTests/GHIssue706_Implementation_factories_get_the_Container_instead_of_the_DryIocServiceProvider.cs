using NUnit.Framework;
using System;
using Microsoft.Extensions.DependencyInjection;
using DryIoc.Microsoft.DependencyInjection;

namespace DryIoc.IssuesTests;

[TestFixture]
public class GHIssue706_Implementation_factories_get_the_Container_instead_of_the_DryIocServiceProvider : ITest
{
    public int Run()
    {
        Original_case();
        return 1;
    }

    [Test]
    public void Original_case()
    {
        const string key = "key";
        var services = new ServiceCollection();
        services.AddKeyedSingleton(key, (_, _) => new ExampleA());
        services.AddSingleton(sp =>
        {
            Assert.IsInstanceOf<DryIocServiceProvider>(sp);
            Assert.IsNotNull(sp.GetKeyedService<ExampleA>(key));
            return new ExampleB();
        });

        var provider = new DryIocServiceProviderFactory().CreateBuilder(services);
        Assert.IsNotNull(provider.GetRequiredService<ExampleB>());
    }

    class ExampleA { }
    class ExampleB { }
}
