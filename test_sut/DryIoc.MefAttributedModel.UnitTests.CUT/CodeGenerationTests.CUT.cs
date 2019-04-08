using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [ExportMany(ContractKey = 1), PartCreationPolicy(CreationPolicy.Shared)]
    public class PrintToCodeExample : IPrintToCode { }

    [ExportMany(ContractKey = 2)]
    public class PrintToCodeNoCreationPolicyExample : IPrintToCode { }

    public interface IPrintToCode { }
}
