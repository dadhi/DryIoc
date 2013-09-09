using System;
using NUnit.Framework;

namespace DryIoc.UnitTests.Playground
{
    [TestFixture]
    public class ExportGenericWrapperPlayTests
    {
        [Test]
        public void Injecting_IFactory_should_work()
        {
            var container = new Container();
            container.RegisterExports(GetType().Assembly);

            var consumer = container.Resolve<FactoryConsumer>();

            Assert.That(consumer.One, Is.Not.Null);
        }
    }

    [Export]
    public class FactoryConsumer
    {
        public FactoryConsumer(IFactory<One>[] oneFactory)
        {
            One = oneFactory[0].Create();
        }

        public One One { get; set; }
    }

    [Export("one"), Export("two")]
    public class One { }

    public interface IFactory<TService>
    {
        TService Create();
    }

    [ExportPublicTypes, ExportAsGenericWrapper]
    class DryFactory<TService> : IFactory<TService>
    {
        public DryFactory(Func<TService> create)
        {
            _create = create;
        }

        public TService Create()
        {
            return _create();
        }

        private readonly Func<TService> _create;
    }
}
