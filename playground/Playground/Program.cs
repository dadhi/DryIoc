using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DryIoc;
using ImTools;
using PerformanceTests;

namespace Playground
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<ImHashMapBenchmarks.Lookup>();
            //BenchmarkRunner.Run<ImHashMapBenchmarks.Populate>();

            //BenchmarkRunner.Run<ImMapBenchmarks.Populate>();
            //BenchmarkRunner.Run<ImMapBenchmarks.Lookup>();

            //BenchmarkRunner.Run<StructEnumerableTest>();
            //BenchmarkRunner.Run<PropertyAccess>();

            //BenchmarkRunner.Run<FindMethodInClass>();
            //BenchmarkRunner.Run<FactoryMethodInvoke_vs_ActivateCreateInstanceBenchmark>();

            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.CreateContainerAndRegister>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.CreateContainerAndRegister_FirstTimeOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.FirstTimeOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientScopedDeps.OpenScopeResolveAfterWarmUp>();

            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientDeps.CreateContainerRegister_FirstTimeOpenScopeResolve>();
            //BenchmarkRunner.Run<OpenScopeAndResolveScopedWithSingletonTransientDeps.FirstTimeOpenScopeResolve>();

            //BenchmarkRunner.Run<OpenNamedScopeAndResolveNamedScopedWithTransientNamedScopedDeps.BenchmarkRegistrationAndResolution>();
            //BenchmarkRunner.Run<OpenNamedScopeAndResolveNamedScopedWithTransientAndNamedScopedDeps.BenchmarkResolution>();

            //BenchmarkRunner.Run<ActivatorCreateInstance_vs_CtorInvoke>();
            //BenchmarkRunner.Run<AutoConcreteTypeResolutionBenchmark.Resolve>();
            //BenchmarkRunner.Run<EnumerableWhere_vs_ArrayMatch_Have_some_matches>();
            //BenchmarkRunner.Run<EnumerableWhere_vs_ArrayMatch_Have_all_matches>();
            //BenchmarkRunner.Run<ResolveSingleInstanceWith10NestedSingleInstanceParametersOncePerContainer.BenchmarkRegistrationAndResolution>();
            //BenchmarkRunner.Run<ResolveInstancePerDependencyWith2ParametersOncePerContainer.BenchmarkRegistrationAndResolution>();
            //BenchmarkRunner.Run<BenchmarkResolution>();
            //BenchmarkRunner.Run<IfVsNullСoalescingOperator>();
            //BenchmarkRunner.Run<IfVsTernaryOperator>();
            //BenchmarkRunner.Run<ArrayAccessVsGetOrAddItem>();
            //new BenchmarkRunner().RunCompetition(new ExpressionCompileVsEmit());
            //new BenchmarkRunner().RunCompetition(new RunResultOfCompileVsEmit());
            //var result = ExpressionVsEmit();
            //Console.WriteLine("Ignored result: " + result);
        }

        public class M { }

        public class K
        {
            public K(M m) { }
        }

        public class ActivatorCreateInstance_vs_CtorInvoke
        {
            private readonly M _m = new M();

            [Benchmark]
            public K ActivatorCreateInstance()
            {
                return (K)Activator.CreateInstance(typeof(K), _m);
            }

            [Benchmark]
            public K CtorInvoke()
            {
                var uninitializedX = FormatterServices.GetUninitializedObject(typeof(K));
                return (K)typeof(K).GetConstructors()[0].Invoke(uninitializedX, new object[] { _m });
            }
        }

        public class EnumerableWhere_vs_ArrayMatch_Have_some_matches
        {
            private int[] _arr = { 1, 2, 3 };

            [Benchmark]
            public int[] EnumerableWhere()
            {
                return _arr.Where(it => it % 2 == 0).ToArray();
            }

            [Benchmark]
            public int[] ArrayMatch()
            {
                return _arr.Match(it => it % 2 == 0);
            }
        }

        public class EnumerableWhere_vs_ArrayMatch_Have_all_matches
        {
            private int[] _arr = { 1, 2, 3 };

            [Benchmark]
            public int[] EnumerableWhere()
            {
                return _arr.Where(it => it > 0).ToArray();
            }

            [Benchmark]
            public int[] ArrayMatch()
            {
                return _arr.Match(it => it > 0);
            }
        }

        public class IfVsNullСoalescingOperator
        {
            private readonly object x = "a";

            [Benchmark]
            public string If()
            {
                var s = x as string;
                if (s != null) return s;
                return string.Empty;
            }

            [Benchmark]
            public string NullCoalescingOperator()
            {
                return (x as string) ?? string.Empty;
            }
        }

        public class IfVsTernaryOperator
        {
            private readonly object x = "a";

            [Benchmark]
            public string If()
            {
                if (x is string) return (string)x;
                return string.Empty;
            }

            [Benchmark]
            public string TernaryOperator()
            {
                return x is string ? (string)x : string.Empty;
            }
        }

        public class ExpressionCompileVsEmit
        {
            private static Expression<Func<object[], object>> _funcExpr = CreateComplexExpression();

            [Benchmark]
            public void Compile()
            {
                _funcExpr.Compile();
            }

            [Benchmark]
            public void Emit()
            {
                EmitDelegateFromExpression(_funcExpr.Body);
            }
        }

        //[BenchmarkTask(platform: BenchmarkPlatform.X86, jitVersion: BenchmarkJitVersion.LegacyJit)]
        //[BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.LegacyJit)]
        //[BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.RyuJit)]
        public class RunResultOfCompileVsEmit
        {
            private Func<object[], object> _funcCompiled;
            private Func<object[], object> _funcEmitted;
            private object[] _state;

            [GlobalSetup]
            public void SetupData()
            {
                var expression = CreateComplexExpression();
                _funcCompiled = expression.Compile();
                _funcEmitted = EmitDelegateFromExpression(expression.Body);
                _state = new object[15];
                _state[11] = "x";

            }

            [Benchmark]
            public void RunCompiled()
            {
                _funcCompiled(_state);
            }

            [Benchmark]
            public void RunEmitted()
            {
                _funcEmitted(_state);
            }
        }

        // Result delegate to be created by CreateComplexExpression
        public object CreateA(object[] state)
        {
            return new A(new B(), (string)state[11], new ID[] { new D1(), new D2() }) { Prop = new P(new B()), Bop = new B() };
        }

        private static Expression<Func<object[], object>> CreateComplexExpression()
        {
            var stateParamExpr = Expression.Parameter(typeof(object[]));

            var funcExpr = Expression.Lambda<Func<object[], object>>(
                Expression.MemberInit(
                    Expression.New(typeof(A).GetConstructors()[0],
                        Expression.New(typeof(B).GetConstructors()[0]),
                        Expression.Convert(Expression.ArrayIndex(stateParamExpr, Expression.Constant(11)), typeof(string)),
                        Expression.NewArrayInit(typeof(ID),
                            Expression.New(typeof(D1).GetConstructors()[0]),
                            Expression.New(typeof(D2).GetConstructors()[0]))),
                    Expression.Bind(typeof(A).GetProperty("Prop"),
                        Expression.New(typeof(P).GetConstructors()[0],
                            Expression.New(typeof(B).GetConstructors()[0]))),
                    Expression.Bind(typeof(A).GetField("Bop"),
                        Expression.New(typeof(B).GetConstructors()[0]))),
                stateParamExpr);

            return funcExpr;
        }

        private static Func<object[], object> EmitDelegateFromExpression(Expression expr)
        {
            var method = new DynamicMethod("CreateA", typeof(object), new[] { typeof(object[]) });
            var il = method.GetILGenerator();

            var ok = EmittingVisitor.TryVisit(expr, il);

            il.Emit(OpCodes.Ret);

            return (Func<object[], object>)method.CreateDelegate(typeof(Func<object[], object>));
        }
    }

    public class ArrayAccessVsGetOrAddItem
    {
        private readonly object[] _state = new object[32];
        private object _result;

        [GlobalSetup]
        public void Setup()
        {
            _state[15] = "a";
        }

        [Benchmark]
        public void ArrayAccess()
        {
            _result = _state[15];
        }

        [Benchmark]
        public void GetOrAddItem()
        {
            var a = _state[15];
            if (a == null)
            {
                lock (_state)
                {
                    if (a == null)
                    {
                        a = "b";
                        _state[15] = "b";
                    }
                }
            }
            _result = a;
        }
    }

    public class A
    {
        public P Prop { get; set; }
        public B Bop;

        public A(B b, string s, IEnumerable<ID> ds) { }
    }

    public class B { }

    public class P { public P(B b) { } }

    public interface ID { }
    public class D1 : ID { }

    public static class EmittingVisitor
    {
        public static bool TryVisit(Expression expr, ILGenerator il)
        {
            switch (expr.NodeType)
            {
                case ExpressionType.Convert:
                    return VisitConvert((UnaryExpression)expr, il);
                case ExpressionType.ArrayIndex:
                    return VisitArrayIndex((BinaryExpression)expr, il);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expr, il);
                case ExpressionType.Parameter:
                    il.Emit(OpCodes.Ldarg_0); // state is the first argument
                    Debug.WriteLine("Ldarg_0");
                    return true;
                case ExpressionType.New:
                    return VisitNew((NewExpression)expr, il);
                case ExpressionType.NewArrayInit:
                    return VisitNewArray((NewArrayExpression)expr, il);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)expr, il);
                default:
                    // not supported nodes
                    return false;
            }
        }

        private static bool VisitBinary(BinaryExpression b, ILGenerator il)
        {
            var ok = TryVisit(b.Left, il);
            if (ok)
                ok = TryVisit(b.Right, il);
            // skips TryVisit(b.Conversion) for NodeType.Coalesce (?? operation)
            return ok;
        }

        private static bool VisitExpressionList(IList<Expression> eList, ILGenerator state)
        {
            var ok = true;
            for (int i = 0, n = eList.Count; i < n && ok; i++)
                ok = TryVisit(eList[i], state);
            return ok;
        }

        private static bool VisitConvert(UnaryExpression node, ILGenerator il)
        {
            var ok = TryVisit(node.Operand, il);
            if (ok)
            {
                var convertTargetType = node.Type;
                if (convertTargetType != typeof(object)) // cast to object is not required
                {
                    il.Emit(OpCodes.Castclass, convertTargetType);
                    Debug.WriteLine("Castclass " + convertTargetType);
                }
                else
                {
                    ok = false;
                }
            }
            return ok;
        }

        private static bool VisitConstant(ConstantExpression node, ILGenerator il)
        {
            var value = node.Value;
            if (value == null)
            {
                il.Emit(OpCodes.Ldnull);
                Debug.WriteLine("Ldnull");
            }
            else if (value is int || value.GetType().IsEnum)
            {
                EmitLoadConstantInt(il, (int)value);
                Debug.WriteLine("Ldc_I4 " + value);
            }
            else if (value is string)
            {
                il.Emit(OpCodes.Ldstr, (string)value);
                Debug.WriteLine("Ldstr " + value);
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool VisitNew(NewExpression node, ILGenerator il)
        {
            var ok = VisitExpressionList(node.Arguments, il);
            if (ok)
            {
                il.Emit(OpCodes.Newobj, node.Constructor);
                Debug.WriteLine("Newobj " + node.Constructor.DeclaringType);
            }
            return ok;
        }

        private static bool VisitNewArray(NewArrayExpression node, ILGenerator il)
        {
            var elems = node.Expressions;
            var arrType = node.Type;
            var elemType = arrType.GetElementType();
            var isElemOfValueType = elemType.IsValueType;

            var arrVar = il.DeclareLocal(arrType);

            EmitLoadConstantInt(il, elems.Count);

            il.Emit(OpCodes.Newarr, elemType);
            Debug.WriteLine("Newarr " + elemType);
            il.Emit(OpCodes.Stloc, arrVar);
            Debug.WriteLine("Stloc_0");

            var ok = true;
            for (int i = 0, n = elems.Count; i < n && ok; i++)
            {
                il.Emit(OpCodes.Ldloc, arrVar);
                Debug.WriteLine("Ldloc array");

                EmitLoadConstantInt(il, i);

                if (isElemOfValueType)
                {
                    il.Emit(OpCodes.Ldelema, elemType); // loading element address for later copying of value into it.
                    Debug.WriteLine("Ldelema " + elemType);
                }

                ok = TryVisit(elems[i], il);
                if (ok)
                {
                    if (isElemOfValueType)
                    {
                        il.Emit(OpCodes.Stobj, elemType); // store element of value type by array element address
                        Debug.WriteLine("Stobj " + elemType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stelem_Ref);
                        Debug.WriteLine("Stelem_Ref");
                    }
                }
            }

            il.Emit(OpCodes.Ldloc, arrVar);
            Debug.WriteLine("Ldloc_0");

            return ok;
        }

        private static bool VisitArrayIndex(BinaryExpression node, ILGenerator il)
        {
            var ok = VisitBinary(node, il);
            if (ok)
            {
                il.Emit(OpCodes.Ldelem_Ref);
                Debug.WriteLine("Ldelem_Ref");
            }
            return ok;
        }

        private static bool VisitMemberInit(MemberInitExpression mi, ILGenerator il)
        {
            var ok = VisitNew(mi.NewExpression, il);
            if (!ok) return false;

            var obj = il.DeclareLocal(mi.Type);
            il.Emit(OpCodes.Stloc, obj);
            Debug.WriteLine("Stloc " + obj);

            var bindings = mi.Bindings;
            for (int i = 0, n = bindings.Count; i < n; i++)
            {
                var binding = bindings[i];
                if (binding.BindingType != MemberBindingType.Assignment)
                    return false;

                il.Emit(OpCodes.Ldloc, obj);
                Debug.WriteLine("Ldloc " + obj);

                ok = TryVisit(((MemberAssignment)binding).Expression, il);
                if (!ok) return false;

                var prop = binding.Member as PropertyInfo;
                if (prop != null)
                {
                    var setMethod = prop.GetSetMethod();
                    if (setMethod == null)
                        return false;

                    if (setMethod.IsVirtual)
                    {
                        il.Emit(OpCodes.Callvirt, setMethod);
                        Debug.WriteLine("Callvirt " + setMethod);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, setMethod);
                        Debug.WriteLine("Call " + setMethod);
                    }
                }
                else
                {
                    var field = binding.Member as FieldInfo;
                    if (field == null)
                        return false;

                    il.Emit(OpCodes.Stfld, field);
                    Debug.WriteLine("Stfld " + field);
                }
            }

            il.Emit(OpCodes.Ldloc, obj);
            Debug.WriteLine("Ldloc " + obj);
            return true;
        }

        private static void EmitLoadConstantInt(ILGenerator il, int i)
        {
            switch (i)
            {
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4, i);
                    break;
            }
        }
    }
    public class D2 : ID { }
}
