using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
    [TestFixture]
    public class ExportOnImportTests
    {
        [Test]
        public void I_can_export_ctor_param_service_on_resolve()
        {
            var container = new Container();
            container.Register<NativeUser>();
            container.ResolutionRules.ConstructorParameters =
                container.ResolutionRules.ConstructorParameters.Append((parameter, _, registry) =>
                {
                    object key;
                    var attributes = parameter.GetCustomAttributes(false);
                    return AttributedRegistrator.TryGetServiceKeyFromExportOnImportAttribute(out key, parameter.ParameterType, registry, attributes)
                        ? key : null;
                });

            var user = container.Resolve<NativeUser>();

            Assert.That(user.Tool, Is.Not.Null);
        }

        [Test]
        public void I_can_specify_constructor_while_exporting_once_a_ctor_param_service()
        {
            var container = new Container();
            container.Register<HomeUser>();
            container.ResolutionRules.ConstructorParameters =
                container.ResolutionRules.ConstructorParameters.Append((parameter, _, registry) =>
                {
                    object key;
                    var attributes = parameter.GetCustomAttributes(false);
                    return AttributedRegistrator.TryGetServiceKeyFromExportOnImportAttribute(out key, parameter.ParameterType, registry, attributes)
                        ? key : null;
                });

            var user = container.Resolve<HomeUser>();

            Assert.That(user.Tool.Message, Is.EqualTo("blah"));
        }

        [Test]
        public void I_can_specify_metadata()
        {
            var container = new Container();
            container.ResolutionRules.ConstructorParameters =
                container.ResolutionRules.ConstructorParameters.Append((parameter, _, registry) =>
                {
                    object key;
                    var attributes = parameter.GetCustomAttributes(false);
                    return AttributedRegistrator.TryGetServiceKeyFromExportOnImportAttribute(out key, parameter.ParameterType, registry, attributes)
                        ? key : null;
                });
            
            container.Register<MyCode>();

            var code = container.Resolve<MyCode>();

            Assert.That(code.Tool, Is.Not.Null);
            Assert.That(code.ToolMeta, Is.EqualTo(MineMeta.Green));
        }

        [Test]
        public void Import_using_should_work_for_field_and_properties()
        {
            var container = new Container();
            container.ResolutionRules.UseImportAttributes();
            container.RegisterExported(typeof(ServiceWithFieldAndProperty));

            var service = container.Resolve<ServiceWithFieldAndProperty>();

            Assert.That(service.Field, Is.InstanceOf<AnotherService>());
            Assert.That(service.Property, Is.InstanceOf<AnotherService>());
        }
    }

    [ExportAll]
    public class ServiceWithFieldAndProperty
    {
        [ExportOnImport(ImplementationType = typeof(AnotherService), ContractName = "blah")]
        public IService Field;

        [ExportOnImport(ImplementationType = typeof(AnotherService), ContractName = "blah")]
        public IService Property { get; set; }
    }

    #region CUT

    public class NativeUser
    {
        public IForeignTool Tool { get; set; }

        public NativeUser(
            [ExportOnImport(ImplementationType = typeof (ForeignTool), CreationPolicy = CreationPolicy.NonShared)] IForeignTool tool)
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

    public class HomeUser
    {
        public ExternalTool Tool { get; set; }

        public HomeUser([ExportOnImport(ConstructorSignature = new[] {typeof (string)})] Func<string, ExternalTool> getTool)
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

    public class MyCode
    {
        public ExternalTool Tool { get; set; }
        public MineMeta ToolMeta { get; set; }

        public MyCode(
            [ExportOnImport(Metadata = MineMeta.Green, ConstructorSignature = new Type[0])] Meta<Lazy<ExternalTool>, MineMeta> tool)
        {
            Tool = tool.Value.Value;
            ToolMeta = tool.Metadata;
        }
    }

    #endregion
}
