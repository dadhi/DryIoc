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


    class ShortReuse: IReuse
    {
        public int Lifespan { get { return 50; } }

        public object Name { get { return null; } }

        public Expression Apply(Request request, bool trackTransientDisposable, Expression serviceFactoryExpr)
        {
            return Reuse.Singleton.Apply(request, trackTransientDisposable, serviceFactoryExpr);
        }

        public bool CanApply(Request request)
        {
            return true;
        }

        public Expression ToExpression(Func<object, Expression> fallbackConverter)
        {
            return Expression.New(GetType().GetConstructors()[0], ArrayTools.Empty<Expression>());
        }
    }
}
