using System;
using System.ComponentModel.Composition;
using DryIoc.MefAttributedModel;
using NUnit.Framework;
using DryIocAttributes;

#if FEC_EXPRESSION_INFO
using static FastExpressionCompiler.ExpressionInfo;
using Expr = FastExpressionCompiler.ExpressionInfo;
#else
using static System.Linq.Expressions.Expression;
using Expr = System.Linq.Expressions.Expression;
#endif

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue377_Support_custom_IReuse_with_MEF_attributes
    {
        [Test]
        public void Test()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(A), typeof(B));

            var a = container.Resolve<A>();

            Assert.AreSame(a, container.Resolve<A>());
        }

        [Export, Reuse(typeof(CustomReuse))]
        public class A { }

        /// <summary>Singleton with modified lifespan to pass the check.</summary>
        public class CustomReuse : IReuse
        {
            public static readonly CustomReuse Value = new CustomReuse();

            public int Lifespan { get { return 0; } }

            /// <inheritdoc />
            public object Name { get { return null; } }

            public Expr Apply(Request request, Expr serviceFactoryExpr)
            {
                return Reuse.Singleton.Apply(request, serviceFactoryExpr);
            }

            public bool CanApply(Request request)
            {
                return true;
            }

            private static Expr _valueExpr = Field(null, typeof(CustomReuse).Field(nameof(Value)));
            public Expr ToExpression(Func<object, Expr> fallbackConverter) => _valueExpr;
        }
    }
}
