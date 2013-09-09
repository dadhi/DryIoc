using System;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
    [TestFixture]
    public class ImportUsingTests
    {
        [Test]
        public void I_can_export_ctor_param_service_on_resolve()
        {
            var container = new Container();
            container.Register<NativeUser>();
            container.Setup.AddConstructorParamServiceKeyResolutionRule(AttributedRegistrator.TryImportUsing);

            var user = container.Resolve<NativeUser>();

            Assert.That(user.Tool, Is.Not.Null);
        }

        [Test]
        public void I_can_specify_constructor_while_exporting_once_a_ctor_param_service()
        {
            var container = new Container();
            container.Register<HomeUser>();
            container.Setup.AddConstructorParamServiceKeyResolutionRule(AttributedRegistrator.TryImportUsing);

            var user = container.Resolve<HomeUser>();

            Assert.That(user.Tool.Message, Is.EqualTo("blah"));
        }

        [Test]
        public void I_can_specify_metadata()
        {
            var container = new Container();
            container.Setup.AddConstructorParamServiceKeyResolutionRule(AttributedRegistrator.TryImportUsing);
            
            container.Register<MineCode>();

            var code = container.Resolve<MineCode>();

            Assert.That(code.Tool, Is.Not.Null);
            Assert.That(code.ToolMeta, Is.EqualTo(MineMeta.Green));
        }
    }

    public class NativeUser
    {
        public IForeignTool Tool { get; set; }

        public NativeUser(
            [ImportUsing(ImplementationType=typeof(ForeignTool), CreationPolicy=CreationPolicy.NonShared)]
            IForeignTool tool)
        {
            Tool = tool;
        }
    }

    public interface IForeignTool { }

    public class ForeignTool : IForeignTool { }

    public class HomeUser
    {
        public ExternalTool Tool { get; set; }

        public HomeUser([ImportUsing(ConstructorSignature = new[] { typeof(string) })] Func<string, ExternalTool> getTool)
        {
            Tool = getTool("blah");
        }
    }

    public class ExternalTool
    {
        public string Message { get; set; }

        public ExternalTool() { }

        public ExternalTool(string message)
        {
            Message = message;
        }
    }

    public enum MineMeta { Red, Green };

    public class MineCode
    {
        public ExternalTool Tool { get; set; }
        public MineMeta ToolMeta { get; set; }

        public MineCode([ImportUsing(Metadata = MineMeta.Green, ConstructorSignature = new Type[0])] Meta<Lazy<ExternalTool>, MineMeta> tool)
        {
            Tool = tool.Value.Value;
            ToolMeta = tool.Metadata;
        }
    }
}
