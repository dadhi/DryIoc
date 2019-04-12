using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using FastExpressionCompiler.LightExpression;
using static FastExpressionCompiler.LightExpression.Expression;

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
            public int Lifespan => 10;

            public object Name => null;

            public Expression Apply(Request request, Expression serviceFactoryExpr) => serviceFactoryExpr;

            public bool CanApply(Request request) => true;

            public Expression ToExpression(Func<object, Expression> fallbackConverter) => 
                fallbackConverter("SpecialScopeName");
        }
    }
}
