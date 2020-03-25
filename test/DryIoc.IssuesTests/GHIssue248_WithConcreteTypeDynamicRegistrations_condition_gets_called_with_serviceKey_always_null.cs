using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    [Ignore("fixme")]
    public class GHIssue248_WithConcreteTypeDynamicRegistrations_condition_gets_called_with_serviceKey_always_null
    {
        [Test]
        public void Test()
        {
            const string serviceKey = "some service key here, probably a cat (meow)";

            var container = new Container(rules => rules.WithConcreteTypeDynamicRegistrations((_, s) =>
            {
                Assert.AreEqual(serviceKey, s);
                return true;
            }));

            container.Resolve<A>(serviceKey);

        }
        public class A { }
    }
}
