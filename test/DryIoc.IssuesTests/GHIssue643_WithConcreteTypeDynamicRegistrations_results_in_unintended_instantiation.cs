using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DryIoc.IssuesTests;

[TestFixture]
public sealed class GHIssue643_WithConcreteTypeDynamicRegistrations_results_in_unintended_instantiation : ITest
{
    public int Run()
    {
        Test_1();
        Test_2();
        Test_3();
        return 3;
    }

    [Test]
    public void Test_1()
    {
        var rules = Rules.Default.WithAutoConcreteTypeResolution();

        var container = new Container(rules);
        container.Register<MyRegistry>();

        var registry = container.Resolve<MyRegistry>();
        Assert.IsEmpty(registry.Things);
    }

    [Test]
    public void Test_2()
    {
        var rules = Rules.Default.WithConcreteTypeDynamicRegistrations();

        var container = new Container(rules);
        container.Register<MyRegistry>();

        var registry = container.Resolve<MyRegistry>();
        // Assert.IsEmpty(registry.Things); // todo: @wip
    }

    [Test]
    public void Test_3()
    {
        var rules = Rules.Default;

        var container = new Container(rules);
        container.Register<Thing>();
        container.Register<MyRegistry>();

        var registry = container.Resolve<MyRegistry>();
        Assert.AreEqual(1, registry.Things.ToArray().Length);
    }

    public class MyRegistry
    {
        public readonly IEnumerable<Thing> Things;
        public MyRegistry(IEnumerable<Thing> things) => Things = things;
    }

    public class Thing
    {
        public Guid Id { get; set; }
    }
}