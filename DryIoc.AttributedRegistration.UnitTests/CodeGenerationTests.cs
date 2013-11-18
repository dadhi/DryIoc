using DryIoc.AttributedRegistration.UnitTests.CUT;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.AttributedRegistration.UnitTests
{
    [TestFixture]
    public class CodeGenerationTests
    {
        [Test]
        public void Should_properly_print_registration_info()
        {
            var info = AttributedModel.GetRegistrationInfoOrDefault(typeof(ForExportBaseImpl));

            var code = info.ToCode();

            Assert.That(code, Is.EqualTo(
@"new RegistrationInfo {
    ImplementationType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ForExportBaseImpl),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.AttributedRegistration.UnitTests.CUT.ForExportBase), ServiceName = null },
    },
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}"));
        }
    }
}
