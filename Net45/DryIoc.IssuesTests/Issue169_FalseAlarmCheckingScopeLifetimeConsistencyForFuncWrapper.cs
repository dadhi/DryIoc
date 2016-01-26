using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

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


    class ShortReuse : IReuse, IConvertibleToExpression
    {
        public int Lifespan { get { return 50; } }

        public IScope GetScopeOrDefault(Request request)
        {
            return request.Scopes.SingletonScope;
        }

        public Expression GetScopeExpression(Request request)
        {
            return Expression.Property(Container.ScopesExpr, "SingletonScope");
        }

        public int GetScopedItemIdOrSelf(int factoryID, Request request)
        {
            return request.Scopes.SingletonScope.GetScopedItemIdOrSelf(factoryID);
        }

        public Expression Convert()
        {
            return Expression.New(GetType().GetConstructors()[0], ArrayTools.Empty<Expression>());
        }
    }
}
