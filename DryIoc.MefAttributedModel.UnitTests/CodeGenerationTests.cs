using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class CodeGenerationTests
    {
        [Test]
        public void Should_properly_print_registration_info()
        {
            var info = AttributedModel.GetExportInfoOrDefault(typeof(PrintToCodeExample));

            var code = info.AppendCode();
            var codeValue = code.ToString();
            Assert.That(codeValue, Is.EqualTo(
@"new TypeExportInfo {
    Type = typeof(DryIoc.MefAttributedModel.UnitTests.PrintToCodeExample),
    Exports = new[] {
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.PrintToCodeExample), ServiceKey = 1 },
        new ExportInfo { ServiceType = typeof(DryIoc.MefAttributedModel.UnitTests.IPrintToCode), ServiceKey = 1 },
        }
    IsSingleton = true,
    MetadataAttributeIndex = -1,
    FactoryType = DryIoc.FactoryType.Service
}"));
        }
    }

    [ExportAll(ContractKey = 1)]
    public class PrintToCodeExample : IPrintToCode { }

    public interface IPrintToCode { }
}
