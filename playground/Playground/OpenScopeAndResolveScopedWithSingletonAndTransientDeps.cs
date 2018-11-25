using Autofac;
using BenchmarkDotNet.Attributes;
using DryIoc;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Lifestyle;
using LightInject;
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

        public static DependencyInjectionContainer PrepareGrace()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<Parameter1>();
                c.Export<Parameter2>().Lifestyle.Singleton();
                c.Export<ScopedBlah>().Lifestyle.SingletonPerScope();
            });

            return container;
        }

        public static object Measure(DependencyInjectionContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Locate<ScopedBlah>();
        }

        public static ServiceContainer PrepareLightInject()
        {
            var c = new ServiceContainer();

            c.Register<Parameter1>();
            c.Register<Parameter2>(new PerContainerLifetime());
            c.Register<ScopedBlah>(new PerScopeLifetime());

            return c;
        }

        public static object Measure(ServiceContainer container)
        {
            using (var scope = container.BeginScope())
                return scope.GetInstance<ScopedBlah>();
        }

        #region CUT

        internal class Parameter1 { }

        internal class Parameter2 { }

        internal class ScopedBlah
        {
            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2) { }
        }

        #endregion

        /* ## 9.11.2018
                Method |       Mean |       Error |      StdDev |     Median | Ratio | RatioSD |
         ------------- |-----------:|------------:|------------:|-----------:|------:|--------:|
          BmarkAutofac | 2,267.0 ns | 224.7508 ns | 662.6829 ns | 1,953.0 ns | 20.66 |    4.49 |
           BmarkDryIoc |   127.1 ns |   0.8580 ns |   0.7165 ns |   127.0 ns |  1.00 |    0.00 |

        ## 25.11.2018: Adding Grace and LightInject 

        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156252 Hz, Resolution=463.7677 ns, Timer=TSC
        .NET Core SDK=2.1.500
          [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
          DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT

               Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
         BmarkAutofac | 1,565.4 ns | 8.9117 ns | 8.3360 ns | 11.93 |    0.09 |      0.5302 |           - |           - |              2504 B |
          BmarkDryIoc |   131.3 ns | 0.6650 ns | 0.5895 ns |  1.00 |    0.00 |      0.0608 |           - |           - |               288 B |
           BmarkGrace |   148.0 ns | 0.7295 ns | 0.6823 ns |  1.13 |    0.01 |      0.0608 |           - |           - |               288 B |

        */
        [MemoryDiagnoser]
        public class FirstTime_OpenScope_Resolve
        {
            private static readonly IContainer _autofac = PrepareAutofac();
            private static readonly global::DryIoc.IContainer _dryioc = PrepareDryIoc();
            private static readonly DependencyInjectionContainer _grace = PrepareGrace();
            private static readonly ServiceContainer _lightInject = PrepareLightInject();

            [Benchmark]
            public object BmarkAutofac() => Measure(_autofac);

            [Benchmark(Baseline = true)]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark]
            public object BmarkGrace() => Measure(_grace);

            [Benchmark]
            public object BmarkLightInject() => Measure(_lightInject);
        }

        /*
        ## Initial result

               Method |        Mean |    StdErr |    StdDev |      Median | Scaled | Scaled-StdDev |  Gen 0 | Allocated |
        ------------- |------------ |---------- |---------- |------------ |------- |-------------- |------- |---------- |
         BmarkAutofac |  47.0789 us | 0.6439 us | 6.4390 us |  44.9588 us |   0.12 |          0.02 | 7.7637 |  14.39 kB |
          BmarkDryIoc | 393.8075 us | 1.1701 us | 4.3782 us | 393.4204 us |   1.00 |          0.00 |      - |    6.4 kB |
        
        ## 09.11.2018

        BenchmarkDotNet=v0.11.2, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
            .NET Core SDK=2.1.403
        [Host]     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
        DefaultJob : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
        Method       |      Mean |     Error |   StdDev |    Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        -------------|----------:|----------:|---------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
        BmarkAutofac |  47.21 us |  4.363 us | 12.86 us |  54.11 us |  0.17 |    0.05 |      5.1880 |           - |           - |            24.15 KB |
        BmarkDryIoc  | 282.05 us | 12.234 us | 10.85 us | 279.34 us |  1.00 |    0.00 |      1.4648 |      0.4883 |           - |             8.19 KB |

        ## 07.11.2018: OMG !!! Result with TryInterpret
        
        BenchmarkDotNet=v0.11.2, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
        Intel Core i7-8750H CPU 2.20GHz(Coffee Lake), 1 CPU, 12 logical and 6 physical cores
        Frequency=2156252 Hz, Resolution=463.7677 ns, Timer=TSC
        .NET Core SDK=2.1.500
          [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
          DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT

               Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
         BmarkAutofac | 29.542 us | 0.3323 us | 0.3108 us |  5.60 |    0.07 |      5.2185 |           - |           - |            24.15 KB |
          BmarkDryIoc |  5.280 us | 0.0382 us | 0.0357 us |  1.00 |    0.00 |      1.4114 |           - |           - |             6.52 KB |

        ## 25.11.2018: Adding Grace and LighInject for comparison

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156252 Hz, Resolution=463.7677 ns, Timer=TSC
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


           Method |       Mean |      Error |      StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
----------------- |-----------:|-----------:|------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
     BmarkAutofac | 1,581.1 ns | 74.3184 ns | 124.1694 ns | 11.97 |    1.24 |      0.5302 |           - |           - |              2504 B |
      BmarkDryIoc |   133.2 ns |  0.5088 ns |   0.4511 ns |  1.00 |    0.00 |      0.0608 |           - |           - |               288 B |
       BmarkGrace |   148.5 ns |  0.6353 ns |   0.5943 ns |  1.11 |    0.00 |      0.0608 |           - |           - |               288 B |
 BmarkLightInject |   648.4 ns |  5.4780 ns |   5.1241 ns |  4.87 |    0.05 |      0.1488 |           - |           - |               704 B |

 */

        [MemoryDiagnoser]
        public class Register_FirstTime_OpenScope_Resolve
        {
            [Benchmark]
            public object BmarkAutofac() => Measure(PrepareAutofac());

            [Benchmark(Baseline = true)]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            [Benchmark]
            public object BmarkLightInject() => Measure(PrepareLightInject());
        }
    }
}
