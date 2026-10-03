#pragma warning disable 1591

using System.Collections.Generic;

namespace Example
{
    public interface IService { }

    public class MyService : IService
    {
        public MyService(IDependencyA a, DependencyB<string> b, RuntimeDependencyC c) { }
    }

    public interface IDependencyA { }

    public class DependencyA : IDependencyA { }

    // let's make a struct for fun
    public struct DependencyB<T>
    {
        public readonly IDependencyA A;
        public DependencyB(IDependencyA a) => A = a;
    }

    public class RuntimeDependencyC { }

    public abstract class BaseA { }
    public class KeyedA : BaseA { }
    public class NonKeyedA : BaseA { }
    public class BaseAConsumer
    {
        public IDictionary<object, BaseA> Addict;
        public BaseAConsumer(IDictionary<object, BaseA> addict) => Addict = addict;
    }
}
