using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using DryIoc;

namespace Playground
{
    public class Program
    {
        static void Main()
        {
            //var createA = GenerateCreateADelegate();
            //var state = new object[15];
            //state[11] = "x";
            //var a = createA(state);
            //Console.WriteLine(a);

            ExpressionVsEmit();

            Console.ReadKey();
        }

        private static object ExpressionVsEmit()
        {
            const int times = 3000;
            const int runTimes = 5000000;
            Func<object[], object> func = null;
            var funcExpr = CreateExpression();
            var state = new object[15];
            state[11] = "x";
            object result = null;

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                func = funcExpr.Compile();
            }
            stopwatch.Stop();
            Console.WriteLine("Expression Compile: " + stopwatch.ElapsedMilliseconds);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < runTimes; i++)
            {
                result = func(state);
            }
            stopwatch.Stop();
            Console.WriteLine("Expression Compiled Run: " + stopwatch.ElapsedMilliseconds);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                func = CreateDelegateFromExpression(funcExpr.Body);
            }
            stopwatch.Stop();
            Console.WriteLine("Expression Emit: " + stopwatch.ElapsedMilliseconds);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < runTimes; i++)
            {
                result = func(state);
            }
            stopwatch.Stop();
            Console.WriteLine("Expression Emit Run: " + stopwatch.ElapsedMilliseconds);

            return result;
        }

        private static Expression<Func<object[], object>> CreateExpression()
        {
            var stateParamExpr = Expression.Parameter(typeof(object[])); 

            var funcExpr = Expression.Lambda<Func<object[], object>>(
                Expression.New(typeof(A).GetConstructors()[0],
                    Expression.New(typeof(B).GetConstructors()[0], ArrayTools.Empty<Expression>()),
                    Expression.Convert(Expression.ArrayIndex(stateParamExpr, Expression.Constant(11)), typeof(string)),
                    Expression.NewArrayInit(typeof(ID), 
                        Expression.New(typeof(D1).GetConstructors()[0]),
                        Expression.New(typeof(D2).GetConstructors()[0]))),
                stateParamExpr);

            return funcExpr;
        }

        private static Func<object[], object> CreateDelegateFromExpression(Expression expr)
        {
            var method = new DynamicMethod("CreateA", typeof(object), new[] { typeof(object[]) });
            var il = method.GetILGenerator();

            EmittingVisitor.TryVisit(expr, il);
            il.Emit(OpCodes.Ret);

            return (Func<object[], object>)method.CreateDelegate(typeof(Func<object[], object>));
        }

        public object CreateA(object[] state)
        {
            return new A(new B(), (string)state[11], new ID[2] { new D1(), new D2() });
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
            if (expr == null)
                return false;

            switch (expr.NodeType)
            {
                case ExpressionType.Convert:
                    return VisitConvert((UnaryExpression)expr, il);
                case ExpressionType.ArrayIndex:
                    return VisitArrayIndex((BinaryExpression)expr, il);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expr, il);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expr, il);
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

        private static bool VisitParameter(ParameterExpression expr, ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_0);
            Debug.WriteLine("Ldarg_0");
            return true;
        }

        private static bool VisitConvert(UnaryExpression node, ILGenerator il)
        {
            var ok = TryVisit(node.Operand, il);
            if (ok)
            {
                il.Emit(OpCodes.Castclass, node.Type);
                Debug.WriteLine("Castclass " + node.Type);
            }
            return ok;
        }

        private static bool VisitConstant(ConstantExpression node, ILGenerator il)
        {
            var value = node.Value;
            var ok = value is int;
            if (ok)
            {
                il.Emit(OpCodes.Ldc_I4, (int)value);
                Debug.WriteLine("Ldc_I4 " + value);
            }
            return ok;
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
            var arrVar = il.DeclareLocal(arrType);

            il.Emit(OpCodes.Ldc_I4, elems.Count);
            Debug.WriteLine("Ldc_I4 " + elems.Count);
            il.Emit(OpCodes.Newarr, arrType.GetElementType());
            Debug.WriteLine("Newarr " + arrType.GetElementType());
            il.Emit(OpCodes.Stloc, arrVar);
            Debug.WriteLine("Stloc_0");

            var ok = true;
            for (int i = 0, n = elems.Count; i < n && ok; i++)
            {
                il.Emit(OpCodes.Ldloc, arrVar);
                Debug.WriteLine("Ldloc_0");

                il.Emit(OpCodes.Ldc_I4, i); 
                Debug.WriteLine("Ldc_I4 " + i);

                ok = TryVisit(elems[i], il);

                il.Emit(OpCodes.Stelem_Ref);
                Debug.WriteLine("Stelem_Ref");
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
            if (ok)
                ok = VisitBindingList(mi.Bindings, il);
            return ok;
        }

        private static bool VisitBindingList(IList<MemberBinding> bindings, ILGenerator il)
        {
            var ok = true;
            for (int i = 0, n = bindings.Count; i < n && ok; i++)
            {
                var binding = bindings[i];
                ok = binding.BindingType == MemberBindingType.Assignment 
                     && TryVisit(((MemberAssignment)binding).Expression, il);
            }
            return ok;
        }
    }

    public class D2 : ID { }
}
