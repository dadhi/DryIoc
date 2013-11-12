using System;
using System.ComponentModel.Composition;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.AttributedRegistration.UnitTests
{
    [TestFixture]
    public class InheritedExportTests
    {
        [Test]
        public void It_is_possible_to_mark_interface_to_export_all_its_implementations()
        {
            var container = new Container();
            container.RegisterExported(typeof(ForExport));

            var forExport = container.Resolve<IForExport>();

            Assert.That(forExport, Is.InstanceOf<ForExport>());
        }

        [Test]
        public void It_is_possible_to_mark_abstract_class_to_export_all_its_implementations()
        {
            var container = new Container();
            container.RegisterExported(typeof(ForExportBaseImpl));

            var forExport = container.Resolve<ForExportBase>();

            Assert.That(forExport, Is.InstanceOf<ForExportBaseImpl>());
        }

        [Test]
        public void It_is_possible_to_mark_class_as_not_discoverable()
        {
            var container = new Container();
            container.RegisterExported(typeof(Undicoverable));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IForExport>());
        }

        [Test]
        public void Can_handle_multiple_inherited_and_export_attributes()
        {
            var container = new Container();
            container.RegisterExported(typeof(MultiExported));

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
            Assert.IsTrue(typeof(ForExport).GetInterfaces().Any(type => Attribute.IsDefined(type, typeof(InheritedExportAttribute), false)));
            Assert.IsTrue(Attribute.IsDefined(typeof(ForExportBaseImpl), typeof(InheritedExportAttribute), true));
        }
    }

    [InheritedExport]
    public interface IForExport { }

    class ForExport : IForExport { }

    [InheritedExport]
    public abstract class ForExportBase {}

    class ForExportBaseImpl : ForExportBase {}

    [PartNotDiscoverable]
    public class Undicoverable : IForExport {}

    [InheritedExport("i"), InheritedExport("j")]
    public interface IMultiExported {}

    [Export("a"), Export("b")]
    [ExportAll(ContractName = "c")]
    public class MultiExported : IMultiExported {}
}
