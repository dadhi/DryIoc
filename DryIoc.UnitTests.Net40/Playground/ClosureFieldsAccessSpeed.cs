using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;

namespace DryIoc.UnitTests.Net40.Playground
{
    [TestFixture]
    [Ignore]
    public class ClosureFieldsAccessSpeed
    {
        public static void TestExpr()
        {
            var consts = new object[]
            {
                new A(),
                new B(),
                new C()
            };

            var one = Expression.Lambda<Func<X>>(CreateOne(), null).Compile();
            var two = CreateTwo();
            var twoFromOne = Expression.Lambda<Func<X>>(CreateTwoFromOne(), null).Compile();
            var fromArray = CreateFromArray();
            var fromArrayCompiledToMethod = CreateFromArrayCompiledToMethod();
            var fromArrayTwo = CreateFromArrayTwo();

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

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = fromArray(consts);
            }
            timer.Stop();
            Console.WriteLine("FromArray took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = fromArrayTwo(consts);
            }
            timer.Stop();
            Console.WriteLine("FromArrayTwo took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

            GC.Collect();
            timer = Stopwatch.StartNew();
            for (var t = 0; t < times; t++)
            {
                x = fromArrayCompiledToMethod(consts);
            }
            timer.Stop();
            Console.WriteLine("FromArrayCompiledToMethod took {0} milliseconds to complete.", timer.ElapsedMilliseconds);

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

        [Test]
        public static Func<object[], X> CreateFromArray()
        {
            var arrayExpr = Expression.Parameter(typeof(object[]));

            var aExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(0)), typeof(A));
            var bExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(1)), typeof(B));
            var cExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(2)), typeof(C));

            var newExpr =
                Expression.New(typeof(X).GetConstructors()[0],
                    aExpr, bExpr, cExpr,
                    Expression.New(typeof(A1).GetConstructors()[0], aExpr),
                    Expression.New(typeof(B1).GetConstructors()[0], bExpr),
                    Expression.New(typeof(C1).GetConstructors()[0], cExpr));

            var funcExpr = Expression.Lambda<Func<object[], X>>(newExpr, arrayExpr);
            var func = funcExpr.Compile();
            return func;
        }

        [Test]
        public static Func<object[], X> CreateFromArrayCompiledToMethod()
        {
            var arrayExpr = Expression.Parameter(typeof(object[]));

            var aExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(0)), typeof(A));
            var bExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(1)), typeof(B));
            var cExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(2)), typeof(C));

            var newExpr =
                Expression.New(typeof(X).GetConstructors()[0],
                    aExpr, bExpr, cExpr,
                    Expression.New(typeof(A1).GetConstructors()[0], aExpr),
                    Expression.New(typeof(B1).GetConstructors()[0], bExpr),
                    Expression.New(typeof(C1).GetConstructors()[0], cExpr));

            var funcExpr = Expression.Lambda<Func<object[], X>>(newExpr, arrayExpr);

            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("assembly"), AssemblyBuilderAccess.Run);
            var mod = ab.DefineDynamicModule("module");
            var tb = mod.DefineType("type", TypeAttributes.Public);
            var mb = tb.DefineMethod("test3", MethodAttributes.Public | MethodAttributes.Static, typeof(X), new[] { typeof(object[]) });
            funcExpr.CompileToMethod(mb);
            var t = tb.CreateType();
            var func = (Func<object[], X>)Delegate.CreateDelegate(typeof(Func<object[], X>), t.GetMethod("test3"));
            return func;
        }

        [Test]
        public static Func<object[], X> CreateFromArrayCompiledToMethod2()
        {
            var arrayExpr = Expression.Parameter(typeof(object[]));

            var aExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(0)), typeof(A));
            var bExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(1)), typeof(B));
            var cExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(2)), typeof(C));

            var newExpr =
                Expression.New(typeof(X).GetConstructors()[0],
                    aExpr, bExpr, cExpr,
                    Expression.New(typeof(A1).GetConstructors()[0], aExpr),
                    Expression.New(typeof(B1).GetConstructors()[0], bExpr),
                    Expression.New(typeof(C1).GetConstructors()[0], cExpr));

            var funcExpr = Expression.Lambda<Func<object[], X>>(newExpr, arrayExpr);

            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("assembly"), AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("module");

            var typeBuilder = moduleBuilder.DefineType("type1", TypeAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod("test", MethodAttributes.Public | MethodAttributes.Static, 
                typeof(X), new[] { typeof(object[]) });
            funcExpr.CompileToMethod(methodBuilder);
            var createdType = typeBuilder.CreateType();
            var func = (Func<object[], X>)Delegate.CreateDelegate(typeof(Func<object[], X>), createdType.GetMethod("test"));

            var typeBuilder2 = moduleBuilder.DefineType("type2", TypeAttributes.Public);
            var methodBuilder2 = typeBuilder2.DefineMethod("test", MethodAttributes.Public | MethodAttributes.Static, 
                typeof(X), new[] { typeof(object[]) });
            funcExpr.CompileToMethod(methodBuilder2);
            var t2 = typeBuilder2.CreateType();
            var func2 = (Func<object[], X>)Delegate.CreateDelegate(typeof(Func<object[], X>), t2.GetMethod("test"));

            return func + func2;
        }

        private static DynamicMethod CreateDynamicMethod()
        {
            var dynamicMethod = new DynamicMethod(
                "DynamicMethod", typeof(object), new[] { typeof(object[]) }, typeof(ClosureFieldsAccessSpeed).Module, true);

            return dynamicMethod;
        }

        [Test]
        public static Func<object[], X> CreateFromArrayTwo()
        {
            var arrayExpr = Expression.Parameter(typeof(object[]));

            var aExpr = Expression.Variable(typeof(A));
            var bExpr = Expression.Variable(typeof(B));
            var cExpr = Expression.Variable(typeof(C));

            var aValExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(0)), typeof(A));
            var bValExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(1)), typeof(B));
            var cValExpr = Expression.Convert(Expression.ArrayIndex(arrayExpr, Expression.Constant(2)), typeof(C));

            var newExpr =
                Expression.Block(
                    new[] { aExpr, bExpr, cExpr },

                    Expression.Assign(aExpr, aValExpr),
                    Expression.Assign(bExpr, bValExpr),
                    Expression.Assign(cExpr, cValExpr),

                    Expression.New(typeof(X).GetConstructors()[0],
                        aExpr, bExpr, cExpr,
                        Expression.New(typeof(A1).GetConstructors()[0], aExpr),
                        Expression.New(typeof(B1).GetConstructors()[0], bExpr),
                        Expression.New(typeof(C1).GetConstructors()[0], cExpr)));

            var funcExpr = Expression.Lambda<Func<object[], X>>(newExpr, arrayExpr);
            var func = funcExpr.Compile();
            return func;
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

    public class X
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

    public class A { }

    public class B { }

    public class C { }

    public class A1
    {
        public A A { get; set; }

        public A1(A a)
        {
            A = a;
        }
    }

    public class B1
    {
        public B B { get; set; }

        public B1(B b)
        {
            B = b;
        }
    }

    public class C1
    {
        public C C { get; set; }

        public C1(C c)
        {
            C = c;
        }
    }
}
