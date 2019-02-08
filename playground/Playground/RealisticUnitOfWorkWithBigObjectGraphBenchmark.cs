using System;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using IContainer = DryIoc.IContainer;


namespace PerformanceTests
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


        public static IServiceProvider PrepareMsDi() =>
            GetServices().BuildServiceProvider();

        public static object Measure(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
                return scope.ServiceProvider.GetService<R>();
        }

        public static IServiceProvider PrepareDryIocMsDi() => 
            new Container().WithDependencyInjectionAdapter(GetServices());

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
 BmarkMicrosoftDependencyInjection |   3.328 us | 0.0210 us | 0.0186 us |  1.00 |    0.00 |      1.3657 |           - |           - |              6.3 KB |
                       BmarkDryIoc |   7.465 us | 0.0990 us | 0.0926 us |  2.24 |    0.03 |      3.0670 |           - |           - |            14.14 KB |
                   BmarkDryIocMsDi |   7.989 us | 0.0754 us | 0.0705 us |  2.40 |    0.02 |      3.7231 |           - |           - |            17.16 KB |
                        BmarkGrace |  19.484 us | 0.3751 us | 0.3684 us |  5.86 |    0.12 |      5.1575 |      0.0305 |           - |            23.82 KB |
                      BmarkAutofac |  89.109 us | 0.5113 us | 0.4783 us | 26.78 |    0.19 |     15.8691 |      0.1221 |           - |            73.67 KB |
                    BmarkGraceMsDi | 144.011 us | 1.3689 us | 1.2805 us | 43.29 |    0.40 |      8.3008 |      0.2441 |           - |            38.55 KB |

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
 BmarkMicrosoftDependencyInjection |    23.18 us |  0.1323 us |  0.1173 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    53.76 us |  0.3287 us |  0.2745 us |   2.32 |    0.02 |     11.9019 |           - |           - |            55.11 KB |
                      BmarkAutofac |   147.99 us |  0.8766 us |  0.8199 us |   6.39 |    0.05 |     28.0762 |      0.2441 |           - |           129.45 KB |
                   BmarkDryIocMsDi | 1,823.35 us |  8.3675 us |  7.8269 us |  78.69 |    0.52 |     25.3906 |     11.7188 |           - |           117.45 KB |
                        BmarkGrace | 5,448.31 us | 56.9761 us | 53.2955 us | 234.90 |    2.40 |     46.8750 |     23.4375 |           - |           216.17 KB |
                    BmarkGraceMsDi | 7,261.23 us | 90.1823 us | 84.3566 us | 313.59 |    3.87 |     62.5000 |     31.2500 |           - |           308.78 KB |

            ## Use instance optimizations

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    22.64 us |  0.1726 us |  0.1442 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    56.18 us |  0.1210 us |  0.1073 us |   2.48 |    0.02 |     11.9629 |           - |           - |            55.21 KB |
                      BmarkAutofac |   154.26 us |  1.5197 us |  1.4215 us |   6.81 |    0.08 |     28.0762 |      0.2441 |           - |           129.45 KB |
                   BmarkDryIocMsDi | 1,643.29 us | 10.0812 us |  8.9367 us |  72.60 |    0.76 |     21.4844 |      9.7656 |           - |            103.6 KB |
                        BmarkGrace | 5,439.60 us | 23.1980 us | 21.6994 us | 240.25 |    2.00 |     46.8750 |     23.4375 |           - |           216.23 KB |
                    BmarkGraceMsDi | 7,190.90 us | 55.1042 us | 48.8484 us | 317.53 |    3.35 |     62.5000 |     31.2500 |           - |           314.21 KB |

            ## Use instance everywhere (wip)

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    22.71 us |  0.0851 us |  0.0754 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    55.87 us |  0.3672 us |  0.3435 us |   2.46 |    0.02 |     11.9629 |           - |           - |            55.21 KB |
                      BmarkAutofac |   147.56 us |  1.0940 us |  1.0233 us |   6.50 |    0.05 |     28.0762 |      0.2441 |           - |           129.45 KB |
                   BmarkDryIocMsDi | 1,595.67 us | 10.4423 us |  9.7677 us |  70.21 |    0.46 |     15.6250 |      7.8125 |           - |            79.56 KB |
                        BmarkGrace | 5,423.84 us | 41.2778 us | 38.6112 us | 239.02 |    1.75 |     46.8750 |     23.4375 |           - |           216.08 KB |
                    BmarkGraceMsDi | 7,194.49 us | 69.7629 us | 65.2563 us | 316.76 |    3.14 |     62.5000 |     31.2500 |           - |           314.24 KB |

            ## After shaving factory selection (wip)

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    22.92 us |  0.2458 us |  0.2052 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    54.52 us |  0.1413 us |  0.1322 us |   2.38 |    0.02 |     11.9629 |           - |           - |            55.21 KB |
                      BmarkAutofac |   145.66 us |  0.9167 us |  0.8575 us |   6.36 |    0.04 |     28.0762 |      0.2441 |           - |           129.45 KB |
                   BmarkDryIocMsDi | 1,644.43 us | 14.6430 us | 13.6971 us |  71.71 |    0.96 |     19.5313 |      9.7656 |           - |            93.37 KB |
                        BmarkGrace | 5,440.88 us | 46.6670 us | 43.6524 us | 237.61 |    3.03 |     46.8750 |     23.4375 |           - |           216.14 KB |
                    BmarkGraceMsDi | 7,226.64 us | 45.4676 us | 42.5304 us | 314.89 |    2.59 |     62.5000 |     31.2500 |           - |            314.2 KB |
            */

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(PrepareMsDi());

            [Benchmark]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(PrepareDryIocMsDi());

            [Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            [Benchmark]
            public object BmarkGraceMsDi() => Measure(PrepareGraceMsDi());

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
 BmarkMicrosoftDependencyInjection |  1.415 us | 0.0121 us | 0.0101 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                       BmarkDryIoc |  1.842 us | 0.0049 us | 0.0043 us |  1.30 |    0.01 |      0.5074 |           - |           - |             2.34 KB |
                        BmarkGrace |  2.007 us | 0.0038 us | 0.0034 us |  1.42 |    0.01 |      1.0033 |           - |           - |             4.63 KB |
                    BmarkGraceMsDi |  2.577 us | 0.0140 us | 0.0131 us |  1.82 |    0.02 |      1.2703 |           - |           - |             5.87 KB |
                   BmarkDryIocMsDi |  3.508 us | 0.0158 us | 0.0148 us |  2.48 |    0.02 |      0.9804 |           - |           - |             4.52 KB |
                      BmarkAutofac | 13.075 us | 0.0651 us | 0.0609 us |  9.24 |    0.09 |      3.9368 |           - |           - |            18.18 KB |

            ## After using еру Use method:

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 1.354 us | 0.0063 us | 0.0059 us |  1.00 |      0.3433 |           - |           - |             1.59 KB |
                    BmarkGraceMsDi | 1.938 us | 0.0099 us | 0.0092 us |  1.43 |      0.8926 |           - |           - |             4.12 KB |
                   BmarkDryIocMsDi | 2.689 us | 0.0121 us | 0.0113 us |  1.99 |      0.6905 |           - |           - |              3.2 KB |

            */

            private IServiceProvider _msDi;
            private IContainer _dryioc;
            private IServiceProvider _dryIocMsDi;
            private DependencyInjectionContainer _grace;
            private IServiceProvider _graceMsDi;
            private Autofac.IContainer _autofac;

            [GlobalSetup]
            public void WarmUp()
            {
                _msDi = PrepareMsDi();
                _dryioc = PrepareDryIoc();
                _dryIocMsDi = PrepareDryIocMsDi();
                _grace = PrepareGrace();
                _graceMsDi = PrepareGraceMsDi();
                _autofac = PrepareAutofac();
            }

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(_msDi);

            //[Benchmark]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(_dryIocMsDi);

            //[Benchmark]
            public object BmarkGrace() => Measure(_grace);

            [Benchmark]
            //[Benchmark(Baseline = true)]
            public object BmarkGraceMsDi() => Measure(_graceMsDi);

            //[Benchmark]
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