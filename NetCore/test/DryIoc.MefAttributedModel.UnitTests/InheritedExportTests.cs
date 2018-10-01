using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class InheritedExportTests
    {
        [Test]
        public void It_is_possible_to_mark_interface_to_export_all_its_implementations()
        {
            var container = new Container();
            container.RegisterExports(typeof(ForExport));

            var forExport = container.Resolve<IForExport>();

            Assert.That(forExport, Is.InstanceOf<ForExport>());
        }

        [Test]
        public void It_is_possible_to_mark_abstract_class_to_export_all_its_implementations()
        {
            var container = new Container();
            container.RegisterExports(typeof(ForExportBaseImpl));

            var forExport = container.Resolve<ForExportBase>();

            Assert.That(forExport, Is.InstanceOf<ForExportBaseImpl>());
        }

        [Test]
        public void It_is_possible_to_mark_class_as_not_discoverable()
        {
            var container = new Container();
            container.RegisterExports(typeof(Undicoverable));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IForExport>());
        }

        [Test]
        public void Can_handle_multiple_inherited_and_export_attributes()
        {
            var container = new Container();
            container.RegisterExports(typeof(MultiExported));

            Assert.DoesNotThrow(() =>
            {
                container.Resolve<MultiExported>("a");
                container.Resolve<MultiExported>("b");
                container.Resolve<MultiExported>("c");
                container.Resolve<IMultiExported>("c");
                container.Resolve<IMultiExported>("i");
                container.Resolve<IMultiExported>("j");
            });
        }

        [Test]
        public void API_TEST_Can_discover_attribute_from_implemented_interface_OR_inherited_class()
        {
            Assert.IsTrue(typeof(ForExport).GetImplementedInterfaces().Any(type => type.GetAttributes(typeof(InheritedExportAttribute)).Any()));
            Assert.IsTrue(typeof(ForExportBaseImpl).GetAttributes(typeof(InheritedExportAttribute), true).Any());
        }
    }
}
