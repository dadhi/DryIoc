namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [ExportAll(ContractKey = 1)]
    public class PrintToCodeExample : IPrintToCode { }

    public interface IPrintToCode { }
}
