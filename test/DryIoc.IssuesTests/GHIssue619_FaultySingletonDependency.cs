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
            Resolve_second_time_Fails_for_Lazy_of_singleton_failed_the_first_time_WithoutEagerSingleton_WithoutUseInterpretation();
            Resolve_second_time_Fails_for_Lazy_of_singleton_failed_the_first_time_WithoutEagerSingleton_rule();
            Resolve_second_time_Succeeds_for_Lazy_of_Singleton_failed_the_first_time_After_Replacing_the_registration();
            Resolve_second_time_Fails_for_Lazy_of_singleton_failed_the_first_time();
            Resolve_second_time_Succeeds_for_Lazy_of_Singleton_failed_the_first_time_After_Adding_new_registration();
            return 5;
        }

        [Test]
        public void Resolve_second_time_Fails_for_Lazy_of_singleton_failed_the_first_time()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(SingletonDependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            // controlling the fail in Dependency constructor
            var config = new Config { DependencyCtorThrows = true };
            container.Use(config);

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            config.DependencyCtorThrows = false;

            // Still failing because the singleton is saved in the singleton scope
            Assert.Throws<InvalidOperationException>(() =>
                container.Resolve<ServiceWithNormalImport>());

            // Still failing because Lazy does not re-evaluate the Value
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s3.DoWork());
        }

        [Test]
        public void Resolve_second_time_Fails_for_Lazy_of_singleton_failed_the_first_time_WithoutEagerSingleton_rule()
        {
            var container = new Container(
                Rules.Default.WithoutEagerCachingSingletonForFasterAccess()
            ).WithMef();

            container.RegisterExports(typeof(SingletonDependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            // controlling the fail in Dependency constructor
            var config = new Config { DependencyCtorThrows = true };
            container.Use(config);

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            config.DependencyCtorThrows = false;

            // Still failing because the singleton is saved in the singleton scope
            Assert.Throws<InvalidOperationException>(() =>
                container.Resolve<ServiceWithNormalImport>());

            // Still failing because Lazy does not re-evaluate the Value
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s3.DoWork());
        }

        [Test]
        public void Resolve_second_time_Fails_for_Lazy_of_singleton_failed_the_first_time_WithoutEagerSingleton_WithoutUseInterpretation()
        {
            var container = new Container(
                Rules.Default.WithoutEagerCachingSingletonForFasterAccess().WithoutUseInterpretation()
            ).WithMef();

            container.RegisterExports(typeof(SingletonDependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            // controlling the fail in Dependency constructor
            var config = new Config { DependencyCtorThrows = true };
            container.Use(config);

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            config.DependencyCtorThrows = false;

            // Still failing because the singleton is saved in the singleton scope
            Assert.Throws<InvalidOperationException>(() =>
                container.Resolve<ServiceWithNormalImport>());

            // Still failing because Lazy does not re-evaluate the Value
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s3.DoWork());
        }

        [Test]
        public void Resolve_second_time_Succeeds_for_Lazy_of_Singleton_failed_the_first_time_After_Adding_new_registration()
        {
            var container = new Container(
                Rules.Default.WithFactorySelector(Rules.SelectLastRegisteredFactory())
            ).WithMef();

            container.RegisterExports(typeof(SingletonDependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            var config = new Config { DependencyCtorThrows = true };
            container.Use(config);

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            config.DependencyCtorThrows = false;

            // Add the second registration of the SingletonDependency, and it will be resolved now due to the SelectLastRegisteredFactory rule
            container.RegisterExports(typeof(SingletonDependency));

            // Re-resolving the failed dependency that now succeeds, because now it looks to the
            var s2 = container.Resolve<ServiceWithNormalImport>();
            Assert.IsInstanceOf<SingletonDependency>(s2.Dependency);

            // Success because we are resolving the newly added Lazy
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.IsInstanceOf<SingletonDependency>(s3.LazyDependency.Value);
        }

        [Test]
        public void Resolve_second_time_Succeeds_for_Lazy_of_Singleton_failed_the_first_time_After_Replacing_the_registration()
        {
            var container = new Container().WithMef();

            container.RegisterExports(typeof(SingletonDependency), typeof(ServiceWithLazyImport), typeof(ServiceWithNormalImport));

            var config = new Config { DependencyCtorThrows = true };
            container.Use(config);

            // dependency initialization failed (test passes)
            var s1 = container.Resolve<ServiceWithLazyImport>();
            Assert.Throws<InvalidOperationException>(() =>
                s1.DoWork());

            config.DependencyCtorThrows = false;

            // Add the second registration of the SingletonDependency, and it will be resolved now due to the SelectLastRegisteredFactory rule
            container.RegisterExports(typeof(SingletonDependency),
                reg =>
                {
                    foreach (var export in reg.Exports)
                        export.IfAlreadyRegistered = IfAlreadyRegistered.Replace;
                    return reg;
                });

            // Re-resolving the failed dependency that now succeeds, because now it looks to the
            var s2 = container.Resolve<ServiceWithNormalImport>();
            Assert.IsInstanceOf<SingletonDependency>(s2.Dependency);

            // Success because we are resolving the newly replace Lazy
            var s3 = container.Resolve<ServiceWithLazyImport>();
            Assert.IsInstanceOf<SingletonDependency>(s3.LazyDependency.Value);
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        public class ServiceWithLazyImport
        {
            [Import]
            public Lazy<IDependency> LazyDependency { get; set; }

            public void DoWork() => LazyDependency.Value.DoWork();
        }

        [Export, PartCreationPolicy(CreationPolicy.NonShared)]
        public class ServiceWithNormalImport
        {
            [Import]
            public IDependency Dependency { get; set; }

            public void DoWork() => Dependency.DoWork();
        }

        public interface IDependency
        {
            void DoWork();
        }

        public class Config
        {
            public bool DependencyCtorThrows;
        }

        [Export(typeof(IDependency))]
        public class SingletonDependency : IDependency
        {
            public SingletonDependency(Config config)
            {
                if (config.DependencyCtorThrows)
                    throw new InvalidOperationException("The first initialization failed " +
                        "due to a temporary problem, e.g. database connection timeout.");
            }

            public void DoWork()
            {
            }
        }
    }
}