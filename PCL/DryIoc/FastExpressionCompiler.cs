namespace FastExpressionCompiler
{
    using System;
    using System.Linq.Expressions;

    /// <summary>Polyfill for absence of FastExpressionCompiler: https://github.com/dadhi/FastExpressionCompiler </summary>
    public static class ExpressionCompiler
    {
        internal static TDelegate TryCompile<TDelegate>(Expression bodyExpr,
            ParameterExpression[] paramExprs, Type[] paramTypes, Type returnType) where TDelegate : class
        {
            return null;
        }
    }
}
