using System;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue109_Using_Func_wrapper_and_FastExpressionCompiler
    {
        [Test]
        public void Should_be_able_to_resolve_Func()
        {
            using (var c = new Container(Rules.Default.WithoutFastExpressionCompiler()))
            {
                c.Register(Made.Of(() => new CancellationTokenSource()), Reuse.Singleton);
                c.Register(Made.Of(r => ServiceInfo.Of<CancellationTokenSource>(), 
                    cts => cts.Token), Reuse.Singleton);

                c.Register<IAlpha, Alpha>();

                var f = c.Resolve<Func<IAlpha>>();
                var a = f();
                Assert.IsNotNull(a);
            }
        }

        [Test]
        public void Should_be_able_to_resolve_Func_of_scoped()
        {
            using (var c = new Container(Rules.Default.WithoutFastExpressionCompiler()))
            {
                c.Register(Made.Of(() => new CancellationTokenSource()), Reuse.Scoped);
                c.Register(Made.Of(r => ServiceInfo.Of<CancellationTokenSource>(),
                    cts => cts.Token), Reuse.Scoped);

                c.Register<IAlpha, Alpha>();

                using (var scope = c.OpenScope())
                {
                    var a = scope.Resolve<Func<IAlpha>>().Invoke();
                    Assert.IsNotNull(a);
                }
            }
        }

        private interface IAlpha
        {
            CancellationToken Ct { get; }
        }

        private class Alpha : IAlpha
        {
            public CancellationToken Ct { get; }
            public Alpha(CancellationToken ct)
            {
                Ct = ct;
            }
        }
    }
}
