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

            container.Register<Parameter1>(Reuse.Transient);
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

            services.AddTransient<Parameter1>();
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

            builder.RegisterType<Parameter1>().AsSelf().InstancePerDependency();
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
                c.Export<Parameter1>();
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

            c.Register<Parameter1>();
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

        public static Lamar.Container PrepareLamar()
        {
            return new Lamar.Container(c =>
            {
                c.AddTransient<Parameter1>();
                c.AddSingleton<Parameter2>();
                c.AddScoped<Parameter3>();

                c.AddScoped<ScopedBlah>();
            });
        }

        public static object Measure(Lamar.Container container)
        {
            using (var scope = container.CreateScope())
                return scope.ServiceProvider.GetRequiredService<ScopedBlah>();
        }


        public static SimpleInjector.Container PrepareSimpleInjector()
        {
            var c = new SimpleInjector.Container();
            c.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            c.Register<Parameter1>(Lifestyle.Singleton);
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

        public class Parameter1 { }
        public class Parameter2 { }
        public class Parameter3 { }

        public class ScopedBlah
        {
            public Parameter1 Parameter1 { get; }
            public Parameter2 Parameter2 { get; }
            public Parameter3 Parameter3 { get; }

            public ScopedBlah(
                Parameter1 parameter1,
                Parameter2 parameter2, 
                Parameter3 parameter3)
            {
                Parameter1 = parameter1;
                Parameter2 = parameter2;
                Parameter3 = parameter3;
            }
        }

        public class R
        {
            public Single1    Single1 { get; }
            public Single2    Single2 { get; }

            public Scoped1    Scoped1 { get; }
            public Scoped2    Scoped2 { get; }

            public Trans1     Trans1 { get; }
            public Trans2     Trans2 { get; }

            public ScopedFac1 ScopedFac1 { get; }
            public ScopedFac2 ScopedFac2 { get; }

            public SingleObj1 SingleObj1 { get; }
            public SingleObj2 SingleObj2 { get; }

            public R(
                Single1 single1,
                Single2 single2,
                Scoped1 scoped1,
                Scoped2 scoped2,
                Trans1  trans1,
                Trans2  trans2,
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
                Trans1  = trans1;
                Trans2  = trans2;
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

# 25.01.2019
                             Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
----------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                        BmarkDryIoc |   172.6 ns |  1.664 ns |  1.556 ns |  0.61 |    0.01 |      0.0896 |           - |           - |               424 B |
 BmarkMicrosoftSDependencyInjection |   285.0 ns |  1.564 ns |  1.463 ns |  1.00 |    0.00 |      0.0758 |           - |           - |               360 B |
                         BmarkGrace |   301.3 ns |  4.890 ns |  4.574 ns |  1.06 |    0.02 |      0.1216 |           - |           - |               576 B |
                   BmarkLightInject | 1,079.8 ns | 14.331 ns | 13.406 ns |  3.79 |    0.05 |      0.2422 |           - |           - |              1144 B |
                       BmarkAutofac | 2,017.9 ns |  9.589 ns |  8.501 ns |  7.08 |    0.05 |      0.6676 |           - |           - |              3152 B |

        */
        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class FirstTimeOpenScopeResolve
        {
            private readonly IContainer _autofac = PrepareAutofac();
            private readonly DryIoc.IContainer _dryioc = PrepareDryIoc();
            private readonly IServiceProvider _msDi = PrepareMsDi();
            private readonly DependencyInjectionContainer _grace = PrepareGrace();
            private readonly ServiceContainer _lightInject = PrepareLightInject();
            private readonly Lamar.Container _lamar = PrepareLamar();
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

            //[Benchmark]
            public object BmarkLamar() => Measure(_lamar);

            //[Benchmark]
            public object BmarkSimpleInjector() => Measure(_simpleInjector);
        }

        /*
## 28.11.2018: Starting point - no scoped dependency interpretation for DryIoc yet

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

## 30.11.2018: First DryIoc version with interpreted scoped dependency

                            Method |       Mean |      Error |      StdDev |     Median |          P95 |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|-----------:|------------:|-----------:|-------------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |   4.797 us |  0.1125 us |   0.2904 us |   4.724 us |     5.754 us |   1.00 |    0.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc |   5.547 us |  0.0401 us |   0.0313 us |   5.535 us |     5.594 us |   1.17 |    0.01 |      1.6251 |           - |           - |             7.49 KB |
                      BmarkAutofac |  36.195 us |  0.1435 us |   0.1198 us |  36.232 us |    36.332 us |   7.64 |    0.03 |      6.4697 |           - |           - |            29.83 KB |
                        BmarkGrace | 776.478 us |  5.3626 us |   4.4780 us | 774.993 us |   783.422 us | 163.89 |    1.13 |      8.7891 |      3.9063 |           - |            42.44 KB |
                  BmarkLightInject | 799.472 us | 79.1998 us | 231.0294 us | 658.761 us | 1,299.696 us | 169.62 |   51.94 |      8.7891 |      3.9063 |           - |            43.12 KB |


## 09.12.2018: Shaved some bites from registration phase

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.689 us | 0.0256 us | 0.0239 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 5.519 us | 0.0571 us | 0.0534 us |  1.18 |      1.5488 |           - |           - |             7.17 KB |


NO DIFFERENCE FROM Asp.NET / Core 2.2

## 11.12.2018: Shaved almost a 1kb by optimizing Request allocations

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.876 us | 0.0346 us | 0.0307 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 5.524 us | 0.0505 us | 0.0447 us |  1.13 |      1.3733 |           - |           - |             6.33 KB |

## 13.12.2018: Never know where you can win until you measure!

                            Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc | 4.920 us | 0.0229 us | 0.0191 us |  0.99 |    0.06 |      1.1444 |           - |           - |              5.3 KB |
 BmarkMicrosoftDependencyInjection | 4.982 us | 0.3916 us | 0.3472 us |  1.00 |    0.00 |      1.0529 |           - |           - |             4.87 KB |

## 16.12.2018: Removing more closure - now in `Scope.GetOrAdd` method family

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.728 us | 0.0065 us | 0.0058 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 4.928 us | 0.0246 us | 0.0230 us |  1.04 |      1.0834 |           - |           - |                5 KB |

## 16.12.2018: Removing internal Data class from ImHashMap

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.742 us | 0.0101 us | 0.0095 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 4.786 us | 0.0340 us | 0.0318 us |  1.01 |      1.0605 |           - |           - |             4.91 KB |


## 25.12.2018: Split conflicts from ImHashMap - Win in memory over MS.DI

                            Method |       Mean |     Error |    StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |-----------:|----------:|----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |   4.697 us | 0.0162 us | 0.0151 us |   1.00 |    0.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc |   4.901 us | 0.0264 us | 0.0234 us |   1.04 |    0.01 |      1.0452 |           - |           - |             4.84 KB |
                      BmarkAutofac |  35.076 us | 0.4491 us | 0.3981 us |   7.47 |    0.08 |      6.4697 |           - |           - |            29.83 KB |
                  BmarkLightInject | 649.004 us | 5.4030 us | 5.0539 us | 138.18 |    1.04 |      8.7891 |      3.9063 |           - |            43.12 KB |
                        BmarkGrace | 768.939 us | 3.1469 us | 2.9437 us | 163.71 |    0.66 |      8.7891 |      3.9063 |           - |            42.44 KB |

## 25.12.2018: Split Branch from Node from ImHashMap

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.640 us | 0.0168 us | 0.0140 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 4.872 us | 0.0303 us | 0.0283 us |  1.05 |      1.0300 |           - |           - |             4.76 KB |

## 01.01.2018: Optimized Lookup speed in ImMap

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.599 us | 0.0165 us | 0.0147 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 4.773 us | 0.0284 us | 0.0252 us |  1.04 |      1.0300 |           - |           - |             4.76 KB |

## 11.01.2019: Optimized ImHashMap without virtual methods

                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc | 4.679 us | 0.0465 us | 0.0435 us |  0.99 |      1.0300 |           - |           - |             4.76 KB |
 BmarkMicrosoftDependencyInjection | 4.736 us | 0.0236 us | 0.0221 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |

## 25.01.2019: _data is back
                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.677 us | 0.0275 us | 0.0257 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 4.863 us | 0.0370 us | 0.0328 us |  1.04 |      1.0834 |           - |           - |                5 KB |

## 28.01.2019: After improvements in ImMap
                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
                       BmarkDryIoc | 4.703 us | 0.0225 us | 0.0188 us |  0.99 |      1.0681 |           - |           - |             4.95 KB |
 BmarkMicrosoftDependencyInjection | 4.750 us | 0.0082 us | 0.0076 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |

## Final cut
                            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection | 4.671 us | 0.0223 us | 0.0208 us |  1.00 |      1.0529 |           - |           - |             4.87 KB |
                       BmarkDryIoc | 5.147 us | 0.0237 us | 0.0198 us |  1.10 |      1.0986 |           - |           - |             5.08 KB |
*/
        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerAndRegister_FirstTimeOpenScopeResolve
        {
            //[Benchmark]
            public object BmarkAutofac() => Measure(PrepareAutofac());

            [Benchmark]
            public object BmarkDryIoc() => Measure(PrepareDryIoc());

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => Measure(PrepareMsDi());

            //[Benchmark]
            public object BmarkGrace() => Measure(PrepareGrace());

            //[Benchmark]
            public object BmarkLightInject() => Measure(PrepareLightInject());

            //[Benchmark]
            public object BmarkLamar() => Measure(PrepareLamar());

            //[Benchmark]
            public object BmarkSimpleInjector() => Measure(PrepareSimpleInjector());
        }

        /*
         ## Initial:
                                    Method |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ---------------------------------- |----------:|----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
         BmarkMicrosoftDependencyInjection |  1.071 us | 0.0224 us | 0.0306 us |  1.065 us |  1.00 |    0.00 |      0.5436 |           - |           - |             2.51 KB |
                               BmarkDryIoc |  1.228 us | 0.0260 us | 0.0587 us |  1.209 us |  1.16 |    0.08 |      0.4864 |           - |           - |             2.25 KB |
                                BmarkGrace |  8.158 us | 0.9760 us | 2.8776 us |  8.028 us |  8.30 |    2.68 |      1.4801 |           - |           - |             6.84 KB |
                          BmarkLightInject |  8.511 us | 1.0015 us | 2.9529 us |  7.533 us |  6.18 |    1.49 |      3.8376 |      0.0076 |           - |             17.7 KB |
                              BmarkAutofac | 32.694 us | 3.3321 us | 9.7725 us | 30.384 us | 33.91 |   10.09 |      4.5471 |      0.0305 |           - |            20.96 KB |

        ## After converting lambdas to local functions in Register:

                                    Method |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ---------------------------------- |---------:|----------:|----------:|---------:|------:|--------:|------------:|------------:|------------:|--------------------:|
         BmarkMicrosoftDependencyInjection | 1.078 us | 0.0039 us | 0.0030 us | 1.078 us |  1.00 |    0.00 |      0.5436 |           - |           - |             2.51 KB |
                               BmarkDryIoc | 1.373 us | 0.0670 us | 0.1812 us | 1.284 us |  1.22 |    0.20 |      0.4807 |           - |           - |             2.22 KB |

        ## After removing lambdas altogether in Register:

                                    Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
         BmarkMicrosoftDependencyInjection | 1.054 us | 0.0069 us | 0.0061 us |  1.00 |      0.5436 |           - |           - |             2.51 KB |
                               BmarkDryIoc | 1.190 us | 0.0082 us | 0.0077 us |  1.13 |      0.4253 |           - |           - |             1.97 KB |

        ## After stripping conflicts from ImHashMap:

                                    Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ---------------------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
         BmarkMicrosoftDependencyInjection | 1.070 us | 0.0052 us | 0.0048 us |  1.00 |      0.5436 |           - |           - |             2.51 KB |
                               BmarkDryIoc | 1.188 us | 0.0086 us | 0.0080 us |  1.11 |      0.3986 |           - |           - |             1.84 KB |

        ## After back to initial ImHashMap structure but with optimized AddOrUpdate / balancing:

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.068 us | 0.0019 us | 0.0016 us |  1.00 |    0.00 |      0.5436 |           - |           - |             2.51 KB |
                       BmarkDryIoc |  1.155 us | 0.0032 us | 0.0030 us |  1.08 |    0.00 |      0.4253 |           - |           - |             1.97 KB |
                        BmarkGrace |  5.079 us | 0.0383 us | 0.0358 us |  4.76 |    0.03 |      1.4725 |           - |           - |             6.82 KB |
                  BmarkLightInject |  5.812 us | 0.0208 us | 0.0195 us |  5.44 |    0.02 |      3.8376 |      0.0076 |           - |             17.7 KB |
                      BmarkAutofac | 23.668 us | 0.1853 us | 0.1642 us | 22.16 |    0.15 |      4.5471 |      0.0305 |           - |            20.96 KB |

        ## After improvements in ImMap

                            Method |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------------------- |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 BmarkMicrosoftDependencyInjection |  1.038 us | 0.0048 us | 0.0043 us |  1.00 |    0.00 |      0.5436 |           - |           - |             2.51 KB |
                       BmarkDryIoc |  1.113 us | 0.0062 us | 0.0055 us |  1.07 |    0.00 |      0.4253 |           - |           - |             1.97 KB |
                        BmarkGrace |  5.140 us | 0.0247 us | 0.0231 us |  4.95 |    0.03 |      1.4725 |           - |           - |             6.82 KB |
                  BmarkLightInject |  5.821 us | 0.0285 us | 0.0253 us |  5.61 |    0.04 |      3.8376 |      0.0076 |           - |             17.7 KB |
                      BmarkAutofac | 23.733 us | 0.5465 us | 0.6506 us | 22.85 |    0.60 |      4.5166 |      0.0610 |           - |            20.96 KB |
             */
        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class CreateContainerAndRegister
        {
            [Benchmark]
            public object BmarkAutofac() => PrepareAutofac();

            [Benchmark]
            public object BmarkDryIoc() => PrepareDryIoc();

            [Benchmark(Baseline = true)]
            public object BmarkMicrosoftDependencyInjection() => PrepareMsDi();

            [Benchmark]
            public object BmarkGrace() => PrepareGrace();

            [Benchmark]
            public object BmarkLightInject() => PrepareLightInject();

            //[Benchmark]
            public object BmarkLamar() => Measure(PrepareLamar());

            //[Benchmark]
            public object BmarkSimpleInjector() => Measure(PrepareSimpleInjector());
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class SecondOpenScopeResolve
        {
            /*
## 25.01.2019
                             Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
----------------------------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                        BmarkDryIoc |   176.6 ns | 0.5879 ns | 0.5499 ns |  0.63 |    0.00 |      0.0896 |           - |           - |               424 B |
                         BmarkGrace |   266.3 ns | 0.8584 ns | 0.8029 ns |  0.96 |    0.00 |      0.1216 |           - |           - |               576 B |
 BmarkMicrosoftSDependencyInjection |   278.1 ns | 1.2454 ns | 1.1041 ns |  1.00 |    0.00 |      0.0758 |           - |           - |               360 B |
                   BmarkLightInject | 1,009.5 ns | 4.3021 ns | 4.0242 ns |  3.63 |    0.02 |      0.2422 |           - |           - |              1144 B |
                       BmarkAutofac | 2,014.8 ns | 6.9735 ns | 5.8232 ns |  7.24 |    0.03 |      0.6676 |           - |           - |              3152 B |
             */

            private static readonly IContainer _autofac = PrepareAutofac();
            private static readonly DryIoc.IContainer _dryioc = PrepareDryIoc();
            private static readonly IServiceProvider _msDi = PrepareMsDi();
            private static readonly DependencyInjectionContainer _grace = PrepareGrace();
            private static readonly ServiceContainer _lightInject = PrepareLightInject();
            private static readonly Lamar.Container _lamar = PrepareLamar();
            private static readonly SimpleInjector.Container _simpleInjector = PrepareSimpleInjector();

            [GlobalSetup]
            public void WarmUp()
            {
                for (var i = 0; i < 5; i++)
                {
                    Measure(_autofac);
                    Measure(_dryioc);
                    Measure(_msDi);
                    Measure(_grace);
                    Measure(_lightInject);
                    //Measure(_lamar);
                }
            }

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

            //[Benchmark]
            public object BmarkLamar() => Measure(_lamar);

            //[Benchmark]
            public object BmarkSimpleInjector() => Measure(_simpleInjector);
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class ThirdOpenScopeResolve
        {
            /*
            ## 31.01.2019
            */

            private static readonly IContainer _autofac = PrepareAutofac();
            private static readonly DryIoc.IContainer _dryioc = PrepareDryIoc();
            private static readonly IServiceProvider _msDi = PrepareMsDi();
            private static readonly DependencyInjectionContainer _grace = PrepareGrace();
            private static readonly ServiceContainer _lightInject = PrepareLightInject();
            private static readonly Lamar.Container _lamar = PrepareLamar();
            private static readonly SimpleInjector.Container _simpleInjector = PrepareSimpleInjector();

            [GlobalSetup]
            public void WarmUp()
            {
                for (var i = 0; i < 5; i++)
                {
                    Measure(_autofac);
                    Measure(_autofac);

                    Measure(_dryioc);
                    Measure(_dryioc);

                    Measure(_msDi);
                    Measure(_msDi);

                    Measure(_grace);
                    Measure(_grace);

                    Measure(_lightInject);
                    Measure(_lightInject);
                }
            }

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

            //[Benchmark]
            public object BmarkLamar() => Measure(_lamar);

            //[Benchmark]
            public object BmarkSimpleInjector() => Measure(_simpleInjector);
        }
    }
}
