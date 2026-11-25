using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue733_Initializer_target_retained_after_disposal
    {
        [Test]
        public void Initializer_target_is_collectible_after_container_disposal()
        {
            var reference = CreateAndDispose();
            for (var i = 0; i < 10 && reference.IsAlive; ++i)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }

            Assert.IsFalse(reference.IsAlive);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateAndDispose()
        {
            var owner = new Owner();
            var reference = new WeakReference(owner);
            using (var container = new Container())
            {
                container.Register<Service>();
                container.RegisterInitializer<Service>(owner.Initialize, Reuse.Singleton);
                container.Resolve<Service>();
            }
            return reference;
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Dependency_stack_does_not_reach_empty_terminal_request(bool opensResolutionScope, bool syntheticRoot)
        {
            var terminal = opensResolutionScope ? Request.EmptyOpensResolutionScope : Request.Empty;
            using (var container = new Container())
            {
                var root = syntheticRoot
                    ? terminal.Push(typeof(Service), 1, typeof(Service), null)
                    : Request.Create(container, typeof(Service), preResolveParent: terminal);
                var request = root;
                var stackField = typeof(Request).GetField("DepRequestStack", BindingFlags.Instance | BindingFlags.NonPublic);
                for (var i = 0; i < 8; ++i)
                {
                    request = request.PushServiceType(typeof(Service));
                    Assert.IsTrue(stackField.GetValue(terminal) == null);
                }
            }
        }

        public sealed class Service { }

        public sealed class Owner
        {
            public void Initialize(Service service, IResolverContext resolver) { }
        }
    }
}
