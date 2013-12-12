using System;
using DryIoc.AttributedRegistration.UnitTests.CUT;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.AttributedRegistration.UnitTests
{
    [TestFixture]
    public class ExportAsGenericWrapperTests
    {
        [Test]
        public void Exporting_IFactory_as_generic_wrapper_should_work()
        {
            var container = new Container();
            container.RegisterExports(typeof(FactoryConsumer), typeof(One), typeof(DryFactory<>));

            var consumer = container.Resolve<FactoryConsumer>();

            Assert.That(consumer.One, Is.Not.Null);
        }

        [Test]
        public void Exporting_IFactory_with_arguments_as_generic_wrapper_should_work()
        {
            var container = new Container();
            container.RegisterExports(typeof(FactoryWithArgsConsumer), typeof(Two), typeof(DryFactory<,>));

            var consumer = container.Resolve<Func<string, FactoryWithArgsConsumer>>();

            Assert.That(consumer("blah").Two.Message, Is.EqualTo("blah"));
        }
    }
}