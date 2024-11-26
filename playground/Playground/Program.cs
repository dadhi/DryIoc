using DryIoc;
using PerformanceTests;
using DryIoc.FastExpressionCompiler.LightExpression;
using BenchmarkDotNet.Running;
using System;

namespace Playground
{
    public class Program
    {
        public static void Main()
        {
            // var di = RealisticUnitOfWorkBenchmark.PrepareDryIoc();
            // RealisticUnitOfWorkBenchmark.Measure(di);

            // BenchmarkRunner.Run<ZeroArgs_CtorInvoke_Vs_ActivatorCreate>();

            // var x = new InvokeVsInvokeUnsafeBenchmark().InvokeUnsafe();
            // BenchmarkRunner.Run<InvokeVsInvokeUnsafeBenchmark>();

            // BenchmarkRunner.Run<GetFuncInvokeMethodBenchmark>();

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

            // BenchmarkRunner.Run<FindMethodInClass>();
            // BenchmarkRunner.Run<GetConstructor>();
            // var cs = new GetConstructor().GetAll();
            // Console.WriteLine(cs.Length);
            //BenchmarkRunner.Run<FactoryMethodInvoke_vs_ActivateCreateInstanceBenchmark>();

            // var x = RealisticUnitOfWorkWithBigObjectGraphBenchmark.Measure(
            //    RealisticUnitOfWorkWithBigObjectGraphBenchmark.PrepareDryIoc());
            //Debug.Assert(x != null);

            // var di = RealisticUnitOfWorkBenchmark.PrepareDryIocMsDi();
            // var x = RealisticUnitOfWorkBenchmark.Measure(di);
            // var y = RealisticUnitOfWorkBenchmark.Measure(di);
            // Console.ReadKey();

            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CreateContainerAndRegisterServices>();
            // BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.FirstTimeOpenScopeAndResolve>();
            //BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.SecondTimeOpenScopeAndResolve>();
            // BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CreateContainerAndRegisterServices_Then_FirstTimeOpenScopeAndResolve>();

            // BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.CompileResolutionExpression>();
            BenchmarkRunner.Run<RealisticUnitOfWorkBenchmark.OpenScopeAndResolve>();

            // var d = RealisticUnitOfWorkBenchmark.PrepareDryIoc();
            // var e = RealisticUnitOfWorkBenchmark.ResolveExpression(d);
            // var f = (Func<IResolverContext, object>)e.CompileFast();
            // var r = f(d.OpenScope());
            // var s = e.ToCSharpString();
            // // var s = e.ToExpressionString();
            // Console.WriteLine(s);
            // Console.WriteLine("Press any key");
            // Console.ReadKey();

            // var dms = RealisticUnitOfWorkBenchmark.PrepareDryIocMsDi();
            // var ems = RealisticUnitOfWorkBenchmark.ResolveExpression((IContainer)dms);
            // var sms = ems.ToCSharpString();

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
            // BenchmarkRunner.Run<AutoConcreteTypeResolutionBenchmark.Resolve>();
            // BenchmarkRunner.Run<ConcreteTypeDynamicRegistrationsBenchmarks>();
            //BenchmarkRunner.Run<EnumerableWhere_vs_ArrayMatch_Have_some_matches>();
            //BenchmarkRunner.Run<EnumerableWhere_vs_ArrayMatch_Have_all_matches>();

            //BenchmarkRunner.Run<ResolveSingleInstanceWith10NestedSingleInstanceParametersOncePerContainer.BenchmarkRegistrationAndResolution>();
            //BenchmarkRunner.Run<ResolveInstancePerDependencyWith2ParametersOncePerContainer.BenchmarkRegistrationAndResolution>();

            //BenchmarkRunner.Run<IfVsNullCoalescingOperator>();
            //BenchmarkRunner.Run<IfVsTernaryOperator>();
            //BenchmarkRunner.Run<ArrayAccessVsGetOrAddItem>();
            //new BenchmarkRunner().RunCompetition(new ExpressionCompileVsEmit());
            //new BenchmarkRunner().RunCompetition(new RunResultOfCompileVsEmit());
            //var result = ExpressionVsEmit();
            //Console.WriteLine("Ignored result: " + result);
        }
    }
}
