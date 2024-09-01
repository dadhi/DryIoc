using System;
using DryIoc;

namespace FooBar;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }

    [CompileTimeRegister]
    public static IContainer GetContainerWithRegistrations()
    {
        var container = new Container();

        // TODO: Register compile-time resolved services using the same DryIoc API
        // or move part (or all) of the existing registrations here from your current DI configuration.

        // These are example registrations to build the IService resolution root and its dependencies
        container.Register<IService, MyService>();
        container.Register<IDependencyA, DependencyA>();
        container.Register(typeof(DependencyB<>), setup: Setup.With(asResolutionCall: true));

        // Note that `RegisterDelegate`, `RegisterInstance` and `Use` methods are not supported because
        // they using the run-time state.
        // Instead you may use `RegisterPlaceholder` to put a hole in the generated object graph,
        // then you fill it in with the run-time registration, e.g. `container.Register<RuntimeDependency>();`
        container.RegisterPlaceholder<RuntimeDependencyC>();

        // You may batch register assemblies as well
        // container.RegisterMany(new[] { MyAssembly });

        return container;
    }

    public interface IService { }

    public class MyService : IService
    {
        public MyService(IDependencyA a, DependencyB<string> b, RuntimeDependencyC c) { }
    }

    public interface IDependencyA { }

    public class DependencyA : IDependencyA { }

    // let's make it struct for fun
    public struct DependencyB<T>
    {
        public readonly IDependencyA A;
        public DependencyB(IDependencyA a) => A = a;
    }

    public class RuntimeDependencyC
    {
    }
}