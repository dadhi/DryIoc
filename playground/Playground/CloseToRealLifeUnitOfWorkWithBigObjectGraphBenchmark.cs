using System;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Grace.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using IContainer = DryIoc.IContainer;


namespace PerformanceTests
{
    public class CloseToRealLifeUnitOfWorkWithBigObjectGraphBenchmark
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


        public static IServiceProvider PrepareMsDi() =>
            GetServices().BuildServiceProvider();

        public static object Measure(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
                return scope.ServiceProvider.GetRequiredService<R>();
        }

        public static IServiceProvider PrepareDryIocMsDi(bool msDiVNext = false)
        {
            if (!msDiVNext)
                return new Container().WithDependencyInjectionAdapter(GetServices()).Resolve<IServiceProvider>();

            new Container().WithDependencyInjectionAdapter(out var serviceProvider, GetServices());
            return serviceProvider;
        }

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
        public class CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve
        {
            /*
            ## 31.01.2019: At least first step to real world graph

                            Method |         Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |     8.731 us |  0.0638 us |  0.0566 us |   1.00 |    0.00 |      1.7700 |           - |           - |             8.16 KB |
                       BmarkDryIoc |    14.657 us |  0.0516 us |  0.0482 us |   1.68 |    0.01 |      3.4027 |           - |           - |            15.68 KB |
                      BmarkAutofac |    69.055 us |  0.3195 us |  0.2988 us |   7.91 |    0.07 |     13.5498 |      0.1221 |           - |            62.63 KB |
                   BmarkDryIocMsDi |   744.086 us |  3.9580 us |  3.5087 us |  85.23 |    0.75 |      8.7891 |      3.9063 |           - |            41.26 KB |
                        BmarkGrace | 1,981.725 us | 14.3364 us | 13.4103 us | 227.10 |    2.13 |     19.5313 |      7.8125 |           - |            92.88 KB |

            ## 2 level object graph

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    23.14 us |  0.1099 us |  0.0974 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    54.08 us |  0.3364 us |  0.3147 us |   2.34 |    0.02 |     11.9019 |           - |           - |            55.11 KB |
                      BmarkAutofac |   146.67 us |  0.3466 us |  0.3242 us |   6.34 |    0.03 |     28.0762 |      0.2441 |           - |           129.45 KB |
                   BmarkDryIocMsDi | 1,820.61 us | 15.8273 us | 14.0305 us |  78.68 |    0.74 |     25.3906 |     11.7188 |           - |           117.45 KB |
                        BmarkGrace | 5,445.34 us | 35.4986 us | 33.2054 us | 235.32 |    2.02 |     46.8750 |     23.4375 |           - |           216.11 KB |

             */

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(PrepareMsDi());

            [Benchmark]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(PrepareDryIocMsDi());

            //[Benchmark]
            public object BmarkDryIocMsDiVNext() => Measure(PrepareDryIocMsDi(true));

            [Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            [Benchmark]
            public object BmarkAutofac() => Measure(PrepareAutofac());
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class FirstTimeOpenScopeAndResolve
        {
            /*
            ## 31.01.2019: At least first step to real world graph
                            Method |       Mean |      Error |     StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|-----------:|-----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |   419.5 ns |  0.7336 ns |  0.6862 ns |  0.83 |    0.00 |      0.1421 |           - |           - |               672 B |
 BmarkMicrosoftDependencyInjection |   505.1 ns |  1.8306 ns |  1.6228 ns |  1.00 |    0.00 |      0.1602 |           - |           - |               760 B |
                        BmarkGrace |   787.8 ns |  1.9183 ns |  1.7005 ns |  1.56 |    0.01 |      0.3910 |           - |           - |              1848 B |
                   BmarkDryIocMsDi | 1,013.9 ns |  5.2416 ns |  4.9030 ns |  2.01 |    0.01 |      0.3204 |           - |           - |              1520 B |
                      BmarkAutofac | 4,834.6 ns | 42.2654 ns | 39.5351 ns |  9.57 |    0.08 |      1.5411 |           - |           - |              7280 B |

            ## 2 level graph
                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.375 us | 0.0134 us | 0.0126 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                       BmarkDryIoc |  1.921 us | 0.0142 us | 0.0133 us |  1.40 |    0.01 |      0.5074 |           - |           - |             2.34 KB |
                        BmarkGrace |  2.254 us | 0.0126 us | 0.0105 us |  1.64 |    0.01 |      1.1406 |           - |           - |             5.26 KB |
                   BmarkDryIocMsDi |  3.509 us | 0.0611 us | 0.0572 us |  2.55 |    0.05 |      0.9804 |           - |           - |             4.52 KB |
                      BmarkAutofac | 13.403 us | 0.2648 us | 0.2477 us |  9.75 |    0.21 |      3.9368 |           - |           - |            18.18 KB |

             */

            private IServiceProvider _msDi;
            private IContainer _dryioc;
            private IServiceProvider _dryIocMsDi;
            private DependencyInjectionContainer _grace;
            private Autofac.IContainer _autofac;

            [GlobalSetup]
            public void WarmUp()
            {
                _msDi = PrepareMsDi();
                _dryioc = PrepareDryIoc();
                _dryIocMsDi = PrepareDryIocMsDi();
                _grace = PrepareGrace();
                _autofac = PrepareAutofac();
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
            public object BmarkAutofac() => Measure(_autofac);
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class WarmedUpOpenScopeAndResolve
        {
            /*
            ## 31.01.2019: At least first step to real world graph
                                        Method |       Mean |      Error |     StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
            ---------------------------------- |-----------:|-----------:|-----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                   BmarkDryIoc |   417.8 ns |  1.7928 ns |  1.5892 ns |  0.81 |    0.00 |      0.1421 |           - |           - |               672 B |
             BmarkMicrosoftDependencyInjection |   516.0 ns |  0.7929 ns |  0.7417 ns |  1.00 |    0.00 |      0.1602 |           - |           - |               760 B |
                                    BmarkGrace |   686.8 ns |  0.9498 ns |  0.8885 ns |  1.33 |    0.00 |      0.3366 |           - |           - |              1592 B |
                                  BmarkAutofac | 4,919.1 ns | 67.7257 ns | 63.3506 ns |  9.53 |    0.13 |      1.5411 |           - |           - |              7280 B |

             */

            private IContainer _dryioc;
            private IServiceProvider _msDi;
            private DependencyInjectionContainer _grace;
            private Autofac.IContainer _autofac;

            [GlobalSetup]
            public void WarmUp()
            {
                _dryioc = PrepareDryIoc();
                _msDi = PrepareMsDi();
                _grace = PrepareGrace();
                _autofac = PrepareAutofac();

                for (var i = 0; i < 5; i++)
                {
                    Measure(_dryioc);
                    Measure(_msDi);
                    Measure(_grace);
                    Measure(_autofac);
                }
            }

            [Benchmark]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(_msDi);

            [Benchmark]
            public object BmarkGrace() => Measure(_grace);

            [Benchmark]
            public object BmarkAutofac() => Measure(_autofac);
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