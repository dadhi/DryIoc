using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue259_Possibility_to_prevent_disposal_in_child_container
    {
        [Test][Explicit("todo: @fixme")]
        public void Test1()
        {
            var container = new Container();
            container.Register<MyService>(Reuse.Singleton);

            var someService1 = container.Resolve<MyService>();
            MyService someService2 = null;

            using (var childContainer = container.With(
                container.Rules,
                container.ScopeContext,
                RegistrySharing.CloneAndDropCache,
                container.SingletonScope.Clone()))
            {
                someService2 = childContainer.Resolve<MyService>();
            }

            Assert.IsTrue(someService2.IsDisposed);
            Assert.IsFalse(someService1.IsDisposed);

            var someService3 = container.Resolve<MyService>();
            Assert.IsFalse(someService3.IsDisposed);
        }

        public class MyService : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }
    }
}
