using BenchmarkDotNet.Running;

namespace BenchmarkDryIocV3ForComparison
{
    public class Program
    {
        public static void Main()
        {
            //BenchmarkRunner.Run<RealisticUnitOfWorkWithBigObjectGraphBenchmark.CreateContainerAndRegisterServices>();
            BenchmarkRunner.Run<RealisticUnitOfWorkWithBigObjectGraphBenchmark.CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve>();
            //BenchmarkRunner.Run<RealisticUnitOfWorkWithBigObjectGraphBenchmark.OpenScopeAndResolve>();
        }
    }
}
