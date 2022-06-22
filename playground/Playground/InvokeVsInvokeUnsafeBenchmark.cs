using System;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using System.Runtime.InteropServices;

namespace Playground
{
    [MemoryDiagnoser]
    public class InvokeVsInvokeUnsafeBenchmark
    {
        [Benchmark(Baseline = true)]
        public object Invoke() =>
            SomeMethods.AMethod.Invoke(null, new[] { "a" });

        [Benchmark]
        public object InvokeUnsafe()
        {
            var rtMethodInfo = Type.GetType("System.Reflection.RuntimeMethodInfo");
            Console.WriteLine(rtMethodInfo);
            if (rtMethodInfo == null)
                return null;
            var rtMethodHandle = Type.GetType("System.RuntimeMethodHandle");
            Console.WriteLine(rtMethodHandle);
            if (rtMethodHandle == null)
                return null;

            var sigProp = rtMethodInfo.GetMethod("get_Signature", BindingFlags.NonPublic | BindingFlags.Instance);
            Console.WriteLine(sigProp.Name);
            var invokeMethod = rtMethodHandle.GetMethod("InvokeMethod", BindingFlags.NonPublic | BindingFlags.Static);
            Console.WriteLine(invokeMethod.Name);

            // foreach (var m in rt.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            // {
            //     Console.WriteLine(m.Name + "(" + m.GetParameters().Length + ")");
            // }

            // var invokeWorkerMethod = rt.GetMethod("InvokeWorker", BindingFlags.NonPublic | BindingFlags.Static);
            // Console.WriteLine(invokeWorkerMethod);
            // if (invokeWorkerMethod == null)
            //     return null;

            var m = SomeMethods.AMethod;
            var sig = sigProp.Invoke(m, Type.EmptyTypes);
            Console.WriteLine(sig);

            object arg0 = "a";
            var argSpan = MemoryMarshal.CreateSpan<object>(ref arg0, 1);
            Console.WriteLine(argSpan[0]);

            // todo: @incomplete we need to emit something like this:
            // IL_0000: ldloca.s 0
            // IL_0002: ldsfld object[] C::arr
            // IL_0007: call instance void valuetype [System.Private.CoreLib]System.Span`1<object>::.ctor(!0[])
            // IL_000c: ldloca.s 0
            // IL_000e: ldnull
            // IL_000f: ldc.i4.0
            // IL_0010: ldc.i4.0
            // IL_0011: call object C::InvokeMethod(valuetype [System.Private.CoreLib]System.Span`1<object>&, object, bool, bool)
            // IL_0016: ret

            // invokeMethod.Invoke(null, new object[] { null, (object)argSpan, sig, false, false );

            // return SomeMethods.AMethod.InvokeUnsafe(null, "a");
            return "a";
        }
    }

    [MemoryDiagnoser]
    public class GetFuncInvokeMethodBenchmark
    {
        private const string InvokeMethodName = "Invoke";
        private static Type _funcType = typeof(Func<int, string, bool, object>);
        private static Type _funcOpenType = typeof(Func<,,,>).GetGenericTypeDefinition();
        private static MethodInfo _funcOpenInvokeMethod = _funcOpenType.GetMethod(InvokeMethodName);


        [Benchmark(Baseline = true)]
        public object GetFuncInvokeMethod() =>
            _funcType.GetMethod(InvokeMethodName);

        [Benchmark]
        public object GetFuncInvokeMethodFast() =>
            _funcOpenInvokeMethod.MakeGenericMethod(_funcType.GetGenericArguments());
    }

    public static class InvokeUnsafeTools
    {

        static InvokeUnsafeTools()
        {
            // todo: we need to call this directly
            // private object? RuntimeMethodInfo.InvokeWorker(object? obj, BindingFlags invokeAttr, Span<object?> arguments)
            // {
            //     bool wrapExceptions = (invokeAttr & BindingFlags.DoNotWrapExceptions) == 0;
            //     return RuntimeMethodHandle.InvokeMethod(obj, in arguments, Signature, false, wrapExceptions);
            // }

            // var rtMethodInfo = Type.GetType("System.Reflection.RuntimeMethodInfo");
            // Console.WriteLine(rtMethodInfo);
            // if (rtMethodInfo == null)
            //     return;

            // var invokeWorkerMethod = rtMethodInfo.GetMethod("InvokeWorker", BindingFlags.NonPublic | BindingFlags.Instance);
            // Console.WriteLine(invokeWorkerMethod);
            // if (invokeWorkerMethod == null)
            //     return;

            // // the default allocatee method
            // _getNextLocalVarIndex = (i, t) => i.DeclareLocal(t).LocalIndex;

            // // now let's try to acquire the more efficient less allocating method
            // var ilGenTypeInfo = typeof(MethodInfo).GetTypeInfo();
            // var localSignatureField = ilGenTypeInfo.GetDeclaredField("m_localSignature");
            // if (localSignatureField == null)
            //     return;

            // var localCountField = ilGenTypeInfo.GetDeclaredField("m_localCount");
            // if (localCountField == null)
            //     return;

            // // looking for the `SignatureHelper.AddArgument(Type argument, bool pinned)`
            // MethodInfo addArgumentMethod = null;
            // foreach (var m in typeof(SignatureHelper).GetTypeInfo().GetDeclaredMethods("AddArgument"))
            // {
            //     var ps = m.GetParameters();
            //     if (ps.Length == 2 && ps[0].ParameterType == typeof(Type) && ps[1].ParameterType == typeof(bool))
            //     {
            //         addArgumentMethod = m;
            //         break;
            //     }
            // }

            // if (addArgumentMethod == null)
            //     return;

            // // our own helper - always available
            // var postIncMethod = typeof(ILGeneratorHacks).GetTypeInfo().GetDeclaredMethod(nameof(PostInc));

            // var efficientMethod = new DynamicMethod(string.Empty,
            //     typeof(int), new[] { typeof(ExpressionCompiler.ArrayClosure), typeof(ILGenerator), typeof(Type) },
            //     typeof(ExpressionCompiler.ArrayClosure), skipVisibility: true);
            // var il = efficientMethod.GetILGenerator();

            // // emitting `il.m_localSignature.AddArgument(type);`
            // il.Emit(OpCodes.Ldarg_1);  // load `il` argument (arg_0 is the empty closure object)
            // il.Emit(OpCodes.Ldfld, localSignatureField);
            // il.Emit(OpCodes.Ldarg_2);  // load `type` argument
            // il.Emit(OpCodes.Ldc_I4_0); // load `pinned: false` argument
            // il.Emit(OpCodes.Call, addArgumentMethod);

            // // emitting `return PostInc(ref il.LocalCount);`
            // il.Emit(OpCodes.Ldarg_1); // load `il` argument
            // il.Emit(OpCodes.Ldflda, localCountField);
            // il.Emit(OpCodes.Call, postIncMethod);

            // il.Emit(OpCodes.Ret);

            // _getNextLocalVarIndex = (Func<ILGenerator, Type, int>)efficientMethod.CreateDelegate(
            //     typeof(Func<ILGenerator, Type, int>), ExpressionCompiler.EmptyArrayClosure);
      }

        public static object InvokeUnsafe(this MethodInfo m, object instance, object arg0) =>
            m.Invoke(instance, new[] { arg0 });
    }

    public static class SomeMethods
    {
        public static object A(string s) => s;
        public static MethodInfo AMethod = typeof(SomeMethods).GetMethod(nameof(A), new[] { typeof(string) });
    }
}
