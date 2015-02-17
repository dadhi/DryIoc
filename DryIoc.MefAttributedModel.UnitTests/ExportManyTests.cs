using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ExportManyTests
    {
        [Test]
        public void When_class_public_types_exported_as_singleton_Then_resolving_types_will_return_the_same_instance()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(new [] { typeof(ISomeDb).GetAssembly() });

            var someDb = container.Resolve<ISomeDb>();
            var anotherDb = container.Resolve<IAnotherDb>();

            Assert.That(someDb, Is.SameAs(anotherDb));
        }

        [Test]
        public void ExportMany_should_respect_ContractName()
        {
            var container = new Container();
            container.RegisterExports(typeof(NamedOne));

            var named = container.Resolve<INamed>("blah");
            var one = container.Resolve<IOne>("blah");

            Assert.That(named, Is.Not.Null);
            Assert.That(one, Is.Not.Null);
        }

        [Test]
        public void Individual_Export_is_simply_added_to_ExportMany_settings()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(BothExportManyAndExport));

            var named = container.Resolve<INamed>("named");
            Assert.That(named, Is.Not.Null);

            var namedDefault = container.Resolve<INamed>();
            Assert.That(namedDefault, Is.Not.Null);
        }

        [Test]
        public void If_both_export_and_export_all_specifying_the_same_setup_Then_only_single_will_be_registered()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(WithBothTheSameExports));

            Assert.DoesNotThrow(() =>
                container.Resolve<WithBothTheSameExports>());
        }
    }
}
