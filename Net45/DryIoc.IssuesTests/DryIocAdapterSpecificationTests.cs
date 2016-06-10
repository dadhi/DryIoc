using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class DryIocAdapterSpecificationTests
    {
        [Test, Ignore]
        public void DisposingScopeDisposesService()
        {
            var adapter = new Container(rules => rules.WithTrackingDisposableTransients());

            adapter.Register<IFakeSingletonService, FakeService>(Reuse.Singleton);
            adapter.Register<IFakeScopedService, FakeService>(Reuse.InCurrentScope);
            adapter.Register<IFakeService, FakeService>();
            var fakeService1 = (FakeService)adapter.Resolve<IFakeService>();
            FakeService fakeService3;
            FakeService fakeService4;
            using (var scope = adapter.OpenScope())
            {
                fakeService3 = (FakeService)scope.Resolve<IFakeService>();
                fakeService4 = (FakeService)scope.Resolve<IFakeService>();
                Assert.False(fakeService3.Disposed);
                Assert.False(fakeService4.Disposed);
            }
            Assert.True(fakeService3.Disposed, "fakeService3.Disposed");
            Assert.True(fakeService4.Disposed, "fakeService4.Disposed");

            adapter.Dispose();
            Assert.True(fakeService1.Disposed, "fakeService1.Disposed");
        }

        public interface IFakeService { }
        public interface IFakeScopedService { }
        public interface IFakeSingletonService { }
        public class FakeService : IFakeService, IFakeScopedService, IFakeSingletonService, IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                if (Disposed)
                    throw new ObjectDisposedException("FakeService");
                Disposed = true;
            }
        }
    }
}
