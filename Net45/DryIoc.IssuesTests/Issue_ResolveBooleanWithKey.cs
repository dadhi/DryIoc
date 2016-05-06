using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_ResolveBooleanWithKey
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
