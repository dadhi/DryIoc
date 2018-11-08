using Autofac;
using BenchmarkDotNet.Attributes;
using DryIoc;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    public class OpenScopeAndResolveScopedWithSingletonAndTransientDeps
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
            container.Register<Parameter2>(Reuse.Singleton);
            container.Register<ScopedBlah>(Reuse.Scoped);

            return container;
        }

        public static object Measure(global::DryIoc.IContainer container)
        {
            using (var scope = container.OpenScope())
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

        /* 9.11.2018
        Method |       Mean |       Error |      StdDev |     Median | Ratio | RatioSD |
 ------------- |-----------:|------------:|------------:|-----------:|------:|--------:|
  BmarkAutofac | 2,267.0 ns | 224.7508 ns | 662.6829 ns | 1,953.0 ns | 20.66 |    4.49 |
   BmarkDryIoc |   127.1 ns |   0.8580 ns |   0.7165 ns |   127.0 ns |  1.00 |    0.00 |

         */

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
        // 09.11.2018
        //-----------
        //BenchmarkDotNet=v0.11.2, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
        //Intel Core i7-8750H CPU 2.20GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        //Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
        //    .NET Core SDK=2.1.403
        //[Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
        //DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
        //Method       |      Mean |     Error |   StdDev |    Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        //-------------|----------:|----------:|---------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
        //BmarkAutofac |  47.21 us |  4.363 us | 12.86 us |  54.11 us |  0.17 |    0.05 |      5.1880 |           - |           - |            24.15 KB |
        //BmarkDryIoc  | 282.05 us | 12.234 us | 10.85 us | 279.34 us |  1.00 |    0.00 |      1.4648 |      0.4883 |           - |             8.19 KB |
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