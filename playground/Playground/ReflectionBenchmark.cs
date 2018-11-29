using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using DryIoc;

namespace Playground
{
    [MemoryDiagnoser]
    public class FindMethodInClass
    {
        [Benchmark]
        public MethodInfo ViaSingleMethod() => typeof(GetMyMethods).SingleMethod(nameof(GetMyMethods.C));

        [Benchmark(Baseline = true)] 
        public MethodInfo ViaCast() => ((Func<string, int>)GetMyMethods.C).Method;

        [Benchmark]
        public MethodInfo ViaDeclaredMethod() => typeof(GetMyMethods).GetTypeInfo().GetDeclaredMethod(nameof(GetMyMethods.C));
    }

    public static class GetMyMethods
    {
        public static int A(string s) => 0;
        public static int B(string s) => 0;
        public static int C(string s) => 0;
        public static int D(string s) => 0;
        public static int E(string s) => 0;
    }
}
