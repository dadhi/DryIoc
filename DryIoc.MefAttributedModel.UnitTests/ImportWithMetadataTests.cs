using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportWithMetadataTests
    {
        [Test]
        public void It_should_throw_if_no_service_with_specified_metadata_found()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FooConsumerNotFound), typeof(FooHey), typeof(FooBlah));

            var ex = Assert.Throws<AttributedModelException>(() => 
                container.Resolve<FooConsumerNotFound>());
            Assert.AreEqual(ex.Error, Error.NotFindDependencyWithMetadata);
        }
    }
}
