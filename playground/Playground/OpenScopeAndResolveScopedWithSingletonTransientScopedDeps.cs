using System;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc;
using Grace.DependencyInjection;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using Container = DryIoc.Container;
using IContainer = Autofac.IContainer;

namespace PerformanceTests
{
    public class OpenScopeAndResolveScopedWithSingletonTransientScopedDeps
    {
        public static DryIoc.IContainer PrepareDryIoc()
        {
            var container = new Container();

            //container.Register<Parameter1>(Reuse.Transient);
            container.Register<Parameter2>(Reuse.Singleton);
            container.Register<Parameter3>(Reuse.Scoped);

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

            //services.AddTransient<Parameter1>();
            services.AddSingleton<Parameter2>();
            services.AddScoped<Parameter3>();

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

            //builder.RegisterType<Parameter1>().AsSelf().InstancePerDependency();
            builder.RegisterType<Parameter2>().AsSelf().SingleInstance();
            builder.RegisterType<Parameter3>().AsSelf().InstancePerLifetimeScope();

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
                //c.Export<Parameter1>();
                c.Export<Parameter2>().Lifestyle.Singleton();
                c.Export<Parameter3>().Lifestyle.SingletonPerScope();

                c.Export<ScopedBlah>().Lifestyle.SingletonPerScope();
            });

            return container;
        }

        public static object Measure(DependencyInjectionContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
                return scope.Locate<ScopedBlah>();
        }

        public static LightInject.ServiceContainer PrepareLightInject()
        {
            var c = new LightInject.ServiceContainer();

            //c.Register<Parameter1>();
            c.Register<Parameter2>(new PerContainerLifetime());
            c.Register<Parameter3>(new PerScopeLifetime());

            c.Register<ScopedBlah>(new PerScopeLifetime());

            return c;
        }

        public static object Measure(LightInject.ServiceContainer container)
        {
            using (var scope = container.BeginScope())
                return scope.GetInstance<ScopedBlah>();
        }

        public static SimpleInjector.Container PrepareSimpleInjector()
        {
            var c = new SimpleInjector.Container();
            c.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            //c.Register<Parameter1>(Lifestyle.Singleton);
            c.Register<Parameter2>(Lifestyle.Singleton);
            c.Register<Parameter3>(Lifestyle.Scoped);

            c.Register<ScopedBlah>(Lifestyle.Scoped);

            return c;
        }

        public static object Measure(SimpleInjector.Container container)
        {
            using (AsyncScopedLifestyle.BeginScope(container))
                return container.GetInstance<ScopedBlah>();
        }

        //internal class Parameter1 { }
        internal class Parameter2 { }
        internal class Parameter3 { }

        internal class ScopedBlah
        {
            //public Parameter1 Parameter1 { get; }
            public Parameter2 Parameter2 { get; }
            public Parameter3 Parameter3 { get; }

            public ScopedBlah(
                //Parameter1 parameter1, 
                Parameter2 parameter2, 
                Parameter3 parameter3)
            {
                //Parameter1 = parameter1;
                Parameter2 = parameter2;
                Parameter3 = parameter3;
            }
        }

        /*
        ## 28.11.2018
        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156248 Hz, Resolution=463.7685 ns, Timer=TSC
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


                             Method |       Mean |     Error |     StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
----------------------------------- |-----------:|----------:|-----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkAutofac | 1,970.3 ns | 11.519 ns | 10.7747 ns |  7.17 |    0.06 |      0.6676 |           - |           - |              3152 B |
                        BmarkDryIoc |   207.0 ns |  1.062 ns |  0.9931 ns |  0.75 |    0.01 |      0.0966 |           - |           - |               456 B |
 BmarkMicrosoftSDependencyInjection |   274.9 ns |  2.064 ns |  1.9308 ns |  1.00 |    0.00 |      0.0758 |           - |           - |               360 B |
                         BmarkGrace |   264.8 ns |  1.653 ns |  1.5462 ns |  0.96 |    0.01 |      0.1216 |           - |           - |               576 B |
                   BmarkLightInject |   998.6 ns |  4.589 ns |  4.0676 ns |  3.64 |    0.02 |      0.2422 |           - |           - |              1144 B |

        */
        [MemoryDiagnoser]
        public class FirstTimeOpenScopeResolve
        {
            private readonly IContainer _autofac = PrepareAutofac();
            private readonly DryIoc.IContainer _dryioc = PrepareDryIoc();
            private readonly IServiceProvider _msDi = PrepareMsDi();
            private readonly DependencyInjectionContainer _grace = PrepareGrace();
            private readonly ServiceContainer _lightInject = PrepareLightInject();
            private readonly SimpleInjector.Container _simpleInjector = PrepareSimpleInjector();

            [Benchmark]
            public object BmarkAutofac() => Measure(_autofac);

            [Benchmark]
            public object BmarkDryIoc() => Measure(_dryioc);

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftSDependencyInjection() => Measure(_msDi);

            [Benchmark]
            public object BmarkGrace() => Measure(_grace);

            [Benchmark]
            public object BmarkLightInject() => Measure(_lightInject);

            [Benchmark]
            public object BmarkSimpleInjector() => Measure(_simpleInjector);
        }

        /*
        ## 28.11.2018
        BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156248 Hz, Resolution=463.7685 ns, Timer=TSC
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


                             Method |       Mean |     Error |    StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
----------------------------------- |-----------:|----------:|----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkAutofac |  35.672 us | 0.3983 us | 0.3326 us |   7.32 |    0.08 |      6.4697 |           - |           - |            29.83 KB |
                        BmarkDryIoc | 529.302 us | 2.3636 us | 2.0953 us | 108.60 |    0.69 |      1.9531 |      0.9766 |           - |            12.61 KB |
 BmarkMicrosoftSDependencyInjection |   4.873 us | 0.0267 us | 0.0250 us |   1.00 |    0.00 |      1.0529 |           - |           - |             4.87 KB |
                         BmarkGrace | 783.044 us | 3.8283 us | 3.5810 us | 160.68 |    1.18 |      8.7891 |      3.9063 |           - |            42.44 KB |
                   BmarkLightInject | 666.277 us | 6.2531 us | 5.8492 us | 136.72 |    1.38 |      8.7891 |      3.9063 |           - |            43.12 KB |

        */
        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerAndRegister_FirstTimeOpenScopeResolve
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

            [Benchmark]
            public object BmarkSimpleInjector() => Measure(PrepareSimpleInjector());
        }
    }
}
