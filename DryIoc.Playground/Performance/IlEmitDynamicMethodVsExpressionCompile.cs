using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace DryIoc.Playground.Performance
{
    public static class IlEmitDynamicMethodVsExpressionCompile
    {
        public static void Compare()
        {
            var times = 10 * 1000;

            object result = null;
            GC.Collect();

            var timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = EmitFactory()();
            }
            timer.Stop();
            Console.WriteLine("ILEmit took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = CompileFactory()();
            }
            timer.Stop();
            Console.WriteLine("Expression took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            GC.KeepAlive(result);
        }

        public static void CompareResultDelegates()
        {
            var times = 5 * 1000 * 1000;

            object result = null;
            var enittedFactory = EmitFactory();
            var compiledFactory = CompileFactory();

            GC.Collect();

            var timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = enittedFactory();
            }
            timer.Stop();
            Console.WriteLine("ILEmitted factory took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            timer = Stopwatch.StartNew();
            for (int t = 0; t < times; t++)
            {
                result = compiledFactory();
            }
            timer.Stop();
            Console.WriteLine("Compiled factory took {0} milliseconds to complete.", timer.ElapsedMilliseconds);
            GC.Collect();

            GC.KeepAlive(result);
        }

        public static Func<object> EmitFactory()
        {
            var dependencyCtor = typeof(Dependency).GetConstructors()[0];
            var serviceCtor = typeof(Service).GetConstructors()[0];

            var method = new DynamicMethod("Create", typeof(object), Type.EmptyTypes);
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Newobj, dependencyCtor);
            il.Emit(OpCodes.Newobj, serviceCtor);
            il.Emit(OpCodes.Ret);

            var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
            return func;
        }

        public static Func<object> CompileFactory()
        {
            var dependencyCtor = typeof(Dependency).GetConstructors()[0];
            var serviceCtor = typeof(Service).GetConstructors()[0];

            var funcExpr = Expression.Lambda<Func<object>>(
                Expression.New(serviceCtor, 
                Expression.New(dependencyCtor, null)),
                null);

            var func = funcExpr.Compile();
            return func;
        }
    }

    public class Service
    {
        public readonly Dependency Dependency;

        public Service(Dependency dependency)
        {
            Dependency = dependency;
        }
    }

    public class Dependency { }
}
