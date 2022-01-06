using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue446_Resolving_a_record_without_registration_causes_a_StackOverflowException
    {
        //[Test]
        public void Test()
        {
            var container = new Container(Rules.Default.WithAutoConcreteTypeResolution());
            //container.Register<Foo>();
            container.Resolve<Foo>();
        }

        record Foo;
    }
}
