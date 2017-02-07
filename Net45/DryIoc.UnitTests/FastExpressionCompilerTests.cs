using System;
using System.Linq.Expressions;
using ImTools;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FastExpressionCompilerTests
    {
        public static object CreateLambdaParamAndPassIt()
        {
            var y = new Y123();
            return AcceptLambdaParam(() => new X123(y));
        }

        public static object AcceptLambdaParam(Func<object> lambda)
        {
            return lambda();
        }

        public static Func<X123> CompileDelegate()
        {
            var y = new Y123();
            var expr = Expression.New(typeof(X123).GetConstructors()[0], new Expression[] { Expression.Constant(y) });
            var delgate = FastExpressionCompiler.TryCompile<Func<X123>>(expr, ArrayTools.Empty<ParameterExpression>(),
                ArrayTools.Empty<Type>(), typeof(X123));

            return delgate;
        }

        public static Func<X123> Use_compiled_delegate()
        {
            var delgate = CompileDelegate();
            return () => delgate();
        }

        [Test]
        public void Open_scope_and_resolve()
        {
            var container = new Container();

            container.Register<X123>(Reuse.InCurrentScope);
            container.Register<Y123>();

            using (var scope = container.OpenScope())
            {
                var x = scope.Resolve<X123>();
                Assert.IsInstanceOf<Y123>(x.Y);
            }
        }
    }

    public class X123
    {
        public readonly Y123 Y;

        public X123(Y123 y)
        {
            Y = y;
        }
    }

    public class Y123 { }
}
