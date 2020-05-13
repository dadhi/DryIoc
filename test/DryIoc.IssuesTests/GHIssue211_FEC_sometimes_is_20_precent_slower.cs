using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue211_FEC_sometimes_is_20_precent_slower
    {
        [Test]
        public void Should_get_a_singleton_constant_expression_from_the_cache()
        {
            var c = new Container();

            c.Register<A>();
            c.Register<B>(Reuse.Singleton);

            var a = c.Resolve<A>();

            Assert.IsNotNull(a);
        }

        public class A
        {
            public A(B b1, B b2)
            {

            }
        }

        public class B
        {
        }
    }
}
