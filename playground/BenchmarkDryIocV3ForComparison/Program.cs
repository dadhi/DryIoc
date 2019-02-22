using BenchmarkDotNet.Running;
using PerformanceTests;

namespace BenchmarkDryIocV3ForComparison
{
    public class Program
    {
        public static void Main()
        {
            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CreateContainerAndRegisterServices>();
            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve>();
            BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.OpenScopeAndResolve>();
        }
    }
}
