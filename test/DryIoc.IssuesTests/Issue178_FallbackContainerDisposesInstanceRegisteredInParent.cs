using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue178_FallbackContainerDisposesInstanceRegisteredInParent
    {
        [Test]
        public void Test_WithPreventDisposal()
        {
            var container = new Container();

            var a = new A();

            container.RegisterInstance<IA>(a, preventDisposal: true);

            using (var c2 = container.CreateFacade())
            {
                c2.Register<B>(serviceKey: ContainerTools.FacadeKey);

                var p2 = c2.Resolve<B>();
            }

            Assert.IsFalse(a.IsDisposed);
        }

        [Test]
        public void Test_WithPreventDisposalAndReplace()
        {
            var container = new Container();

            var a = new A();

            container.RegisterInstance<IA>(a, preventDisposal: true);

            using (var c2 = container.CreateFacade())
            {
                c2.Register<B>(serviceKey: ContainerTools.FacadeKey);
                var b1 = c2.Resolve<B>();
                Assert.IsNotNull(b1.A);
            }

            Assert.IsFalse(a.IsDisposed);
        }

        [Test]
        public void Test_WithPreventDisposalAndWeaklyReferenced()
        {
            var container = new Container();

            var a = new A();

            container.RegisterInstance<IA>(a, preventDisposal: true, weaklyReferenced: true);

            using (var c2 = container.CreateFacade())
            {
                c2.Register<B>(serviceKey: ContainerTools.FacadeKey);

                var p2 = c2.Resolve<B>();
            }

            Assert.IsFalse(a.IsDisposed);
            GC.KeepAlive(a);
        }

        public interface IA { }
        public class A : IA, IDisposable
        {
            public Boolean IsDisposed = false;
            public void Dispose()
            {
                IsDisposed = true;
            }

            public override string ToString()
            {
                if (IsDisposed)
                    return "Object is disposed";
                else
                    return "Ok";
            }
        }

        public class B
        {
            public B(IA a)
            {
                this.A = a;
            }

            public IA A;
        }
    }
}
