using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class CodeGenerationTests : ITest
    {
        public int Run()
        {
            Should_properly_print_registration_info_with_Singleton_reuse_type();
            Should_properly_print_registration_info_with_default_reuse_type();
            return 2;
        }

        [Test]
        public void Should_properly_print_registration_info_with_Singleton_reuse_type()
        {
            var info = AttributedModel.GetExportedRegistrations(typeof(PrintToCodeExample)).Single();

            var code = info.ToCode();
            var codeValue = code.ToString();
            Assert.That(codeValue, Is.EqualTo(@"
    new ExportedRegistrationInfo {
        ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample),
        Exports = new[] {
            new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeExample), 1, DryIoc.IfAlreadyRegistered.AppendNotKeyed),
            new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 1, DryIoc.IfAlreadyRegistered.AppendNotKeyed),
        },
        Reuse = new ReuseInfo { ReuseType = DryIocAttributes.ReuseType.Singleton },
        OpenResolutionScope = false,
        AsResolutionCall = false,
        AsResolutionRoot = false,
        PreventDisposal = false,
        WeaklyReferenced = false,
        AllowDisposableTransient = null,
        TrackDisposableTransient = false,
        UseParentReuse = false,
        HasMetadataAttribute = false,
        FactoryType = DryIoc.FactoryType.Service,
        ConditionType = null
    }"));
        }

        [Test]
        public void Should_properly_print_registration_info_with_default_reuse_type()
        {
            var info = AttributedModel.GetExportedRegistrations(typeof(PrintToCodeNoCreationPolicyExample)).Single();

            var code = info.ToCode();
            var codeValue = code.ToString();
            Assert.That(codeValue, Is.EqualTo(@"
    new ExportedRegistrationInfo {
        ImplementationType = typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeNoCreationPolicyExample),
        Exports = new[] {
            new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.PrintToCodeNoCreationPolicyExample), 2, DryIoc.IfAlreadyRegistered.AppendNotKeyed),
            new ExportInfo(typeof(DryIoc.MefAttributedModel.UnitTests.CUT.IPrintToCode), 2, DryIoc.IfAlreadyRegistered.AppendNotKeyed),
        },
        OpenResolutionScope = false,
        AsResolutionCall = false,
        AsResolutionRoot = false,
        PreventDisposal = false,
        WeaklyReferenced = false,
        AllowDisposableTransient = null,
        TrackDisposableTransient = false,
        UseParentReuse = false,
        HasMetadataAttribute = false,
        FactoryType = DryIoc.FactoryType.Service,
        ConditionType = null
    }"));
        }
    }
}
