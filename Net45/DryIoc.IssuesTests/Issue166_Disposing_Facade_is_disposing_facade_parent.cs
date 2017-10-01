using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue166_Disposing_Facade_is_disposing_facade_parent
    {
        [Test]
        public void Test()
        {
            var parent = new Container();
            parent.Register<A>(Reuse.Singleton);
            var a = parent.Resolve<A>(); // stores instance of A in parent singleton scope 

            B b;
            using (var child = parent.CreateFacade().WithoutSingletonsAndCache())
            {
                child.Register<B>(Reuse.Singleton, serviceKey: ContainerTools.FacadeKey);
                b = child.Resolve<B>(); // stores B and B.A in child singleton scope.
            }

            Assert.IsTrue(b.IsDisposed);
            Assert.IsTrue(b.A.IsDisposed);
            Assert.IsFalse(a.IsDisposed);   // Parent A is not disposed - because parent kernel is not disposed.

            parent.Dispose();
            Assert.IsTrue(a.IsDisposed);    // Disposed now.
        }

        public class A : IDisposable
        {
            public bool IsDisposed;

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class B : IDisposable
        {
            public A A;
            public bool IsDisposed;

            public B(A a)
            {
                A = a;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
