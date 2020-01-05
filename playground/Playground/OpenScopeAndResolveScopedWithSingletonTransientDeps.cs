using System;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using Grace.DependencyInjection;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    public class OpenScopeAndResolveScopedWithSingletonTransientDeps
    {
        public static DryIoc.IContainer PrepareDryIoc()
        {
            var container = new Container();

            container.Register<Parameter1>(Reuse.Transient);
            container.Register<Parameter2>(Reuse.Singleton);

            container.Register<ScopedBlah>(Reuse.Scoped);

            return container;
        }

        public static object Measure(DryIoc.IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<ScopedBlah>();
        }

        public static IServiceProvider PrepareMsDi()
        {
            var services = new ServiceCollection();

            services.AddTransient<Parameter1>();
            services.AddSingleton<Parameter2>();

            services.AddScoped<ScopedBlah>();

            return services.BuildServiceProvider();
        }

        public static object Measure(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
                return scope.ServiceProvider.GetRequiredService<ScopedBlah>();
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

        //public static Lamar.Container PrepareLamar() => 
        //    new Lamar.Container(c =>
        //    {
        //        c.AddTransient<Parameter1>();
        //        c.AddSingleton<Parameter2>();
        //        c.AddScoped<ScopedBlah>();
        //    });

        //public static object Measure(Lamar.Container container)
        //{
        //    using (var scope = container.CreateScope())
        //        return scope.ServiceProvider.GetService<ScopedBlah>();
        //}

        #region CUT

        internal class Parameter1 { }
        internal class Parameter2 { }

        internal class ScopedBlah
        {
            public Parameter1 Parameter1 { get; }
            public Parameter2 Parameter2 { get; }

            public ScopedBlah(Parameter1 parameter1, Parameter2 parameter2)
            {
                Parameter1 = parameter1;
                Parameter2 = parameter2;
            }
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

         ## 25.11.2018: Adding MS.DI as reference

                            Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                      BmarkAutofac | 1,554.5 ns | 7.3436 ns | 6.1322 ns |  6.49 |    0.06 |      0.5322 |           - |           - |              2512 B |
                       BmarkDryIoc |   131.3 ns | 0.2331 ns | 0.2181 ns |  0.55 |    0.00 |      0.0627 |           - |           - |               296 B |
 BmarkMicrosoftDependencyInjection |   239.5 ns | 1.5359 ns | 1.4367 ns |  1.00 |    0.00 |      0.0691 |           - |           - |               328 B |
                        BmarkGrace |   146.3 ns | 0.8087 ns | 0.7565 ns |  0.61 |    0.01 |      0.0677 |           - |           - |               320 B |
                  BmarkLightInject |   593.7 ns | 1.5277 ns | 1.4290 ns |  2.48 |    0.02 |      0.1554 |           - |           - |               736 B |

        ## 25.01.2019: After some work on perf and memory utilization

                            Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |   114.9 ns | 0.3553 ns | 0.3324 ns |  0.47 |    0.00 |      0.0627 |           - |           - |               296 B |
                        BmarkGrace |   153.3 ns | 0.4556 ns | 0.4262 ns |  0.63 |    0.00 |      0.0677 |           - |           - |               320 B |
 BmarkMicrosoftDependencyInjection |   243.0 ns | 1.0791 ns | 1.0094 ns |  1.00 |    0.00 |      0.0691 |           - |           - |               328 B |
                  BmarkLightInject |   609.3 ns | 3.6483 ns | 3.4126 ns |  2.51 |    0.02 |      0.1554 |           - |           - |               736 B |
                      BmarkAutofac | 1,654.2 ns | 6.8377 ns | 5.7098 ns |  6.81 |    0.04 |      0.5322 |           - |           - |              2512 B |

        */
        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class FirstTimeOpenScopeResolve
        {
            private readonly IContainer _autofac = PrepareAutofac();
            private readonly DryIoc.IContainer _dryioc = PrepareDryIoc();
            private readonly IServiceProvider _msDi = PrepareMsDi();
            private readonly DependencyInjectionContainer _grace = PrepareGrace();
            private readonly ServiceContainer _lightInject = PrepareLightInject();

            [Benchmark]
            public object BmarkAutofac() => Measure(_autofac);

            [Benchmark]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(_msDi);

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

## 25.11.2018: Adding MS.DI as reference

                            Method |       Mean |     Error |    StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|----------:|----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
                      BmarkAutofac |  29.486 us | 0.1535 us | 0.1436 us |   7.75 |    0.08 |      5.2185 |           - |           - |            24.15 KB |
                       BmarkDryIoc |   4.032 us | 0.0175 us | 0.0163 us |   1.06 |    0.01 |      1.2131 |           - |           - |             5.61 KB |
 BmarkMicrosoftDependencyInjection |   3.803 us | 0.0273 us | 0.0255 us |   1.00 |    0.00 |      0.9232 |           - |           - |             4.26 KB |
                        BmarkGrace | 540.672 us | 4.1734 us | 3.9038 us | 142.19 |    1.34 |      5.8594 |      2.9297 |           - |            30.24 KB |
                  BmarkLightInject | 426.462 us | 4.2650 us | 3.9895 us | 112.15 |    1.27 |      6.8359 |      3.4180 |           - |            32.42 KB |

## 25.01.2019: Results after some work on memory and performance

                            Method |       Mean |     Error |    StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|----------:|----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |   3.193 us | 0.0117 us | 0.0110 us |   0.84 |    0.01 |      0.7782 |           - |           - |              3.6 KB |
 BmarkMicrosoftDependencyInjection |   3.823 us | 0.0297 us | 0.0278 us |   1.00 |    0.00 |      0.9232 |           - |           - |             4.26 KB |
                      BmarkAutofac |  30.870 us | 0.2843 us | 0.2660 us |   8.08 |    0.08 |      5.2185 |           - |           - |            24.15 KB |
                  BmarkLightInject | 435.833 us | 3.6918 us | 3.4533 us | 114.01 |    1.03 |      6.8359 |      3.4180 |           - |            32.42 KB |
                        BmarkGrace | 552.673 us | 3.3771 us | 3.1590 us | 144.57 |    1.06 |      5.8594 |      2.9297 |           - |            30.24 KB |

## v4.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT


|                            Method |       Mean |     Error |    StdDev |  Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |-----------:|----------:|----------:|-------:|--------:|-------:|-------:|------:|----------:|
|                       BmarkDryIoc |   2.934 us | 0.0192 us | 0.0179 us |   0.74 |    0.02 | 0.6409 | 0.0038 |     - |   2.95 KB |
| BmarkMicrosoftDependencyInjection |   3.941 us | 0.0752 us | 0.0805 us |   1.00 |    0.00 | 1.3428 | 0.0191 |     - |   6.18 KB |
|                      BmarkAutofac |  28.563 us | 0.2652 us | 0.2215 us |   7.22 |    0.15 | 6.5002 | 0.2136 |     - |  29.97 KB |
|                        BmarkGrace | 473.166 us | 2.5459 us | 2.3814 us | 120.03 |    2.83 | 5.8594 | 2.9297 |     - |  29.22 KB |
|                  BmarkLightInject | 482.806 us | 1.5686 us | 1.4672 us | 122.47 |    2.68 | 6.8359 | 2.9297 |     - |  32.33 KB |
 */
        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerRegister_FirstTimeOpenScopeResolve
        {
            [Benchmark]
            public object BmarkAutofac() => Measure(PrepareAutofac());

            [Benchmark]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(PrepareMsDi());

            [Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            [Benchmark]
            public object BmarkLightInject() => Measure(PrepareLightInject());
        }
    }
}
