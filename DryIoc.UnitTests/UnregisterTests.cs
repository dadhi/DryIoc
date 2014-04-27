using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class UnregisterTests
    {
        [Test]
        public void Unregister_default_registration()
        {
            var container = new Container();
            container.Register<IService, Service>();
            Assert.IsTrue(container.IsRegistered<IService>());

            container.Unregister(typeof(IService));
            Assert.IsFalse(container.IsRegistered<IService>());
        }
    }
}
