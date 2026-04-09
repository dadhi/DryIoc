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
        Test_2_with_Thing_registered_should_appear_in_collection();
        Test_2_IList_should_be_empty();
        Test_2_ICollection_should_be_empty();
        Test_2_array_should_be_empty();
        return 7;
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
        Assert.IsEmpty(registry.Things);
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

    [Test]
    public void Test_2_with_Thing_registered_should_appear_in_collection()
    {
        var rules = Rules.Default.WithConcreteTypeDynamicRegistrations();

        var container = new Container(rules);
        container.Register<Thing>();
        container.Register<MyRegistry>();

        var registry = container.Resolve<MyRegistry>();
        Assert.AreEqual(1, registry.Things.ToArray().Length);
    }

    [Test]
    public void Test_2_IList_should_be_empty()
    {
        var rules = Rules.Default.WithConcreteTypeDynamicRegistrations();

        var container = new Container(rules);
        container.Register<MyRegistryWithList>();

        var registry = container.Resolve<MyRegistryWithList>();
        Assert.IsEmpty(registry.Things);
    }

    [Test]
    public void Test_2_ICollection_should_be_empty()
    {
        var rules = Rules.Default.WithConcreteTypeDynamicRegistrations();

        var container = new Container(rules);
        container.Register<MyRegistryWithCollection>();

        var registry = container.Resolve<MyRegistryWithCollection>();
        Assert.IsEmpty(registry.Things);
    }

    [Test]
    public void Test_2_array_should_be_empty()
    {
        var rules = Rules.Default.WithConcreteTypeDynamicRegistrations();

        var container = new Container(rules);
        container.Register<MyRegistryWithArray>();

        var registry = container.Resolve<MyRegistryWithArray>();
        Assert.IsEmpty(registry.Things);
    }

    public class MyRegistry
    {
        public readonly IEnumerable<Thing> Things;
        public MyRegistry(IEnumerable<Thing> things) => Things = things;
    }

    public class MyRegistryWithList
    {
        public readonly IList<Thing> Things;
        public MyRegistryWithList(IList<Thing> things) => Things = things;
    }

    public class MyRegistryWithCollection
    {
        public readonly ICollection<Thing> Things;
        public MyRegistryWithCollection(ICollection<Thing> things) => Things = things;
    }

    public class MyRegistryWithArray
    {
        public readonly Thing[] Things;
        public MyRegistryWithArray(Thing[] things) => Things = things;
    }

    public class Thing
    {
        public Guid Id { get; set; }
    }
}