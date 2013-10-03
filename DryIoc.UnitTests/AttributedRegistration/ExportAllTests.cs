using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
    [TestFixture]
    public class ExportAllTests
    {
        [Test]
        public void When_class_public_types_exported_as_singleton_Then_resolving_types_will_return_the_same_instance()
        {
            var container = new Container();
            container.RegisterExported(typeof(ISomeDb).Assembly);

            var someDb = container.Resolve<ISomeDb>();
            var anotherDb = container.Resolve<IAnotherDb>();

            Assert.That(someDb, Is.SameAs(anotherDb));
        }

        [Test]
        public void When_generic_class_all_public_types_exported_Then_resolving_internal_type_should_throw()
        {
            var container = new Container();
            container.RegisterExported(typeof(ISomeDb).Assembly);

            container.Resolve<ISomeDb<int>>();

            Assert.Throws<ContainerException>(
                () => container.Resolve<DbMan<int>>());
        }

        [Test]
        public void ExportAll_should_respect_ContractName()
        {
            var container = new Container();
            container.RegisterExported(typeof(NamedOne));

            var named = container.Resolve<INamed>("blah");
            var one = container.Resolve<IOne>("blah");

            Assert.That(named, Is.Not.Null);
            Assert.That(one, Is.Not.Null);
        }

        [Test]
        public void Individual_Export_should_override_ExportAll_settings()
        {
            var container = new Container(AttributedRegistrator.DefaultSetup);
            container.RegisterExported(typeof(BothExportAllAndExport));

            var named = container.Resolve<INamed>("named");
            Assert.That(named, Is.Not.Null);

            var namedDefault = container.Resolve<INamed>(IfUnresolved.ReturnNull);
            Assert.That(namedDefault, Is.Null);
        }
    }

    public interface IOne {}

    public interface INamed {}

    [ExportAll(ContractName = "blah")]
    public class NamedOne : INamed, IOne {}

    [ExportAll, Export("named", typeof(INamed))]
    public class BothExportAllAndExport : INamed, IOne
    {
    }
}
