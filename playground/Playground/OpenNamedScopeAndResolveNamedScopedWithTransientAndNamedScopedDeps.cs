using Autofac;
using BenchmarkDotNet.Attributes;
using DryIoc;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    public class OpenNamedScopeAndResolveNamedScopedWithTransientAndNamedScopedDeps
    {
        public void DryIoc_test()
        {
            Measure(PrepareDryIoc());
        }

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
            container.Register<Parameter2>(Reuse.InWebRequest);
            container.Register<ScopedBlah>(Reuse.InWebRequest);

            return container;
        }

        public static object Measure(global::DryIoc.IContainer container)
        {
            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                return scope.Resolve<ScopedBlah>();
        }

        public void Autofac_test()
        {
            Measure(PrepareAutofac());
        }

        public static IContainer PrepareAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Parameter1>().AsSelf().InstancePerDependency();
            builder.RegisterType<Parameter2>().AsSelf().InstancePerMatchingLifetimeScope(Reuse.WebRequestScopeName);
            builder.RegisterType<ScopedBlah>().AsSelf().InstancePerMatchingLifetimeScope(Reuse.WebRequestScopeName);

            return builder.Build();
        }

        public static object Measure(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope(Reuse.WebRequestScopeName))
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
            public Parameter1 Parameter1 { get; set; }
            public Parameter2 Parameter2 { get; set; }

            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2)
            {
                Parameter1 = parameter1;
                Parameter2 = parameter2;
            }
        }

        #endregion

        [MemoryDiagnoser]
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