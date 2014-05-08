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
            var info = AttributedModel.GetRegistrationInfoOrDefault(typeof(PrintToCodeExample));

            var code = info.AppendAsCode();
            var codeValue = code.ToString();
            Assert.That(codeValue, Is.EqualTo(
@"new RegistrationInfo {
    ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample),
    Exports = new[] {
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1),
        new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1),
        },
    IsSingleton = true,
    HasMetadataAttribute = false,
    FactoryType = DryIoc.FactoryType.Service
}"));
        }
    }
}
