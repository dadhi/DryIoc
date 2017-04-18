using System;
using System.Linq.Expressions;
using FastExpressionCompiler;
using NUnit.Framework;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FastExpressionCompilerTests
    {
        [Test]
        public void Expressions_with_small_long_casts_should_not_crash()
        {
            var x = 65535;
            var y = 65535;
            Assert.IsTrue(ExpressionCompiler.Compile(() => x == (long)y)());
        }

        [Test]
        public void Expressions_with_larger_long_casts_should_not_crash()
        {
            var y = 65536;
            var yn1 = y + 1;
            Assert.IsTrue(ExpressionCompiler.Compile(() => yn1 != (long)y)());
        }

        [Test]
        public void Expressions_with_long_constants_and_casts()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0L == (long)"x".Length)());
        }

        [Test]
        public void Expressions_with_ulong_constants_and_casts()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0UL == (ulong)"x".Length)());
        }

        [Test]
        public void Expressions_with_DateTime()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0 == DateTime.Now.Day)());
        }

        [Test]
        public void Expressions_with_DateTime_and_long_constant()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0L == (long)DateTime.Now.Day)());
        }

        [Test]
        public void Expressions_with_DateTime_and_ulong_constant()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0UL == (ulong)DateTime.Now.Day)());
        }

        [Test]
        public void Expressions_with_DateTime_and_uint_constant()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0u == (uint)DateTime.Now.Day)());
        }

        [Test]
        public void Expressions_with_max_uint_constant()
        {
            const uint maxuint = UInt32.MaxValue;
            Assert.IsFalse(maxuint == -1);
            Assert.IsFalse(ExpressionCompiler.Compile(() => maxuint == -1)());
        }

        [Test]
        public void Expressions_with_DateTime_and_double_constant()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => (double)DateTime.Now.Day == 0d)());
        }

        [Test]
        public void Expressions_with_DateTime_and_float_constant()
        {
            Assert.IsFalse(ExpressionCompiler.Compile(() => 0f == (float)DateTime.Now.Day)());
        }

        [Test]
        public void Expressions_with_char_and_int()
        {
            Assert.IsTrue(ExpressionCompiler.Compile(() => 'z' != 0)());
        }

        [Test]
        public void Expressions_with_char_and_short()
        {
            Assert.IsTrue(ExpressionCompiler.Compile(() => 'z' != (ushort)0)());
        }

        [Test]
        public void Given_hoisted_expr_with_closure_over_parameters_in_nested_lambda_should_work()
        {
            Expression<Func<object, object>> funcExpr = a =>
                new Func<object>(() =>
                new Func<object>(() => a)())();

            var func = funcExpr.Compile();

            var arg1 = new object();
            Assert.AreSame(arg1, func(arg1));

            var arg2 = new object();
            Assert.AreSame(arg2, func(arg2));

            var funcFec = ExpressionCompiler.TryCompile<Func<object, object>>(funcExpr);

            Assert.AreSame(arg1, funcFec(arg1));
            Assert.AreSame(arg2, funcFec(arg2));
        }

        [Test]
        public void Given_composed_expr_with_closure_over_parameters_in_nested_lambda_should_work()
        {
            var argExpr = Expression.Parameter(typeof(object));
            var funcExpr = Expression.Lambda(
                Expression.Invoke(Expression.Lambda(
                    Expression.Invoke(Expression.Lambda(argExpr)))),
                argExpr);

            var funcFec = ExpressionCompiler.TryCompile<Func<object, object>>(funcExpr);

            var arg1 = new object();
            Assert.AreSame(arg1, funcFec(arg1));

            var arg2 = new object();
            Assert.AreSame(arg2, funcFec(arg2));

            Assert.AreSame(arg1, funcFec(arg1));
        }

        [Test]
        public void Given_composed_expr_with_closure_over_parameters_used_in_2_levels_of_nested_lambda()
        {
            //Func<A, A> funcEthalon = a => a.Increment(() => a.Increment(() => a.Increment(null)));

            var aExpr = Expression.Parameter(typeof(A));
            var funcExpr = Expression.Lambda(
                Expression.Call(aExpr, "Increment", new Type[0],
                    Expression.Lambda(
                        Expression.Call(aExpr, "Increment", new Type[0],
                            Expression.Lambda(
                                Expression.Call(aExpr, "Increment", new Type[0],
                                    Expression.Constant(null, typeof(Func<A>))
                                )
                            )
                        )
                    )
                ),
                aExpr);

            var func = ExpressionCompiler.TryCompile<Func<A, A>>(funcExpr);

            var a1 = new A();
            var result1 = func(a1);
            Assert.AreEqual(3, result1.X);
            Assert.AreSame(a1, result1);

            var a2 = new A();
            Assert.AreSame(a2, func(a2));
        }

        [Test]
        public void Given_composed_expr_with_closure_over_2_parameters_used_in_2_levels_of_nested_lambda()
        {
            Func<A, A, A> funcEthalon = (a, b) => a.Increment(b, () => a.Increment(b, () => a.Increment(b, null)));
            var aa = new A();
            var bb = new A();
            funcEthalon(aa, bb);
            Assert.AreEqual(3, aa.X);
            Assert.AreEqual(-3, bb.X);

            var aExpr = Expression.Parameter(typeof(A));
            var bExpr = Expression.Parameter(typeof(A));

            var funcExpr = Expression.Lambda(
                Expression.Call(aExpr, "Increment", new Type[0],
                    bExpr,
                    Expression.Lambda(
                        Expression.Call(aExpr, "Increment", new Type[0],
                            bExpr,
                            Expression.Lambda(
                                Expression.Call(aExpr, "Increment", new Type[0],
                                    bExpr,
                                    Expression.Constant(null, typeof(Func<A>))
                                )
                            )
                        )
                    )
                ),
                aExpr, bExpr);

            var func = ExpressionCompiler.TryCompile<Func<A, A, A>>(funcExpr);

            var a1 = new A();
            var b1 = new A();
            var result1 = func(a1, b1);
            Assert.AreEqual(3, result1.X);
            Assert.AreSame(a1, result1);

            var a2 = new A();
            var b2 = new A();
            Assert.AreSame(a2, func(a2, b2));
        }

        public class A
        {
            public int X { get; private set; }

            public A Increment(Func<A> then)
            {
                X += 1;
                if (then == null)
                    return this;
                return then();
            }

            public A Increment(A b, Func<A> then)
            {
                X += 1;
                b.X -= 1;
                if (then == null)
                    return this;
                return then();
            }
        }
    }
}
