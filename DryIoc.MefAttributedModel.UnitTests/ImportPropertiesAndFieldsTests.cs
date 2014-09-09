using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportPropertiesAndFieldsTests
    {
        [Test]
        public void Resolve_property_for_already_resolved_instance()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(Service));

            var client = new PropertyClient();
            container.ResolvePropertiesAndFields(client);

            Assert.That(client.Some, Is.InstanceOf<Service>());
            Assert.That(client.Other, Is.Null);
        }

        [Test]
        public void Resolving_unregistered_property_marked_by_Import_should_not_throw()
        {
            var container = new Container(AttributedModel.DefaultSetup);
            container.RegisterExports(typeof(Service));

            var client = new NamedPropertyClient();
            container.ResolvePropertiesAndFields(client);

            Assert.That(client.Some, Is.Null);
        }

        [Test]
        public void Resolving_unregistered_property_without_attribute_model_should_not_throw_so_the_property_stays_null()
        {
            var container = new Container();
            container.Register<Service>();

            var client = new NamedPropertyClient();
            container.ResolvePropertiesAndFields(client);

            Assert.That(client.Some, Is.Null);
        }

        [Export]
        public class PropertyClient
        {
            [Import]
            public IService Some { get; set; }

            public IService Other { get; set; }
        }

        [Export]
        public class NamedPropertyClient
        {
            [Import("k")]
            public IService Some { get; set; }

            public IService Other { get; set; }
        }
    }
}
