using FastExpressionCompiler.LightExpression;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue173_Validate_method_throws_TypeInitializationException_for_OpenGenericTypeKey
    {
        [Test]
        public void Test()
        {
            var key = new OpenGenericTypeKey(GetType(), "the-key");

            var expr = key.ToExpression(k => Expression.Constant(k));

            Assert.IsNotNull(expr);
        }
    }
}