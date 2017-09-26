using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ContainerHierarchyTests
    {
        private CompositionContainer Mef => new CompositionContainer(new AssemblyCatalog(typeof(ILogTableManager).Assembly));

        private IContainer Container => CreateContainer();

        private static IContainer CreateContainer()
        {
            // set up the Container to work like Mef without changing the composable parts
            var container = new Container().WithMef().With(rules => rules.WithDefaultReuse(Reuse.Scoped));

            container.RegisterExports(new[] { typeof(ILogTableManager).GetAssembly() });
            return container;
        }

        private bool IsNonSharedOrAny(ComposablePartDefinition def)
        {
            var md = def.Metadata;
            var key = CompositionConstants.PartCreationPolicyMetadataName;
            var allowedPolicy = new[] { CreationPolicy.Any, CreationPolicy.NonShared };

            // Condition: CreationPolicy is not specified or is either Any or NonShared
            return !md.ContainsKey(key) || allowedPolicy.Contains((CreationPolicy)md[key]);
        }

        [Test]
        public void Mef_can_use_container_hierarchy_to_have_scoped_composable_parts()
        {
            var rootContainer = Mef;
            var rootCatalog = rootContainer.Catalog;
            var nonSharedPartsCatalog = new FilteredCatalog(rootCatalog, def => IsNonSharedOrAny(def));

            IDisposableScopedService scoped1, scoped2;
            IDisposableSingletonService singleton1, singleton2;

            using (var scope1 = new CompositionContainer(nonSharedPartsCatalog, rootContainer))
            {
                scoped1 = scope1.GetExport<IDisposableScopedService>().Value;
                singleton1 = scope1.GetExport<IDisposableSingletonService>().Value;
                Assert.IsFalse(scoped1.IsDisposed);
                Assert.IsFalse(singleton1.IsDisposed);

                using (var scope2 = new CompositionContainer(nonSharedPartsCatalog, rootContainer))
                {
                    scoped2 = scope2.GetExport<IDisposableScopedService>().Value;
                    singleton2 = scope2.GetExport<IDisposableSingletonService>().Value;
                    Assert.AreNotSame(scoped1, scoped2);
                    Assert.AreSame(singleton1, singleton2);
                }

                Assert.IsTrue(scoped2.IsDisposed);
                Assert.IsFalse(scoped1.IsDisposed);
                Assert.IsFalse(singleton2.IsDisposed);
            }

            Assert.IsTrue(scoped1.IsDisposed);
            Assert.IsFalse(singleton1.IsDisposed);

            rootContainer.Dispose();
            Assert.IsTrue(singleton1.IsDisposed);
        }

        [Test]
        public void DryIoc_can_use_container_hierarchy_to_have_scoped_same_Mef_composable_parts()
        {
            var container = Container;
            IDisposableScopedService scoped1, scoped2;
            IDisposableSingletonService singleton1, singleton2;

            using (var scope1 = container.OpenScope())
            {
                scoped1 = scope1.Resolve<IDisposableScopedService>();
                singleton1 = scope1.Resolve<IDisposableSingletonService>();
                Assert.IsFalse(scoped1.IsDisposed);
                Assert.IsFalse(singleton1.IsDisposed);

                using (var scope2 = container.OpenScope())
                {
                    scoped2 = scope2.Resolve<IDisposableScopedService>();
                    singleton2 = scope2.Resolve<IDisposableSingletonService>();
                    Assert.AreNotSame(scoped1, scoped2);
                    Assert.AreSame(singleton1, singleton2);
                }

                Assert.IsTrue(scoped2.IsDisposed);
                Assert.IsFalse(scoped1.IsDisposed);
                Assert.IsFalse(singleton2.IsDisposed);
            }

            Assert.IsTrue(scoped1.IsDisposed);
            Assert.IsFalse(singleton1.IsDisposed);

            container.Dispose();
            Assert.IsTrue(singleton1.IsDisposed);
        }
    }
}
