using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using BenchmarkDotNet.Attributes;
using FastExpressionCompiler;

namespace Playground
{
    /// <summary>
    /// Results confirm that there is not difference.
    /// </summary>
    public class FECvsManualEmit
    {
        public class A { }
        public class B { }
        public class X
        {
            public A A { get; }
            public B B { get; }
            public X(A a, B b)
            {
                A = a;
                B = b;
            }
        }

        private static readonly Expression<Func<object>> _expr = () => new X(new A(), new B());

        [Benchmark]
        public object Fec()
        {
            return ExpressionCompiler.Compile<Func<object>>(_expr);
        }

        [Benchmark]
        public object Manually()
        {
            var method = new DynamicMethod(string.Empty, typeof(object), Type.EmptyTypes,
                typeof(FECvsManualEmit), skipVisibility: true);
            var il = method.GetILGenerator();

            var newX = (NewExpression)_expr.Body;
            var newXArgs = newX.Arguments;
            for (var i = 0; i < newXArgs.Count; i++)
            {
                var arg = newXArgs[i];
                var e = (NewExpression)arg;
                il.Emit(OpCodes.Newobj, e.Constructor);
            }
            il.Emit(OpCodes.Newobj, newX.Constructor);

            il.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof(Func<object>), null);
        }
    }
}
