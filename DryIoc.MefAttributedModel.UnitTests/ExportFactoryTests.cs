using System;
using NUnit.Framework;
using DryIoc.MefAttributedModel.UnitTests.CUT;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportFactoryTests
    {
        [Test]
        public void Could_register_factory_automatically_when_exported()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(OrangeFactory));

            var orange = container.Resolve<Orange>();

            Assert.NotNull(orange);
        }

        [Test]
        public void Could_register_multi_factory_automatically_when_exported()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FruitFactory));

            var orange = container.Resolve<Orange>();
            var apple = container.Resolve<Apple>();

            Assert.NotNull(orange);
            Assert.NotNull(apple);
        }

        [Test]
        public void Could_register_multi_factory_with_separate_named_Exports()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(NamedFruitFactory));

            var orange = container.Resolve<Orange>("orange");
            var apple = container.Resolve<Apple>("apple");

            Assert.NotNull(orange);
            Assert.NotNull(apple);
        }

        [Test]
        public void Reuse_should_be_applied_for_factory_created_services()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(OrangeFactory));

            var one = container.Resolve<Orange>();
            var another = container.Resolve<Orange>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Specified_transient_reuse_should_be_applied_for_factory_created_services()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(TransientOrangeFactory));

            var one = container.Resolve<Orange>();
            var another = container.Resolve<Orange>();

            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Could_export_Func_with_parameters_as_Factory()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(FuncFactory));

            var factory = container.Resolve<Func<string, Orange>>();

            Assert.NotNull(factory("hey"));
        }

        [Test]
        public void You_should_attribute_Create_with_Export_to_register_Otherwise_only_factory_itself_will_be_registered()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(AppleFactory));

            container.Resolve<AppleFactory>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<Apple>());
        }
    }
}
