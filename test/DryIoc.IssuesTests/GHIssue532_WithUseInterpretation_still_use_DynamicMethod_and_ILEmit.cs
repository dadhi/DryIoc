using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue532_WithUseInterpretation_still_use_DynamicMethod_and_ILEmit : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

        [Test]
        public void Test()
        {
            var c = new Container(Rules.Default.WithUseInterpretation());

            c.Register<R>();
            c.Register<A>(Reuse.Scoped);
            c.Register<B>(Reuse.Singleton);

            using var s = c.OpenScope();
            var r0 = s.Resolve<R>();
            Assert.IsNotNull(r0.A);
            var r1 = s.Resolve<R>();
            Assert.IsNotNull(r1.A);
            var r2 = s.Resolve<R>();
            Assert.IsNotNull(r2.A);
        }

        class R
        {
            public readonly A A;
            public R(A a) => A = a;
        }

        class A
        {
            public readonly B B;
            public A(B b) => B = b;
        }

        class B {}
    }
}