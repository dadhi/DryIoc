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

            bool hasAccess = true;
            c.RegisterInstance(hasAccess, serviceKey: "hasAccess");
            c.Register(Made.Of(() => new A(Arg.Of<bool>("hasAccess"))));

            c.Resolve<A>();

        }

        public class A
        {
            public A(bool hasAccess)
            {
                
            }
        }
    }
}
