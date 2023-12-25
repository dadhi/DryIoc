using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue259_Possibility_to_prevent_disposal_in_child_container : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var container = new Container();
            container.Register<MyService>(Reuse.Singleton);

            var service1 = container.Resolve<MyService>();
            MyService service2 = null;

            using (var childContainer = container.WithoutSingletonsAndCache().CreateChild())
            {
                service2 = childContainer.Resolve<MyService>(); // expecting it to be a new singleton, right?
            }

            Assert.IsTrue(service2.IsDisposed);
            Assert.IsFalse(service1.IsDisposed);

            var service3 = container.Resolve<MyService>();
            Assert.AreSame(service1, service3);

            container.Dispose();
            Assert.IsTrue(service1.IsDisposed);
        }

        public class MyService : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }
    }
}
