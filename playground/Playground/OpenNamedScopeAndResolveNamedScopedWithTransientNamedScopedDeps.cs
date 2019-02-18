using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    public class OpenNamedScopeAndResolveNamedScopedWithTransientNamedScopedDeps
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

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class BenchmarkResolution
        {
            /*
                   Method |       Mean |      Error |     StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
            ------------- |-----------:|-----------:|-----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
              BmarkDryIoc |   366.7 ns |  0.3240 ns |  0.2872 ns |  1.00 |    0.00 |      0.1183 |           - |           - |               560 B |
             BmarkAutofac | 2,392.4 ns | 15.0952 ns | 11.7853 ns |  6.52 |    0.03 |      0.8125 |           - |           - |              3840 B |
             */
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

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class BenchmarkRegistrationAndResolution
        {
            /*
       Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
  BmarkDryIoc |  4.394 us | 0.0220 us | 0.0206 us |  1.00 |    0.00 |      0.9842 |           - |           - |             4.56 KB |
 BmarkAutofac | 37.145 us | 0.2879 us | 0.2404 us |  8.45 |    0.06 |      6.5918 |           - |           - |             30.4 KB |
             */
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