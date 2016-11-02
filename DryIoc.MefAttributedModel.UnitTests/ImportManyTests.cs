using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportManyTests
    {
        private IContainer Container
        {
            get { return CreateContainer(); }
        }

        private IContainer CreateContainer()
        {
            var container = new Container().WithMef();
            container.RegisterExports(new[] { typeof(IPasswordHasher).GetAssembly() });
            return container;
        }

        [Test]
        public void ImportMany_works_with_importing_constructor_in_DryIoc1()
        {
            var pw = Container.Resolve<PasswordVerifier1>();

            Assert.NotNull(pw);
            Assert.AreEqual(3, pw.Hashers.Count());
            Assert.IsTrue(pw.ImportsSatisfied);
        }

        [Test]
        public void ImportMany_works_with_importing_constructor_in_DryIoc2()
        {
            var pw = Container.Resolve<PasswordVerifier2>();

            Assert.NotNull(pw);
            Assert.AreEqual(3, pw.Hashers.Count());
            Assert.IsTrue(pw.ImportsSatisfied);
        }

        [Test]
        public void ImportMany_works_with_property_injection_in_DryIoc3()
        {
            var pw = Container.Resolve<PasswordVerifier3>();

            Assert.NotNull(pw);
            Assert.AreEqual(3, pw.Hashers.Count());
            Assert.IsTrue(pw.ImportsSatisfied);
        }

        [Test]
        public void ImportMany_works_with_property_injection_in_DryIoc4()
        {
            var pw = Container.Resolve<PasswordVerifier4>();

            Assert.NotNull(pw);
            Assert.AreEqual(3, pw.Hashers.Count());
            Assert.IsTrue(pw.ImportsSatisfied);
        }

        [Test]
        public void ImportMany_with_required_service_type_should_work()
        {
            var container = new Container(Rules.Default.WithMefAttributedModel());

            container.RegisterExports(typeof(RequiresManyOfType), typeof(SomeDep));

            var r = container.Resolve<RequiresManyOfType>();
            Assert.IsTrue(r.Deps.Any());
        }

        [Test]
        public void ImportMany_with_service_key_should_work()
        {
            var container = new Container(Rules.Default.WithMefAttributedModel());

            container.RegisterExports(typeof(RequiresManyOfName), typeof(BlahDep), typeof(HuhDep));

            var r = container.Resolve<RequiresManyOfName>();
            Assert.IsInstanceOf<BlahDep>(r.Deps.Single());
        }
    }
}
