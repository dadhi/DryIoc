using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue248_WithConcreteTypeDynamicRegistrations_condition_gets_called_with_serviceKey_always_null : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            const string serviceKey = "some service key here, probably a cat (meow)";

            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations((_, key) => serviceKey.Equals(key)));

            container.Resolve<A>(serviceKey);

        }

        public class A { }
    }
}
