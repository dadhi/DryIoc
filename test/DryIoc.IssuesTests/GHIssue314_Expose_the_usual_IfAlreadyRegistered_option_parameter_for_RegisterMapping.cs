using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue314_Expose_the_usual_IfAlreadyRegistered_option_parameter_for_RegisterMapping : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var container = new Container();

            container.Register<LeafOne>(Reuse.Singleton);
            container.Register<LeafTwo>(Reuse.Singleton);

            container.RegisterMapping<Base, LeafOne>();
            container.RegisterMapping<Base, LeafTwo>();

            var bases = container.ResolveMany<Base>().ToArray();
            Assert.AreEqual(2, bases.Length);
        }

        class Base { };
        class LeafOne : Base { };
        class LeafTwo : Base { };
    }
}