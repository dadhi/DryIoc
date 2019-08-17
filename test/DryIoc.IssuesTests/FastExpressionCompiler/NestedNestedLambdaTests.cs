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
                                scopeType.SingleMethod("get_" + nameof(IScope.Parent)),
                            Constant(174, typeof(int)),
                                Lambda<CreateScopedValue>(
                                    Call(null, 
                                        typeof(LogTableManager).GetTypeInfo().GetDeclaredMethod(nameof(LogTableManager.Create)), sParam)),
                                        Constant(0, typeof(Int32))), 
                            typeof(ILogTableManager)),
                        sParam)));

            var fExpr = Lambda<Func<IResolverContext, object>>(body, rParam);

            var f = fExpr.CompileFast(true);

            Assert.IsNotNull(f);
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
