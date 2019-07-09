using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Grace.DependencyInjection;
using Grace.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using RealisticUnitOfWork;
using IContainer = DryIoc.IContainer;

namespace PerformanceTests
{
    public class RealisticUnitOfWorkBenchmark
    {
        public static IContainer PrepareDryIoc()
        {
            var container = new Container();

            // register dummy scoped and singletons services to populate resolution cache and scopes to be close to reality
            RegisterDummyPopulation(container);

            // register graph for benchmarking starting with scoped R(oot) / Controller
            container.Register<R>(Reuse.Scoped);

            container.Register<Scoped1>(Reuse.Scoped);
            container.Register<Scoped2>(Reuse.Scoped);

            container.Register<Trans1>(Reuse.Transient);
            container.Register<Trans2>(Reuse.Transient);

            container.Register<Single1>(Reuse.Singleton);
            container.Register<Single2>(Reuse.Singleton);

            container.RegisterDelegate(
                r => new ScopedFac1(r.Resolve<Scoped1>(), r.Resolve<Scoped3>(), r.Resolve<Single1>(), r.Resolve<SingleObj1>()),
                Reuse.Scoped);
            container.RegisterDelegate(
                r => new ScopedFac2(r.Resolve<Scoped2>(), r.Resolve<Scoped4>(), r.Resolve<Single2>(), r.Resolve<SingleObj2>()),
                Reuse.Scoped);

            container.RegisterInstance(new SingleObj1());
            container.RegisterInstance(new SingleObj2());

            // level 2
            container.Register<Scoped3>(Reuse.Scoped);
            container.Register<Scoped4>(Reuse.Scoped);

            container.Register<Scoped12>(Reuse.Scoped);
            container.Register<Scoped22>(Reuse.Scoped);

            container.Register<Single12>(Reuse.Singleton);
            container.Register<Single22>(Reuse.Singleton);

            container.Register<Trans12>(Reuse.Transient);
            container.Register<Trans22>(Reuse.Transient);

            container.RegisterDelegate(
                r => new ScopedFac12(r.Resolve<Scoped13>(), r.Resolve<Single1>(), r.Resolve<SingleObj13>()), 
                Reuse.Scoped);
            container.RegisterDelegate(
                r => new ScopedFac22(r.Resolve<Scoped23>(), r.Resolve<Single2>(), r.Resolve<SingleObj23>()), 
                Reuse.Scoped);

            container.RegisterInstance(new SingleObj12());
            container.RegisterInstance(new SingleObj22());

            // level 3
            container.Register<Scoped13>(Reuse.Scoped);
            container.Register<Scoped23>(Reuse.Scoped);

            container.Register<Single13>(Reuse.Singleton);
            container.Register<Single23>(Reuse.Singleton);

            container.Register<Trans13>(Reuse.Transient);
            container.Register<Trans23>(Reuse.Transient);

            container.RegisterDelegate(
                r => new ScopedFac13(r.Resolve<Single1>(), r.Resolve<Scoped14>(), r.Resolve<ScopedFac14>()), 
                Reuse.Scoped);
            container.RegisterDelegate(
                r => new ScopedFac23(r.Resolve<Single2>(), r.Resolve<Scoped24>(), r.Resolve<ScopedFac24>()), 
                Reuse.Scoped);

            container.RegisterInstance(new SingleObj13());
            container.RegisterInstance(new SingleObj23());

            // level 4
            container.Register<Scoped14>(Reuse.Scoped);
            container.Register<Scoped24>(Reuse.Scoped);

            container.Register<Single14>(Reuse.Singleton);
            container.Register<Single24>(Reuse.Singleton);

            container.Register<Trans14>(Reuse.Transient);
            container.Register<Trans24>(Reuse.Transient);

            container.RegisterDelegate(r => new ScopedFac14(), Reuse.Scoped);
            container.RegisterDelegate(r => new ScopedFac24(), Reuse.Scoped);

            container.RegisterInstance(new SingleObj14());
            container.RegisterInstance(new SingleObj24());

            ResolveDummyPopulation(container);
            return container;
        }

        public static object Measure(IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<R>();
        }

        private static void RegisterDummyPopulation(Container container)
        {
            container.Register<D1>(Reuse.Scoped);
            container.Register<D2>(Reuse.Scoped);
            container.Register<D3>(Reuse.Scoped);
            container.Register<D4>(Reuse.Scoped);
            container.Register<D5>(Reuse.Scoped);
            container.Register<D6>(Reuse.Scoped);
            container.Register<D7>(Reuse.Scoped);
            container.Register<D8>(Reuse.Scoped);
            container.Register<D9>(Reuse.Scoped);
            container.Register<D10>(Reuse.Scoped);
            container.Register<D11>(Reuse.Scoped);
            container.Register<D12>(Reuse.Scoped);

            container.Register<D13>(Reuse.Singleton);
            container.Register<D14>(Reuse.Singleton);
            container.Register<D15>(Reuse.Singleton);
            container.Register<D16>(Reuse.Singleton);
            container.Register<D17>(Reuse.Singleton);
            container.Register<D18>(Reuse.Singleton);
            container.Register<D19>(Reuse.Singleton);
            container.Register<D20>(Reuse.Singleton);
        }

        public static object ResolveDummyPopulation(IContainer container)
        {
            using (var scope = container.OpenScope())
            {
                scope.Resolve<D1>();
                scope.Resolve<D2>();
                scope.Resolve<D3>();
                scope.Resolve<D4>();
                scope.Resolve<D5>();
                scope.Resolve<D6>();
                scope.Resolve<D7>();
                scope.Resolve<D8>();
                scope.Resolve<D9>();
                scope.Resolve<D10>();
                scope.Resolve<D11>();
                scope.Resolve<D12>();

                scope.Resolve<D13>();
                scope.Resolve<D14>();
                scope.Resolve<D15>();
                scope.Resolve<D16>();
                scope.Resolve<D17>();
                scope.Resolve<D18>();
                scope.Resolve<D19>();
                return scope.Resolve<D20>();
            }
        }

        public static ServiceCollection GetServices()
        {
            var services = new ServiceCollection();

            RegisterDummyPopulation(services);

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

            services.AddScoped(r => new ScopedFac12(r.GetService<Scoped13>(), r.GetService<Single1>(), r.GetService<SingleObj13>()));
            services.AddScoped(r => new ScopedFac22(r.GetService<Scoped23>(), r.GetService<Single2>(), r.GetService<SingleObj23>()));

            services.AddSingleton(new SingleObj12());
            services.AddSingleton(new SingleObj22());

            // level 3
            services.AddScoped<Scoped13>();
            services.AddScoped<Scoped23>();

            services.AddSingleton<Single13>();
            services.AddSingleton<Single23>();

            services.AddTransient<Trans13>();
            services.AddTransient<Trans23>();

            services.AddScoped(r => new ScopedFac13(r.GetService<Single1>(), r.GetService<Scoped14>(), r.GetService<ScopedFac14>()));
            services.AddScoped(r => new ScopedFac23(r.GetService<Single2>(), r.GetService<Scoped24>(), r.GetService<ScopedFac24>()));

            services.AddSingleton(new SingleObj13());
            services.AddSingleton(new SingleObj23());

            // level 4
            services.AddScoped<Scoped14>();
            services.AddScoped<Scoped24>();

            services.AddSingleton<Single14>();
            services.AddSingleton<Single24>();

            services.AddTransient<Trans14>();
            services.AddTransient<Trans24>();

            services.AddScoped(r => new ScopedFac14());
            services.AddScoped(r => new ScopedFac24());

            services.AddSingleton(new SingleObj14());
            services.AddSingleton(new SingleObj24());

            return services;
        }

        public static IServiceProvider PrepareMsDi()
        {
            var serviceProvider = GetServices().BuildServiceProvider();
            ResolveDummyPopulation(serviceProvider);
            return serviceProvider;
        }

        public static object Measure(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
                return scope.ServiceProvider.GetService<R>();
        }

        private static void RegisterDummyPopulation(ServiceCollection services)
        {
            services.AddScoped<D1>();
            services.AddScoped<D2>();
            services.AddScoped<D3>();
            services.AddScoped<D4>();
            services.AddScoped<D5>();
            services.AddScoped<D6>();
            services.AddScoped<D7>();
            services.AddScoped<D8>();
            services.AddScoped<D9>();
            services.AddScoped<D10>();
            services.AddScoped<D11>();
            services.AddScoped<D12>();

            services.AddSingleton<D13>();
            services.AddSingleton<D14>();
            services.AddSingleton<D15>();
            services.AddSingleton<D16>();
            services.AddSingleton<D17>();
            services.AddSingleton<D18>();
            services.AddSingleton<D19>();
            services.AddSingleton<D20>();
        }

        public static object ResolveDummyPopulation(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var s = scope.ServiceProvider;
                s.GetService<D1>();
                s.GetService<D2>();
                s.GetService<D3>();
                s.GetService<D4>();
                s.GetService<D5>();
                s.GetService<D6>();
                s.GetService<D7>();
                s.GetService<D8>();
                s.GetService<D9>();
                s.GetService<D10>();
                s.GetService<D11>();
                s.GetService<D12>();

                s.GetService<D13>();
                s.GetService<D14>();
                s.GetService<D15>();
                s.GetService<D16>();
                s.GetService<D17>();
                s.GetService<D18>();
                s.GetService<D19>();
                return s.GetService<D20>();
            }
        }

        public static IServiceProvider PrepareDryIocMsDi()
        {
            var serviceProvider = DryIocAdapter.Create(GetServices());
            ResolveDummyPopulation(serviceProvider);
            return serviceProvider;
        }

        public static DependencyInjectionContainer PrepareGrace()
        {
            var container = new DependencyInjectionContainer();

            container.Configure(c =>
            {
                RegisterDummyPopulation(c);

                c.Export<R>().Lifestyle.SingletonPerScope();

                c.Export<Scoped1>().Lifestyle.SingletonPerScope();
                c.Export<Scoped2>().Lifestyle.SingletonPerScope();

                c.Export<Trans1>();
                c.Export<Trans2>();

                c.Export<Single1>().Lifestyle.Singleton();
                c.Export<Single2>().Lifestyle.Singleton();

                c.ExportFactory<Scoped1, Scoped3, Single1, SingleObj1, ScopedFac1>(
                    (scoped1, scoped3, single1, singleObj1) => new ScopedFac1(scoped1, scoped3, single1, singleObj1)).Lifestyle.SingletonPerScope();
                c.ExportFactory<Scoped2, Scoped4, Single2, SingleObj2, ScopedFac2>(
                    (scoped2, scoped4, single2, singleObj2) => new ScopedFac2(scoped2, scoped4, single2, singleObj2)).Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj1());
                c.ExportInstance(new SingleObj2());

                // Level 2
                c.Export<Scoped3>().Lifestyle.SingletonPerScope();
                c.Export<Scoped4>().Lifestyle.SingletonPerScope();

                c.Export<Scoped12>().Lifestyle.SingletonPerScope();
                c.Export<Scoped22>().Lifestyle.SingletonPerScope();

                c.Export<Single12>().Lifestyle.Singleton();
                c.Export<Single22>().Lifestyle.Singleton();

                c.Export<Trans12>();
                c.Export<Trans22>();

                c.ExportFactory<Scoped13, Single1, SingleObj13, ScopedFac12>((scoped13, single1, singleObj13) => 
                    new ScopedFac12(scoped13, single1, singleObj13)).Lifestyle.SingletonPerScope();

                c.ExportFactory<Scoped23, Single2, SingleObj23, ScopedFac22>((scoped23, single2, singleObj23) =>
                    new ScopedFac22(scoped23, single2, singleObj23)).Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj12());
                c.ExportInstance(new SingleObj22());

                // level 3
                c.Export<Scoped13>().Lifestyle.SingletonPerScope();
                c.Export<Scoped23>().Lifestyle.SingletonPerScope();

                c.Export<Single13>().Lifestyle.Singleton();
                c.Export<Single23>().Lifestyle.Singleton();

                c.Export<Trans13>();
                c.Export<Trans23>();

                c.ExportFactory<Single1, Scoped14, ScopedFac14, ScopedFac13>(
                    (single1, scoped14, scopedFac14) => new ScopedFac13(single1, scoped14, scopedFac14))
                    .Lifestyle.SingletonPerScope();
                c.ExportFactory<Single2, Scoped24, ScopedFac24, ScopedFac23>(
                    (single2, scoped24, scopedFac24) => new ScopedFac23(single2, scoped24, scopedFac24))
                    .Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj13());
                c.ExportInstance(new SingleObj23());

                // level 4
                c.Export<Scoped14>().Lifestyle.SingletonPerScope();
                c.Export<Scoped24>().Lifestyle.SingletonPerScope();

                c.Export<Single14>().Lifestyle.Singleton();
                c.Export<Single24>().Lifestyle.Singleton();

                c.Export<Trans14>();
                c.Export<Trans24>();

                c.ExportFactory(() => new ScopedFac14()).Lifestyle.SingletonPerScope();
                c.ExportFactory(() => new ScopedFac24()).Lifestyle.SingletonPerScope();

                c.ExportInstance(new SingleObj14());
                c.ExportInstance(new SingleObj24());
            });

            ResolveDummyPopulation(container);
            return container;
        }

        private static void RegisterDummyPopulation(IExportRegistrationBlock block)
        {
            block.Export<D1> ().Lifestyle.SingletonPerScope();
            block.Export<D2> ().Lifestyle.SingletonPerScope();
            block.Export<D3> ().Lifestyle.SingletonPerScope();
            block.Export<D4> ().Lifestyle.SingletonPerScope();
            block.Export<D5> ().Lifestyle.SingletonPerScope();
            block.Export<D6> ().Lifestyle.SingletonPerScope();
            block.Export<D7> ().Lifestyle.SingletonPerScope();
            block.Export<D8> ().Lifestyle.SingletonPerScope();
            block.Export<D9> ().Lifestyle.SingletonPerScope();
            block.Export<D10>().Lifestyle.SingletonPerScope();
            block.Export<D11>().Lifestyle.SingletonPerScope();
            block.Export<D12>().Lifestyle.SingletonPerScope();

            block.Export<D13>().Lifestyle.Singleton();
            block.Export<D14>().Lifestyle.Singleton();
            block.Export<D15>().Lifestyle.Singleton();
            block.Export<D16>().Lifestyle.Singleton();
            block.Export<D17>().Lifestyle.Singleton();
            block.Export<D18>().Lifestyle.Singleton();
            block.Export<D19>().Lifestyle.Singleton();
            block.Export<D20>().Lifestyle.Singleton();
        }

        public static object ResolveDummyPopulation(DependencyInjectionContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                scope.Locate<D1>();
                scope.Locate<D2>();
                scope.Locate<D3>();
                scope.Locate<D4>();
                scope.Locate<D5>();
                scope.Locate<D6>();
                scope.Locate<D7>();
                scope.Locate<D8>();
                scope.Locate<D9>();
                scope.Locate<D10>();
                scope.Locate<D11>();
                scope.Locate<D12>();

                scope.Locate<D13>();
                scope.Locate<D14>();
                scope.Locate<D15>();
                scope.Locate<D16>();
                scope.Locate<D17>();
                scope.Locate<D18>();
                scope.Locate<D19>();
                return scope.Locate<D20>();
            }
        }

        public static IServiceProvider PrepareGraceMsDi()
        {
            var container = new DependencyInjectionContainer();

            container.Populate(GetServices());

            var serviceProvider = container.Locate<IServiceProvider>();

            ResolveDummyPopulation(serviceProvider);
            return serviceProvider;
        }

        public static IServiceProvider PrepareAutofacMsDi()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(GetServices());

            var container = containerBuilder.Build();

            var serviceProvider = new AutofacServiceProvider(container);

            ResolveDummyPopulation(serviceProvider);
            return serviceProvider;
        }

        public static object Measure(DependencyInjectionContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Locate<R>();
        }

        public static Autofac.IContainer PrepareAutofac()
        {
            var builder = new ContainerBuilder();

            RegisterDummyPopulation(builder);

            builder.RegisterType<R>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Scoped1>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Scoped2>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Trans1>().AsSelf().InstancePerDependency();
            builder.RegisterType<Trans2>().AsSelf().InstancePerDependency();

            builder.RegisterType<Single1>().AsSelf().SingleInstance();
            builder.RegisterType<Single2>().AsSelf().SingleInstance();

            builder.Register(c => 
                new ScopedFac1(c.Resolve<Scoped1>(), c.Resolve<Scoped3>(), c.Resolve<Single1>(), c.Resolve<SingleObj1>()))
                .AsSelf().InstancePerLifetimeScope();
            builder.Register(c => 
                new ScopedFac2(c.Resolve<Scoped2>(), c.Resolve<Scoped4>(), c.Resolve<Single2>(), c.Resolve<SingleObj2>()))
                .AsSelf().InstancePerLifetimeScope();

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

            builder.Register(r => 
                new ScopedFac12(r.Resolve<Scoped13>(), r.Resolve<Single1>(), r.Resolve<SingleObj13>()))
                .AsSelf().InstancePerLifetimeScope();
            builder.Register(r => 
                new ScopedFac22(r.Resolve<Scoped23>(), r.Resolve<Single2>(), r.Resolve<SingleObj23>()))
                .AsSelf().InstancePerLifetimeScope();

            builder.RegisterInstance(new SingleObj12()).AsSelf();
            builder.RegisterInstance(new SingleObj22()).AsSelf();

            // level 3
            builder.RegisterType<Scoped13>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Scoped23>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Single13>().AsSelf().SingleInstance();
            builder.RegisterType<Single23>().AsSelf().SingleInstance();

            builder.RegisterType<Trans13>().AsSelf().InstancePerDependency();
            builder.RegisterType<Trans23>().AsSelf().InstancePerDependency();

            builder.Register(
                r => new ScopedFac13(r.Resolve<Single1>(), r.Resolve<Scoped14>(), r.Resolve<ScopedFac14>()))
                .AsSelf().InstancePerLifetimeScope();
            builder.Register(
                r => new ScopedFac23(r.Resolve<Single2>(), r.Resolve<Scoped24>(), r.Resolve<ScopedFac24>()))
                .AsSelf().InstancePerLifetimeScope();

            builder.RegisterInstance(new SingleObj13()).AsSelf();
            builder.RegisterInstance(new SingleObj23()).AsSelf();

            // level 4
            builder.RegisterType<Scoped14>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<Scoped24>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<Single14>().AsSelf().SingleInstance();
            builder.RegisterType<Single24>().AsSelf().SingleInstance();

            builder.RegisterType<Trans14>().AsSelf().InstancePerDependency();
            builder.RegisterType<Trans24>().AsSelf().InstancePerDependency();

            builder.Register(r => new ScopedFac14()).AsSelf().InstancePerLifetimeScope();
            builder.Register(r => new ScopedFac24()).AsSelf().InstancePerLifetimeScope();

            builder.RegisterInstance(new SingleObj14()).AsSelf();
            builder.RegisterInstance(new SingleObj24()).AsSelf();

            var container = builder.Build();
            ResolveDummyPopulation(container);
            return container;
        }

        private static void RegisterDummyPopulation(ContainerBuilder builder)
        {
            builder.RegisterType<D1> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D2> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D3> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D4> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D5> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D6> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D7> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D8> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D9> ().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D10>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D11>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D12>().AsSelf().InstancePerLifetimeScope();

            builder.RegisterType<D13>().AsSelf().SingleInstance();
            builder.RegisterType<D14>().AsSelf().SingleInstance();
            builder.RegisterType<D15>().AsSelf().SingleInstance();
            builder.RegisterType<D16>().AsSelf().SingleInstance();
            builder.RegisterType<D17>().AsSelf().SingleInstance();
            builder.RegisterType<D18>().AsSelf().SingleInstance();
            builder.RegisterType<D19>().AsSelf().SingleInstance();
            builder.RegisterType<D20>().AsSelf().SingleInstance();
        }

        public static object ResolveDummyPopulation(Autofac.IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                scope.Resolve<D1>();
                scope.Resolve<D2>();
                scope.Resolve<D3>();
                scope.Resolve<D4>();
                scope.Resolve<D5>();
                scope.Resolve<D6>();
                scope.Resolve<D7>();
                scope.Resolve<D8>();
                scope.Resolve<D9>();
                scope.Resolve<D10>();
                scope.Resolve<D11>();
                scope.Resolve<D12>();

                scope.Resolve<D13>();
                scope.Resolve<D14>();
                scope.Resolve<D15>();
                scope.Resolve<D16>();
                scope.Resolve<D17>();
                scope.Resolve<D18>();
                scope.Resolve<D19>();
                return scope.Resolve<D20>();
            }
        }

        public static object Measure(Autofac.IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Resolve<R>();
        }

        [MemoryDiagnoser]
        public class CreateContainerAndRegisterServices
        {
            /*
            ## Baseline:

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    37.16 us |  0.1617 us |  0.1434 us |   1.00 |    0.00 |      9.0942 |      0.0610 |           - |            41.93 KB |
                       BmarkDryIoc |    47.27 us |  0.5879 us |  0.5211 us |   1.27 |    0.02 |     11.5967 |      0.0610 |           - |            53.59 KB |
                   BmarkDryIocMsDi |    50.20 us |  0.8907 us |  0.8332 us |   1.35 |    0.02 |     12.3901 |      0.0610 |           - |            57.36 KB |
                  BmarkAutofacMsDi |   416.15 us |  4.9245 us |  4.6064 us |  11.19 |    0.13 |     59.5703 |     14.1602 |           - |           275.06 KB |
                      BmarkAutofac |   417.75 us |  1.9344 us |  1.7148 us |  11.24 |    0.06 |     56.1523 |      3.4180 |           - |           260.33 KB |
                        BmarkGrace | 5,933.57 us | 48.6780 us | 45.5335 us | 159.79 |    1.51 |     70.3125 |     31.2500 |      7.8125 |            338.2 KB |
                    BmarkGraceMsDi | 6,295.46 us | 43.3164 us | 40.5182 us | 169.46 |    0.98 |     78.1250 |     39.0625 |      7.8125 |           371.36 KB |

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

        [MemoryDiagnoser]//, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve
        {
            /*
            ## 31.01.2019: At least first step to real world object graph

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

            ## After shaving factory selection (wip) + Autofac.MS.DI

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    23.03 us |  0.1025 us |  0.0908 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    55.36 us |  0.2364 us |  0.2096 us |   2.40 |    0.02 |     11.9629 |           - |           - |            55.33 KB |
                      BmarkAutofac |   148.79 us |  1.0168 us |  0.9511 us |   6.46 |    0.04 |     28.0762 |           - |           - |           129.45 KB |
                  BmarkAutofacMsDi |   152.68 us |  1.0374 us |  0.9704 us |   6.63 |    0.04 |     30.5176 |      0.2441 |           - |           141.25 KB |
                   BmarkDryIocMsDi | 1,589.43 us |  9.5683 us |  8.9502 us |  68.97 |    0.52 |     19.5313 |      9.7656 |           - |            90.29 KB |
                        BmarkGrace | 5,538.53 us | 28.7673 us | 26.9090 us | 240.47 |    1.31 |     46.8750 |     23.4375 |           - |           216.09 KB |
                    BmarkGraceMsDi | 7,310.95 us | 28.3436 us | 23.6682 us | 317.48 |    1.50 |     62.5000 |     31.2500 |           - |           314.17 KB |

            ## After enabling interpretation of FactoryDelegate registered by RegisterDelegate - found the culprit

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    23.03 us |  0.4313 us |  0.3824 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    57.49 us |  1.0566 us |  0.9884 us |   2.50 |    0.07 |     12.2070 |      0.0610 |           - |            56.28 KB |
                   BmarkDryIocMsDi |    92.48 us |  1.7939 us |  1.5902 us |   4.02 |    0.08 |     16.2354 |      0.1221 |           - |            75.33 KB |
                      BmarkAutofac |   207.44 us |  1.2361 us |  1.1563 us |   9.01 |    0.13 |     35.8887 |      0.2441 |           - |           165.75 KB |
                  BmarkAutofacMsDi |   221.23 us |  1.0479 us |  0.9802 us |   9.61 |    0.17 |     39.3066 |      0.2441 |           - |           181.71 KB |
                        BmarkGrace | 5,391.88 us | 42.6043 us | 39.8521 us | 234.04 |    4.16 |     46.8750 |     23.4375 |           - |           216.12 KB |
                    BmarkGraceMsDi | 7,171.67 us | 29.6879 us | 26.3176 us | 311.45 |    5.72 |     62.5000 |     31.2500 |           - |           314.27 KB |

            ## Optimizing RegisterDelegate (removing separate Resolve call by default - when is not used for generation in DryIocZero)

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    23.05 us |  0.1360 us |  0.1206 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    53.34 us |  0.3052 us |  0.2855 us |   2.31 |    0.02 |     11.4746 |           - |           - |            52.93 KB |
                   BmarkDryIocMsDi |    62.22 us |  0.2062 us |  0.1929 us |   2.70 |    0.02 |     12.2070 |      0.1221 |           - |            56.79 KB |
                      BmarkAutofac |   210.50 us |  1.0462 us |  0.9786 us |   9.13 |    0.06 |     35.8887 |           - |           - |           165.96 KB |
                  BmarkAutofacMsDi |   226.47 us |  2.0552 us |  1.9225 us |   9.82 |    0.10 |     39.3066 |      0.2441 |           - |           181.77 KB |
                        BmarkGrace | 5,492.44 us | 27.8638 us | 26.0638 us | 238.34 |    1.82 |     46.8750 |     23.4375 |           - |           216.16 KB |
                    BmarkGraceMsDi | 7,250.63 us | 56.5489 us | 52.8958 us | 314.39 |    2.46 |     62.5000 |     31.2500 |           - |            314.2 KB |

            ## Optimized interpreting of the ScopeOrSingleton and registered FactoryDelegate - now DI.MS.DI is faster than DI :)

                            Method |        Mean |       Error |     StdDev |      Median |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|------------:|-----------:|------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    26.94 us |   0.5351 us |  1.2923 us |    27.44 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    60.68 us |   0.9147 us |  0.8556 us |    60.93 us |   2.38 |    0.23 |     11.4746 |           - |           - |            52.93 KB |
                   BmarkDryIocMsDi |    66.79 us |   0.9228 us |  0.8180 us |    66.93 us |   2.64 |    0.25 |     11.8408 |           - |           - |            54.79 KB |
                      BmarkAutofac |   247.97 us |   1.5355 us |  1.4363 us |   247.71 us |   9.73 |    0.88 |     35.6445 |      0.4883 |           - |            165.9 KB |
                  BmarkAutofacMsDi |   264.72 us |   1.9329 us |  1.8080 us |   264.03 us |  10.39 |    0.94 |     39.0625 |      0.4883 |           - |           181.76 KB |
                        BmarkGrace | 6,099.80 us | 104.3044 us | 97.5664 us | 6,078.28 us | 239.58 |   24.44 |     46.8750 |     23.4375 |           - |           216.12 KB |
                    BmarkGraceMsDi | 7,923.96 us |  60.4445 us | 56.5398 us | 7,938.68 us | 311.03 |   29.28 |     62.5000 |     31.2500 |           - |           314.19 KB |

                ## Initial RegisterInstance performance - bit it!

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    23.12 us |  0.1317 us |  0.1232 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    42.05 us |  0.2900 us |  0.2713 us |   1.82 |    0.01 |      9.3994 |           - |           - |            43.34 KB |
                   BmarkDryIocMsDi |    52.88 us |  0.2601 us |  0.2172 us |   2.29 |    0.02 |     11.4136 |      0.0610 |           - |            52.76 KB |
                      BmarkAutofac |   207.70 us |  0.8652 us |  0.8093 us |   8.98 |    0.06 |     35.8887 |           - |           - |           165.95 KB |
                  BmarkAutofacMsDi |   224.37 us |  3.5212 us |  3.2938 us |   9.71 |    0.17 |     39.3066 |      0.2441 |           - |           181.67 KB |
                        BmarkGrace | 5,504.00 us | 26.0347 us | 24.3528 us | 238.09 |    1.78 |     46.8750 |     23.4375 |           - |           216.27 KB |
                    BmarkGraceMsDi | 7,235.79 us | 37.6264 us | 35.1958 us | 313.00 |    2.25 |     62.5000 |     31.2500 |           - |           314.23 KB |

                ## Fixing #61 and optimizing rule selection - check memory for BmarkDryIocMsDi

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    23.83 us |  0.0954 us |  0.0846 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    43.34 us |  0.1748 us |  0.1635 us |   1.82 |    0.01 |      9.3994 |           - |           - |            43.34 KB |
                   BmarkDryIocMsDi |    50.31 us |  0.3109 us |  0.2908 us |   2.11 |    0.01 |     10.6812 |      0.0610 |           - |            49.33 KB |
                      BmarkAutofac |   212.30 us |  0.9974 us |  0.9329 us |   8.91 |    0.05 |     35.8887 |      0.2441 |           - |           165.88 KB |
                  BmarkAutofacMsDi |   226.75 us |  0.8951 us |  0.8373 us |   9.52 |    0.05 |     39.3066 |           - |           - |           181.67 KB |
                        BmarkGrace | 5,491.76 us | 38.8408 us | 36.3317 us | 230.42 |    1.85 |     46.8750 |     23.4375 |           - |           216.13 KB |
                    BmarkGraceMsDi | 7,349.26 us | 33.0618 us | 30.9261 us | 308.29 |    1.88 |     62.5000 |     31.2500 |           - |            314.2 KB |

            ## Optimized register instance

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    22.52 us |  0.1295 us |  0.1081 us |   1.00 |    0.00 |      4.2419 |           - |           - |            19.62 KB |
                       BmarkDryIoc |    39.58 us |  0.2562 us |  0.2271 us |   1.76 |    0.01 |      9.0332 |           - |           - |            41.69 KB |
                   BmarkDryIocMsDi |    48.00 us |  0.6945 us |  0.6496 us |   2.13 |    0.03 |     10.3149 |           - |           - |            47.68 KB |
                      BmarkAutofac |   211.89 us |  0.4042 us |  0.3584 us |   9.41 |    0.05 |     35.8887 |      0.2441 |           - |           165.88 KB |
                  BmarkAutofacMsDi |   219.85 us |  1.3451 us |  1.2582 us |   9.76 |    0.06 |     39.3066 |      0.2441 |           - |           181.67 KB |
                        BmarkGrace | 5,370.39 us | 25.0568 us | 22.2122 us | 238.50 |    1.55 |     46.8750 |     23.4375 |           - |           216.17 KB |
                    BmarkGraceMsDi | 7,145.42 us | 38.5442 us | 36.0542 us | 317.47 |    2.15 |     62.5000 |     31.2500 |           - |           314.21 KB |

            ## Test with the Dummy Population (and moved to .Net Core v2.2)

            BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
.NET Core SDK=2.2.100
  [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

            ## Test with 4 level graph + disposables + fixed scoped things for DryIoc:

                            Method |        Mean |       Error |      StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|------------:|------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    134.0 us |   2.1675 us |   2.0275 us |   1.00 |    0.00 |     13.7939 |      0.1221 |           - |            58.65 KB |
                       BmarkDryIoc |    145.3 us |   0.8318 us |   0.7374 us |   1.08 |    0.02 |     30.2734 |           - |           - |           140.35 KB |
                   BmarkDryIocMsDi |    161.6 us |   0.9626 us |   0.9004 us |   1.21 |    0.02 |     32.2266 |           - |           - |           149.19 KB |
                        BmarkGrace | 18,480.8 us | 100.5977 us |  89.1773 us | 137.83 |    2.35 |    156.2500 |     62.5000 |           - |           755.18 KB |
                    BmarkGraceMsDi | 21,640.4 us | 119.7309 us | 106.1383 us | 161.39 |    2.81 |    187.5000 |     93.7500 |     31.2500 |           926.88 KB |
                      BmarkAutofac |    673.2 us |   5.5615 us |   5.2022 us |   5.02 |    0.08 |    101.5625 |     18.5547 |           - |           470.39 KB |
                  BmarkAutofacMsDi |    665.2 us |   5.4804 us |   5.1264 us |   4.96 |    0.08 |    105.4688 |      3.9063 |           - |           487.64 KB |

            ## After returning ScopedOrSingleton to use lambda 

                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    166.9 us |   2.035 us |   1.804 us |   1.00 |    0.00 |     13.6719 |      0.2441 |           - |            58.66 KB |
                       BmarkDryIoc |    160.1 us |   2.670 us |   2.498 us |   0.96 |    0.02 |     30.2734 |      0.4883 |           - |           140.03 KB |
                   BmarkDryIocMsDi |    180.2 us |   2.420 us |   2.263 us |   1.08 |    0.02 |     32.4707 |      0.2441 |           - |           150.03 KB |
                        BmarkGrace | 20,058.3 us | 290.376 us | 257.411 us | 120.22 |    1.59 |    156.2500 |     62.5000 |           - |           755.11 KB |
                    BmarkGraceMsDi | 23,546.7 us | 294.414 us | 275.395 us | 141.02 |    2.04 |    187.5000 |     93.7500 |     31.2500 |           926.86 KB |
                      BmarkAutofac |    790.0 us |   5.206 us |   4.615 us |   4.74 |    0.06 |    101.5625 |      6.8359 |           - |           470.32 KB |
                  BmarkAutofacMsDi |    747.8 us |   7.209 us |   6.391 us |   4.48 |    0.07 |    105.4688 |      7.8125 |           - |            487.8 KB |

            ## DryIoc v4.0.4 and Grace v7

                            Method |        Mean |       Error |      StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|------------:|------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    124.3 us |   2.4610 us |   3.6835 us |   1.00 |    0.00 |     18.3105 |      0.2441 |           - |             79.7 KB |
                       BmarkDryIoc |    152.0 us |   0.5548 us |   0.5189 us |   1.23 |    0.04 |     29.7852 |      0.2441 |           - |           137.95 KB |
                   BmarkDryIocMsDi |    166.1 us |   0.5952 us |   0.5276 us |   1.34 |    0.04 |     31.7383 |           - |           - |           146.85 KB |
                        BmarkGrace | 18,252.3 us | 118.7348 us | 111.0646 us | 147.19 |    4.84 |    156.2500 |     62.5000 |           - |           739.75 KB |
                    BmarkGraceMsDi | 21,820.4 us | 127.6058 us | 119.3625 us | 175.96 |    5.78 |    187.5000 |     93.7500 |           - |           909.13 KB |
                      BmarkAutofac |    653.1 us |   2.1808 us |   1.9332 us |   5.25 |    0.18 |    102.5391 |     23.4375 |           - |           472.85 KB |
                  BmarkAutofacMsDi |    631.7 us |   6.4908 us |   6.0715 us |   5.09 |    0.18 |    105.4688 |      0.9766 |           - |           490.02 KB |

            ## DryIoc v4.0.5

                            Method |        Mean |       Error |      StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|------------:|------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    148.0 us |   1.5359 us |   1.4366 us |   1.00 |    0.00 |     18.4326 |      0.1221 |           - |            79.69 KB |
                       BmarkDryIoc |    149.3 us |   0.6155 us |   0.5757 us |   1.01 |    0.01 |     27.3438 |      0.2441 |           - |           126.06 KB |
                   BmarkDryIocMsDi |    160.8 us |   0.8074 us |   0.7552 us |   1.09 |    0.01 |     29.0527 |      0.7324 |           - |           134.63 KB |
                        BmarkGrace | 19,240.3 us | 174.1336 us | 162.8847 us | 130.03 |    1.86 |    156.2500 |     62.5000 |           - |           739.73 KB |
                    BmarkGraceMsDi | 22,149.9 us | 122.6704 us | 114.7460 us | 149.69 |    1.48 |    187.5000 |     93.7500 |           - |           909.15 KB |
                      BmarkAutofac |    696.3 us |   4.9448 us |   4.3834 us |   4.71 |    0.07 |    102.5391 |      2.9297 |           - |            472.8 KB |
                  BmarkAutofacMsDi |    688.8 us |   7.1206 us |   6.6606 us |   4.66 |    0.07 |    105.4688 |      0.9766 |           - |           489.97 KB |

            ### FEC v2.1 and BDN 0.11.5

|                            Method |     Mean |    Error |    StdDev | Ratio |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|---------:|----------:|------:|--------:|-------:|------:|----------:|
| BmarkMicrosoftDependencyInjection | 120.6 us | 1.223 us | 0.9550 us |  1.00 | 18.6768 | 0.1221 |     - |  80.63 KB |
|                       BmarkDryIoc | 147.2 us | 1.034 us | 0.9673 us |  1.22 | 28.3203 | 0.2441 |     - | 131.55 KB |
|                   BmarkDryIocMsDi | 166.6 us | 1.509 us | 1.4116 us |  1.38 | 30.7617 | 0.7324 |     - | 142.35 KB |
            */

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(PrepareMsDi());

            [Benchmark]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(PrepareDryIocMsDi());

            //[Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            //[Benchmark]
            public object BmarkGraceMsDi() => Measure(PrepareGraceMsDi());

            //[Benchmark]
            public object BmarkAutofac() => Measure(PrepareAutofac());

            //[Benchmark]
            public object BmarkAutofacMsDi() => Measure(PrepareAutofacMsDi());
        }

        [MemoryDiagnoser]//, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class OpenScopeAndResolve
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

            ## After using the Use method:

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 1.354 us | 0.0063 us | 0.0059 us |  1.00 |      0.3433 |           - |           - |             1.59 KB |
                    BmarkGraceMsDi | 1.938 us | 0.0099 us | 0.0092 us |  1.43 |      0.8926 |           - |           - |             4.12 KB |
                   BmarkDryIocMsDi | 2.689 us | 0.0121 us | 0.0113 us |  1.99 |      0.6905 |           - |           - |              3.2 KB |

            ## After optimization in delegate factory registration in DI.MS.DI:

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.378 us | 0.0058 us | 0.0052 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                       BmarkDryIoc |  1.859 us | 0.0027 us | 0.0024 us |  1.35 |    0.00 |      0.5093 |           - |           - |             2.35 KB |
                        BmarkGrace |  2.172 us | 0.0343 us | 0.0320 us |  1.58 |    0.03 |      1.1406 |           - |           - |             5.26 KB |
                    BmarkGraceMsDi |  2.338 us | 0.0454 us | 0.0540 us |  1.68 |    0.04 |      1.0529 |           - |           - |             4.87 KB |
                   BmarkDryIocMsDi |  2.664 us | 0.0505 us | 0.0496 us |  1.93 |    0.04 |      0.6905 |           - |           - |              3.2 KB |
                      BmarkAutofac | 13.193 us | 0.0625 us | 0.0554 us |  9.57 |    0.06 |      3.9368 |           - |           - |            18.18 KB |
                  BmarkAutofacMsDi | 18.649 us | 0.1764 us | 0.1650 us | 13.53 |    0.13 |      5.2795 |           - |           - |            24.42 KB |

                ## Interpreting  factory delegate + register delegate for instance
                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.367 us | 0.0102 us | 0.0080 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                       BmarkDryIoc |  1.851 us | 0.0078 us | 0.0073 us |  1.35 |    0.01 |      0.5093 |           - |           - |             2.35 KB |
                        BmarkGrace |  2.145 us | 0.0073 us | 0.0068 us |  1.57 |    0.01 |      1.1253 |           - |           - |              5.2 KB |
                    BmarkGraceMsDi |  2.275 us | 0.0227 us | 0.0212 us |  1.66 |    0.01 |      1.0681 |           - |           - |             4.93 KB |
                   BmarkDryIocMsDi |  2.606 us | 0.0157 us | 0.0147 us |  1.90 |    0.01 |      0.6905 |           - |           - |              3.2 KB |
                      BmarkAutofac | 13.152 us | 0.0745 us | 0.0660 us |  9.62 |    0.09 |      3.9368 |           - |           - |            18.18 KB |
                  BmarkAutofacMsDi | 18.202 us | 0.0819 us | 0.0726 us | 13.31 |    0.07 |      5.2795 |           - |           - |            24.42 KB |


            ## wip: Use method for instances 
                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.388 us | 0.0094 us | 0.0083 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                       BmarkDryIoc |  1.887 us | 0.0118 us | 0.0105 us |  1.36 |    0.01 |      0.5093 |           - |           - |             2.35 KB |
                   BmarkDryIocMsDi |  1.959 us | 0.0084 us | 0.0078 us |  1.41 |    0.01 |      0.6905 |           - |           - |              3.2 KB |
                    BmarkGraceMsDi |  2.115 us | 0.0093 us | 0.0087 us |  1.52 |    0.01 |      0.9460 |           - |           - |             4.37 KB |
                        BmarkGrace |  2.133 us | 0.0096 us | 0.0085 us |  1.54 |    0.01 |      1.0719 |           - |           - |             4.95 KB |
                      BmarkAutofac | 13.472 us | 0.0888 us | 0.0742 us |  9.71 |    0.06 |      3.9368 |           - |           - |            18.18 KB |
                  BmarkAutofacMsDi | 18.157 us | 0.0679 us | 0.0635 us | 13.08 |    0.09 |      5.2795 |           - |           - |            24.42 KB |


            ## Optimizing RegisterDelegate (removing separate Resolve call by default - when is not used for generation in DryIocZero)

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.419 us | 0.0047 us | 0.0043 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                       BmarkDryIoc |  1.692 us | 0.0057 us | 0.0053 us |  1.19 |    0.01 |      0.5093 |           - |           - |             2.35 KB |
                   BmarkDryIocMsDi |  1.821 us | 0.0068 us | 0.0057 us |  1.28 |    0.01 |      0.8488 |           - |           - |             3.91 KB |
                        BmarkGrace |  1.869 us | 0.0337 us | 0.0299 us |  1.32 |    0.02 |      0.8831 |           - |           - |             4.07 KB |
                    BmarkGraceMsDi |  2.241 us | 0.0069 us | 0.0061 us |  1.58 |    0.01 |      1.0529 |           - |           - |             4.87 KB |
                      BmarkAutofac | 12.845 us | 0.1024 us | 0.0957 us |  9.05 |    0.07 |      3.6163 |           - |           - |             16.7 KB |
                  BmarkAutofacMsDi | 18.235 us | 0.2559 us | 0.2393 us | 12.85 |    0.18 |      5.0964 |           - |           - |            23.55 KB |

            ## Optimized interpreting of the ScopeOrSingleton and registered FactoryDelegate - now DI.MS.DI is faster than DI :)

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.640 us | 0.0301 us | 0.0281 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                   BmarkDryIocMsDi |  1.848 us | 0.0301 us | 0.0251 us |  1.13 |    0.02 |      0.6828 |           - |           - |             3.15 KB |
                       BmarkDryIoc |  1.949 us | 0.0370 us | 0.0346 us |  1.19 |    0.03 |      0.5074 |           - |           - |             2.35 KB |
                        BmarkGrace |  2.197 us | 0.0343 us | 0.0321 us |  1.34 |    0.03 |      0.9079 |           - |           - |              4.2 KB |
                    BmarkGraceMsDi |  2.616 us | 0.0508 us | 0.0499 us |  1.59 |    0.04 |      1.0262 |           - |           - |             4.74 KB |
                      BmarkAutofac | 15.111 us | 0.1772 us | 0.1658 us |  9.22 |    0.17 |      3.6011 |           - |           - |             16.7 KB |
                  BmarkAutofacMsDi | 22.683 us | 0.2758 us | 0.2579 us | 13.84 |    0.26 |      5.4932 |           - |           - |             25.4 KB |

                ## Initial RegisterInstance performance - bit it!

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |  1.023 us | 0.0042 us | 0.0039 us |  0.73 |    0.00 |      0.5093 |           - |           - |             2.35 KB |
 BmarkMicrosoftDependencyInjection |  1.401 us | 0.0080 us | 0.0067 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                   BmarkDryIocMsDi |  1.545 us | 0.0143 us | 0.0134 us |  1.10 |    0.01 |      0.6828 |           - |           - |             3.15 KB |
                    BmarkGraceMsDi |  2.276 us | 0.0382 us | 0.0357 us |  1.62 |    0.02 |      1.0529 |           - |           - |             4.87 KB |
                        BmarkGrace |  2.313 us | 0.0306 us | 0.0286 us |  1.65 |    0.02 |      1.1787 |           - |           - |             5.45 KB |
                      BmarkAutofac | 13.339 us | 0.2588 us | 0.2657 us |  9.50 |    0.19 |      3.6163 |           - |           - |             16.7 KB |
                  BmarkAutofacMsDi | 21.783 us | 0.3098 us | 0.2898 us | 15.51 |    0.20 |      5.4932 |           - |           - |            25.45 KB |

                ## Optimized register instance

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |  1.035 us | 0.0121 us | 0.0113 us |  0.73 |    0.01 |      0.5093 |           - |           - |             2.35 KB |
 BmarkMicrosoftDependencyInjection |  1.411 us | 0.0059 us | 0.0055 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                   BmarkDryIocMsDi |  1.570 us | 0.0198 us | 0.0185 us |  1.11 |    0.01 |      0.6828 |           - |           - |             3.15 KB |
                        BmarkGrace |  1.989 us | 0.0193 us | 0.0181 us |  1.41 |    0.01 |      0.9232 |           - |           - |             4.26 KB |
                    BmarkGraceMsDi |  2.404 us | 0.0392 us | 0.0348 us |  1.70 |    0.02 |      1.0796 |           - |           - |             4.99 KB |
                      BmarkAutofac | 13.317 us | 0.1708 us | 0.1598 us |  9.44 |    0.12 |      3.6163 |           - |           - |             16.7 KB |
                  BmarkAutofacMsDi | 19.549 us | 0.3447 us | 0.3225 us | 13.85 |    0.24 |      5.0964 |           - |           - |            23.55 KB |

                ## Test with the Dummy Population (and moved to .Net Core v2.2)

            BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
.NET Core SDK=2.2.100
  [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
  DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc |  1.036 us | 0.0044 us | 0.0041 us |  0.76 |    0.00 |      0.5093 |           - |           - |             2.35 KB |
 BmarkMicrosoftDependencyInjection |  1.355 us | 0.0046 us | 0.0043 us |  1.00 |    0.00 |      0.3433 |           - |           - |             1.59 KB |
                   BmarkDryIocMsDi |  1.496 us | 0.0059 us | 0.0052 us |  1.10 |    0.00 |      0.6828 |           - |           - |             3.15 KB |
                        BmarkGrace |  2.028 us | 0.0084 us | 0.0079 us |  1.50 |    0.01 |      1.0452 |           - |           - |             4.82 KB |
                    BmarkGraceMsDi |  2.077 us | 0.0099 us | 0.0092 us |  1.53 |    0.01 |      0.9460 |           - |           - |             4.37 KB |
                      BmarkAutofac | 13.465 us | 0.0484 us | 0.0429 us |  9.94 |    0.06 |      3.9825 |           - |           - |            18.38 KB |
                  BmarkAutofacMsDi | 19.185 us | 0.2073 us | 0.1939 us | 14.16 |    0.12 |      5.4932 |           - |           - |             25.4 KB |

            ## Test with 4 level graph + disposables + fixed scoped things for DryIoc:

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  3.222 us | 0.0138 us | 0.0129 us |  1.00 |    0.00 |      0.8354 |           - |           - |             3.87 KB |
                       BmarkDryIoc |  4.274 us | 0.0276 us | 0.0259 us |  1.33 |    0.01 |      1.9531 |           - |           - |             9.02 KB |
                   BmarkDryIocMsDi |  4.498 us | 0.0237 us | 0.0222 us |  1.40 |    0.01 |      1.9608 |           - |           - |             9.06 KB |
                        BmarkGrace |  4.604 us | 0.0271 us | 0.0254 us |  1.43 |    0.01 |      2.3499 |           - |           - |            10.85 KB |
                    BmarkGraceMsDi |  5.280 us | 0.0267 us | 0.0236 us |  1.64 |    0.01 |      2.2202 |           - |           - |            10.24 KB |
                      BmarkAutofac | 37.600 us | 0.5655 us | 0.5289 us | 11.67 |    0.16 |      9.3994 |      0.0610 |           - |            43.47 KB |
                  BmarkAutofacMsDi | 49.487 us | 0.4901 us | 0.4585 us | 15.36 |    0.13 |     13.3667 |      0.1221 |           - |            61.75 KB |

            ## After returning ScopedOrSingleton to use lambda 

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  3.308 us | 0.0178 us | 0.0166 us |  1.00 |    0.00 |      0.8354 |           - |           - |             3.87 KB |
                       BmarkDryIoc |  4.331 us | 0.0239 us | 0.0223 us |  1.31 |    0.01 |      1.9531 |           - |           - |             9.02 KB |
                        BmarkGrace |  4.374 us | 0.0806 us | 0.0754 us |  1.32 |    0.03 |      1.9684 |           - |           - |              9.1 KB |
                   BmarkDryIocMsDi |  5.144 us | 0.0819 us | 0.0766 us |  1.56 |    0.03 |      2.1439 |           - |           - |             9.91 KB |
                    BmarkGraceMsDi |  5.172 us | 0.0858 us | 0.0803 us |  1.56 |    0.03 |      2.1133 |           - |           - |             9.74 KB |
                      BmarkAutofac | 40.098 us | 0.6651 us | 0.6221 us | 12.12 |    0.17 |      9.8267 |           - |           - |            45.37 KB |
                  BmarkAutofacMsDi | 51.747 us | 1.0334 us | 1.4821 us | 15.47 |    0.52 |     12.6953 |           - |           - |            58.53 KB |

            ## DryIoc v4.0.4 and Grace v7

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  3.872 us | 0.0406 us | 0.0360 us |  1.00 |    0.00 |      0.9460 |           - |           - |             4.37 KB |
                       BmarkDryIoc |  4.706 us | 0.0450 us | 0.0399 us |  1.22 |    0.01 |      2.0828 |           - |           - |             9.62 KB |
                   BmarkDryIocMsDi |  4.889 us | 0.0272 us | 0.0255 us |  1.26 |    0.01 |      2.0905 |           - |           - |             9.66 KB |
                        BmarkGrace |  2.577 us | 0.0110 us | 0.0103 us |  0.67 |    0.01 |      0.5798 |           - |           - |             2.69 KB |
                    BmarkGraceMsDi |  3.232 us | 0.0137 us | 0.0128 us |  0.83 |    0.01 |      0.6332 |           - |           - |             2.93 KB |
                      BmarkAutofac | 38.488 us | 0.2379 us | 0.2225 us |  9.94 |    0.12 |      9.7656 |           - |           - |             45.2 KB |
                  BmarkAutofacMsDi | 47.389 us | 0.1995 us | 0.1866 us | 12.24 |    0.11 |     12.5732 |      0.1221 |           - |            58.09 KB |

            ## After scaling the locks

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 3.674 us | 0.0145 us | 0.0121 us |  1.00 |      0.9460 |           - |           - |             4.37 KB |
                       BmarkDryIoc | 4.399 us | 0.0168 us | 0.0140 us |  1.20 |      2.0828 |           - |           - |             9.62 KB |
                   BmarkDryIocMsDi | 4.598 us | 0.0586 us | 0.0548 us |  1.25 |      2.0905 |           - |           - |             9.66 KB |
                        BmarkGrace | 2.606 us | 0.0276 us | 0.0258 us |  0.71 |      0.5798 |           - |           - |             2.69 KB |
                    BmarkGraceMsDi | 3.311 us | 0.0423 us | 0.0396 us |  0.90 |      0.6332 |           - |           - |             2.93 KB |

            ## Hybrid array[tree] collection for scoped items

                            Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 3.836 us | 0.0331 us | 0.0309 us |  1.00 |    0.00 |      0.9460 |           - |           - |             4.37 KB |
                       BmarkDryIoc | 4.000 us | 0.0265 us | 0.0248 us |  1.04 |    0.01 |      1.6174 |           - |           - |             7.47 KB |
                   BmarkDryIocMsDi | 4.135 us | 0.0322 us | 0.0301 us |  1.08 |    0.01 |      1.6251 |           - |           - |             7.52 KB |
                        BmarkGrace | 2.590 us | 0.0071 us | 0.0066 us |  0.68 |    0.01 |      0.5798 |           - |           - |             2.69 KB |
                    BmarkGraceMsDi | 3.336 us | 0.0560 us | 0.0523 us |  0.87 |    0.02 |      0.6332 |           - |           - |             2.93 KB |
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
                Measure(_msDi = PrepareMsDi());
                Measure(_dryioc = PrepareDryIoc());
                Measure(_dryIocMsDi = PrepareDryIocMsDi());
                Measure(_grace = PrepareGrace());
                Measure(_graceMsDi = PrepareGraceMsDi());
                Measure(_autofac = PrepareAutofac());
                Measure(_autofacMsDi = PrepareAutofacMsDi());
            }

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(_msDi);

            [Benchmark]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark]
            public object BmarkDryIocMsDi() => Measure(_dryIocMsDi);

            //[Benchmark]
            public object BmarkGrace() => Measure(_grace);

            //[Benchmark]
            public object BmarkGraceMsDi() => Measure(_graceMsDi);

            //[Benchmark]
            public object BmarkAutofac() => Measure(_autofac);

            //[Benchmark]
            public object BmarkAutofacMsDi() => Measure(_autofacMsDi);
        }
    }
}
