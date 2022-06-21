using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using DryIoc.ImTools;

namespace Playground
{
    [MemoryDiagnoser]
    public class ZeroArgs_CtorInvoke_Vs_ActivatorCreate
    {
        [Benchmark(Baseline = true)]
        public object CtorInvoke() => _ctor.Invoke(ArrayTools.Empty<object>()); 

        [Benchmark]
        public object ActivatorCreate() => Activator.CreateInstance(typeof(A));

        class A {}
        static readonly ConstructorInfo _ctor = typeof(A).GetConstructors()[0];
    }
}

