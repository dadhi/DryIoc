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

            container.RegisterDelegate(_ => new ScopedFac1());
            container.RegisterDelegate(_ => new ScopedFac2());

            container.UseInstance(new SingleObj1());
            container.UseInstance(new SingleObj2());

            return container;
        }

        public static object Measure(IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<R>();
        }

        public static IServiceProvider PrepareMsDi()
        {
            var services = new ServiceCollection();

            services.AddScoped<R>();

            services.AddScoped<Scoped1>();
            services.AddScoped<Scoped2>();

            services.AddTransient<Trans1>();
            services.AddTransient<Trans2>();

            services.AddSingleton<Single1>();
            services.AddSingleton<Single2>();

            services.AddScoped(_ => new ScopedFac1());
            services.AddScoped(_ => new ScopedFac2());

            services.AddSingleton(new SingleObj1());
            services.AddSingleton(new SingleObj2());

            return services.BuildServiceProvider();
        }

        public static object Measure(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
                return scope.ServiceProvider.GetRequiredService<R>();
        }

        public static IServiceProvider PrepareDryIocMsDi(bool msDiVNext = false)
        {
            var services = new ServiceCollection();

            services.AddScoped<R>();

            services.AddScoped<Scoped1>();
            services.AddScoped<Scoped2>();

            services.AddTransient<Trans1>();
            services.AddTransient<Trans2>();

            services.AddSingleton<Single1>();
            services.AddSingleton<Single2>();

            services.AddScoped(_ => new ScopedFac1());
            services.AddScoped(_ => new ScopedFac2());

            services.AddSingleton(new SingleObj1());
            services.AddSingleton(new SingleObj2());

            if (!msDiVNext)
                return new Container().WithDependencyInjectionAdapter(services).Resolve<IServiceProvider>();

            new Container().WithDependencyInjectionAdapter(out var serviceProvider, services);
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

                c.ExportFactory(() => new ScopedFac1()).Lifestyle.SingletonPerScope();
                c.ExportFactory(() => new ScopedFac2()).Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj1());
                c.ExportInstance(new SingleObj2());
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

            builder.Register(_ => new ScopedFac1()).AsSelf().InstancePerLifetimeScope();
            builder.Register(_ => new ScopedFac2()).AsSelf().InstancePerLifetimeScope();

            builder.RegisterInstance(new SingleObj1()).AsSelf();
            builder.RegisterInstance(new SingleObj2()).AsSelf();

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
                                        Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
            ---------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                                   BmarkDryIoc |   415.6 ns |  2.957 ns |  2.469 ns |  0.79 |    0.00 |      0.1421 |           - |           - |               672 B |
             BmarkMicrosoftDependencyInjection |   525.1 ns |  1.174 ns |  1.041 ns |  1.00 |    0.00 |      0.1602 |           - |           - |               760 B |
                                    BmarkGrace |   692.3 ns |  2.166 ns |  2.026 ns |  1.32 |    0.00 |      0.3366 |           - |           - |              1592 B |
                                  BmarkAutofac | 4,867.2 ns | 95.883 ns | 84.997 ns |  9.27 |    0.16 |      1.5411 |           - |           - |              7280 B |
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
        }

        public class Single2
        {
        }

        public class Scoped1
        {
        }

        public class Scoped2
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
        }

        public class ScopedFac2
        {
        }

        public class Trans1
        {
        }

        public class Trans2
        {
        }
    }
}