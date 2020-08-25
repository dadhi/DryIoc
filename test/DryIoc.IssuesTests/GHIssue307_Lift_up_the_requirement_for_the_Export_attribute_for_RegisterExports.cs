using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue307_Lift_up_the_requirement_for_the_Export_attribute_for_RegisterExports
    {
        [Test]
        public void Test() 
        {
            var c = new Container();

            c.RegisterExportsAndTypes(typeof(A), typeof(B));

            var a = c.Resolve<A>();
            Assert.IsNotNull(a);
        }
        

        public class A 
        {
            public A(B b) {}

        }

        [Export]
        public class B {}
    }
}