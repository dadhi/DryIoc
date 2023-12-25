using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Issue58_HandleReusedObjectsIDisposableAndGCing : ITest
    {
        public int Run()
        {
            ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc();
            // ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc2();
            return 1;
        }

        interface ITest : IDisposable
        {
        }

        public class Test : ITest
        {
            public void Dispose() { }
        }

        [Test]
        public void ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc2()
        {
            WeakReference @ref;
            var c = new Container();
            c.Register<ITest, Test>(Reuse.Scoped);
            var cx = c.OpenScope();
            RunTheTest(out @ref, cx);
            cx.Dispose();
            GC.Collect();
            Assert.IsFalse(@ref.IsAlive);
        }

        [Test, Explicit("Relies on GC")]
        public void ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc()
        {
            WeakReference @ref;
            var c = new Container(scopeContext: new ThreadScopeContext());
            c.Register<ITest, Test>(Reuse.InCurrentScope);
            var cx = c.OpenScope();
            var scopeRef = new WeakReference(cx);
            RunTheTest(out @ref, cx);
            cx.Dispose();
            GC.Collect();
            Assert.IsTrue(scopeRef.IsAlive);
            Assert.IsFalse(@ref.IsAlive);
        }

        private static void RunTheTest(out WeakReference @ref, IResolver cx)
        {
            var inst = cx.Resolve<ITest>();
            @ref = new WeakReference(inst);
            inst.Dispose();
        }
    }
}
