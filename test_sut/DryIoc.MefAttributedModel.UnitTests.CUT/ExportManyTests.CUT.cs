using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export, ExportMany, Export]
    public class WithBothTheSameExports { }

    public interface IOne { }

    public interface INamed { }

    [ExportMany(ContractName = "blah", IfAlreadyExported = IfAlreadyExported.Keep)]
    public class NamedOne : INamed, IOne { }

    [Export("named", typeof(INamed)), ExportMany, Export("named", typeof(INamed))]
    public class BothExportManyAndExport : INamed, IOne {}
}
