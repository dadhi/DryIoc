using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue512_Optimize_injection_of_IResolverContext : ITest
    {
        public int Run()
        {
            Test_1();
            Test_2();
            Test_3_decorator_of_IResolverContext();
            return 3;
        }

        [Test]
        public void Test_1()
        {
            var container = new Container(Rules.Default.WithTrackingDisposableTransients());

            container.Register<Kon>(Reuse.Scoped);

            var scope = container.OpenScope();

            var kon = scope.Resolve<Kon>();

            scope.Dispose();

            Assert.IsTrue(kon.Context.IsDisposed);
        }

        class Kon
        {
            public readonly IResolverContext Context;
            public Kon(IResolverContext context) => Context = context;
        }

        [Test]
        public void Test_2()
        {
            var container = new Container();

            container.Register<Foo>(Reuse.Singleton);
            container.Register<Bar>(Reuse.Scoped);

            var foo = container.Resolve<Foo>();

            Assert.IsNotNull(foo.Bar);
        }

        [Test]
        public void Test_3_decorator_of_IResolverContext()
        {
            var container = new Container();

            container.Register<Foo>(Reuse.Singleton);
            container.Register<Bar>(Reuse.Scoped);

            var wasThere = false;
            container.RegisterDelegate<IResolverContext, IResolverContext>(r => {
                wasThere = true;
                return r;
            }, 
            Reuse.Transient,
            setup: Setup.DecoratorWith(allowDisposableTransient: true));

            var foo = container.Resolve<Foo>();

            Assert.IsNotNull(foo.Bar);
            Assert.IsTrue(wasThere);
        }

        class Foo : IDisposable
        {
            public readonly Bar Bar;
            public readonly IResolverContext _scope;
            public Foo(IResolverContext r, Func<IResolverContext, Bar> getBar)
            {
                _scope = r.OpenScope();
                Bar = getBar(_scope);
            }

            public void Dispose() => _scope.Dispose();
        }

        class Bar { }
    }
}