using System;
using Xunit;

namespace DryIoc.IssuesTests
{
    public class Issue58_HandleReusedObjectsIDisposableAndGCing
    {
        interface ITest : IDisposable
        {
        }

        public class Test : ITest
        {
            public void Dispose() { }
        }

        [Fact]
        public void ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc()
        {
            WeakReference @ref;
            var c = new Container();
            c.Register<ITest, Test>(Reuse.InCurrentScope);
            var cx = c.BeginScope();
            var scopeRef = new WeakReference(cx);

            RunTheTest(out @ref, c, cx);
            cx.Dispose();
            GC.Collect();
            Assert.True(scopeRef.IsAlive);
            Assert.False(@ref.IsAlive);
        }

        private static void RunTheTest(out WeakReference @ref, Container c, Container cx)
        {
            var inst = cx.Resolve<ITest>();
            @ref = new WeakReference(inst);
            inst.Dispose();
        }
    }
}
