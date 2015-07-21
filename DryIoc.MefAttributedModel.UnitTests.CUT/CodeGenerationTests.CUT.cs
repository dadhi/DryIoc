using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [ExportMany(ContractKey = 1)]
    public class PrintToCodeExample : IPrintToCode { }

    public interface IPrintToCode { }
}
