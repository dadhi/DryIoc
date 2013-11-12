using System.ComponentModel.Composition;

namespace DryIoc.AttributedRegistration.UnitTests.CUT
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

    [Export("a"), Export("b")]
    [ExportAll(ContractName = "c")]
    public class MultiExported : IMultiExported { }
}
