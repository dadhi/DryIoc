using DryIoc.AttributedRegistration.UnitTests.CUT;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.AttributedRegistration.UnitTests
{
    [TestFixture]
    public class ImportWithMetadataTests
    {
        [Test]
        public void I_should_be_able_to_import_single_service_based_on_specified_metadata()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(FooConsumer), typeof(FooHey), typeof(FooBlah));

            var service = container.Resolve<FooConsumer>();

            Assert.That(service.Foo.Value, Is.InstanceOf<FooBlah>());
        }

        [Test]
        public void It_should_throw_if_no_service_with_specified_metadata_found()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(FooConsumerNotFound), typeof(FooHey), typeof(FooBlah));

            Assert.Throws<ContainerException>(() => 
                container.Resolve<FooConsumerNotFound>());
        }
    }
}
