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
            var serviceProvider = new Container().WithDependencyInjectionAdapter(GetServices()).BuildServiceProvider();
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
            block.Export<D1>().Lifestyle.SingletonPerScope();
            block.Export<D2>().Lifestyle.SingletonPerScope();
            block.Export<D3>().Lifestyle.SingletonPerScope();
            block.Export<D4>().Lifestyle.SingletonPerScope();
            block.Export<D5>().Lifestyle.SingletonPerScope();
            block.Export<D6>().Lifestyle.SingletonPerScope();
            block.Export<D7>().Lifestyle.SingletonPerScope();
            block.Export<D8>().Lifestyle.SingletonPerScope();
            block.Export<D9>().Lifestyle.SingletonPerScope();
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
            builder.RegisterType<D1>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D2>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D3>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D4>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D5>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D6>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D7>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D8>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<D9>().AsSelf().InstancePerLifetimeScope();
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

        [MemoryDiagnoser]//, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve
        {
            /*
            ## DryIoc v3 and DI.MS.DI v2.1

            BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
            Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
            Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
            .NET Core SDK=2.2.100
              [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
              DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT


                            Method |        Mean |      Error |     StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|-----------:|-----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    128.3 us |   1.591 us |   1.489 us |   1.00 |    0.00 |     13.7939 |           - |           - |            58.66 KB |
                      BmarkAutofac |    425.8 us |   2.081 us |   1.947 us |   3.32 |    0.05 |     76.6602 |      2.4414 |           - |           354.91 KB |
                  BmarkAutofacMsDi |    434.9 us |   1.827 us |   1.709 us |   3.39 |    0.03 |     80.0781 |      0.4883 |           - |           371.22 KB |
                        BmarkGrace | 17,839.6 us |  99.023 us |  87.781 us | 138.99 |    1.72 |    156.2500 |     62.5000 |           - |           781.49 KB |
                    BmarkGraceMsDi | 20,568.8 us | 116.222 us | 108.714 us | 160.39 |    2.00 |    187.5000 |     93.7500 |     31.2500 |           954.13 KB |
                       BmarkDryIoc | 47,340.9 us | 175.076 us | 146.196 us | 368.47 |    3.79 |     90.9091 |           - |           - |            759.4 KB |
                   BmarkDryIocMsDi | 46,669.8 us | 302.005 us | 282.496 us | 363.92 |    4.44 |    181.8182 |     90.9091 |           - |           855.37 KB |

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

            [Benchmark]
            public object BmarkAutofacMsDi() => Measure(PrepareAutofacMsDi());
        }

        [MemoryDiagnoser]//, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class OpenScopeAndResolve
        {
            /*
            ## DryIoc v3 and DI.MS.DI v2.1

             BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
            Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
            Frequency=2156251 Hz, Resolution=463.7679 ns, Timer=TSC
            .NET Core SDK=2.2.100
              [Host]     : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT
              DefaultJob : .NET Core 2.2.0 (CoreCLR 4.6.27110.04, CoreFX 4.6.27110.04), 64bit RyuJIT


                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  3.407 us | 0.0131 us | 0.0116 us |  1.00 |    0.00 |      0.8354 |           - |           - |             3.87 KB |
                        BmarkGrace |  3.954 us | 0.0102 us | 0.0095 us |  1.16 |    0.01 |      1.8921 |           - |           - |             8.73 KB |
                    BmarkGraceMsDi |  5.253 us | 0.0126 us | 0.0111 us |  1.54 |    0.01 |      2.2736 |           - |           - |            10.49 KB |
                       BmarkDryIoc | 25.944 us | 0.0288 us | 0.0270 us |  7.61 |    0.03 |      3.8757 |           - |           - |            17.88 KB |
                   BmarkDryIocMsDi | 29.552 us | 0.0841 us | 0.0746 us |  8.67 |    0.04 |      4.7302 |           - |           - |            21.94 KB |
                      BmarkAutofac | 40.997 us | 0.1914 us | 0.1790 us | 12.04 |    0.08 |     10.8643 |           - |           - |            50.23 KB |
                  BmarkAutofacMsDi | 51.114 us | 0.2980 us | 0.2788 us | 15.00 |    0.08 |     14.1602 |           - |           - |            65.39 KB |
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
    }
}
