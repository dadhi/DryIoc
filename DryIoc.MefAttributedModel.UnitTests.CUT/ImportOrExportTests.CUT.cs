using System;
using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    [Export]
    public class NativeUser
    {
        public IForeignTool Tool { get; set; }

        public NativeUser(
            [ExportIfNeeded(ImplementationType = typeof(ForeignTool), CreationPolicy = CreationPolicy.NonShared)] 
            IForeignTool tool)
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

        public HomeUser([ExportIfNeeded(ConstructorSignature = new[] { typeof(string) })] Func<string, ExternalTool> getTool)
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

    public enum MineMeta
    {
        Red,
        Green
    };

    [ExportAll]
    public class MyCode
    {
        public ExternalTool Tool { get; set; }
        public MineMeta ToolMeta { get; set; }

        public MyCode(
            [ExportIfNeeded(Metadata = MineMeta.Green, ConstructorSignature = new Type[0])] 
            Meta<Lazy<ExternalTool>, MineMeta> tool)
        {
            Tool = tool.Value.Value;
            ToolMeta = tool.Metadata;
        }
    }

    [ExportAll]
    public class ServiceWithFieldAndProperty
    {
        [ExportIfNeeded(ImplementationType = typeof(AnotherService), ContractName = "blah")]
        public IService Field;

        [ExportIfNeeded(ImplementationType = typeof(AnotherService), ContractName = "blah")]
        public IService Property { get; set; }
    }
}