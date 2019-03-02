using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue318_RegisterInstance_doesnt_honour_current_OpenScope
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            using (var c = container.OpenScope())
            {
                var impl = new int1impl();
                c.Use<int1>(impl);

                Assert.AreSame(impl, c.Resolve<int1>());
            }

            using (var c = container.OpenScope())
            {
                var impl = new int1impl();
                c.Use<int1>(impl);

                c.Resolve<int1>();
                Assert.AreSame(impl, c.Resolve<int1>());
            }
        }

        public interface int1 {}

        public class int1impl : int1 {}
    }
}
