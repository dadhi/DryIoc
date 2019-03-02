using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue278_Arg_Of_does_not_recognize_service_key_of_non_primitive_type
    {
        [Test]
        public void Test()
        {
            var c = new Container();
            
            c.RegisterInstance(false, serviceKey: "skipAuthz");

            var accessAll = new object();
            c.RegisterInstance(true, serviceKey: accessAll);
            c.Register(Made.Of(() => new A(Arg.Of<bool>(accessAll))));

            Assert.DoesNotThrow(() => c.Resolve<A>());
        }

        public class A
        {
            public A(bool hasAccess) {}
        }
    }
}
