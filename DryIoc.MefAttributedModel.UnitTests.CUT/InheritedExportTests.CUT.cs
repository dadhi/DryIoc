using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [InheritedExport]
    public interface IForExport { }

    public class ForExport : IForExport { }

    [InheritedExport]
    public abstract class ForExportBase { }

    public class ForExportBaseImpl : ForExportBase { }

    [PartNotDiscoverable]
    public class Undicoverable : IForExport { }

    [InheritedExport("i"), InheritedExport("j")]
    public interface IMultiExported { }

    [Export("a")]
    [ExportAll(ContractName = "c")]
    [Export("b")]
    public class MultiExported : IMultiExported { }
}
