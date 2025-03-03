using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterAttributeTests : ITest
    {
        public int Run()
        {
            Can_register_service_with_tracking_disposable_reuse();

            return 1;
        }

        [Test]
        public void Can_register_service_with_tracking_disposable_reuse()
        {
            var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(Registrations));
            Assert.AreEqual(1, count);

            var ad = c.Resolve<ID>();
            Assert.IsNotNull(ad);

            c.Dispose();
            Assert.IsTrue(((AD)ad).IsDisposed);
        }

        [Register<ID, AD, TransientReuse>(TrackDisposableTransient = DisposableTracking.TrackDisposableTransient)]
        public static class Registrations { }

        public interface ID { }

        class AD : ID, IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }
    }
}
