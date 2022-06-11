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

    public class DependencyB<T>
    {
    }

    public class RuntimeDependencyC
    {
    }
}
