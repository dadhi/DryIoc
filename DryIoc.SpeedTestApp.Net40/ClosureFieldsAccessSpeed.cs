using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.SpeedTestApp.Net40
{
    [TestFixture]
    public class ClosureFieldsAccessSpeed
    {
        public static void TestExpr()
        {
            var one = Expression.Lambda<Func<X>>(CreateOne(), null).Compile();
            var two = CreateTwo();
            var twoFromOne = Expression.Lambda<Func<X>>(CreateTwoFromOne(), null).Compile();

            var times = 1 * 1000 * 1000;
            Console.WriteLine("Testing {0:n0} times in for cycle: ", times);

            X x = null;

            GC.Collect();
            var timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = two();
            }
            timer.Stop();
            Console.WriteLine("Two took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = one();
            }
            timer.Stop();
            Console.WriteLine("One took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = twoFromOne();
            }
            timer.Stop();
            Console.WriteLine("TwoFromOne took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.KeepAlive(x);
        }

        private static Expression CreateOne()
        {
            var a = new A();
            var b = new B();
            var c = new C();

            var aExpr = Expression.Constant(a);
            var bExpr = Expression.Constant(b);
            var cExpr = Expression.Constant(c);

            var body =
                Expression.New(typeof(X).GetConstructors()[0],
                    aExpr, bExpr, cExpr,

                    Expression.New(typeof(A1).GetConstructors()[0], aExpr),
                    Expression.New(typeof(B1).GetConstructors()[0], bExpr),
                    Expression.New(typeof(C1).GetConstructors()[0], cExpr));
            return body;
        }

        private static Func<X> CreateTwo()
        {
            var a = new A();
            var b = new B();
            var c = new C();

            var aExpr = Expression.Variable(typeof(A));
            var bExpr = Expression.Variable(typeof(B));
            var cExpr = Expression.Variable(typeof(C));

            var block = Expression.Block(
                new[] { aExpr, bExpr, cExpr },

                Expression.Assign(aExpr, Expression.Constant(a)),
                Expression.Assign(bExpr, Expression.Constant(b)),
                Expression.Assign(cExpr, Expression.Constant(c)),

                Expression.New(typeof(X).GetConstructors()[0],
                    aExpr, bExpr, cExpr,

                    Expression.New(typeof(A1).GetConstructors()[0], aExpr),
                    Expression.New(typeof(B1).GetConstructors()[0], bExpr),
                    Expression.New(typeof(C1).GetConstructors()[0], cExpr)));

            var lambda = Expression.Lambda<Func<X>>(block, null);
            return lambda.Compile();
        }

        [Test]
        public static Expression CreateTwoFromOne()
        {
            var funcExpr = CreateOne();
            var visitor = new MoveConstantsToVariableVisitor();
            var funcExprRefactored = visitor.Visit(funcExpr);
            if (visitor.Vars.Count == 0)
                return funcExpr;

            var block = Expression.Block(
                visitor.Vars.Values,
                visitor.Vars.Select(item => Expression.Assign(item.Value, item.Key)).Concat(
                new[] { funcExprRefactored }));

            return block;
        }

    }

    class MoveConstantsToVariableVisitor : ExpressionVisitor
    {
        public Dictionary<ConstantExpression, ParameterExpression>
            Vars = new Dictionary<ConstantExpression, ParameterExpression>();

        protected override Expression VisitConstant(ConstantExpression constExpr)
        {
            ParameterExpression varExpr;
            if (!Vars.TryGetValue(constExpr, out varExpr))
                Vars.Add(constExpr, varExpr = Expression.Variable(constExpr.Type));
            return varExpr;
        }
    }

    class X
    {
        public A A { get; set; }
        public B B { get; set; }
        public C C { get; set; }

        public A1 A1 { get; set; }
        public B1 B1 { get; set; }
        public C1 C1 { get; set; }

        public X(A a, B b, C c, A1 a1, B1 b1, C1 c1)
        {
            A = a;
            B = b;
            C = c;

            A1 = a1;
            B1 = b1;
            C1 = c1;
        }
    }

    class A { }
    class B { }
    class C { }

    class A1
    {
        public A A { get; set; }

        public A1(A a)
        {
            A = a;
        }
    }

    class B1
    {
        public B B { get; set; }

        public B1(B b)
        {
            B = b;
        }
    }

    class C1
    {
        public C C { get; set; }

        public C1(C c)
        {
            C = c;
        }
    }
}
