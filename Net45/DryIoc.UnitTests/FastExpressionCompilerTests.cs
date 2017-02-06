using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FastExpressionCompilerTests
    {
        public static Func<X123> Examine_nested_lambda_il()
        {
            var y = new Y123();
            return () => new X123(y);
        }

        [Test]
        public void Open_scope_and_resolve()
        {
            var container = new Container();

            container.Register<X123>(Reuse.InCurrentScope);
            container.Register<Y123>();

            using (var scope = container.OpenScope())
                scope.Resolve<X123>();
        }
    }

    public class X123
    {
        private Y123 y;

        public X123(Y123 y)
        {
            this.y = y;
        }
    }

    public class Y123 { }
}
