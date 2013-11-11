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
            Assert.Fail();
        }

        [Test]
        public void It_is_possible_to_mark_abstract_class_to_export_all_its_implementations()
        {
            Assert.Fail();
        }

        [Test]
        public void It_is_possible_to_mark_class_as_not_discoverable()
        {
            Assert.Fail();
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
}
