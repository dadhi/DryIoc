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

        private static Func<object[], object> GenerateCreateADelegate()
        {
            var method = new DynamicMethod("CreateA", typeof(object), new[] { typeof(object[]) });
            var il = method.GetILGenerator();

            il.Emit(OpCodes.Newobj, typeof(B).GetConstructors()[0]);
            il.Emit(OpCodes.Ldarg_0);  
            il.Emit(OpCodes.Ldc_I4, 11);  
            il.Emit(OpCodes.Ldelem_Ref);
            il.Emit(OpCodes.Castclass, typeof(string));
            il.Emit(OpCodes.Newobj, typeof(A).GetConstructors()[0]);
            il.Emit(OpCodes.Ret);

            return (Func<object[], object>)method.CreateDelegate(typeof(Func<object[], object>));
        }

        private static Func<object[], object> CreateDelegateFromExpression(Expression expr)
        {
            var method = new DynamicMethod("CreateA", typeof(object), new[] { typeof(object[]) });
            var il = method.GetILGenerator();

            var visitor = new EmittingVisitor(il);
            visitor.Visit(expr);

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
        public A(B b, string s, IEnumerable<ID> ds) { }
    }

    public class B
    {
        public B() { }
    }

    public interface ID { }
    public class D1 : ID { }
    public class D2 : ID { }

    public sealed class EmittingVisitor : ExpressionVisitor
    {
        private readonly ILGenerator _il;

        public EmittingVisitor(ILGenerator il, StringBuilder log = null)
        {
            _il = il;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var elems = node.Expressions;
            var arrType = node.Type;
            var arrVar = _il.DeclareLocal(arrType);

            _il.Emit(OpCodes.Ldc_I4, elems.Count);
            Debug.WriteLine("Ldc_I4 " + elems.Count);
            _il.Emit(OpCodes.Newarr, arrType.GetElementType());
            Debug.WriteLine("Newarr " + arrType.GetElementType());
            _il.Emit(OpCodes.Stloc, arrVar);
            Debug.WriteLine("Stloc_0");

            for (var i = 0; i < elems.Count; i++)
            {
                _il.Emit(OpCodes.Ldloc, arrVar);
                Debug.WriteLine("Ldloc_0");

                _il.Emit(OpCodes.Ldc_I4, i); 
                Debug.WriteLine("Ldc_I4 " + i);

                Visit(elems[i]);                

                _il.Emit(OpCodes.Stelem_Ref);
                Debug.WriteLine("Stelem_Ref");
            }

            _il.Emit(OpCodes.Ldloc, arrVar);
            Debug.WriteLine("Ldloc_0");

            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _il.Emit(OpCodes.Ldarg_0);
            Debug.WriteLine("Ldarg_0");
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.VisitBinary(node);
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                _il.Emit(OpCodes.Ldelem_Ref);
                Debug.WriteLine("Ldelem_Ref");

            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            if (value is int)
            {
                _il.Emit(OpCodes.Ldc_I4, (int)value);
                Debug.WriteLine("Ldc_I4 " + value);
            }
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            base.VisitNew(node);

            _il.Emit(OpCodes.Newobj, node.Constructor);
            Debug.WriteLine("Newobj " + node.Constructor.DeclaringType);

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            base.VisitUnary(node);
            if (node.NodeType == ExpressionType.Convert)
            {
                _il.Emit(OpCodes.Castclass, node.Type);
                Debug.WriteLine("Castclass " + node.Type);
            }
            return node;
        }
    }
}
