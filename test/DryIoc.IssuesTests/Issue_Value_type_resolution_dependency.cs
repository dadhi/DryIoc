using NUnit.Framework;
using Example;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue_Value_type_resolution_dependency : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register<Dad>();
            c.Register<Major>(setup: Setup.With(asResolutionCall: true));
            c.Register<Profit>();

            var d = c.Resolve<Dad>();

            Assert.IsNotNull(d.M.P);
        }

        public class Dad 
        {
            public readonly Major M;
            public Dad(Major m) => M = m;
        }

        public struct Major 
        {
            public readonly Profit P;
            public Major(Profit p) => P = p;
        }

        public class Profit {}
    }
}
