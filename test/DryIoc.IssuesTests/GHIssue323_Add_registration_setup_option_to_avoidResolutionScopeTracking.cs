using DryIoc.MefAttributedModel;
using NUnit.Framework;
using System;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue323_Add_registration_setup_option_to_avoidResolutionScopeTracking
    {
        [Test]
        public void ExportSingletonPropertyWorks()
        {
            var c = new Container();

            c.Register<A>(setup: Setup.With(openResolutionScope: true, avoidResolutionScopeTracking: true));
            c.Register<B>(Reuse.Scoped);

            var a = c.Resolve<A>();
            a.R.Dispose();
            Assert.True(a.B.IsDisposed);

            var s = c.Resolve<LambdaExpression, A>().ToString();
            StringAssert.Contains(", False)", s);
        }

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