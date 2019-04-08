using System;
using System.ComponentModel.Composition;
using DryIocAttributes;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export, TransientReuse]
    public class NativeUser
    {
        public IForeignTool Tool { get; set; }

        public NativeUser([ImportExternal(typeof(ForeignTool), contractType: typeof(ForeignTool)), TransientReuse] IForeignTool tool)
        {
            Tool = tool;
        }
    }

    public interface IForeignTool
    {
    }

    public class ForeignTool : IForeignTool
    {
    }

    [Export]
    public class HomeUser
    {
        public ExternalTool Tool { get; set; }

        public HomeUser([ImportExternal(constructorSignature: new[] { typeof(string) })] Func<string, ExternalTool> getTool)
        {
            Tool = getTool("blah");
        }
    }

    public class ExternalTool
    {
        public string Message { get; set; }

        public ExternalTool()
        {
        }

        public ExternalTool(string message)
        {
            Message = message;
        }
    }

    [ExportMany]
    public class ServiceWithFieldAndProperty
    {
        [ImportExternal(typeof(AnotherService), contractKey: "blah")]
        public IService Field;

        [ImportExternal(ImplementationType = typeof(AnotherService), ContractKey = "blah")]
        public IService Property { get; set; }
    }

    [Export]
    public class OneDependsOnExternalTool
    {
        public readonly ExternalTool Tool;

        public OneDependsOnExternalTool([ImportExternal(constructorSignature: new Type[0], contractKey: 13)]ExternalTool tool)
        {
            Tool = tool;
        }
    }

    [Export]
    public class OtherDependsOnExternalTool
    {
        public readonly ExternalTool Tool;

        public OtherDependsOnExternalTool([ImportEx(13)]ExternalTool tool)
        {
            Tool = tool;
        }
    }

    [ExportMany]
    public class WithUnregisteredExternalDependency
    {
        public ExternalTool Tool { get; set; }

        public WithUnregisteredExternalDependency([SomeOther]ExternalTool tool)
        {
            Tool = tool;
        }
    }

    public class SomeOtherAttribute : Attribute
    {
    }
}