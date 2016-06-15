using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ImportManyTests
    {
        private CompositionContainer Mef { get; } = new CompositionContainer(new AssemblyCatalog(typeof(IPasswordHasher).Assembly));

        private IContainer Container => CreateContainer();

        private IContainer CreateContainer()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(new[] { typeof(IPasswordHasher).Assembly });
            return container;
        }

        [Test]
        private void ImportMany_works_with_importing_constructor_in_Mef1()
        {
            var pw = Mef.GetExport<PasswordVerifier1>();

            Assert.NotNull(pw);
            Assert.NotNull(pw.Value);
            Assert.AreEqual(3, pw.Value.Hashers.Count());
            Assert.IsTrue(pw.Value.ImportsSatisfied);
        }

        [Test]
        private void ImportMany_works_with_importing_constructor_in_Mef2()
        {
            var pw = Mef.GetExport<PasswordVerifier2>();

            Assert.NotNull(pw);
            Assert.NotNull(pw.Value);
            Assert.AreEqual(3, pw.Value.Hashers.Count());
            Assert.IsTrue(pw.Value.ImportsSatisfied);
        }

        [Test]
        private void ImportMany_works_with_property_injection_in_Mef3()
        {
            var pw = Mef.GetExport<PasswordVerifier3>();

            Assert.NotNull(pw);
            Assert.NotNull(pw.Value);
            Assert.AreEqual(3, pw.Value.Hashers.Count());
            Assert.IsTrue(pw.Value.ImportsSatisfied);
        }

        [Test]
        private void ImportMany_works_with_property_injection_in_Mef4()
        {
            var pw = Mef.GetExport<PasswordVerifier4>();

            Assert.NotNull(pw);
            Assert.NotNull(pw.Value);
            Assert.AreEqual(3, pw.Value.Hashers.Count());
            Assert.IsTrue(pw.Value.ImportsSatisfied);
        }
    }
}
