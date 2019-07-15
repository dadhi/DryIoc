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
            var r = Parameter(typeof(IResolverContext), "r");
            var s = Parameter(typeof(String), "_String0");

            var body = MemberInit(New(typeof(LogTableManagerConsumer2).GetTypeInfo().DeclaredConstructors.ToArray()[0],
                    new Expression[0]),
                Bind(
                    typeof(LogTableManagerConsumer2).GetTypeInfo().DeclaredMembers.ToArray()[4],
                    Lambda(typeof(Func<String, ILogTableManager>),
                        Convert(Call(Property(r,
                                typeof(IResolverContext).GetTypeInfo().DeclaredProperties.ToArray()[3]),
                            typeof(IScope).GetTypeInfo().DeclaredMethods.ToArray()[0],
                            Constant(174, typeof(Int32)),
                            Lambda(typeof(CreateScopedValue),
                                Call(null,
                                    typeof(LogTableManager).GetTypeInfo().DeclaredMethods.ToArray()[2],
                                    s),
                                new ParameterExpression[0]),
                            Constant(0, typeof(Int32))), typeof(ILogTableManager)),
                        s)));

            var fExpr = Lambda<Func<IResolverContext, object>>(body, r);

            var f = fExpr.CompileFast(true);

            Assert.IsNotNull(f);
        }

        public class LogTableManagerConsumer2
        {
            private Func<string, ILogTableManager> GetLogTableManager { get; set; }

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
