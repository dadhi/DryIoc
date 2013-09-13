using System;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
    [TestFixture]
    public class ExportGenericWrapperTests
    {
        [Test]
        public void Exporting_IFactory_as_genric_wrapper_should_work()
        {
            var container = new Container();
            container.RegisterExports(GetType().Assembly);

            var consumer = container.Resolve<FactoryConsumer>();

            Assert.That(consumer.One, Is.Not.Null);
        }
    }

    #region CUT

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
    public class One
    {
    }

    public interface IFactory<TService>
    {
        TService Create();
    }

    [ExportPublicTypes, ExportAsGenericWrapper]
    internal class DryFactory<TService> : IFactory<TService>
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

    #endregion
}


