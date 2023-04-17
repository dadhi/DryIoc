using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class DefaultReuseTest : ITest
    {
        public int Run()
        {
            Default_scoped_reuse();
            return 1;
        }

        [Test]
        public void Default_scoped_reuse()
        {
            var container = new Container(rules => rules.WithDefaultReuse(Reuse.Scoped));

            container.Register<ScopeService>();

            using (var scope = container.OpenScope())
            {
                var oSingleton = scope.Resolve<ScopeService>();

                var oSingleton2 = scope.Resolve<ScopeService>();

                Assert.AreSame(oSingleton, oSingleton2);
            }
        }

        class ScopeService
        {
        }
    }
}
