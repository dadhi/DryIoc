using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
    [TestFixture]
    public class ExportAsDecoratorTests
    {
        [Test]
        public void Decorator_can_be_applied_based_on_Name()
        {
            var container = new Container();
            container.RegisterExported(typeof(LoggingHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var slow = container.Resolve<IHandler>("slow");
            Assert.That(slow, Is.InstanceOf<LoggingHandlerDecorator>());
            Assert.That(((LoggingHandlerDecorator)slow).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_Metadata()
        {
            var container = new Container();
            container.RegisterExported(typeof(RetryHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("slow"), Is.InstanceOf<SlowHandler>());

            var fast = container.Resolve<IHandler>("fast");
            Assert.That(fast, Is.InstanceOf<RetryHandlerDecorator>());
            Assert.That(((RetryHandlerDecorator)fast).Handler, Is.InstanceOf<FastHandler>());
        }

        [Test]
        public void Decorator_can_be_applied_based_on_both_name_and_Metadata()
        {
            var container = new Container();
            container.RegisterExported(typeof(TransactHandlerDecorator), typeof(FastHandler), typeof(SlowHandler),
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
            container.RegisterExported(typeof(CustomHandlerDecorator), typeof(FastHandler), typeof(SlowHandler));

            Assert.That(container.Resolve<IHandler>("fast"), Is.InstanceOf<FastHandler>());

            var fast = container.Resolve<IHandler>("slow");
            Assert.That(fast, Is.InstanceOf<CustomHandlerDecorator>());
            Assert.That(((CustomHandlerDecorator)fast).Handler, Is.InstanceOf<SlowHandler>());
        }

        [Test]
        public void Decorator_will_ignore_Import_attribute_on_decorated_service_constructor()
        {
            var container = new Container();
            container.RegisterExported(typeof(FastHandler), typeof(SlowHandler), typeof(DecoratorWithFastHandlerImport));

            var slow = container.Resolve<IHandler>("slow");

            Assert.That(slow, Is.InstanceOf<DecoratorWithFastHandlerImport>());
            Assert.That(((DecoratorWithFastHandlerImport) slow).Handler, Is.InstanceOf<SlowHandler>());
        }
    }

    public interface IHandler
    {
    }

    [ExportAll(ContractName = "fast"), ExportWithMetadata(2)]
    internal class FastHandler : IHandler
    {
    }

    [ExportAll(ContractName = "slow"), ExportWithMetadata(1)]
    internal class SlowHandler : IHandler
    {
    }

    [ExportAll(ContractName = "transact"), ExportWithMetadata(1)]
    internal class TransactHandler : IHandler
    {
    }

    [ExportAll, ExportAsDecorator(OfName = "slow")]
    internal class LoggingHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public LoggingHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, ExportAsDecorator(OfMetadata = true), ExportWithMetadata(2)]
    internal class RetryHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public RetryHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, ExportAsDecorator(OfName = "transact", OfMetadata = true), ExportWithMetadata(1)]
    internal class TransactHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public TransactHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, ExportAsDecorator(ConditionChecker = typeof(ConditionChecker))]
    internal class CustomHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public CustomHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }

        class ConditionChecker : IDecoratorConditionChecker
        {
            public bool Check(Request request)
            {
                return request.ImplementationType == typeof(SlowHandler);
            }
        }
    }

    [ExportAll, ExportAsDecorator]
    public class DecoratorWithFastHandlerImport : IHandler
    {
        public IHandler Handler { get; set; }

        public DecoratorWithFastHandlerImport([Import("fast")]IHandler handler)
        {
            Handler = handler;
        }
    }
}


