using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

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

            public Expression Apply(Request request, Expression serviceFactoryExpr)
            {
                return serviceFactoryExpr;
            }

            public bool CanApply(Request request)
            {
                return true;
            }

            public Expression ToExpression(Func<object, Expression> fallbackConverter)
            {
                return fallbackConverter("SpecialScopeName");
            }
        }
    }
}
