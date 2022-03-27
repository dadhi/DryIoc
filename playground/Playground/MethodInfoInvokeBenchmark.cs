using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Playground
{
    [MemoryDiagnoser]
    public class InvokeVsInvokeUnsafeBenchmark
    {
        [Benchmark(Baseline = true)] 
        public object Invoke() => 
            SomeMethods.AMethod.Invoke(null, new[] { "a" });

        [Benchmark]
        public object InvokeUnsafe() => 
            SomeMethods.AMethod.InvokeUnsafe(null, new[] { "a" });
    }

    public static class InvokeUnsafeTools
    {
        public static object InvokeUnsafe(this MethodInfo m, object instance, object arg0) => 
            m.Invoke(null, new[] { arg0 });
    }

    public static class SomeMethods
    {
        public static object A(string s) => s;
        public static MethodInfo AMethod = typeof(SomeMethods).GetMethod(nameof(A), new[] { typeof(string) });
    }
}
