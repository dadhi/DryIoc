using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Playground
{
    public class FactoryMethodInvoke_vs_ActivateCreateInstanceBenchmark
    {
        private readonly MethodInfo _factoryMethod = Abc.CreateMethods[0].MakeGenericMethod(typeof(string));

        private readonly Type _type = typeof(Abc<>).MakeGenericType(typeof(string));

        private readonly string _arg = "hello";

        [Benchmark]
        public object FactoryMethodInvoke()
        {
            return _factoryMethod.Invoke(null, new object[] { _arg });
        }

        [Benchmark]
        public object ActivatorCreateInstance()
        {
            return Activator.CreateInstance(_type, _arg);
        }
    }

    internal static class Abc
    {
        public static readonly MethodInfo[] CreateMethods =
            typeof(Abc).GetTypeInfo().DeclaredMethods.ToArray();

        public static Abc<T1> CreateAbc<T1>(T1 v1)
        {
            return new Abc<T1>(v1);
        }
    }

    public sealed class Abc<T1>
    {
        public T1 V1;

        public Abc(T1 v1)
        {
            V1 = v1;
        }
    }
}
