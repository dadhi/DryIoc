using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ConstructionTests
    {
        [Test]
        public void Can_use_static_method_for_service_creation()
        {
            var container = new Container();
            container.Register<SomeService>(setup: Setup.With(
                (t, _) => ConstructionInfo.Of(t.GetDeclaredMethod("Create"))));

            var service = container.Resolve<SomeService>();

            Assert.That(service.Message, Is.EqualTo("yes!"));
        }

        [Test]
        public void Can_use_instance_method_for_service_creation()
        {
            var container = new Container();
            container.Register<ServiceFactory>();
            container.Register<SomeService>(setup: Setup.With(
                (_, r) => ConstructionInfo.Of(r.Resolve<ServiceFactory>(), typeof(ServiceFactory).GetDeclaredMethod("CreateService"))));

            var service = container.Resolve<SomeService>();

            Assert.That(service.Message, Is.EqualTo("yep!"));
        }

        [Test]
        public void Can_use_instance_method_with_resolved_parameter()
        {
            var container = new Container();
            container.Register<ServiceFactory>();
            container.RegisterInstance("dah!");
            container.Register<SomeService>(setup: Setup.With(
                (_, r) => ConstructionInfo.Of(r.Resolve<ServiceFactory>(), typeof(ServiceFactory).GetDeclaredMethod("CreateService", new[] { typeof(string) }))));

            var service = container.Resolve<SomeService>();

            Assert.That(service.Message, Is.EqualTo("dah!"));
        }

        internal class SomeService
        {
            public string Message { get; private set; }

            internal SomeService(string message)
            {
                Message = message;
            }

            public static SomeService Create()
            {
                return new SomeService("yes!");
            }
        }

        internal class ServiceFactory
        {
            public SomeService CreateService()
            {
                return new SomeService("yep!");
            }

            public SomeService CreateService(string message)
            {
                return new SomeService(message);
            }
        }
    }
}
