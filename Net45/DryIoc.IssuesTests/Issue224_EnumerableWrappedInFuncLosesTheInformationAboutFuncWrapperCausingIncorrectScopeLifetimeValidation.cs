using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
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
            public IScope Scope = new Scope();

            public int Lifespan { get { return 10; } }

            public int GetScopedItemIdOrSelf(int factoryID, Request request) { return factoryID; }

            public Expression GetScopeExpression(Request request)
            {
                return Expression.Field(Expression.Constant(this), "Scope");
            }

            public IScope GetScopeOrDefault(Request request) { return Scope; }
        }
    }
}
