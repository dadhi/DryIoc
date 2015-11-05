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

        public delegate object Creator(object[] state);

        private static object ExpressionVsEmit()
        {
            const int times = 3000;
            const int runTimes = 5000000;
            Func<object[], object> func = null;
            var funcExpr = CreateATreeExpr();
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

        private static Expression<Func<object[], object>> CreateATreeExpr()
        {
            var stateParamExpr = Expression.Parameter(typeof(object[])); 

            var funcExpr = Expression.Lambda<Func<object[], object>>(
                Expression.New(typeof(A).GetConstructors()[0],
                    Expression.New(typeof(B).GetConstructors()[0], ArrayTools.Empty<Expression>()),
                    Expression.Convert(Expression.ArrayIndex(stateParamExpr, Expression.Constant(11)), typeof(string))),
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
            var sb = new StringBuilder();

            var visitor = new EmittingVisitor(il);
            visitor.Visit(expr);

            il.Emit(OpCodes.Ret);

            var s = sb.ToString();
            return (Func<object[], object>)method.CreateDelegate(typeof(Func<object[], object>));
        }

        public object CreateA(object[] state)
        {
            return new A(new B(), (string)state[11]);
        }

        private static string ForeachOfArrayVsCustomEnumerable()
        {
            var array = new[] { "a", "b", "c", "d", "e" };
            var result  = " ";

            const int times = 5000000;

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                foreach (var item in array)
                {
                    result = item;
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Array: " + stopwatch.ElapsedMilliseconds);

            var items = Items.Of("a", Items.Of("a", Items.Of("c", Items.Of("d", Items.Of("e", null)))));

            stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < times; i++)
            {
                foreach (var item in items)
                {
                    result = item;
                }
            }
            stopwatch.Stop();
            Console.WriteLine("Array: " + stopwatch.ElapsedMilliseconds);

            return result;
        }
    }

    public class A
    {
        public A(B b, string s) { }
    }

    public class B
    {
        public B() { }
    }

    public class EmittingVisitor : ExpressionVisitor
    {
        private readonly ILGenerator _il;

        public EmittingVisitor(ILGenerator il, StringBuilder log = null)
        {
            _il = il;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _il.Emit(OpCodes.Ldarg_0);
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.VisitBinary(node);
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                _il.Emit(OpCodes.Ldelem_Ref);
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            if (value is int)
            {
                _il.Emit(OpCodes.Ldc_I4, (int)value);
            }
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            base.VisitNew(node);
            _il.Emit(OpCodes.Newobj, node.Constructor);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            base.VisitUnary(node);
            if (node.NodeType == ExpressionType.Convert)
            {
                _il.Emit(OpCodes.Castclass, node.Type);
            }
            return node;
        }
    }

    public static class Items
    {
        public static Items<T> Of<T>(T item, Items<T> next)
        {
            return new Items<T>(item, next);
        }
    }

    public sealed class Items<T> : IEnumerable<T>
    {
        public readonly T Item;
        public readonly Items<T> Next;
        private Enumerator _enumerator;

        public Items(T item, Items<T> next)
        {
            Item = item;
            Next = next;
            _enumerator = new Enumerator(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerator;
        }

        private sealed class Enumerator : IEnumerator<T>
        {
            private Items<T> _items;

            public T Current
            {
                get { return _items.Item; }
            }

            public Enumerator(Items<T> items)
            {
                _items = items;
            }

            public bool MoveNext()
            {
                return (_items = _items.Next) != null;
            }

            public void Reset()
            {
            }

            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
