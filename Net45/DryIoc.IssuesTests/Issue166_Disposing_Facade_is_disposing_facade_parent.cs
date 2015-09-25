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
            var parentKernel = new Container();
            parentKernel.Register<A>(Reuse.Singleton);
            var a = parentKernel.Resolve<A>(); // stores instance of A in parent singleton scope 

            B b;
            using (var childKernel = parentKernel.CreateFacade())
            {
                childKernel.Register<B>(Reuse.Singleton);
                b = childKernel.Resolve<B>(); // stores B and B.A in child singleton scope. 
                Assert.AreNotSame(a, b.A);    // B.A is different from a resolved from parent
            }

            Assert.IsTrue(b.IsDisposed);
            Assert.IsTrue(b.A.IsDisposed);
            Assert.IsFalse(a.IsDisposed);   // Parent a is not disposed - because parent is not disposed.

            parentKernel.Dispose();
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
