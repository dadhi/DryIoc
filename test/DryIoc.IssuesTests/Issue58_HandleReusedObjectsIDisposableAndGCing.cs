using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    public class Issue58_HandleReusedObjectsIDisposableAndGCing : ITest
    {
        public int Run()
        {
            ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc();
            ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc2();
            return 2;
        }

        interface ITested : IDisposable { }

        public class Tested : ITested
        {
            public void Dispose() { }
        }

        [Test, Explicit("Relies on GC")]
        public void ExternallyOwnedInstanceShouldNotBeAliveAfterDisposalAndGc2()
        {
            WeakReference @ref;
            var c = new Container();
            c.Register<ITested, Tested>(Reuse.Scoped);
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
            c.Register<ITested, Tested>(Reuse.InCurrentScope);
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
            var inst = cx.Resolve<ITested>();
            @ref = new WeakReference(inst);
            inst.Dispose();
        }
    }
}
