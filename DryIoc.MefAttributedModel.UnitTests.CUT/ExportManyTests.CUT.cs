using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [ExportMany, Export]
    public class WithBothTheSameExports { }

    public interface IOne { }

    public interface INamed { }

    [ExportMany(ContractName = "blah")]
    public class NamedOne : INamed, IOne { }

    [Export("named", typeof(INamed)), ExportMany]
    public class BothExportManyAndExport : INamed, IOne {}
}
