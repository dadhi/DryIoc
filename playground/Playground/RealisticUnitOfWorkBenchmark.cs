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
using DryIoc.FastExpressionCompiler.LightExpression;

namespace PerformanceTests
{
    public class RealisticUnitOfWorkBenchmark
    {
        public static IContainer PrepareDryIoc() =>
            PrepareDryIoc(new Container());

        public static IContainer PrepareDryIoc_WebRequestScoped() =>
            PrepareDryIoc_WebRequestScoped(new Container());

        public static IContainer PrepareDryIocInterpretationOnly() =>
            PrepareDryIoc(new Container(Rules.Default.WithUseInterpretation()));

        private static IContainer PrepareDryIoc(IContainer container)
        {
            // register dummy scoped and singletons services to populate resolution cache and scopes to be close to reality
            RegisterDummyPopulation(container);

            // register graph for benchmarking starting with scoped R (root) / Controller
            container.Register<R>(Reuse.Scoped);

            container.Register<Scoped1>(Reuse.Scoped);
            container.Register<Scoped2>(Reuse.Scoped);

            container.Register<Trans1>(Reuse.Transient);
            container.Register<Trans2>(Reuse.Transient);

            container.Register<Single1>(Reuse.Singleton);
            container.Register<Single2>(Reuse.Singleton);

            container.RegisterDelegate<Scoped1, Scoped3, Single1, SingleObj1, ScopedFac1>(
                (scoped1, scoped3, single1, singleObj1) => new ScopedFac1(scoped1, scoped3, single1, singleObj1),
                Reuse.Scoped);

            container.RegisterDelegate<Scoped2, Scoped4, Single2, SingleObj2, ScopedFac2>(
                (scoped2, scoped4, single2, singleObj2) => new ScopedFac2(scoped2, scoped4, single2, singleObj2),
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

            container.RegisterDelegate<Scoped13, Single1, SingleObj13, ScopedFac12>(
                (scoped13, single1, singleObj13) => new ScopedFac12(scoped13, single1, singleObj13),
                Reuse.Scoped);

            container.RegisterDelegate<Scoped23, Single2, SingleObj23, ScopedFac22>(
                (scoped23, single2, singleObj23) => new ScopedFac22(scoped23, single2, singleObj23),
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

            container.RegisterDelegate<Single1, Scoped14, ScopedFac14, ScopedFac13>(
                (single1, scoped14, scopedFac14) => new ScopedFac13(single1, scoped14, scopedFac14),
                Reuse.Scoped);

            container.RegisterDelegate<Single2, Scoped24, ScopedFac24, ScopedFac23>(
                (single2, scoped24, scopedFac24) => new ScopedFac23(single2, scoped24, scopedFac24),
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

        private static IContainer PrepareDryIoc_WebRequestScoped(IContainer container)
        {
            // register dummy scoped and singletons services to populate resolution cache and scopes to be close to reality
            RegisterDummyPopulation(container);

            // register graph for benchmarking starting with scoped R (root) / Controller
            container.Register<R>(Reuse.InWebRequest);

            container.Register<Scoped1>(Reuse.InWebRequest);
            container.Register<Scoped2>(Reuse.InWebRequest);

            container.Register<Trans1>(Reuse.Transient);
            container.Register<Trans2>(Reuse.Transient);

            container.Register<Single1>(Reuse.Singleton);
            container.Register<Single2>(Reuse.Singleton);

            container.RegisterDelegate<Scoped1, Scoped3, Single1, SingleObj1, ScopedFac1>(
                (scoped1, scoped3, single1, singleObj1) => new ScopedFac1(scoped1, scoped3, single1, singleObj1),
                Reuse.InWebRequest);

            container.RegisterDelegate<Scoped2, Scoped4, Single2, SingleObj2, ScopedFac2>(
                (scoped2, scoped4, single2, singleObj2) => new ScopedFac2(scoped2, scoped4, single2, singleObj2),
                Reuse.InWebRequest);

            container.RegisterInstance(new SingleObj1());
            container.RegisterInstance(new SingleObj2());

            // level 2
            container.Register<Scoped3>(Reuse.InWebRequest);
            container.Register<Scoped4>(Reuse.InWebRequest);

            container.Register<Scoped12>(Reuse.InWebRequest);
            container.Register<Scoped22>(Reuse.InWebRequest);

            container.Register<Single12>(Reuse.Singleton);
            container.Register<Single22>(Reuse.Singleton);

            container.Register<Trans12>(Reuse.Transient);
            container.Register<Trans22>(Reuse.Transient);

            container.RegisterDelegate<Scoped13, Single1, SingleObj13, ScopedFac12>(
                (scoped13, single1, singleObj13) => new ScopedFac12(scoped13, single1, singleObj13),
                Reuse.InWebRequest);

            container.RegisterDelegate<Scoped23, Single2, SingleObj23, ScopedFac22>(
                (scoped23, single2, singleObj23) => new ScopedFac22(scoped23, single2, singleObj23),
                Reuse.InWebRequest);

            container.RegisterInstance(new SingleObj12());
            container.RegisterInstance(new SingleObj22());

            // level 3
            container.Register<Scoped13>(Reuse.InWebRequest);
            container.Register<Scoped23>(Reuse.InWebRequest);

            container.Register<Single13>(Reuse.Singleton);
            container.Register<Single23>(Reuse.Singleton);

            container.Register<Trans13>(Reuse.Transient);
            container.Register<Trans23>(Reuse.Transient);

            container.RegisterDelegate<Single1, Scoped14, ScopedFac14, ScopedFac13>(
                (single1, scoped14, scopedFac14) => new ScopedFac13(single1, scoped14, scopedFac14),
                Reuse.InWebRequest);

            container.RegisterDelegate<Single2, Scoped24, ScopedFac24, ScopedFac23>(
                (single2, scoped24, scopedFac24) => new ScopedFac23(single2, scoped24, scopedFac24),
                Reuse.InWebRequest);

            container.RegisterInstance(new SingleObj13());
            container.RegisterInstance(new SingleObj23());

            // level 4
            container.Register<Scoped14>(Reuse.Scoped);
            container.Register<Scoped24>(Reuse.Scoped);

            container.Register<Single14>(Reuse.Singleton);
            container.Register<Single24>(Reuse.Singleton);

            container.Register<Trans14>(Reuse.Transient);
            container.Register<Trans24>(Reuse.Transient);

            container.RegisterDelegate(r => new ScopedFac14(), Reuse.InWebRequest);
            container.RegisterDelegate(r => new ScopedFac24(), Reuse.InWebRequest);

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

        public static LambdaExpression ResolveExpression(IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<LambdaExpression>(typeof(R));
        }

        public static object Measure_WebRequestScoped(IContainer container)
        {
            using (var scope = container.OpenScope(Reuse.WebRequestScopeName))
                return scope.Resolve<R>();
        }

        private static void RegisterDummyPopulation(IContainer container)
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

        public static ServiceCollection AddServices()
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
            var serviceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(AddServices());
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
            var serviceProvider = DryIocAdapter.BuildDryIocServiceProvider(AddServices());
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

            container.Populate(AddServices());

            var serviceProvider = container.Locate<IServiceProvider>();

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

        public static IServiceProvider PrepareAutofacMsDi()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(AddServices());

            var container = containerBuilder.Build();

            var serviceProvider = new AutofacServiceProvider(container);

            ResolveDummyPopulation(serviceProvider);
            return serviceProvider;
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

        public static IServiceProvider PrepareLamarMsDi() =>
            new Lamar.Container(AddServices());


        [MemoryDiagnoser]
        public class CompileResolutionExpression
        {
            /*

            ## Baseline 27.03.2022

            BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
            Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
            .NET Core SDK=6.0.201
              [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
              DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT


            |                  Method |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 |  Gen 2 | Allocated |
            |------------------------ |-----------:|---------:|---------:|------:|--------:|--------:|-------:|-------:|----------:|
            |  CompileLightExpression |   430.1 us |  8.42 us | 16.62 us |  1.00 |    0.00 | 18.5547 | 9.2773 | 3.4180 | 115.17 KB |
            | CompileSystemExpression | 1,072.8 us | 14.31 us | 13.38 us |  2.55 |    0.17 | 35.1563 | 9.7656 |      - | 216.11 KB |

            ## Intrinsic, no array, single lambda, no GetParameters for GetCurrentScopeOrThrow

            |                  Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 |   Gen 1 |  Gen 2 | Allocated |
            |------------------------ |---------:|--------:|--------:|------:|--------:|--------:|--------:|-------:|----------:|
            |  CompileLightExpression | 369.1 us | 3.39 us | 2.83 us |  1.00 |    0.00 | 18.0664 |  8.7891 | 3.4180 | 112.88 KB |
            | CompileSystemExpression | 712.7 us | 8.17 us | 6.82 us |  1.93 |    0.03 | 35.1563 | 10.7422 |      - |  216.1 KB |

            ## RegisterDelegate wins

            |                  Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 |   Gen 1 |  Gen 2 | Allocated |
            |------------------------ |---------:|--------:|--------:|------:|--------:|--------:|--------:|-------:|----------:|
            |  CompileLightExpression | 363.1 us | 6.89 us | 6.11 us |  1.00 |    0.00 | 18.0664 |  8.7891 | 2.9297 | 111.51 KB |
            | CompileSystemExpression | 702.9 us | 2.52 us | 2.10 us |  1.94 |    0.03 | 35.1563 | 10.7422 |      - |  216.1 KB |

            ## Optimizing Invoke

            |                  Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |   Gen 1 |  Gen 2 | Allocated |
            |------------------------ |---------:|---------:|---------:|------:|--------:|--------:|--------:|-------:|----------:|
            |  CompileLightExpression | 445.2 us |  8.81 us | 20.07 us |  1.00 |    0.00 | 33.2031 | 15.6250 | 3.9063 | 111.36 KB |
            | CompileSystemExpression | 842.9 us | 16.06 us | 25.00 us |  1.91 |    0.11 | 69.3359 | 17.5781 |      - | 216.18 KB |
            */
            LambdaExpression _lightExpr;
            System.Linq.Expressions.LambdaExpression _sysExpr;

            [GlobalSetup]
            public void Setup()
            {
                _lightExpr = ResolveExpression(PrepareDryIoc());
                _sysExpr = _lightExpr.ToLambdaExpression();
            }

            [Benchmark(Baseline = true)]
            public object CompileLightExpression() => _lightExpr.CompileFast();

            [Benchmark]
            public object CompileSystemExpression() => _sysExpr.Compile();
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

            ## v4.1

            BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
            Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
            .NET Core SDK=3.1.100
              [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
              DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT


            |                            Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |   Gen 0 |   Gen 1 | Gen 2 | Allocated |
            |---------------------------------- |------------:|----------:|----------:|-------:|--------:|--------:|--------:|------:|----------:|
            | BmarkMicrosoftDependencyInjection |    29.24 us |  0.233 us |  0.218 us |   1.00 |    0.00 |  9.0332 |  0.0305 |     - |  41.54 KB |
            |                       BmarkDryIoc |    35.03 us |  0.223 us |  0.186 us |   1.20 |    0.01 |  8.7891 |  0.0610 |     - |  40.63 KB |
            |                   BmarkDryIocMsDi |    35.96 us |  0.233 us |  0.218 us |   1.23 |    0.01 |  9.5825 |  0.1831 |     - |   44.3 KB |
            |                        BmarkGrace | 5,210.27 us | 28.299 us | 26.471 us | 178.18 |    1.81 | 70.3125 | 31.2500 |     - | 332.61 KB |
            |                    BmarkGraceMsDi | 5,277.32 us | 37.934 us | 31.677 us | 180.23 |    1.77 | 78.1250 | 39.0625 |     - | 365.01 KB |
            |                      BmarkAutofac |   300.60 us |  2.431 us |  2.030 us |  10.27 |    0.11 | 53.7109 | 15.6250 |     - | 248.44 KB |
            |                  BmarkAutofacMsDi |   307.78 us |  1.040 us |  0.922 us |  10.52 |    0.09 | 55.1758 | 17.0898 |     - | 254.09 KB |

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

            ## After enabling interpretation of Func<IResolverContext, object> registered by RegisterDelegate - found the culprit

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

            ## Optimized interpreting of the ScopeOrSingleton and registered Func<IResolverContext, object> - now DI.MS.DI is faster than DI :)

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

            ## DryIoc v4.0.7

                            Method |        Mean |       Error |      StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |------------:|------------:|------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |    148.0 us |   1.5359 us |   1.4366 us |   1.00 |    0.00 |     18.4326 |      0.1221 |           - |            79.69 KB |
                       BmarkDryIoc |    149.3 us |   0.6155 us |   0.5757 us |   1.01 |    0.01 |     27.3438 |      0.2441 |           - |           126.06 KB |
                   BmarkDryIocMsDi |    160.8 us |   0.8074 us |   0.7552 us |   1.09 |    0.01 |     29.0527 |      0.7324 |           - |           134.63 KB |
                        BmarkGrace | 19,240.3 us | 174.1336 us | 162.8847 us | 130.03 |    1.86 |    156.2500 |     62.5000 |           - |           739.73 KB |
                    BmarkGraceMsDi | 22,149.9 us | 122.6704 us | 114.7460 us | 149.69 |    1.48 |    187.5000 |     93.7500 |           - |           909.15 KB |
                      BmarkAutofac |    696.3 us |   4.9448 us |   4.3834 us |   4.71 |    0.07 |    102.5391 |      2.9297 |           - |            472.8 KB |
                  BmarkAutofacMsDi |    688.8 us |   7.1206 us |   6.6606 us |   4.66 |    0.07 |    105.4688 |      0.9766 |           - |           489.97 KB |


            ### FEC v3.0 and multiple improvements: fan-out cache, and scope storage, per container expression cache, etc.

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT

|       Method |         Mean |      Error |     StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 | Gen 2 | Allocated |
|------------- |-------------:|-----------:|-----------:|-------:|--------:|---------:|--------:|------:|----------:|
|         MsDI |     76.74 us |   0.570 us |   0.505 us |   1.00 |    0.00 |  16.1133 |  0.2441 |     - |  74.23 KB |
|       DryIoc |     92.62 us |   0.763 us |   0.714 us |   1.21 |    0.02 |  15.1367 |  1.3428 |     - |  69.55 KB |
|  DryIoc_MsDI |    116.60 us |   1.849 us |   1.544 us |   1.52 |    0.03 |  19.2871 |  1.8311 |     - |  88.85 KB |
|        Grace | 15,844.41 us |  72.839 us |  64.570 us | 206.48 |    1.70 | 156.2500 | 62.5000 |     - | 729.29 KB |
|   Grace_MsDI | 19,203.81 us | 139.461 us | 130.452 us | 250.25 |    2.78 | 187.5000 | 93.7500 |     - | 899.61 KB |
|      Autofac |    517.68 us |   1.748 us |   1.635 us |   6.75 |    0.06 | 101.5625 | 24.4141 |     - | 468.08 KB |
| Autofac_MsDI |    524.51 us |   2.640 us |   2.340 us |   6.84 |    0.06 | 101.5625 | 24.4141 |     - |  466.9 KB |


### DryIoc 4.1.3 (.MsDI 3.0.3), MsDI 3.1.3, Grace 7.1.0 (.MsDI 7.0.1), Autofac 5.1.2 (.MsDI 6.0.0), Lamar 4.2.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.200
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT

|       Method |         Mean |     Error |    StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 | Gen 2 | Allocated |
|------------- |-------------:|----------:|----------:|-------:|--------:|---------:|--------:|------:|----------:|
|         MsDI |     99.02 us |  1.956 us |  2.806 us |   1.00 |    0.00 |  16.1133 |  0.2441 |     - |  74.24 KB |
|       DryIoc |     97.25 us |  0.493 us |  0.461 us |   0.97 |    0.03 |  15.1367 |  1.3428 |     - |  69.79 KB |
|  DryIoc_MsDI |    124.04 us |  1.770 us |  1.655 us |   1.24 |    0.04 |  19.2871 |  1.8311 |     - |   89.1 KB |
|        Grace | 16,869.55 us | 80.435 us | 75.239 us | 168.94 |    5.72 | 156.2500 | 62.5000 |     - | 727.59 KB |
|   Grace_MsDI | 20,468.19 us | 66.869 us | 62.549 us | 204.98 |    7.02 | 187.5000 | 93.7500 |     - | 898.37 KB |
|   Lamar_MsDI |  6,060.29 us | 23.102 us | 20.479 us |  60.55 |    2.06 | 140.6250 | 23.4375 |     - | 646.33 KB |
|      Autofac |    583.26 us | 18.342 us | 17.157 us |   5.84 |    0.21 | 102.5391 | 28.3203 |     - | 472.86 KB |
| Autofac_MsDI |    561.82 us |  4.129 us |  3.862 us |   5.63 |    0.20 | 101.5625 | 27.3438 |     - | 467.85 KB |

## DryIoc v4.2.0

|      Method |     Mean |   Error |  StdDev |   Median | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|--------:|--------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|        MsDI | 145.4 us | 4.46 us | 9.70 us | 140.6 us |  1.00 |    0.00 | 16.9678 | 0.1221 |     - |  73.15 KB |
|      DryIoc | 102.2 us | 1.89 us | 1.67 us | 102.1 us |  0.63 |    0.01 | 14.4043 | 0.1221 |     - |  66.77 KB |
| DryIoc_MsDI | 126.2 us | 2.00 us | 1.87 us | 126.1 us |  0.78 |    0.02 | 19.0430 | 0.2441 |     - |  87.93 KB |

## DryIoc 4.2.5

|      Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|--------:|--------:|------:|--------:|--------:|-------:|------:|----------:|
|        MsDI | 113.8 us | 2.26 us | 6.06 us |  1.00 |    0.00 | 18.0664 | 0.1221 |     - |  73.85 KB |
|      DryIoc | 107.6 us | 1.83 us | 1.71 us |  1.00 |    0.13 | 16.4795 | 0.1221 |     - |  67.52 KB |
| DryIoc_MsDI | 129.8 us | 1.03 us | 0.91 us |  1.22 |    0.18 | 21.4844 | 0.2441 |     - |   88.6 KB |

## DryIoc 4.4.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT

|      Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|--------:|--------:|------:|--------:|--------:|-------:|------:|----------:|
|        MsDI | 132.4 us | 1.78 us | 1.66 us |  1.00 |    0.00 | 18.0664 | 0.2441 |     - |  73.86 KB |
|      DryIoc | 112.1 us | 1.33 us | 1.24 us |  0.85 |    0.01 | 16.4795 | 0.1221 |     - |  67.52 KB |
| DryIoc_MsDI | 141.0 us | 1.54 us | 1.36 us |  1.07 |    0.02 | 21.4844 | 0.2441 |     - |   88.6 KB |

## DryIoc 4.5.0 (.MsDI 5.0.0), MsDI 3.1.8, Grace 7.1.1 (.MsDI 7.0.1), Autofac 6.0.0 (.MsDI 7.0.2), Lamar 4.3.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


|       Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|------------- |------------:|----------:|----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|         MsDI |    150.8 us |   2.83 us |   3.03 us |   1.00 |    0.00 |  18.0664 |  0.2441 |      - |  73.86 KB |
|       DryIoc |    129.6 us |   1.90 us |   1.68 us |   0.86 |    0.02 |  16.3574 |  0.2441 |      - |  67.52 KB |
|  DryIoc_MsDI |    161.9 us |   1.74 us |   1.63 us |   1.07 |    0.03 |  21.4844 |  0.2441 |      - |   88.6 KB |
|        Grace | 21,380.9 us | 375.46 us | 351.21 us | 141.65 |    2.83 | 156.2500 | 62.5000 |      - | 729.12 KB |
|   Grace_MsDI | 24,102.4 us | 243.21 us | 203.09 us | 159.26 |    3.52 | 187.5000 | 93.7500 |      - | 894.57 KB |
|   Lamar_MsDI | 10,938.2 us | 308.25 us | 874.46 us |  70.86 |    4.29 |        - |       - |      - | 696.16 KB |
|      Autofac |    789.4 us |  19.84 us |  20.38 us |   5.24 |    0.18 |  50.7813 | 25.3906 | 1.9531 | 311.12 KB |
| Autofac_MsDI |    784.9 us |  15.04 us |  18.47 us |   5.20 |    0.15 |  54.6875 | 27.3438 | 1.9531 | 335.07 KB |


## V5

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.202
  [Host]     : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT


|      Method |     Mean |   Error |  StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|--------:|--------:|------:|--------:|--------:|-------:|------:|----------:|
|        MsDI | 113.2 us | 2.66 us | 7.42 us |  1.00 |    0.00 | 16.1133 | 0.1221 |     - |  74.23 KB |
|      DryIoc | 115.0 us | 2.87 us | 8.18 us |  1.02 |    0.10 | 14.4043 | 1.2207 |     - |  66.76 KB |
| DryIoc_MsDI | 141.3 us | 3.51 us | 9.96 us |  1.26 |    0.12 | 19.0430 | 1.7090 |     - |  87.85 KB |

## V5 + ImTools V3

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.200
  [Host]     : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT
  DefaultJob : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT


|              Method |         Mean |      Error |     StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|-------------------- |-------------:|-----------:|-----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|                MsDI |     99.60 us |   1.880 us |   2.012 us |   1.00 |    0.00 |  11.4746 |  2.8076 |      - |  70.54 KB |
|              DryIoc |    103.15 us |   1.769 us |   1.655 us |   1.03 |    0.03 |  10.2539 |  0.7324 |      - |  62.95 KB |
|  DryIoc_MsDIAdapter |    128.59 us |   2.566 us |   2.853 us |   1.29 |    0.04 |  13.6719 |  1.2207 |      - |  84.21 KB |
|               Grace | 17,297.74 us | 336.184 us | 492.774 us | 174.61 |    6.18 |  93.7500 | 31.2500 |      - |  729.5 KB |
|   Grace_MsDIAdapter | 19,746.27 us | 257.440 us | 240.810 us | 197.80 |    3.07 | 125.0000 | 62.5000 |      - | 893.23 KB |
|   Lamar_MsDIAdapter |  6,154.49 us |  99.811 us |  83.346 us |  61.56 |    1.82 | 101.5625 | 31.2500 |      - | 656.43 KB |
|             Autofac |    609.34 us |   8.014 us |   6.692 us |   6.09 |    0.15 |  50.7813 | 25.3906 | 1.9531 | 315.88 KB |
| Autofac_MsDIAdapter |    599.20 us |   6.511 us |   5.771 us |   5.99 |    0.15 |  54.6875 | 27.3438 | 1.9531 | 339.42 KB |


## V5 + FECv3 + ImToolsv3

|              Method |        Mean |     Error |    StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|-------------------- |------------:|----------:|----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|              DryIoc |    134.9 us |   1.82 us |   1.70 us |   0.98 |    0.02 |  10.0098 |  0.7324 |      - |  61.84 KB |
|  DryIoc_MsDIAdapter |    170.9 us |   2.54 us |   2.25 us |   1.24 |    0.04 |  13.1836 |  1.2207 |      - |  80.84 KB |
|                MsDI |    139.2 us |   2.74 us |   3.47 us |   1.00 |    0.00 |  11.4746 |  2.6855 |      - |  70.55 KB |
|               Grace | 25,575.2 us | 444.52 us | 394.06 us | 185.23 |    5.56 |  93.7500 | 31.2500 |      - | 729.54 KB |
|   Grace_MsDIAdapter | 30,124.8 us | 420.66 us | 393.49 us | 217.89 |    7.17 | 125.0000 | 62.5000 |      - | 893.17 KB |
|   Lamar_MsDIAdapter | 12,497.3 us | 249.21 us | 686.38 us |  87.91 |    4.85 |        - |       - |      - | 707.34 KB |
|             Autofac |    923.6 us |  12.14 us |  11.35 us |   6.68 |    0.16 |  50.7813 | 25.3906 | 1.9531 | 315.93 KB |
| Autofac_MsDIAdapter |    891.9 us |  14.95 us |  13.98 us |   6.45 |    0.17 |  54.6875 | 27.3438 | 2.9297 | 339.43 KB |

## V5 release

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT

|       Method |         Mean |      Error |     StdDev |  Ratio | RatioSD |    Gen 0 |   Gen 1 |  Gen 2 | Allocated |
|------------- |-------------:|-----------:|-----------:|-------:|--------:|---------:|--------:|-------:|----------:|
|       DryIoc |     82.22 us |   1.209 us |   1.072 us |   1.00 |    0.00 |   6.3477 |  0.3662 |      - |  39.42 KB |
|  DryIoc_MsDI |     94.18 us |   1.207 us |   1.070 us |   1.15 |    0.02 |   8.0566 |  0.6104 |      - |  49.87 KB |
|         MsDI |     94.60 us |   0.715 us |   0.597 us |   1.15 |    0.01 |  11.8408 |  4.2725 |      - |  72.59 KB |
|      Autofac |    543.45 us |   4.570 us |   3.568 us |   6.60 |    0.10 |  51.7578 | 25.3906 | 1.9531 | 317.19 KB |
| Autofac_MsDI |    534.64 us |   5.919 us |   5.247 us |   6.50 |    0.10 |  54.6875 | 27.3438 | 1.9531 | 340.17 KB |
|   Lamar_MsDI |  7,053.46 us | 140.273 us | 402.469 us |  77.97 |    2.84 |        - |       - |      - | 649.68 KB |
|        Grace | 15,990.58 us | 123.798 us | 109.744 us | 194.52 |    2.21 |  93.7500 | 31.2500 |      - | 736.12 KB |
|   Grace_MsDI | 18,884.30 us | 321.388 us | 268.373 us | 229.50 |    4.25 | 125.0000 | 62.5000 |      - |  904.7 KB |


## v5.0.1

|      Method |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------ |----------:|---------:|---------:|----------:|------:|--------:|--------:|------:|------:|----------:|
|      DryIoc |  93.89 us | 2.847 us | 8.167 us |  91.64 us |  1.00 |    0.00 | 12.8174 |     - |     - |  39.37 KB |
| DryIoc_MsDI | 110.50 us | 2.545 us | 7.262 us | 109.15 us |  1.19 |    0.13 | 16.2354 |     - |     - |  49.82 KB |


## v6 - ImTools v4

|      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc | 86.96 us | 1.477 us | 1.758 us |  1.00 |    0.00 |  6.2256 | 0.3662 |     - |  38.57 KB |
| DryIoc_MsDI | 99.10 us | 1.255 us | 1.112 us |  1.13 |    0.02 |  7.9346 | 0.6104 |     - |  48.91 KB |
|        MsDI | 93.91 us | 1.228 us | 1.026 us |  1.07 |    0.02 | 11.8408 | 4.2725 |     - |   72.6 KB |

## v6 - Optimizing the Factory Expression Cache and more!

|      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc | 84.44 us | 1.621 us | 1.516 us |  1.00 |    0.00 |  6.2256 | 0.3662 |     - |  38.54 KB |
| DryIoc_MsDI | 98.39 us | 1.011 us | 0.946 us |  1.17 |    0.02 |  7.9346 | 0.6104 |     - |  48.89 KB |
|        MsDI | 92.17 us | 0.421 us | 0.352 us |  1.09 |    0.02 | 11.8408 | 4.2725 |     - |   72.6 KB |

## v6 - SmallArrayPool (stashed)

|      Method |      Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |----------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc |  87.82 us | 1.493 us | 1.397 us |  1.00 |    0.00 |  5.9814 | 0.3662 |     - |  37.38 KB |
| DryIoc_MsDI | 100.88 us | 1.175 us | 1.099 us |  1.15 |    0.02 |  7.6904 | 0.4883 |     - |  47.71 KB |
|        MsDI |  91.39 us | 0.972 us | 0.811 us |  1.04 |    0.02 | 11.8408 | 4.2725 |     - |  72.58 KB |

## v6 - RequestStack optimized and ResolutionRoot request pool

|      Method |      Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |----------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc |  93.61 us | 1.870 us | 4.025 us |  1.00 |    0.00 | 10.8643 |      - |     - |  33.56 KB |
| DryIoc_MsDI | 108.99 us | 2.145 us | 4.972 us |  1.17 |    0.08 | 13.4277 |      - |     - |  41.25 KB |
|        MsDI | 106.98 us | 2.114 us | 3.353 us |  1.15 |    0.07 | 22.9492 | 0.4883 |     - |  70.04 KB |


## v6 - FactoryDelegate is replaced with Func avoiding the need for conversion

|      Method |      Mean |    Error |   StdDev |    Median | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |----------:|---------:|---------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc |  98.57 us | 3.044 us | 8.974 us |  95.87 us |  1.00 |    0.00 | 10.8643 |      - |     - |  33.56 KB |
| DryIoc_MsDI |  99.77 us | 0.970 us | 0.757 us |  99.65 us |  1.08 |    0.04 | 13.1836 |      - |     - |  40.75 KB |
|        MsDI | 101.72 us | 1.980 us | 3.519 us | 100.69 us |  1.08 |    0.07 | 22.9492 | 0.6104 |     - |  70.04 KB |

## v6 - No conversion to FactoryDelegate for cached constants results

|      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc | 85.50 us | 1.312 us | 1.227 us |  1.00 |    0.00 |  5.3711 | 0.4883 |     - |  33.25 KB |
| DryIoc_MsDI | 97.42 us | 0.646 us | 0.572 us |  1.14 |    0.01 |  6.4697 | 0.6104 |     - |   40.2 KB |
|        MsDI | 93.91 us | 1.406 us | 1.315 us |  1.10 |    0.02 | 11.8408 | 4.2725 |     - |  72.53 KB |

## v6 Storing the ServiceDetails as ServiceInfo in Request

|      Method |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|--------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc | 162.8 us | 7.15 us | 21.08 us | 155.5 us |  1.00 |    0.00 | 10.7422 |      - |     - |  33.25 KB |
| DryIoc_MsDI | 180.5 us | 7.23 us | 20.98 us | 174.1 us |  1.13 |    0.20 | 12.6953 |      - |     - |  39.34 KB |
|        MsDI | 232.2 us | 4.64 us | 10.18 us | 232.5 us |  1.41 |    0.20 | 22.9492 | 0.9766 |     - |  70.04 KB |

## v6 Directly recognizing and using InvokeFactoryDelegateExpression

|      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc | 85.37 us | 1.618 us | 1.731 us |  1.00 |    0.00 |  5.3711 | 0.4883 |     - |   33.2 KB |
| DryIoc_MsDI | 96.08 us | 1.806 us | 1.855 us |  1.13 |    0.04 |  6.3477 | 0.6104 |     - |  39.15 KB |
|        MsDI | 93.17 us | 0.802 us | 0.750 us |  1.09 |    0.02 | 11.8408 | 4.2725 |     - |  72.61 KB |

## v6 Removing convert for scoped expression

|      Method |      Mean |    Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |----------:|---------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc |  83.59 us | 1.614 us |  2.315 us |  1.00 |    0.00 |  5.2490 | 0.4883 |     - |  32.23 KB |
| DryIoc_MsDI | 165.19 us | 5.806 us | 16.657 us |  1.94 |    0.16 |       - |      - |     - |  38.52 KB |
|        MsDI |  96.35 us | 1.108 us |  0.982 us |  1.16 |    0.05 | 11.8408 | 4.2725 |     - |  72.62 KB |

## v6 Removing the FactoryDelegateExpression wrapper

|      Method |     Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |---------:|---------:|---------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc | 79.95 us | 1.016 us | 0.950 us |  1.00 |    0.00 |  5.1270 | 0.4883 |     - |  31.55 KB |
| DryIoc_MsDI | 89.66 us | 0.951 us | 0.843 us |  1.12 |    0.02 |  5.9814 | 0.4883 |     - |  37.31 KB |
|        MsDI | 91.18 us | 0.974 us | 0.911 us |  1.14 |    0.02 | 11.8408 | 4.2725 |     - |  72.61 KB |

|      Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------ |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|      DryIoc |  84.49 us |  1.688 us |  1.579 us |  1.00 |    0.00 |  5.1270 | 0.4883 |     - |  31.55 KB |
| DryIoc_MsDI | 263.42 us | 24.769 us | 72.644 us |  3.04 |    0.82 |       - |      - |     - |  37.84 KB |
|        MsDI |  92.93 us |  1.298 us |  1.084 us |  1.10 |    0.03 | 11.8408 | 4.2725 |     - |  72.59 KB |

## v6 after update to MS.Ext.DI to v7.0.0

|      Method |      Mean |    Error |    StdDev |    Median | Ratio | RatioSD |    Gen0 |   Gen1 | Allocated | Alloc Ratio |
|------------ |----------:|---------:|----------:|----------:|------:|--------:|--------:|-------:|----------:|------------:|
|      DryIoc |  95.50 us | 1.783 us |  3.477 us |  94.44 us |  1.00 |    0.00 | 10.2539 |      - |  31.64 KB |        1.00 |
| DryIoc_MsDI | 122.30 us | 5.922 us | 17.462 us | 115.89 us |  1.17 |    0.11 | 12.0850 |      - |  37.23 KB |        1.18 |
|        MsDI | 119.71 us | 2.642 us |  7.580 us | 116.47 us |  1.25 |    0.09 | 23.4375 | 0.3662 |  71.44 KB |        2.26 |

## v6 after update to .NET 7

BenchmarkDotNet=v0.13.3, OS=Windows 10 (10.0.19042.928/20H2/October2020Update)
Intel Core i5-8350U CPU 1.70GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

|      Method |     Mean |   Error |   StdDev |   Median | Ratio | RatioSD |    Gen0 |   Gen1 | Allocated | Alloc Ratio |
|------------ |---------:|--------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
|      DryIoc | 138.5 us | 9.07 us | 26.75 us | 129.7 us |  1.00 |    0.00 | 10.2539 |      - |  31.74 KB |        1.00 |
| DryIoc_MsDI | 150.0 us | 6.67 us | 18.93 us | 147.7 us |  1.12 |    0.24 | 11.9629 |      - |  37.22 KB |        1.17 |
|        MsDI | 205.2 us | 4.08 us | 11.70 us | 202.9 us |  1.53 |    0.27 | 23.1934 | 0.4883 |  71.36 KB |        2.25 |

## v6 without expression caching

|      Method |     Mean |   Error |   StdDev | Ratio | RatioSD |    Gen0 |   Gen1 | Allocated | Alloc Ratio |
|------------ |---------:|--------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
|      DryIoc | 164.4 us | 6.39 us | 17.91 us |  1.00 |    0.00 | 12.2070 |      - |  37.63 KB |        1.00 |
| DryIoc_MsDI | 159.9 us | 4.89 us | 13.38 us |  0.98 |    0.13 | 13.9160 |      - |  43.05 KB |        1.14 |
|        MsDI | 213.8 us | 4.37 us | 12.68 us |  1.31 |    0.16 | 23.1934 | 0.4883 |  71.34 KB |        1.90 |


## v6.0.0 + .NET 9.0




*/

            [Benchmark(Baseline = true)]
            public object DryIoc() => Measure(PrepareDryIoc());

            [Benchmark]
            public object DryIoc_MsDI() => Measure(PrepareDryIocMsDi());

            [Benchmark]
            public object MsDI() => Measure(PrepareMsDi());

            // note: no need for this because it is the same as DryIoc benchmark
            // [Benchmark]
            public object DryIoc_InterpretationOnly() => Measure(PrepareDryIocInterpretationOnly());

            [Benchmark]
            public object Autofac() => Measure(PrepareAutofac());

            [Benchmark]
            public object Autofac_MsDI() => Measure(PrepareAutofacMsDi());

            [Benchmark]
            public object Lamar_MsDI() => Measure(PrepareLamarMsDi());

            [Benchmark]
            public object Grace() => Measure(PrepareGrace());

            [Benchmark]
            public object Grace_MsDI() => Measure(PrepareGraceMsDi());
        }

        [MemoryDiagnoser()]
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

            ## Optimized interpreting of the ScopeOrSingleton and registered Func<IResolverContext, object> - now DI.MS.DI is faster than DI :)

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

            ## DryIoc v4.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT


|                    Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|                      MsDI |  3.352 us | 0.0195 us | 0.0163 us |  1.00 |    0.00 |  0.9460 | 0.0153 |     - |   4.35 KB |
|                    DryIoc |  1.645 us | 0.0078 us | 0.0069 us |  0.49 |    0.00 |  0.6180 | 0.0076 |     - |   2.84 KB |
|        DryIoc_MsDIAdapter |  2.098 us | 0.0171 us | 0.0152 us |  0.63 |    0.01 |  0.6218 | 0.0076 |     - |   2.87 KB |
| DryIoc_InterpretationOnly | 13.798 us | 0.0718 us | 0.0671 us |  4.11 |    0.03 |  1.4496 | 0.0153 |     - |    6.7 KB |
|                     Grace |  1.736 us | 0.0188 us | 0.0167 us |  0.52 |    0.01 |  0.6886 | 0.0095 |     - |   3.17 KB |
|         Grace_MsDIAdapter |  2.228 us | 0.0279 us | 0.0261 us |  0.67 |    0.01 |  0.7401 | 0.0076 |     - |   3.41 KB |
|                   Autofac | 37.386 us | 0.2686 us | 0.2513 us | 11.13 |    0.04 | 10.5591 | 0.6714 |     - |  48.66 KB |
|       Autofac_MsDIAdapter | 44.416 us | 0.1591 us | 0.1488 us | 13.25 |    0.06 | 12.5732 | 0.7324 |     - |  57.78 KB |

|            Method |      Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------ |----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|              MsDI |  3.854 us | 0.0309 us | 0.0258 us |  1.00 |    0.00 | 0.9460 |     - |     - |   4.37 KB |
|            DryIoc |  1.699 us | 0.0030 us | 0.0028 us |  0.44 |    0.00 | 0.6409 |     - |     - |   2.96 KB |
| DryIoc_WithoutFEC | 16.344 us | 0.1252 us | 0.1045 us |  4.24 |    0.05 | 1.0681 |     - |     - |   4.93 KB |

                
|                             Method |      Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------------- |----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                               MsDI |  3.852 us | 0.0255 us | 0.0239 us |  1.00 |    0.00 | 0.9460 |     - |     - |   4.37 KB |
|            DryIoc_WebRequestScoped |  2.128 us | 0.0113 us | 0.0106 us |  0.55 |    0.00 | 0.6866 |     - |     - |   3.17 KB |
| DryIoc_WebRequestScoped_WithoutFEC | 17.713 us | 0.1377 us | 0.1288 us |  4.60 |    0.04 | 1.0986 |     - |     - |   5.14 KB |

            
### DryIoc 4.1.3 (.MsDI 3.0.3), MsDI 3.1.3, Grace 7.1.0 (.MsDI 7.0.1), Autofac 5.1.2 (.MsDI 6.0.0), Lamar 4.2.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.200
  [Host]     : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT
  DefaultJob : .NET Core 3.1.2 (CoreCLR 4.700.20.6602, CoreFX 4.700.20.6702), X64 RyuJIT

|              Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|                MsDI |  3.551 us | 0.0142 us | 0.0126 us |  1.00 |    0.00 |  0.9460 | 0.0153 |     - |   4.35 KB |
|              DryIoc |  1.647 us | 0.0050 us | 0.0042 us |  0.46 |    0.00 |  0.6428 |      - |     - |   2.96 KB |
|  DryIoc_MsDIAdapter |  2.400 us | 0.0172 us | 0.0161 us |  0.68 |    0.01 |  0.6485 | 0.0076 |     - |   2.98 KB |
|               Grace |  1.699 us | 0.0047 us | 0.0037 us |  0.48 |    0.00 |  0.6886 |      - |     - |   3.17 KB |
|   Grace_MsDIAdapter |  2.322 us | 0.0163 us | 0.0136 us |  0.65 |    0.00 |  0.7401 | 0.0076 |     - |   3.41 KB |
|          Lamar_MsDI |  7.281 us | 0.0586 us | 0.0520 us |  2.05 |    0.02 |  0.9308 | 0.4654 |     - |    5.7 KB |
|             Autofac | 50.146 us | 0.5242 us | 0.4377 us | 14.13 |    0.14 | 10.4980 |      - |     - |  48.54 KB |
| Autofac_MsDIAdapter | 62.118 us | 0.1595 us | 0.1492 us | 17.50 |    0.07 | 12.9395 | 0.8545 |     - |  59.89 KB |


### DryIoc v4.2

|             Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|               MsDI | 3.890 us | 0.0158 us | 0.0140 us |  1.00 |    0.00 | 0.9460 |     - |     - |   4.37 KB |
|             DryIoc | 1.701 us | 0.0014 us | 0.0013 us |  0.44 |    0.00 | 0.6409 |     - |     - |   2.96 KB |
| DryIoc_MsDIAdapter | 2.629 us | 0.0523 us | 0.0603 us |  0.68 |    0.02 | 0.6447 |     - |     - |   2.98 KB |


### DryIoc v4.2.2 - replacing the AddOrKeep with AddOrKeepEntry and getting rid off GetEntryOrDefault afterwards and getting rid of created value check


|             Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|---------:|------:|--------:|-------:|-------:|------:|----------:|
|               MsDI | 3.944 us | 0.0817 us | 0.2108 us | 3.867 us |  1.00 |    0.00 | 0.9460 | 0.0153 |     - |   4.35 KB |
|             DryIoc | 1.663 us | 0.0354 us | 0.0408 us | 1.654 us |  0.42 |    0.03 | 0.6428 | 0.0076 |     - |   2.96 KB |
| DryIoc_MsDIAdapter | 2.521 us | 0.0488 us | 0.0542 us | 2.498 us |  0.63 |    0.04 | 0.6485 | 0.0076 |     - |   2.98 KB |

### DryIoc v4.4.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


|             Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|               MsDI | 4.232 us | 0.1002 us | 0.0938 us |  1.00 |    0.00 | 1.0605 |     - |     - |   4.35 KB |
|             DryIoc | 1.868 us | 0.0226 us | 0.0200 us |  0.44 |    0.01 | 0.7248 |     - |     - |   2.96 KB |
| DryIoc_MsDIAdapter | 2.651 us | 0.0404 us | 0.0338 us |  0.63 |    0.02 | 0.7286 |     - |     - |   2.98 KB |

### DryIoc 4.5.0 (.MsDI 5.0.0), MsDI 3.1.8, Grace 7.1.1 (.MsDI 7.0.1), Autofac 6.0.0 (.MsDI 7.0.2), Lamar 4.3.1

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
Intel Core i7-8565U CPU 1.80GHz (Whiskey Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.402
  [Host]     : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT
  DefaultJob : .NET Core 3.1.8 (CoreCLR 4.700.20.41105, CoreFX 4.700.20.41903), X64 RyuJIT


|       Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|         MsDI |  4.530 us | 0.0437 us | 0.0388 us |  1.00 |    0.00 |  1.0605 |      - |     - |   4.35 KB |
|       DryIoc |  1.653 us | 0.0118 us | 0.0104 us |  0.37 |    0.00 |  0.7229 |      - |     - |   2.96 KB |
|  DryIoc_MsDI |  2.629 us | 0.0524 us | 0.0644 us |  0.58 |    0.01 |  0.7286 |      - |     - |   2.98 KB |
|        Grace |  2.229 us | 0.0432 us | 0.0546 us |  0.49 |    0.02 |  0.7744 |      - |     - |   3.17 KB |
|   Grace_MsDI |  3.007 us | 0.0586 us | 0.0675 us |  0.67 |    0.02 |  0.8354 |      - |     - |   3.41 KB |
|   Lamar_MsDI |  9.270 us | 0.0788 us | 0.0737 us |  2.05 |    0.03 |  0.9308 | 0.4578 |     - |    5.7 KB |
|      Autofac | 60.151 us | 0.5309 us | 0.4707 us | 13.28 |    0.15 | 11.4746 |      - |     - |  47.28 KB |
| Autofac_MsDI | 74.027 us | 0.5597 us | 0.4370 us | 16.36 |    0.21 | 16.1133 |      - |     - |  66.09 KB |
		
### DryIoc v5

.NET Core SDK=3.1.202
  [Host]     : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT
  DefaultJob : .NET Core 3.1.4 (CoreCLR 4.700.20.20201, CoreFX 4.700.20.22101), X64 RyuJIT

|             Method |     Mean |     Error |    StdDev | Ratio |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|------:|-------:|-------:|------:|----------:|
|               MsDI | 3.432 us | 0.0298 us | 0.0249 us |  1.00 | 0.9460 | 0.0153 |     - |   4.35 KB |
|             DryIoc | 1.611 us | 0.0077 us | 0.0068 us |  0.47 | 0.6428 | 0.0076 |     - |   2.96 KB |
| DryIoc_MsDIAdapter | 2.168 us | 0.0130 us | 0.0109 us |  0.63 | 0.6485 | 0.0076 |     - |   2.98 KB |
|              Grace | 1.665 us | 0.0081 us | 0.0076 us |  0.49 | 0.6886 |      - |     - |   3.17 KB |
|  Grace_MsDIAdapter | 2.258 us | 0.0108 us | 0.0096 us |  0.66 | 0.7401 | 0.0076 |     - |   3.41 KB |

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.200
  [Host]     : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT
  DefaultJob : .NET Core 3.1.12 (CoreCLR 4.700.21.6504, CoreFX 4.700.21.6905), X64 RyuJIT

### DryIoc v5 + ImTools v3

|              Method |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------- |----------:|----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|                MsDI |  3.675 us | 0.0730 us | 0.1070 us |  3.699 us |  1.00 |    0.00 |  0.7095 | 0.0114 |     - |   4.35 KB |
|              DryIoc |  1.359 us | 0.0147 us | 0.0138 us |  1.354 us |  0.37 |    0.01 |  0.4768 | 0.0057 |     - |   2.93 KB |
|  DryIoc_MsDIAdapter |  2.051 us | 0.0408 us | 0.0437 us |  2.048 us |  0.56 |    0.02 |  0.4807 | 0.0038 |     - |   2.95 KB |
|               Grace |  1.751 us | 0.0339 us | 0.0377 us |  1.748 us |  0.47 |    0.02 |  0.5150 | 0.0076 |     - |   3.17 KB |
|   Grace_MsDIAdapter |  2.395 us | 0.0578 us | 0.0594 us |  2.402 us |  0.65 |    0.03 |  0.5569 |      - |     - |   3.41 KB |
|   Lamar_MsDIAdapter |  6.802 us | 0.0675 us | 0.0563 us |  6.800 us |  1.85 |    0.06 |  1.5335 | 0.7629 |     - |   9.44 KB |
|             Autofac | 50.699 us | 0.9995 us | 2.3947 us | 49.903 us | 14.13 |    0.81 |  7.7515 | 0.6104 |     - |  47.84 KB |
| Autofac_MsDIAdapter | 60.233 us | 1.1734 us | 1.2050 us | 60.089 us | 16.38 |    0.46 | 10.7422 | 0.8545 |     - |  66.26 KB |

|             Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
|               MsDI | 3.466 us | 0.0675 us | 0.0878 us |  1.00 |    0.00 | 0.7095 | 0.0114 |     - |   4.35 KB |
|             DryIoc | 1.257 us | 0.0134 us | 0.0112 us |  0.36 |    0.01 | 0.4711 | 0.0057 |     - |   2.89 KB |
| DryIoc_MsDIAdapter | 2.044 us | 0.0344 us | 0.0322 us |  0.59 |    0.02 | 0.4768 | 0.0038 |     - |   2.94 KB |

|             Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
|             DryIoc | 1.983 us | 0.0383 us | 0.0456 us |  1.00 |    0.00 | 0.4730 | 0.0038 |     - |   2.91 KB |
| DryIoc_MsDIAdapter | 3.294 us | 0.0651 us | 0.0847 us |  1.66 |    0.06 | 0.4807 | 0.0076 |     - |   2.96 KB |
|               MsDI | 4.903 us | 0.0974 us | 0.1042 us |  2.47 |    0.07 | 0.7095 | 0.0076 |     - |   4.35 KB |
|              Grace | 2.381 us | 0.0410 us | 0.0770 us |  1.21 |    0.06 | 0.5150 | 0.0076 |     - |   3.17 KB |

## DryIoc v5

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19043
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT

|       Method |      Mean |     Error |    StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------------- |----------:|----------:|----------:|------:|--------:|--------:|-------:|------:|----------:|
|       DryIoc |  1.535 us | 0.0143 us | 0.0111 us |  1.00 |    0.00 |  0.4749 | 0.0076 |     - |   2.91 KB |
|  DryIoc_MsDI |  2.405 us | 0.0277 us | 0.0246 us |  1.57 |    0.02 |  0.4807 | 0.0076 |     - |   2.96 KB |
|         MsDI |  3.655 us | 0.0726 us | 0.0807 us |  2.40 |    0.05 |  0.7629 | 0.0114 |     - |   4.68 KB |
|        Grace |  1.807 us | 0.0241 us | 0.0213 us |  1.18 |    0.02 |  0.5169 | 0.0076 |     - |   3.17 KB |
|   Grace_MsDI |  2.576 us | 0.0421 us | 0.0394 us |  1.68 |    0.03 |  0.5569 | 0.0076 |     - |   3.41 KB |
|   Lamar_MsDI |  6.673 us | 0.0876 us | 0.0732 us |  4.35 |    0.06 |  0.9995 | 0.4959 |     - |   6.16 KB |
|      Autofac | 47.040 us | 0.7367 us | 0.6531 us | 30.65 |    0.48 |  7.7515 | 0.6104 |     - |  47.73 KB |
| Autofac_MsDI | 59.566 us | 0.8734 us | 0.7742 us | 38.76 |    0.61 | 11.3525 | 0.9155 |     - |  69.59 KB |

## DryIoc v6

BenchmarkDotNet=v0.13.3, OS=Windows 10 (10.0.19042.928/20H2/October2020Update)
Intel Core i5-8350U CPU 1.70GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

|      Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------ |---------:|----------:|----------:|---------:|------:|--------:|-------:|----------:|------------:|
|      DryIoc | 1.804 us | 0.0361 us | 0.0768 us | 1.787 us |  1.00 |    0.00 | 0.9499 |   2.91 KB |        1.00 |
| DryIoc_MsDI | 2.698 us | 0.1138 us | 0.3266 us | 2.789 us |  1.50 |    0.16 | 0.9575 |   2.94 KB |        1.01 |
|        MsDI | 3.911 us | 0.0782 us | 0.1991 us | 3.853 us |  2.18 |    0.18 | 1.5259 |   4.68 KB |        1.61 |


## DryIoc v6.0.0 + .NET 9.0 + Degradation

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4391/23H2/2023Update/SunValley3)
Intel Core i9-8950HK CPU 2.90GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

| Method       | Mean      | Error     | StdDev    | Median    | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
|------------- |----------:|----------:|----------:|----------:|------:|--------:|--------:|-------:|----------:|------------:|
| DryIoc       |  6.123 us | 0.1225 us | 0.2740 us |  6.249 us |  1.00 |    0.06 |  0.6866 | 0.0076 |   4.22 KB |        1.00 |
| DryIoc_MsDI  |  6.479 us | 0.1290 us | 0.3237 us |  6.637 us |  1.06 |    0.07 |  0.6790 | 0.0076 |   4.19 KB |        0.99 |
| MsDI         |  3.016 us | 0.0594 us | 0.0751 us |  3.039 us |  0.49 |    0.03 |  0.7896 | 0.0114 |   4.85 KB |        1.15 |
| Grace        |  1.460 us | 0.0291 us | 0.0797 us |  1.483 us |  0.24 |    0.02 |  0.5169 | 0.0038 |   3.17 KB |        0.75 |
| Grace_MsDI   |  1.844 us | 0.0420 us | 0.1232 us |  1.823 us |  0.30 |    0.02 |  0.5493 | 0.0038 |   3.37 KB |        0.80 |
| Lamar_MsDI   |  5.550 us | 0.1110 us | 0.2243 us |  5.634 us |  0.91 |    0.05 |  0.9766 | 0.9689 |   5.99 KB |        1.42 |
| Autofac      | 37.527 us | 0.7087 us | 1.1241 us | 37.528 us |  6.14 |    0.33 |  7.2021 | 0.4272 |  44.33 KB |       10.51 |
| Autofac_MsDI | 43.211 us | 0.3978 us | 0.3106 us | 43.284 us |  7.07 |    0.32 | 10.1318 | 0.6714 |  62.21 KB |       14.75 |

*/
#pragma warning disable CS0169
            private IServiceProvider _msDi;
            private IContainer _dryIoc;
            private IContainer _dryIocWithoutFEC;
            private IContainer _dryIocWebRequestScoped;
            private IContainer _dryIocWebRequestScopedWithoutFEC;
            private IContainer _dryIocInterpretationOnly;
            private IServiceProvider _dryIocMsDi;
            private DependencyInjectionContainer _grace;
            private IServiceProvider _graceMsDi;
            private Autofac.IContainer _autofac;
            private IServiceProvider _autofacMsDi;
            private IServiceProvider _lamarMsDi;

            [GlobalSetup]
            public void WarmUp()
            {
                Measure(_msDi = PrepareMsDi());
                Measure(_dryIoc = PrepareDryIoc());
                Measure_WebRequestScoped(_dryIocWebRequestScoped = PrepareDryIoc_WebRequestScoped());
                Measure(_dryIocInterpretationOnly = PrepareDryIocInterpretationOnly());
                Measure(_dryIocMsDi = PrepareDryIocMsDi());
                Measure(_grace = PrepareGrace());
                Measure(_graceMsDi = PrepareGraceMsDi());
                Measure(_autofac = PrepareAutofac());
                Measure(_autofacMsDi = PrepareAutofacMsDi());
                Measure(_lamarMsDi = PrepareLamarMsDi());
            }

            [Benchmark(Baseline = true)]
            public object DryIoc() => Measure(_dryIoc);

            [Benchmark]
            public object DryIoc_MsDI() => Measure(_dryIocMsDi);

            [Benchmark]
            public object MsDI() => Measure(_msDi);

            // [Benchmark]
            public object DryIoc_WebRequestScoped() => Measure_WebRequestScoped(_dryIocWebRequestScoped);

            //[Benchmark]
            public object DryIoc_InterpretationOnly() => Measure(_dryIocInterpretationOnly);

            [Benchmark]
            public object Grace() => Measure(_grace);

            [Benchmark]
            public object Grace_MsDI() => Measure(_graceMsDi);

            [Benchmark]
            public object Lamar_MsDI() => Measure(_lamarMsDi);

            [Benchmark]
            public object Autofac() => Measure(_autofac);

            [Benchmark]
            public object Autofac_MsDI() => Measure(_autofacMsDi);
        }
    }
}
