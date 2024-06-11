using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using DryIoc;
using System;

namespace Playground;

public class AutoConcreteTypeResolutionBenchmark
{
    /*
                          Method |        Mean |    StdDev | Scaled | Scaled-StdDev | Allocated |
-------------------------------- |------------ |---------- |------- |-------------- |---------- |
      AutoConcreteTypeResolution | 175.7071 us | 9.8643 us |   1.00 |          0.00 |   3.32 kB |
 ConcreteTypeDynamicRegistration | 214.4462 us | 7.9487 us |   1.22 |          0.08 |   7.89 kB |
    */
    [MemoryDiagnoser]
    public class PrepareAndResolve
    {
        [Benchmark(Baseline = true)]
        public object AutoConcreteTypeResolution()
        {
            var c = new Container(r => r.WithAutoConcreteTypeResolution());
            c.Register<A>();
            c.Register<IB, B>();

            return c.Resolve<A>();
        }

        [Benchmark]
        public object ConcreteTypeDynamicRegistration()
        {
            var c = new Container(r => r.WithConcreteTypeDynamicRegistrations());
            c.Register<A>();
            c.Register<IB, B>();

            return c.Resolve<A>();
        }
    }

    /*
                      Method |       Mean |    StdErr |    StdDev | Scaled | Scaled-StdDev |  Gen 0 | Allocated |
-------------------------------- |----------- |---------- |---------- |------- |-------------- |------- |---------- |
  AutoConcreteTypeResolution | 61.5579 ns | 0.4869 ns | 1.8858 ns |   1.00 |          0.00 | 0.0235 |      40 B |
ConcreteTypeDynamicRegistration | 63.7615 ns | 0.6909 ns | 3.5900 ns |   1.04 |          0.06 | 0.0230 |      40 B |

    */
    [MemoryDiagnoser]
    public class Resolve
    {
        private static Container PrepareContainerWithAutoConcreteRule()
        {
            var c = new Container(r => r.WithAutoConcreteTypeResolution());
            c.Register<A>();
            c.Register<IB, B>();
            return c;
        }

        private static Container PrepareContainerWithDynamicRegistration()
        {
            var c = new Container(r => r.WithConcreteTypeDynamicRegistrations());
            c.Register<A>();
            c.Register<IB, B>();
            return c;
        }

        private readonly IContainer _containerWithAutoConcreteRule = PrepareContainerWithAutoConcreteRule();
        private readonly IContainer _containerWithDynamicRegistration = PrepareContainerWithDynamicRegistration();

        [Benchmark(Baseline = true)]
        public object AutoConcreteTypeResolution() => _containerWithAutoConcreteRule.Resolve<A>();

        [Benchmark]
        public object ConcreteTypeDynamicRegistration() => _containerWithDynamicRegistration.Resolve<A>();
    }

    public class A
    {
        public IB B { get; }
        public X X { get; }

        public A(IB b, X x)
        {
            B = b;
            X = x;
        }
    }

    public interface IB { }
    public class B : IB { }
    public class X { }
}

[MemoryDiagnoser]
// [SimpleJob(RuntimeMoniker.Net481)]
[SimpleJob(RuntimeMoniker.Net80)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ConcreteTypeDynamicRegistrationsBenchmarks
{
    private IContainer m_dryioc_withConcreteTypeRegistrations;

    [GlobalSetup]
    public void Setup()
    {
        m_dryioc_withConcreteTypeRegistrations = RegisterDryIoc_WithConcreteTypeRegistrations();
    }

    public Container RegisterDryIoc_WithConcreteTypeRegistrations()
    {
        var container = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());
        container.Register<IServiceWithoutDependencies, ServiceWithoutDependencies>();
        // container.Register<IDependencyViaProperty, DependencyViaProperty>();
        container.Register<IDependencyViaCtor, DependencyViaCtor>();

        return container;
    }

    public static Value[] ResolveDryIoc_WithConcreteTypeRegistrations_Types() => new Value[]{
        new (typeof(IServiceWithoutDependencies), "INoDep"),
        new (typeof(ServiceWithoutDependencies),"NoDep"),
        new (typeof(DependencyViaCtor),"CtorDep"),
        new (typeof(IDependencyViaCtor),"ICtorDep"),
    };

    [Benchmark]
    [ArgumentsSource(nameof(ResolveDryIoc_WithConcreteTypeRegistrations_Types))]
    public object DryIoc_WithConcreteTypeRegistrations(Value type) =>
        m_dryioc_withConcreteTypeRegistrations.Resolve(type.Type);
}

public interface IDependencyViaCtor { }

public class DependencyViaCtorBase
{
    public IServiceWithoutDependencies Bar { get; }
    public DependencyViaCtorBase(IServiceWithoutDependencies bar = null) => Bar = bar;
}

public class DependencyViaCtor : DependencyViaCtorBase, IDependencyViaCtor
{
    public DependencyViaCtor(IServiceWithoutDependencies bar = null) : base(bar) { }
}

public interface IServiceWithoutDependencies
{ }

public class ServiceWithoutDependencies : IServiceWithoutDependencies
{
}

public record Value(Type Type, string Name)
{
    public override string ToString() => Name;
}