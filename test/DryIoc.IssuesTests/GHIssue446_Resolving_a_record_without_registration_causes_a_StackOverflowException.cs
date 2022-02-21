using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue446_Resolving_a_record_without_registration_causes_a_StackOverflowException
    {
        [Test]
        public void Test1()
        {
            var container = new Container(Rules.Default.WithAutoConcreteTypeResolution());
            var foo = container.Resolve<Foo>();
            Assert.IsInstanceOf<Foo>(foo);
        }

        [Test]
        public void Test2()
        {
            var container = new Container(Rules.Default.WithConcreteTypeDynamicRegistrations());
            var foo = container.Resolve<Foo>();
            Assert.IsInstanceOf<Foo>(foo);
        }

        //record Foo;
        class Foo
        {
            public Foo() {}
            protected Foo(Foo f) {}
        }
    }
}
