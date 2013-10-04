using DryIoc.AttributedRegistration.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.AttributedRegistration.UnitTests
{
    [TestFixture]
    public class ImportOrExportTests
    {
        [Test]
        public void I_can_export_ctor_param_service_on_resolve()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(typeof(NativeUser));

            var user = container.Resolve<NativeUser>();

            Assert.That(user.Tool, Is.Not.Null);
        }

        [Test]
        public void I_can_specify_constructor_while_exporting_once_a_ctor_param_service()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(typeof(HomeUser));

            var user = container.Resolve<HomeUser>();

            Assert.That(user.Tool.Message, Is.EqualTo("blah"));
        }

        [Test]
        public void I_can_specify_metadata()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(typeof(MyCode));

            var code = container.Resolve<MyCode>();

            Assert.That(code.Tool, Is.Not.Null);
            Assert.That(code.ToolMeta, Is.EqualTo(MineMeta.Green));
        }

        [Test]
        public void Import_using_should_work_for_field_and_properties()
        {
            var container = new Container();
            container.ResolutionRules.UseImportExportAttributes();
            container.RegisterExported(typeof(ServiceWithFieldAndProperty));

            var service = container.Resolve<ServiceWithFieldAndProperty>();

            Assert.That(service.Field, Is.InstanceOf<AnotherService>());
            Assert.That(service.Property, Is.InstanceOf<AnotherService>());
        }
    }
}
