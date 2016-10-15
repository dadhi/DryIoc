using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
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

        [Test]
        public void Mef_supports_ExportFactoryWithMetadata_for_non_shared_parts()
        {
            var service = Mef.GetExport<ImportsNamedServiceExportFactories>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.Value);
            Assert.IsNotNull(service.Value.NamedServiceFactories);

            var services = service.Value.NamedServiceFactories.OrderBy(s => s.Metadata.Name).ToArray();
            Assert.AreEqual(2, services.Length);
            Assert.AreEqual("One", services[0].Metadata.Name);
            Assert.AreEqual("Two", services[1].Metadata.Name);

            LazyNamedService1 ls1;
            LazyNamedService2 ls2;

            using (var s1 = services[0].CreateExport())
            {
                Assert.IsNotNull(s1.Value);
                Assert.IsInstanceOf<LazyNamedService1>(s1.Value);
                Assert.IsFalse(s1.Value.IsDisposed);
                ls1 = (LazyNamedService1)s1.Value;

                using (var s2 = services[1].CreateExport())
                {
                    Assert.IsNotNull(s2.Value);
                    Assert.IsInstanceOf<LazyNamedService2>(s2.Value);
                    Assert.IsFalse(s2.Value.IsDisposed);
                    Assert.IsNotNull(s1.Value);
                    Assert.IsFalse(s1.Value.IsDisposed);

                    ls2 = (LazyNamedService2)s2.Value;
                    Assert.IsNotNull(ls2.NonSharedDependency);
                    Assert.IsFalse(ls2.NonSharedDependency.IsDisposed);
                }

                Assert.IsTrue(ls2.IsDisposed);
                Assert.IsFalse(s1.Value.IsDisposed);
                Assert.IsNotNull(ls2.NonSharedDependency);
                Assert.IsTrue(ls2.NonSharedDependency.IsDisposed);
            }

            Assert.IsTrue(ls1.IsDisposed);
        }
    }
}
