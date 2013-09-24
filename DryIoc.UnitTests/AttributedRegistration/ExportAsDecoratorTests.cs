using System.Runtime.Remoting.Messaging;
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
            Assert.That(container.Resolve<IHandler>("slow"), Is.InstanceOf<LoggingHandlerDecorator>());
        }
    }

    public interface IHandler { }

    [ExportAll(ContractName = "fast")]
    class FastHandler : IHandler { }

    [ExportAll(ContractName = "slow")]
    class SlowHandler : IHandler { }

    [ExportAll, ExportAsDecorator(OfName = "slow")]
    class LoggingHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public LoggingHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }
}
