using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue565_Is_ignoring_ReuseScoped_setting_expected_behaviour_when_also_set_to_openResolutionScope : ITest
    {
        public int Run()
        {
            TestScope();
            TestScope2();
            TestScope4();
            return 2;
        }

        [Test]
        public void TestScope()
        {
            var container = new Container();
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<IBar, Bar>(reuse: Reuse.Scoped);
            container.Register<IFoo, Foo>(Reuse.Singleton);
            container.Resolve<ICar>().GetId();
        }

        [Test]
        public void TestScope2()
        {
            var container = new Container();
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<IBar, Bar>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Scoped);
            container.Register<IFoo, Foo>(Reuse.Singleton);
            container.Resolve<ICar>().GetId();
        }

        [Test]
        public void TestScope4()
        {
            var container = new Container();
            container.Register(typeof(WrapScopeInternally<>), reuse: Reuse.Scoped);
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<Bar>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<IBar>(made: Made.Of(r => ServiceInfo.Of<WrapScopeInternally<Bar>>(), f => f.SubObject), reuse: Reuse.Transient);
            container.Register<IFoo, Foo>(Reuse.Singleton);
            container.Resolve<ICar>().GetId();
        }

        public class WrapScopeInternally<TSubObject>
        {
            public readonly TSubObject SubObject;
            public WrapScopeInternally(TSubObject subObject) => SubObject = subObject;
        }

        public interface IFoo
        {
            public Guid GetId();
        }

        public interface IBar
        {
            public Guid GetId();
        }

        public interface ICar
        {
            public Guid GetId();
        }


        public class Foo : IFoo
        {
            private Guid _id;
            public Foo(IResolverContext container)
            {
                _id = Guid.NewGuid();
            }

            public Guid GetId()
            {
                return _id;
            }
        }

        public class Bar : IBar
        {
            private readonly IFoo _foo;
            private Guid _id;
            public Bar(IResolverContext container, IFoo foo)
            {
                _foo = foo;
                _id = Guid.NewGuid();
                Console.WriteLine($"Bar ID :{_id} IN Bar Constructor");
            }

            public Guid GetId()
            {
                Console.WriteLine($"Foo ID :{_foo.GetId()} IN Bar GetId()");
                return _id;
            }
        }

        public class Car : ICar
        {
            private readonly IBar _bar;
            private Guid _id;
            public Car(IResolverContext container, IBar bar)
            {
                _bar = bar;
                _id = Guid.NewGuid();

                Console.WriteLine($"Car ID 1 :{_id} IN Car (constructor)");
                Console.WriteLine($"Bar ID 1 :{_bar.GetId()} IN Car (constructor object)");
                Console.WriteLine($"Bar ID 2 :{container.Resolve<IBar>().GetId()} IN Car (resolved object)");
                Console.WriteLine($"Bar ID 3 :{container.Resolve<IBar>().GetId()} IN Car (resolved object)");

                using (var scope = container.OpenScope())
                {
                    Console.WriteLine($"Bar ID 4 :{scope.Resolve<IBar>().GetId()} IN Car New Scope");
                    Console.WriteLine($"Bar ID 5 :{scope.Resolve<IBar>().GetId()} IN Car New Scope");
                }
            }

            public Guid GetId()
            {
                Console.WriteLine($"Car ID 2 :{_id} IN Car GetId()");
                return _id;
            }
        }
    }
}
