using System.ComponentModel.Design;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    public class ResolveInstancePerDependencyWith2ParametersOncePerContainer
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
            container.Register<Parameter2>(Reuse.Transient);
            container.Register<InstancePerDependency>(Reuse.Transient);

            return container;
        }

        public static object Measure(global::DryIoc.IContainer container)
        {
            return container.Resolve<InstancePerDependency>();
        }

        public void Autofac_test()
        {
            Measure(PrepareAutofac());
        }

        public static IContainer PrepareAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Parameter1>().AsSelf().InstancePerDependency();
            builder.RegisterType<Parameter2>().AsSelf().InstancePerDependency();
            builder.RegisterType<InstancePerDependency>().AsSelf().InstancePerDependency();

            return builder.Build();
        }

        public static object Measure(IContainer container)
        {
            return container.Resolve<InstancePerDependency>();
        }

        #region CUT

        internal class Parameter1
        {

        }

        internal class Parameter2
        {

        }

        internal class InstancePerDependency
        {
            public InstancePerDependency(Parameter1 parameter1, Parameter2 parameter2)
            {
            }
        }

        #endregion

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
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

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class BenchmarkRegistrationAndResolution
        {
            /*
       Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
  BmarkDryIoc |  2.673 us | 0.0065 us | 0.0061 us |  1.00 |    0.00 |      0.6523 |           - |           - |             3.02 KB |
 BmarkAutofac | 34.457 us | 0.2094 us | 0.1958 us | 12.89 |    0.09 |      6.1035 |           - |           - |            28.32 KB |
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