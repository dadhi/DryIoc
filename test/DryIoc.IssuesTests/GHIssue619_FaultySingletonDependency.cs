using DryIoc.MefAttributedModel;
using NUnit.Framework;
using System;
using System.ComponentModel.Composition;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public sealed class GHIssue619_FaultySingletonDependency : ITest
    {
        public int Run()
        {
            // Resolve_second_time_the_Lazy_failed_the_first_time_with_Lazy_singletons_rule();
            Resolve_second_time_the_Lazy_failed_the_first_time();
            return 1;
        }

        [Test]
        public void Resolve_second_time_the_Lazy_failed_the_first_time()
        {
            // default MEF reuse is a singleton
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Dependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            // should it fail or should it work? (test fails)
            Assert.Throws<InvalidOperationException>(() =>
                container.Resolve<ServiceWithNormalImport>());

            // should it fail or should it work?
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s3.DoWork());
        }

        public void Resolve_second_time_the_Lazy_failed_the_first_time_with_Lazy_singletons_rule()
        {
            // default MEF reuse is a singleton
            var container = new Container(Rules.Default.WithoutEagerCachingSingletonForFasterAccess()).WithMef();
            container.RegisterExports(typeof(Dependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            // should it fail or should it work? (test fails)
            Assert.Throws<InvalidOperationException>(() =>
                container.Resolve<ServiceWithNormalImport>());

            // should it fail or should it work?
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s3.DoWork());
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        public class ServiceWithLazyImport
        {
            [Import]
            private Lazy<IDependency> LazyDependency { get; set; }

            public void DoWork() => LazyDependency.Value.DoWork();
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        public class ServiceWithNormalImport
        {
            [Import]
            private IDependency Dependency { get; set; }

            public void DoWork() => Dependency.DoWork();
        }

        public interface IDependency
        {
            void DoWork();
        }

        [Export(typeof(IDependency))]
        public class Dependency : IDependency
        {
            static bool firstTime = true;

            public Dependency()
            {
                if (firstTime)
                {
                    firstTime = false;
                    throw new InvalidOperationException("The first initialization failed " +
                        "due to a temporary problem, e.g. database connection timeout.");
                }
            }

            public void DoWork()
            {
            }
        }
    }
}