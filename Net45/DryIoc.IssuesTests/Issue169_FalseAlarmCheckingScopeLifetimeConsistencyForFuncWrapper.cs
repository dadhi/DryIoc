using System;
using System.Linq.Expressions;
using NUnit.Framework;
using ImTools;

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


    class ShortReuse: IReuse, IReuseV3
    {
        public int Lifespan { get { return 50; } }

        public Expression Apply(Request request, bool trackTransientDisposable, Expression createItemExpr)
        {
            return ((IReuseV3)Reuse.Singleton).Apply(request, trackTransientDisposable, createItemExpr);
        }

        public bool CanApply(Request request)
        {
            return true;
        }

        public Expression ToExpression(Func<object, Expression> fallbackConverter)
        {
            return Expression.New(GetType().GetConstructors()[0], ArrayTools.Empty<Expression>());
        }

        #region Obsolete

        public IScope GetScopeOrDefault(Request request)
        {
            throw new NotSupportedException();
        }

        public Expression GetScopeExpression(Request request)
        {
            throw new NotSupportedException();
        }

        public int GetScopedItemIdOrSelf(int factoryID, Request request)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
