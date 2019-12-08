#if !PCL && !NET35 && !NET40 && !NET403 && !NETSTANDARD1_0 && !NETSTANDARD1_1 && !NETSTANDARD1_2 && !NETCOREAPP1_0 && !NETCOREAPP1_1
#define SUPPORTS_FAST_EXPRESSION_COMPILER
#endif

#if SUPPORTS_FAST_EXPRESSION_COMPILER
using FastExpressionCompiler.LightExpression;
using static FastExpressionCompiler.LightExpression.Expression;
#else
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;
#endif

using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FEC_Reusing_lambda_param_in_nested_lambda_convert_to_sys_expr
    {
        [Test]
        public void Test()
        {
            var blah = Constant("blah");
            var fExp = Lambda<FactoryDelegate>(
                Invoke(Lambda<FactoryDelegate>(blah, FactoryDelegateCompiler.ResolverContextParamExpr),
                    FactoryDelegateCompiler.ResolverContextParamExpr),
                FactoryDelegateCompiler.ResolverContextParamExpr);

            var f = fExp.CompileFast(ifFastFailedReturnNull: true);
            Assert.AreEqual("blah", f(null));

            var expr = fExp.ToLambdaExpression();
            f = expr.Compile();
            Assert.AreEqual("blah", f(null)); }
    }
}
