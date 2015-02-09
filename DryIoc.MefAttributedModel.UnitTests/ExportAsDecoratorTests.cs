using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportAsDecoratorTests
    {
        [Test]
        public void Decorator_can_be_applied_based_on_Name()
        {
            var container = new Container();
            container.RegisterExports(typeof(LoggingHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var slow = container.Resolve<IHandler>("slow");
            Assert.That(slow, Is.InstanceOf<LoggingHandlerDecorator>());
            Assert.That(((LoggingHandlerDecorator)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_Metadata()
        {
            var container = new Container();
            container.RegisterExports(typeof(RetryHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("slow"), Is.InstanceOf<SlowHandler>());

            var fast = container.Resolve<IHandler>("fast");
            Assert.That(fast, Is.InstanceOf<RetryHandlerDecorator>());
            Assert.That(((RetryHandlerDecorator)fast).Handler, Is.InstanceOf<FastHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_both_name_and_Metadata()
        {
            var container = new Container();
            container.RegisterExports(typeof(TransactHandlerDecorator), typeof(FastHandler), typeof(SlowHandler),
                typeof(TransactHandler));

            Assert.That(container.Resolve<IHandler>("slow"), Is.InstanceOf<SlowHandler>());
            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var transact = container.Resolve<IHandler>("transact");
            Assert.That(transact, Is.InstanceOf<TransactHandlerDecorator>());
            Assert.That(((TransactHandlerDecorator)transact).Handler, Is.InstanceOf<TransactHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_custom_condition()
        {
            var container = new Container();
            container.RegisterExports(typeof(CustomHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var fast = container.Resolve<IHandler>("slow");
            Assert.That(fast, Is.InstanceOf<CustomHandlerDecorator>());
            Assert.That(((CustomHandlerDecorator)fast).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_will_ignore_Import_attribute_on_decorated_service_constructor()
        {
            var container = new Container();
            container.RegisterExports(typeof(FastHandler), typeof(SlowHandler), typeof(DecoratorWithFastHandlerImport));

            var slow = container.Resolve<IHandler>("slow");

            Assert.That(slow, Is.InstanceOf<DecoratorWithFastHandlerImport>());
            Assert.That(((DecoratorWithFastHandlerImport)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_supports_matching_by_service_key()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FoohHandler), typeof(BlahHandler), typeof(FoohDecorator));

            var handler = container.Resolve<IHandler>(BlahFooh.Fooh);

            Assert.That(handler, Is.InstanceOf<FoohDecorator>());
            Assert.That(((FoohDecorator)handler).Handler, Is.InstanceOf<FoohHandler>());
        }
    }
}


