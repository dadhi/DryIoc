using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.AttributedRegistration
{
    [TestFixture]
    public class ExportPublicTypesTests
    {
        [Test]
        public void When_class_public_types_exported_as_singleton_Then_resolving_types_will_return_the_same_instance()
        {
            var container = new Container();
            container.RegisterExportedAssemblies(typeof(ISomeDb).Assembly);

            var someDb = container.Resolve<ISomeDb>();
            var anotherDb = container.Resolve<IAnotherDb>();

            Assert.That(someDb, Is.SameAs(anotherDb));
        }

        [Test]
        public void When_generic_class_public_types_exported_Then_resolving_internal_type_should_throw()
        {
            var container = new Container();
            container.RegisterExportedAssemblies(typeof(ISomeDb).Assembly);

            container.Resolve<ISomeDb<int>>();

            Assert.Throws<ContainerException>(
                () => container.Resolve<DbMan<int>>());
        }
    }
}
