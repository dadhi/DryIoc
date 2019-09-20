using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue142_Request_stack
    {
        [Test]
        public void Should_reuse_the_request_instance_for_the_second_dependency()
        {
            var c = new Container();

            c.Register<A>();
            c.Register<B1>();
            c.Register<B2>();

            var a = c.Resolve<A>();

            Assert.IsNotNull(a);
        }

        public class A
        {
            public B1 B1 { get; }
            public B2 B2 { get; }

            public A(B1 b1, B2 b2)
            {
                B1 = b1;
                B2 = b2;
            }
        }

        public class B1 { }

        public class B2 { }
    }
}
