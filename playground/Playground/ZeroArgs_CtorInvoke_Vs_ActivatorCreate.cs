using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using DryIoc.ImTools;

namespace Playground
{
    /*
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i5-8350U CPU 1.70GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=6.0.201
  [Host]     : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET Core 6.0.3 (CoreCLR 6.0.322.12309, CoreFX 6.0.322.12309), X64 RyuJIT


|          Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|      CtorInvoke | 82.97 ns | 2.312 ns | 6.780 ns | 79.85 ns |  1.00 |    0.00 | 0.0076 |     - |     - |      24 B |
| ActivatorCreate | 13.56 ns | 0.357 ns | 0.512 ns | 13.39 ns |  0.16 |    0.02 | 0.0076 |     - |     - |      24 B |
    */

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

