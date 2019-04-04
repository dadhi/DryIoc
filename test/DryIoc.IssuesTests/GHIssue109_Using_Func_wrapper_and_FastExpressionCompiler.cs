using System;
using System.Threading;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue109_Using_Func_wrapper_and_FastExpressionCompiler
    {
        [Test][Ignore("fixme")]
        public void Should_be_able_to_resolve_Func()
        {
            using (var c = new Container(Rules.Default.WithoutFastExpressionCompiler()))
            {
                c.Register<CancellationTokenSource>(reuse: Reuse.Singleton, made: Made.Of(() => new CancellationTokenSource()));
                c.Register<CancellationToken>(reuse: Reuse.Singleton, made: Made.Of(r => ServiceInfo.Of<CancellationTokenSource>(), cts => cts.Token));

                c.Register<IAlpha, Alpha>();

                Assert.DoesNotThrow(() => c.Resolve<Func<IAlpha>>());
            }
        }

        [Test][Ignore("fixme")]
        public void Resolved_Func_should_not_throw()
        {
            using (var c = new Container(Rules.Default))
            {
                c.Register<CancellationTokenSource>(reuse: Reuse.Singleton, made: Made.Of(() => new CancellationTokenSource()));
                c.Register<CancellationToken>(reuse: Reuse.Singleton, made: Made.Of(r => ServiceInfo.Of<CancellationTokenSource>(), cts => cts.Token));

                c.Register<IAlpha, Alpha>();

                var func = c.Resolve<Func<IAlpha>>();
                Assert.DoesNotThrow(() => func());
            }
        }

        private interface IAlpha { }

        private class Alpha : IAlpha
        {
            public Alpha(CancellationToken ct) { }
        }
    }
}
