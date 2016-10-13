using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class MefExportFactoryTests
    {
        private CompositionContainer Mef => new CompositionContainer(new AssemblyCatalog(typeof(ILogTableManager).Assembly));

        [Test]
        public void Mef_supports_ExportFactory_for_non_shared_parts()
        {
            var mef = Mef;
            var service = mef.GetExport<UsesExportFactoryOfNonSharedService>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Value);
            Assert.IsNotNull(service.Value.Factory);

            NonSharedService nonSharedService;
            using (var export = service.Value.Factory.CreateExport())
            {
                nonSharedService = export.Value;
                Assert.IsNotNull(nonSharedService);
                Assert.IsFalse(nonSharedService.IsDisposed);

                Assert.IsNotNull(nonSharedService.NonSharedDependency);
                Assert.IsFalse(nonSharedService.NonSharedDependency.IsDisposed);

                Assert.IsNotNull(nonSharedService.SharedDependency);
                Assert.IsFalse(nonSharedService.SharedDependency.IsDisposed);
            }

            Assert.IsTrue(nonSharedService.IsDisposed);
            Assert.IsTrue(nonSharedService.NonSharedDependency.IsDisposed);
            Assert.IsFalse(nonSharedService.SharedDependency.IsDisposed);

            mef.Dispose();
            Assert.IsTrue(nonSharedService.IsDisposed);
            Assert.IsTrue(nonSharedService.NonSharedDependency.IsDisposed);
            Assert.IsTrue(nonSharedService.SharedDependency.IsDisposed);
        }

        [Test]
        public void Mef_doesnt_support_ExportFactory_for_shared_parts()
        {
            var mef = Mef;

            Assert.Throws<ImportCardinalityMismatchException>(() =>
            mef.GetExport<UsesExportFactoryOfSharedService>());
        }
    }
}
