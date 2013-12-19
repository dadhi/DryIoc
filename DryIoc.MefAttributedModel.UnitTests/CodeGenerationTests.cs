using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class CodeGenerationTests
    {
        [Test]
        public void Should_properly_print_registration_info()
        {
            var info = AttributedModel.GetExportInfoOrDefault(typeof(ForExportBaseImpl));

            var code = info.ToCode();

            Assert.That(code, Is.EqualTo(
@"new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBaseImpl),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.ForExportBase), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}"));
        }
    }
}
