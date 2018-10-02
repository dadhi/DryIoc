using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue355_UnexpectedSingletonDisposal
    {
        private IContainer Container { get; } = CreateContainer();

        private static IContainer CreateContainer()
        {
            var c = new Container().WithMef().With(rules => rules
                .WithDefaultReuse(Reuse.ScopedOrSingleton));

            c.RegisterExports(new[] { typeof(Issue355_UnexpectedSingletonDisposal).GetAssembly() });
            return c;
        }

        [Test]
        public void Externally_owned_singleton_shouldnt_be_tracked_and_disposed_of()
        {
            // set up my singleton
            var singleton = MySingletonService.Instance;
            Container.InjectPropertiesAndFields(singleton);
            Assert.IsFalse(singleton.IsDisposed);

            // when we don't import the singleton, everything is fine
            using (var s = Container.OpenScope())
            {
                s.Resolve<DoesntImportMySingleton>();
            }

            Assert.IsFalse(singleton.IsDisposed);
            Assert.IsFalse(singleton.DataContext.IsDisposed);

            // when we import it, the container tracks it and disposes of it with the scope
            using (var s = Container.OpenScope())
            {
                var us = s.Resolve<ImportsMySingleton>();
                Assert.AreSame(singleton, us.Singleton);
            }

            Assert.IsFalse(singleton.IsDisposed); // fails here
            Assert.IsFalse(singleton.DataContext.IsDisposed);

            // disposing of the container should release everything,
            Container.Dispose();
            Assert.IsTrue(singleton.IsDisposed);
            Assert.IsTrue(singleton.DataContext.IsDisposed);
        }

        [Export]
        public class DoesntImportMySingleton
        {
        }

        [Export]
        public class ImportsMySingleton
        {
            [Import]
            public MySingletonService Singleton { get; set; }
        }

        [PartCreationPolicy(CreationPolicy.Shared)]
        public class MySingletonService : IDisposable
        {
            [Export]
            public static MySingletonService Instance { get; } = new MySingletonService();

            [Import]
            public DataContext DataContext { get; set; }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Export]
        public class DataContext : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
