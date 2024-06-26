using System;
using NUnit.Framework;
using DryIoc.ImTools;

using DryIoc.FastExpressionCompiler.LightExpression;
using static DryIoc.FastExpressionCompiler.LightExpression.Expression;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue169_FalseAlarmCheckingScopeLifetimeConsistencyForFuncWrapper : ITest
    {
        public int Run()
        {
            Test();
            return 1;
        }

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

        public Expression Apply(Request request, Expression serviceFactoryExpr)
        {
            return Reuse.Singleton.Apply(request, serviceFactoryExpr);
        }

        public bool CanApply(Request request)
        {
            return true;
        }

        public Expression ToExpression<S>(S state, Func<S, object, Expression> fallbackConverter)
        {
            return New(GetType().GetConstructors()[0], ArrayTools.Empty<Expression>());
        }
    }
}
