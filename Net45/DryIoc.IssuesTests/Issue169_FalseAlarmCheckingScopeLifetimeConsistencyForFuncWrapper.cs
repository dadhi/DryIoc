using System;
using NUnit.Framework;
using ImTools;

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
    public class Issue169_FalseAlarmCheckingScopeLifetimeConsistencyForFuncWrapper
    {
        [Test]
        public void Test()
        {
            var ctr = new Container( Rules.Default.With(
                    propertiesAndFields: PropertiesAndFields.Auto ));

            ctr.Register<C>( reuse: Reuse.Singleton );

            var shortReuse = new ShortReuse();
            ctr.Register<B>( reuse: shortReuse );
            ctr.Register<D>( reuse: shortReuse );

            var c = ctr.Resolve<C>();
            var d = c.D();
            Assert.IsNotNull(d.B.Value);
        }
    }

    class B { }
    
    class D
    {
        public Lazy<B> B { get; set; }
    }

    class C
    {
        public Func<D> D { get; set; }
    }


    class ShortReuse: IReuse
    {
        public int Lifespan { get { return 50; } }

        public object Name { get { return null; } }

        public Expr Apply(Request request, Expr serviceFactoryExpr)
        {
            return Reuse.Singleton.Apply(request, serviceFactoryExpr);
        }

        public bool CanApply(Request request)
        {
            return true;
        }

        public Expr ToExpression(Func<object, Expr> fallbackConverter)
        {
            return New(GetType().GetConstructors()[0], ArrayTools.Empty<Expr>());
        }
    }
}
