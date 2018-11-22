using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Playground
{
    [MemoryDiagnoser]
    public class FactoryMethodInvoke_vs_ActivateCreateInstanceBenchmark
    {
        private static readonly MethodInfo FactoryMethod = Abc.CreateMethods[0].MakeGenericMethod(typeof(string));

        private static readonly Type AbcType = typeof(Abc<>).MakeGenericType(typeof(string));
        private static readonly ConstructorInfo AbcCtor = AbcType.GetConstructors()[0];

        private static readonly object[] Args = { "hello" };

        //[Benchmark]
        public object FactoryMethodInvoke() => FactoryMethod.Invoke(null, Args);

        [Benchmark(Baseline = true)]
        public object ActivatorCreateInstance() => Activator.CreateInstance(AbcType, Args);

        [Benchmark]
        public object ConstructorInvoke() => AbcCtor.Invoke(Args);
    }

    internal static class Abc
    {
        public static readonly MethodInfo[] CreateMethods =
            typeof(Abc).GetTypeInfo().DeclaredMethods.ToArray();

        public static Abc<T1> CreateAbc<T1>(T1 v1) => new Abc<T1>(v1);
    }

    public sealed class Abc<T1>
    {
        public T1 V1;
        public Abc(T1 v1) => V1 = v1;
    }
}
