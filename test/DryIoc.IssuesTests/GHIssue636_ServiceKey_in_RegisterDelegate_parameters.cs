using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Reflection;

namespace DryIoc.IssuesTests;

[TestFixture]
public sealed class GHIssue636_ServiceKey_in_RegisterDelegate_parameters : ITest
{
    public int Run()
    {
        Test_feature();
        return 1;
    }

    [Test]
    public void Test_feature()
    {
        var c = new Container(Rules.Default.With(parameters: static request => static par =>
            {
                var attr = par.GetCustomAttribute<KeyedAttribute>();
                return attr != null ? ParameterServiceInfo.Of(par, ServiceDetails.Of(serviceKey: attr.ServiceKey)) : null;
            }));

        c.RegisterInstance("firstInstance", serviceKey: "first");
        c.RegisterInstance("secondInstance", serviceKey: "second");

        c.RegisterDelegate(([Keyed("first")] string first, [Keyed("second")] string second) => first, serviceKey: "test");

        var test = c.Resolve<string>("test");
        Assert.AreEqual("firstInstance", test);
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class KeyedAttribute : Attribute
    {
        public readonly object ServiceKey;
        public KeyedAttribute(object serviceKey) => ServiceKey = serviceKey;
    }
}