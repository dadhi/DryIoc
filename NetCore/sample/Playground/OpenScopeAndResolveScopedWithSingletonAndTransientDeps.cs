using Autofac;
using BenchmarkDotNet.Attributes;
using DryIoc;
using NUnit.Framework;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    [TestFixture]
    public class OpenScopeAndResolveScopedWithSingletonAndTransientDeps
    {
        [Test]
        public void DryIoc_test()
        {
            Measure(PrepareDryIoc());
        }

        // This test is for profiling
        [Test, Explicit]
        public void DryIoc_test_1000_times()
        {
            for (var i = 0; i < 1000; i++)
            {
                Measure(PrepareDryIoc());
            }
        }

        public static global::DryIoc.IContainer PrepareDryIoc()
        {
            var container = new Container();

            container.Register<Parameter1>(Reuse.Transient);
            container.Register<Parameter2>(Reuse.Singleton);
            container.Register<ScopedBlah>(Reuse.Scoped);

            return container;
        }

        public static object Measure(global::DryIoc.IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<ScopedBlah>();
        }

        [Test]
        public void Autofac_test()
        {
            Measure(PrepareAutofac());
        }

        public static IContainer PrepareAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Parameter1>().AsSelf().InstancePerDependency();
            builder.RegisterType<Parameter2>().AsSelf().SingleInstance();
            builder.RegisterType<ScopedBlah>().AsSelf().InstancePerLifetimeScope();

            return builder.Build();
        }

        public static object Measure(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Resolve<ScopedBlah>();
        }

        #region CUT

        internal class Parameter1
        {

        }

        internal class Parameter2
        {

        }

        internal class ScopedBlah
        {
            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2)
            {
            }
        }

        #endregion

        public class BenchmarkResolution
        {
            private IContainer _autofac = PrepareAutofac();

            private global::DryIoc.IContainer _dryioc = PrepareDryIoc();

            [Benchmark]
            public object BmarkAutofac()
            {
                return Measure(_autofac);
            }

            [Benchmark(Baseline = true)]
            public object BmarkDryIoc()
            {
                return Measure(_dryioc);
            }
        }

        //       Method |        Mean |    StdErr |    StdDev |      Median | Scaled | Scaled-StdDev |  Gen 0 | Allocated |
        //------------- |------------ |---------- |---------- |------------ |------- |-------------- |------- |---------- |
        // BmarkAutofac |  47.0789 us | 0.6439 us | 6.4390 us |  44.9588 us |   0.12 |          0.02 | 7.7637 |  14.39 kB |
        //  BmarkDryIoc | 393.8075 us | 1.1701 us | 4.3782 us | 393.4204 us |   1.00 |          0.00 |      - |    6.4 kB |
        //
        [MemoryDiagnoser]
        public class BenchmarkRegistrationAndResolution
        {
            [Benchmark]
            public object BmarkAutofac()
            {
                return Measure(PrepareAutofac());
            }

            [Benchmark(Baseline = true)]
            public object BmarkDryIoc()
            {
                return Measure(PrepareDryIoc());
            }
        }
    }
}