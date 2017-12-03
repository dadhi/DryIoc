using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

#if FEC_EXPRESSION_INFO
using Expr = FastExpressionCompiler.ExpressionInfo;
#else
using Expr = System.Linq.Expressions.Expression;
#endif

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue224_EnumerableWrappedInFuncLosesTheInformationAboutFuncWrapperCausingIncorrectScopeLifetimeValidation
    {
        [Test]
        public void Test()
        {
            var ctr = new Container(Rules.Default
             .With(propertiesAndFields: req => req.ImplementationType.GetProperties().Select(PropertyOrFieldServiceInfo.Of))
             .WithResolveIEnumerableAsLazyEnumerable());

            var shortReuse = new ShortReuse();

            ctr.Register<Y>(Reuse.Singleton);
            ctr.Register<IX>(shortReuse, Made.Of(() => new X()));

            var y = ctr.Resolve<Y>();
            var o = y.Xs().FirstOrDefault(); // Exception here
        }

        interface IX { }

        class X : IX { }

        class Y
        {
            public Func<IEnumerable<IX>> Xs { get; set; }
        }

        class ShortReuse : IReuse
        {
            public int Lifespan { get { return 10; } }

            public object Name { get { return null; } }

            public Expr Apply(Request request, Expr serviceFactoryExpr)
            {
                return serviceFactoryExpr;
            }

            public bool CanApply(Request request)
            {
                return true;
            }

            public Expr ToExpression(Func<object, Expr> fallbackConverter)
            {
                return fallbackConverter("SpecialScopeName");
            }
        }
    }
}
