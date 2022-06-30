#pragma warning disable 1591

namespace Example
{
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
