using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using FastExpressionCompiler.LightExpression;
using static FastExpressionCompiler.LightExpression.Expression;

namespace DryIoc.IssuesTests.FastExpressionCompiler
{
    [TestFixture]
    public class NestedNestedLambdaTests
    {
        [Test]
        public void Outer_parameters_should_be_correctly_used_for_the_nested_lambda()
        {
            var rParam = Parameter(typeof(IResolverContext), "r");
            var sParam = Parameter(typeof(String), "_String0");

            var logTableManType = typeof(LogTableManagerConsumer2).GetTypeInfo();
            var rCtxType = typeof(IResolverContext).GetTypeInfo();
            var scopeType = typeof(IScope).GetTypeInfo();
            var body = MemberInit(
                New(logTableManType.DeclaredConstructors.First()),
                Bind(
                    logTableManType.GetDeclaredProperty(nameof(LogTableManagerConsumer2.GetLogTableManager)),
                    Lambda<Func<String, ILogTableManager>>(
                        Convert(Call(
                            Property(rParam, rCtxType.GetDeclaredProperty(nameof(IResolverContext.SingletonScope))),
                                scopeType.SingleMethod(nameof(IScope.GetOrAdd)),
                            Constant(174, typeof(int)),
                            Lambda<CreateScopedValue>(
                                Call(null, 
                                    typeof(LogTableManager).GetTypeInfo().GetDeclaredMethod(nameof(LogTableManager.Create)), sParam)),
                            Constant(0)), 
                            typeof(ILogTableManager)),
                        sParam)));

            var fExpr = Lambda<Func<IResolverContext, object>>(body, rParam);

            var f = fExpr.CompileFast(true);

            Assert.IsNotNull(f);
        }

        [Test]
        public void I_can_compile_the_Expression_with_invocation_of_nested_lambda()
        {
            var rParam = Parameter(typeof(IResolverContext), "r");

            var c = new C();

            var expr = Lambda<Func<IResolverContext, A>>(
                New(typeof(A).GetTypeInfo().DeclaredConstructors.First(),
                    Invoke(Lambda<Func<B>>(New(typeof(B).GetTypeInfo().DeclaredConstructors.First(), Constant(c)))),
                    Constant(c)),
                rParam);

            var f = expr.CompileFast(true);
            Assert.IsInstanceOf<A>(f(null));
        }


        // todo: WIP - prepare for compiling the expression with the nested lambdas
        //[Test, Ignore("todo: WIP - prepare for compiling the expression with the nested lambdas")]
        //public void I_can_compile_the_Expression_with_invocation_of_nested_lambda_with_pre_created_closure_constants()
        //{
        //    var rParam = Parameter(typeof(IResolverContext), "r");

        //    var c = new C();

        //    var expr = Lambda<Func<IResolverContext, A>>(
        //        New(typeof(A).GetTypeInfo().DeclaredConstructors.First(),
        //            Invoke(Lambda<Func<B>>(New(typeof(B).GetTypeInfo().DeclaredConstructors.First(), Constant(c)))),
        //            Constant(c)),
        //        rParam);

        //    var closureInfo = new ExpressionCompiler.ClosureInfo(ClosureStatus.UserProvided | ClosureStatus.HasClosure, closureConstants, constantUsage);

        //    var f = expr.TryCompileWithPreCreatedClosure<Func<IResolverContext, A>>(new object[] { c }, new int[]{ 1 });
        //    Assert.IsInstanceOf<A>(f(null));
        //}

        public class A
        {
            public B B { get; }
            public C C { get; }

            public A(B b, C c)
            {
                B = b;
                C = c;
            }
        }

        public class B
        {
            public C C { get; }

            public B(C c)
            {
                C = c;
            }
        }

        public class C
        {
        }

        public class LogTableManagerConsumer2
        {
            public Func<string, ILogTableManager> GetLogTableManager { get; set; }

            private ILogTableManager logTableManager;

            public ILogTableManager LogTableManager
            {
                get
                {
                    return logTableManager ?? (logTableManager = GetLogTableManager("SCHEMA2"));
                }
            }
        }

        public interface ILogTableManager
        {
            string TableName { get; }
        }

        public class LogTableManager : ILogTableManager
        {
            public const string FactoryMethodExportName = "LogTableManagerFactory";

            private LogTableManager(string schemaName) => TableName = $"{schemaName}.LOG_ENTRIES";

            public string TableName { get; private set; }

            public static ILogTableManager Create(string schemaName) => new LogTableManager(schemaName);
        }

    }
}
