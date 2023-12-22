using NUnit.Framework;
using System;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue323_Add_registration_setup_option_to_avoidResolutionScopeTracking : ITest
    {
        public int Run()
        {
            ExportSingletonPropertyWorks();
            return 1;
        }

        [Test]
        public void ExportSingletonPropertyWorks()
        {
            var c = new Container();

            c.Register<A>(setup: Setup.With(openResolutionScope: true, avoidResolutionScopeTracking: true));
            c.Register<B>(Reuse.Scoped);

            var a = c.Resolve<A>();
            a.R.Dispose();
            Assert.True(a.B.IsDisposed);

            var e = c.Resolve<LambdaExpression, A>();
            var s = e.ToString();
            StringAssert.Contains("false).Resolve", s);
        }

        public override int GetHashCode() => base.GetHashCode();
        public override string ToString() => base.ToString();

        class A
        {
            public B B;
            public IResolverContext R;
            public A(B b, IResolverContext r)
            {
                B = b;
                R = r;
            }
        }

        class B : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }
    }
}