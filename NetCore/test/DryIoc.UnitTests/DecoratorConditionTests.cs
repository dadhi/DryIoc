using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DecoratorConditionTests
    {
        [Test]
        public void Can_register_decorator_with_condition_based_on_service_name()
        {
            var container = new Container();
            container.Register<IHandler, FastHandler>(serviceKey: "fast");
            container.Register<IHandler, SlowHandler>(serviceKey: "slow");
            container.Register<IHandler, LoggingHandlerDecorator>(
                setup: Setup.DecoratorWith(request => Equals(request.ServiceKey, "slow")));

            Assert.IsInstanceOf<FastHandler>(container.Resolve<IHandler>("fast"));
            Assert.IsInstanceOf<LoggingHandlerDecorator>(container.Resolve<IHandler>("slow"));
        }
    }

    public interface IHandler
    {
    }

    public class FastHandler : IHandler
    {
    }

    public class SlowHandler : IHandler
    {
    }

    public class LoggingHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public LoggingHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }
}
