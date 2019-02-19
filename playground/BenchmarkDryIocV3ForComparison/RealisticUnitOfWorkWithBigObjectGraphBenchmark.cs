using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IContainer = DryIoc.IContainer;

namespace BenchmarkDryIocV3ForComparison
{
    public class RealisticUnitOfWorkWithBigObjectGraphBenchmark
    {
        public static IContainer PrepareDryIoc()
        {
            var container = new Container();

            container.Register<R>(Reuse.Scoped);

            container.Register<Scoped1>(Reuse.Scoped);
            container.Register<Scoped2>(Reuse.Scoped);

            container.Register<Trans1>(Reuse.Transient);
            container.Register<Trans2>(Reuse.Transient);

            container.Register<Single1>(Reuse.Singleton);
            container.Register<Single2>(Reuse.Singleton);

            container.RegisterDelegate(r => new ScopedFac1(r.Resolve<Scoped1>(), r.Resolve<Scoped3>(), r.Resolve<Single1>(), r.Resolve<SingleObj1>()));
            container.RegisterDelegate(r => new ScopedFac2(r.Resolve<Scoped2>(), r.Resolve<Scoped4>(), r.Resolve<Single2>(), r.Resolve<SingleObj2>()));

            container.UseInstance(new SingleObj1());
            container.UseInstance(new SingleObj2());

            // level 2

            container.Register<Scoped3>(Reuse.Scoped);
            container.Register<Scoped4>(Reuse.Scoped);

            container.Register<Scoped12>(Reuse.Scoped);
            container.Register<Scoped22>(Reuse.Scoped);

            container.Register<Single12>(Reuse.Singleton);
            container.Register<Single22>(Reuse.Singleton);

            container.Register<Trans12>(Reuse.Transient);
            container.Register<Trans22>(Reuse.Transient);

            container.RegisterDelegate(r => new ScopedFac12());
            container.RegisterDelegate(r => new ScopedFac22());

            container.UseInstance(new SingleObj12());
            container.UseInstance(new SingleObj22());

            return container;
        }

        public static object Measure(IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<R>();
        }

        public static ServiceCollection GetServices()
        {
            var services = new ServiceCollection();

            services.AddScoped<R>();

            services.AddScoped<Scoped1>();
            services.AddScoped<Scoped2>();

            services.AddTransient<Trans1>();
            services.AddTransient<Trans2>();

            services.AddSingleton<Single1>();
            services.AddSingleton<Single2>();

            services.AddScoped(s => new ScopedFac1(s.GetService<Scoped1>(), s.GetService<Scoped3>(), s.GetService<Single1>(), s.GetService<SingleObj1>()));
            services.AddScoped(s => new ScopedFac2(s.GetService<Scoped2>(), s.GetService<Scoped4>(), s.GetService<Single2>(), s.GetService<SingleObj2>()));

            services.AddSingleton(new SingleObj1());
            services.AddSingleton(new SingleObj2());

            // Level 2

            services.AddScoped<Scoped3>();
            services.AddScoped<Scoped4>();

            services.AddScoped<Scoped12>();
            services.AddScoped<Scoped22>();

            services.AddSingleton<Single12>();
            services.AddSingleton<Single22>();

            services.AddTransient<Trans12>();
            services.AddTransient<Trans22>();

            services.AddScoped(_ => new ScopedFac12());
            services.AddScoped(_ => new ScopedFac22());

            services.AddSingleton(new SingleObj12());
            services.AddSingleton(new SingleObj22());

            return services;
        }

        public static IServiceProvider PrepareMsDi() => GetServices().BuildServiceProvider();

        public static object Measure(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
                return scope.ServiceProvider.GetService<R>();
        }

        public static IServiceProvider PrepareDryIocMsDi() => new Container().WithDependencyInjectionAdapter(GetServices()).BuildServiceProvider();

        public static DependencyInjectionContainer PrepareGrace()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                c.Export<R>().Lifestyle.SingletonPerScope();

                c.Export<Scoped1>().Lifestyle.SingletonPerScope();
                c.Export<Scoped2>().Lifestyle.SingletonPerScope();

                c.Export<Trans1>();
                c.Export<Trans2>();

                c.Export<Single1>().Lifestyle.Singleton();
                c.Export<Single2>().Lifestyle.Singleton();

                c.ExportFactory<Scoped1, Scoped3, Single1, SingleObj1, ScopedFac1>((scoped1, scoped3, single1, singleObj1) => new ScopedFac1(scoped1, scoped3, single1, singleObj1)).Lifestyle.SingletonPerScope();
                c.ExportFactory<Scoped2, Scoped4, Single2, SingleObj2, ScopedFac2>((scoped2, scoped4, single2, singleObj2) => new ScopedFac2(scoped2, scoped4, single2, singleObj2)).Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj1());
                c.ExportInstance(new SingleObj2());

                // Level 3

                c.Export<Scoped3>().Lifestyle.SingletonPerScope();
                c.Export<Scoped4>().Lifestyle.SingletonPerScope();

                c.Export<Scoped12>().Lifestyle.SingletonPerScope();
                c.Export<Scoped22>().Lifestyle.SingletonPerScope();

                c.Export<Single12>().Lifestyle.Singleton();
                c.Export<Single22>().Lifestyle.Singleton();

                c.Export<Trans12>();
                c.Export<Trans22>();

                c.ExportFactory(() => new ScopedFac12()).Lifestyle.SingletonPerScope();
                c.ExportFactory(() => new ScopedFac22()).Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj12());
                c.ExportInstance(new SingleObj22());
            });

            return container;
        }

        public static IServiceProvider PrepareGraceMsDi()
        {
            var container = new DependencyInjectionContainer();

            container.Populate(GetServices());

            return container.Locate<IServiceProvider>();
        }

        public static IServiceProvider PrepareAutofacMsDi()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(GetServices());

            var container = containerBuilder.Build();

            return new AutofacServiceProvider(container);
        }

        public static object Measure(DependencyInjectionContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Locate<R>();
        }

        public static Autofac.IContainer PrepareAutofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<R>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Scoped1>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Scoped2>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Trans1>().AsSelf().InstancePerDependency();
            builder.RegisterType<Trans2>().AsSelf().InstancePerDependency();

            builder.RegisterType<Single1>().AsSelf().SingleInstance();
            builder.RegisterType<Single2>().AsSelf().SingleInstance();

            builder.Register(c => new ScopedFac1(c.Resolve<Scoped1>(), c.Resolve<Scoped3>(), c.Resolve<Single1>(), c.Resolve<SingleObj1>())).AsSelf().InstancePerLifetimeScope();
            builder.Register(c => new ScopedFac2(c.Resolve<Scoped2>(), c.Resolve<Scoped4>(), c.Resolve<Single2>(), c.Resolve<SingleObj2>())).AsSelf().InstancePerLifetimeScope();

            builder.RegisterInstance(new SingleObj1()).AsSelf();
            builder.RegisterInstance(new SingleObj2()).AsSelf();

            // Level 2

            builder.RegisterType<Scoped3>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Scoped4>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Scoped12>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Scoped22>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Single12>().AsSelf().SingleInstance();
            builder.RegisterType<Single22>().AsSelf().SingleInstance();

            builder.RegisterType<Trans12>().AsSelf().InstancePerDependency();
            builder.RegisterType<Trans22>().AsSelf().InstancePerDependency();

            builder.Register(r => new ScopedFac12());
            builder.Register(r => new ScopedFac22());

            builder.RegisterInstance(new SingleObj12()).AsSelf();
            builder.RegisterInstance(new SingleObj22()).AsSelf();

            return builder.Build();
        }

        public static object Measure(Autofac.IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Resolve<R>();
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerAndRegisterServices
        {
            /*
            ## Baseline:

                            Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |   3.590 us | 0.0690 us | 0.0738 us |  1.00 |    0.00 |      1.3657 |           - |           - |              6.3 KB |
                       BmarkDryIoc |   7.424 us | 0.0115 us | 0.0102 us |  2.08 |    0.04 |      3.0823 |           - |           - |            14.26 KB |
                   BmarkDryIocMsDi |   7.646 us | 0.0365 us | 0.0341 us |  2.14 |    0.05 |      3.7384 |           - |           - |            17.28 KB |
                        BmarkGrace |  19.034 us | 0.2705 us | 0.2530 us |  5.32 |    0.11 |      5.1270 |      0.0305 |           - |            23.73 KB |
                  BmarkAutofacMsDi |  82.972 us | 1.3541 us | 1.2666 us | 23.20 |    0.54 |     16.2354 |      0.1221 |           - |            75.02 KB |
                      BmarkAutofac |  86.698 us | 0.6252 us | 0.5848 us | 24.24 |    0.54 |     15.8691 |      0.1221 |           - |            73.67 KB |
                    BmarkGraceMsDi | 146.023 us | 1.5914 us | 1.4886 us | 40.82 |    0.95 |      8.3008 |      0.2441 |           - |            38.55 KB |

             */
            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => PrepareMsDi();

            [Benchmark]
            public object BmarkDryIoc() => PrepareDryIoc();

            [Benchmark]
            public object BmarkDryIocMsDi() => PrepareDryIocMsDi();

            [Benchmark]
            public object BmarkGrace() => PrepareGrace();

            [Benchmark]
            public object BmarkGraceMsDi() => PrepareGraceMsDi();

            [Benchmark]
            public object BmarkAutofac() => PrepareAutofac();

            [Benchmark]
            public object BmarkAutofacMsDi() => PrepareAutofacMsDi();
        }

        [MemoryDiagnoser]
        public class CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve
        {
            /*
            BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
            Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
            Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
            .NET Core SDK=2.2.100
              [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
              DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

            ## VS DryIoc v3.0.2 and DryIoc.Microsoft.DependencyInjection 2.1.0

|                            Method |         Mean |       Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|---------------------------------- |-------------:|------------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
| BmarkMicrosoftDependencyInjection |     23.64 us |   0.0991 us |  0.0927 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
|                      BmarkAutofac |    144.88 us |   0.7696 us |  0.6823 us |   6.13 |    0.03 |     27.8320 |           - |           - |            128.8 KB |
|                  BmarkAutofacMsDi |    150.53 us |   0.6579 us |  0.6154 us |   6.37 |    0.04 |     30.7617 |      0.2441 |           - |           142.48 KB |
|                        BmarkGrace |  5,488.69 us |  49.2015 us | 46.0231 us | 232.16 |    2.04 |     46.8750 |     23.4375 |           - |            223.7 KB |
|                    BmarkGraceMsDi |  7,272.86 us |  37.8745 us | 35.4278 us | 307.63 |    1.63 |     70.3125 |     31.2500 |           - |           324.12 KB |
|                       BmarkDryIoc | 16,475.31 us | 105.1769 us | 98.3826 us | 696.87 |    4.41 |     31.2500 |           - |           - |           220.52 KB |
|                   BmarkDryIocMsDi | 19,829.82 us |  82.4082 us | 77.0847 us | 838.76 |    4.20 |     62.5000 |     31.2500 |           - |           320.24 KB |
            */

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(PrepareMsDi());

            [Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            [Benchmark]
            public object BmarkGraceMsDi() => Measure(PrepareGraceMsDi());

            [Benchmark]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(PrepareDryIocMsDi());

            [Benchmark]
            public object BmarkAutofac() => Measure(PrepareAutofac());

            [Benchmark]
            public object BmarkAutofacMsDi() => Measure(PrepareAutofacMsDi());
        }

        [MemoryDiagnoser]//, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class OpenScopeAndResolve
        {
            /*
            BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
            Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
            Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
            .NET Core SDK=2.2.100
              [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
              DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

            ## VS DryIoc v3.0.2 and DryIoc.Microsoft.DependencyInjection 2.1.0

            |                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
            |---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
            | BmarkMicrosoftDependencyInjection |  1.330 us | 0.0050 us | 0.0046 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
            |                        BmarkGrace |  1.785 us | 0.0035 us | 0.0033 us |  1.34 |    0.00 |      0.9098 |           - |           - |              4.2 KB |
            |                    BmarkGraceMsDi |  1.948 us | 0.0357 us | 0.0334 us |  1.46 |    0.03 |      0.8774 |           - |           - |             4.05 KB |
            |                       BmarkDryIoc |  8.557 us | 0.0411 us | 0.0385 us |  6.44 |    0.04 |      1.1749 |           - |           - |             5.46 KB |
            |                   BmarkDryIocMsDi | 12.990 us | 0.0446 us | 0.0417 us |  9.77 |    0.05 |      1.8463 |           - |           - |             8.56 KB |
            |                      BmarkAutofac | 13.100 us | 0.1340 us | 0.1254 us |  9.85 |    0.09 |      3.9673 |           - |           - |            18.34 KB |
            |                  BmarkAutofacMsDi | 19.468 us | 0.2001 us | 0.1872 us | 14.64 |    0.14 |      5.6458 |           - |           - |            26.06 KB |
            */

            private IServiceProvider _msDi;
            private IContainer _dryioc;
            private IServiceProvider _dryIocMsDi;
            private DependencyInjectionContainer _grace;
            private IServiceProvider _graceMsDi;
            private Autofac.IContainer _autofac;
            private IServiceProvider _autofacMsDi;

            [GlobalSetup]
            public void WarmUp()
            {
                _msDi = PrepareMsDi();
                _dryioc = PrepareDryIoc();
                _dryIocMsDi = PrepareDryIocMsDi();
                _grace = PrepareGrace();
                _graceMsDi = PrepareGraceMsDi();
                _autofac = PrepareAutofac();
                _autofacMsDi = PrepareAutofacMsDi();
            }

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(_msDi);

            [Benchmark]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(_dryIocMsDi);

            [Benchmark]
            public object BmarkGrace() => Measure(_grace);

            [Benchmark]
            public object BmarkGraceMsDi() => Measure(_graceMsDi);

            [Benchmark]
            public object BmarkAutofac() => Measure(_autofac);

            [Benchmark]
            public object BmarkAutofacMsDi() => Measure(_autofacMsDi);
        }

        public class R
        {
            public Single1 Single1 { get; }
            public Single2 Single2 { get; }

            public Scoped1 Scoped1 { get; }
            public Scoped2 Scoped2 { get; }

            public Trans1 Trans1 { get; }
            public Trans2 Trans2 { get; }

            public ScopedFac1 ScopedFac1 { get; }
            public ScopedFac2 ScopedFac2 { get; }

            public SingleObj1 SingleObj1 { get; }
            public SingleObj2 SingleObj2 { get; }

            public R(
                Single1 single1,
                Single2 single2,
                Scoped1 scoped1,
                Scoped2 scoped2,
                Trans1 trans1,
                Trans2 trans2,
                ScopedFac1 scopedFac1,
                ScopedFac2 scopedFac2,
                SingleObj1 singleObj1,
                SingleObj2 singleObj2
            )
            {
                Single1 = single1;
                Single2 = single2;
                Scoped1 = scoped1;
                Scoped2 = scoped2;
                Trans1 = trans1;
                Trans2 = trans2;
                ScopedFac1 = scopedFac1;
                ScopedFac2 = scopedFac2;
                SingleObj1 = singleObj1;
                SingleObj2 = singleObj2;
            }
        }

        public class Single1
        {
            public Single12 Single12 { get; }
            public Single22 Single22 { get; }
            public SingleObj12 SingleObj12 { get; }
            public SingleObj22 SingleObj22 { get; }

            public Single1(
                Single12 single12,
                Single22 single22,
                SingleObj12 singleObj12,
                SingleObj22 singleObj22
                )
            {
                Single12 = single12;
                Single22 = single22;
                SingleObj12 = singleObj12;
                SingleObj22 = singleObj22;
            }
        }

        public class Single2
        {
            public Single12 Single12 { get; }
            public Single22 Single22 { get; }
            public SingleObj12 SingleObj12 { get; }
            public SingleObj22 SingleObj22 { get; }
            public Single2(
                Single12 single12,
                Single22 single22,
                SingleObj12 singleObj12,
                SingleObj22 singleObj22
            )
            {
                Single12 = single12;
                Single22 = single22;
                SingleObj12 = singleObj12;
                SingleObj22 = singleObj22;
            }
        }

        public class Scoped1
        {
            public Single12 Single12 { get; }
            public SingleObj12 SingleObj12 { get; }
            public Scoped12 Scoped12 { get; }
            public ScopedFac12 ScopedFac12 { get; }
            public Trans12  Trans12 { get; }

            public Single1  Single1 { get; }
            public SingleObj1 SingleObj1 { get; }

            public Scoped1(Single12 single12, SingleObj12 singleObj12, ScopedFac12 scopedFac12, Trans12 trans12, Single1 single1, SingleObj1 singleObj1, Scoped12 scoped12)
            {
                Single12 = single12;
                SingleObj12 = singleObj12;
                ScopedFac12 = scopedFac12;
                Trans12 = trans12;
                Single1 = single1;
                SingleObj1 = singleObj1;
                Scoped12 = scoped12;
            }
        }

        public class Scoped2
        {
            public Single22    Single22 { get; }
            public SingleObj22 SingleObj22 { get; }
            public Scoped22    Scoped22 { get; }
            public ScopedFac22 ScopedFac22 { get; }
            public Trans22     Trans22 { get; }

            public Single2 Single2 { get; }
            public SingleObj2 SingleObj2 { get; }

            public Scoped2(Single22 single22, SingleObj22 singleObj22, ScopedFac22 scopedFac22, Trans22 trans22, Single2 single2, SingleObj2 singleObj2, Scoped22 scoped22)
            {
                Single22 = single22;
                SingleObj22 = singleObj22;
                ScopedFac22 = scopedFac22;
                Trans22 = trans22;
                Single2 = single2;
                SingleObj2 = singleObj2;
                Scoped22 = scoped22;
            }
        }

        public class Scoped3 
        {
        }

        public class Scoped4 
        {
        }

        public class SingleObj1
        {
        }

        public class SingleObj2
        {
        }

        public class ScopedFac1
        {
            public Scoped1 Scoped1 { get; }
            public Scoped3 Scoped3 { get; }
            public Single1 Single1 { get; }
            public SingleObj1 SingleObj1 { get; }

            public ScopedFac1(Scoped1 scoped1, Scoped3 scoped3, Single1 single1, SingleObj1 singleObj1)
            {
                Scoped1 = scoped1;
                Scoped3 = scoped3;
                Single1 = single1;
                SingleObj1 = singleObj1;
            }
        }

        public class ScopedFac2
        {
            public Scoped2 Scoped2 { get; }
            public Scoped4 Scoped4 { get; }
            public Single2 Single2 { get; }
            public SingleObj2 SingleObj2 { get; }

            public ScopedFac2(Scoped2 scoped2, Scoped4 scoped4, Single2 single2, SingleObj2 singleObj2)
            {
                Scoped2 = scoped2;
                Scoped4 = scoped4;
                Single2 = single2;
                SingleObj2 = singleObj2;
            }
        }

        public class Trans1
        {
        }

        public class Trans2
        {
        }

        // ## Level 2

        public class Single22
        {
        }

        public class Single12
        {
        }

        public class SingleObj12
        {
        }

        public class SingleObj22
        {
        }

        public class Scoped12
        {
        }

        public class Scoped22
        {
        }


        public class ScopedFac12
        {
        }

        public class Trans12
        {
        }

        public class Trans22
        {
        }

        public class ScopedFac22
        {
        }
    }
}
