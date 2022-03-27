using PerformanceTests;
using DryIoc.FastExpressionCompiler.LightExpression;

namespace Playground
{
    public class Program
    {
        public static void Main()
        {
            // var x = new InvokeVsInvokeUnsafeBenchmark().InvokeUnsafe();
            // BenchmarkRunner.Run<InvokeVsInvokeUnsafeBenchmark>();

            //var bm = new ManualInsertionSortVsOrderBy();
            //bm.SortViaInsertion();
            //BenchmarkRunner.Run<ManualInsertionSortVsOrderBy>();

            //BenchmarkRunner.Run<MatchCaseOrder>();

            //BenchmarkRunner.Run<ImMapBenchmarks.Populate>();
            //BenchmarkRunner.Run<ImMapBenchmarks.Lookup>();

            //BenchmarkRunner.Run<ImHashMapBenchmarks.Populate>();
            //BenchmarkRunner.Run<ImHashMapBenchmarks.Lookup>();

            //BenchmarkRunner.Run<StructEnumerableTest>();
            //BenchmarkRunner.Run<PropertyAccess>();

            //BenchmarkRunner.Run<FindMethodInClass>();
            //BenchmarkRunner.Run<FactoryMethodInvoke_vs_ActivateCreateInstanceBenchmark>();

            //var x = RealisticUnitOfWorkWithBigObjectGraphBenchmark.Measure(
            //    RealisticUnitOfWorkWithBigObjectGraphBenchmark.PrepareDryIoc());
            //Debug.Assert(x != null);

            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CreateContainerAndRegisterServices>();
            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.FirstTimeOpenScopeAndResolve>();
            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.SecondTimeOpenScopeAndResolve>();
            // BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve>();
            // BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CompileResolutionExpression>();
            // BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.OpenScopeAndResolve>();

            //var di = RealisticUnitOfWorkBenchmark.PrepareDryIoc();
            //var expr = RealisticUnitOfWorkBenchmark.ResolveExpression(di);
            //var s = expr.ToCSharpString();

            // var bm = new RealisticUnitOfWorkBenchmark.OpenScopeAndResolve();
            // bm.WarmUp();
            // Console.WriteLine("WarmUp finished!");
            // Console.ReadKey();
            // bm.DryIoc_MsDI();
            // Console.WriteLine("All is done");
            // Console.ReadKey();


            //CloseToRealLifeUnitOfWorkWithBigObjectGraphBenchmark.Measure(
            //CloseToRealLifeUnitOfWorkWithBigObjectGraphBenchmark.PrepareDryIocMsDi());

            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.CreateContainerAndRegister>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.CreateContainerAndRegister_FirstTimeOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.FirstTimeOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.SecondOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.ThirdOpenScopeResolve>();

            // BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientDeps.CreateContainerRegister_FirstTimeOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientDeps.FirstTimeOpenScopeResolve>();

            //BenchmarkRunner.Run<OpenNamedScopeAndResolveNamedScopedWithTransientNamedScopedDeps.BenchmarkRegistrationAndResolution>();
            //BenchmarkRunner.Run<OpenNamedScopeAndResolveNamedScopedWithTransientNamedScopedDeps.BenchmarkFirstTimeResolutionResolution>();

            //BenchmarkRunner.Run<ActivatorCreateInstance_vs_CtorInvoke>();
            //BenchmarkRunner.Run<AutoConcreteTypeResolutionBenchmark.Resolve>();
            //BenchmarkRunner.Run<EnumerableWhere_vs_ArrayMatch_Have_some_matches>();
            //BenchmarkRunner.Run<EnumerableWhere_vs_ArrayMatch_Have_all_matches>();

            //BenchmarkRunner.Run<ResolveSingleInstanceWith10NestedSingleInstanceParametersOncePerContainer.BenchmarkRegistrationAndResolution>();
            //BenchmarkRunner.Run<ResolveInstancePerDependencyWith2ParametersOncePerContainer.BenchmarkRegistrationAndResolution>();

            //BenchmarkRunner.Run<IfVsNullСoalescingOperator>();
            //BenchmarkRunner.Run<IfVsTernaryOperator>();
            //BenchmarkRunner.Run<ArrayAccessVsGetOrAddItem>();
            //new BenchmarkRunner().RunCompetition(new ExpressionCompileVsEmit());
            //new BenchmarkRunner().RunCompetition(new RunResultOfCompileVsEmit());
            //var result = ExpressionVsEmit();
            //Console.WriteLine("Ignored result: " + result);
        }
    }
}
