using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue565_Is_ignoring_ReuseScoped_setting_expected_behaviour_when_also_set_to_openResolutionScope : ITest
    {
        public int Run()
        {
            TestScope_Zero();
            TestScope_One();
            TestScope_One_WithScopeNameOf();
            TestScope_Two();
            return 4;
        }

        [Test]
        public void TestScope_Zero()
        {
            /// This Test works and displays normal behaviour
            var container = new Container();
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<IBar, Bar>(reuse: Reuse.Scoped);
            container.Register<IFoo, Foo>(Reuse.Singleton);
            var car = container.Resolve<ICar>();

            // Get guid id for each class and guid store array
            var iCarId = (nameof(ICar), car.GetId());
            var iBarId = (nameof(IBar), car.GetBarId());
            var iFooId = (nameof(IFoo), car.GetFooId());
            var IdArray = car.GetIdArray();
            Assert.Multiple(() =>
            {
                // Assert that each name id pair matches with that stored
                // in the first three elements of the array
                Assert.That(IdArray[0], Is.EqualTo(iCarId));
                Assert.That(IdArray[1], Is.EqualTo(iBarId));
                Assert.That(IdArray[2], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar and IFoo have been resolved from container in ICar class.
                Assert.That(IdArray[3], Is.EqualTo(iCarId));
                Assert.That(IdArray[4], Is.EqualTo(iBarId));
                Assert.That(IdArray[5], Is.EqualTo(iFooId));
                // New Scope is opened in ICar. Check if ICar and IFoo is the Same
                // but IBar should be new ID. 
                Assert.That(IdArray[6], Is.EqualTo(iCarId));
                Assert.That(IdArray[7], !Is.EqualTo(iBarId));
                Assert.That(IdArray[8], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar should still be new ID. but should resolve twice
                // and be the same as the id from Array index 7
                Assert.That(IdArray[9], Is.EqualTo(iCarId));
                Assert.That(IdArray[10], !Is.EqualTo(iBarId));
                Assert.That(IdArray[10], Is.EqualTo(IdArray[7]));
                Assert.That(IdArray[11], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar Should match the original Ibar Id.
                Assert.That(IdArray[12], Is.EqualTo(iCarId));
                Assert.That(IdArray[13], Is.EqualTo(iBarId));
                Assert.That(IdArray[14], Is.EqualTo(iFooId));
            });
        }

        public sealed class AnyScopeExceptResolutionScopeOf<T> : IScopeName
        {
            public static readonly IScopeName Instance = new AnyScopeExceptResolutionScopeOf<T>();
            private AnyScopeExceptResolutionScopeOf() {}
            public bool Match(object scopeName) => 
                scopeName is not ResolutionScopeName rn || rn.ServiceType != typeof(T);
        }

        [Test]
        public void TestScope_One()
        {
            /// This Test Fails when IBar is registered with openResolutionScope: true.
            /// When iBar is Resolved it is resolved with a new instance each time.
            /// My expectation is that is is still scoped to which ever scope it was resolved to
            /// even though it has opened a new scope internally.
            var container = new Container();
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<IBar, Bar>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.ScopedTo(AnyScopeExceptResolutionScopeOf<IBar>.Instance));
            container.Register<IFoo, Foo>(Reuse.Singleton);
            container.Resolve<ICar>().GetId();
            var car = container.Resolve<ICar>();
            // Get guid id for each class and guid store array
            var iCarId = (nameof(ICar), car.GetId());
            var iBarId = (nameof(IBar), car.GetBarId());
            var iFooId = (nameof(IFoo), car.GetFooId());
            var IdArray = car.GetIdArray();
            Assert.Multiple(() =>
            {
                // Assert that each name id pair matches with that stored
                // in the first three elements of the array
                Assert.That(IdArray[0], Is.EqualTo(iCarId));
                Assert.That(IdArray[1], Is.EqualTo(iBarId));
                Assert.That(IdArray[2], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar and IFoo have been resolved from container in ICar class.
                // IBar should be the same: Fail!!
                Assert.That(IdArray[3], Is.EqualTo(iCarId));
                Assert.That(IdArray[4], Is.EqualTo(iBarId));
                Assert.That(IdArray[5], Is.EqualTo(iFooId));
                // New Scope is opened in ICar. Check if ICar and IFoo is the Same
                // but IBar should be new ID. 
                Assert.That(IdArray[6], Is.EqualTo(iCarId));
                Assert.That(IdArray[7], !Is.EqualTo(iBarId));
                Assert.That(IdArray[8], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar should still be new ID. but should resolve twice
                // and be the same as the id from Array index 7 : Fail!!
                Assert.That(IdArray[9], Is.EqualTo(iCarId));
                Assert.That(IdArray[10], !Is.EqualTo(iBarId));
                Assert.That(IdArray[10], Is.EqualTo(IdArray[7]));
                Assert.That(IdArray[11], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar Should match the original Ibar Id. Fail!!
                Assert.That(IdArray[12], Is.EqualTo(iCarId));
                Assert.That(IdArray[13], Is.EqualTo(iBarId));
                Assert.That(IdArray[14], Is.EqualTo(iFooId));
            });
        }

        [Test]
        public void TestScope_One_WithScopeNameOf()
        {
            /// This Test Fails when IBar is registered with openResolutionScope: true.
            /// When iBar is Resolved it is resolved with a new instance each time.
            /// My expectation is that is is still scoped to which ever scope it was resolved to
            /// even though it has opened a new scope internally.
            var container = new Container();
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);

            var anyScopeExceptBar = ScopeName.Of(n => n is not ResolutionScopeName rn || rn.ServiceType != typeof(IBar));
            container.Register<IBar, Bar>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.ScopedTo(anyScopeExceptBar));

            container.Register<IFoo, Foo>(Reuse.Singleton);
            container.Resolve<ICar>().GetId();
            var car = container.Resolve<ICar>();
            // Get guid id for each class and guid store array
            var iCarId = (nameof(ICar), car.GetId());
            var iBarId = (nameof(IBar), car.GetBarId());
            var iFooId = (nameof(IFoo), car.GetFooId());
            var IdArray = car.GetIdArray();
            Assert.Multiple(() =>
            {
                // Assert that each name id pair matches with that stored
                // in the first three elements of the array
                Assert.That(IdArray[0], Is.EqualTo(iCarId));
                Assert.That(IdArray[1], Is.EqualTo(iBarId));
                Assert.That(IdArray[2], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar and IFoo have been resolved from container in ICar class.
                // IBar should be the same: Fail!!
                Assert.That(IdArray[3], Is.EqualTo(iCarId));
                Assert.That(IdArray[4], Is.EqualTo(iBarId));
                Assert.That(IdArray[5], Is.EqualTo(iFooId));
                // New Scope is opened in ICar. Check if ICar and IFoo is the Same
                // but IBar should be new ID. 
                Assert.That(IdArray[6], Is.EqualTo(iCarId));
                Assert.That(IdArray[7], !Is.EqualTo(iBarId));
                Assert.That(IdArray[8], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar should still be new ID. but should resolve twice
                // and be the same as the id from Array index 7 : Fail!!
                Assert.That(IdArray[9], Is.EqualTo(iCarId));
                Assert.That(IdArray[10], !Is.EqualTo(iBarId));
                Assert.That(IdArray[10], Is.EqualTo(IdArray[7]));
                Assert.That(IdArray[11], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar Should match the original Ibar Id. Fail!!
                Assert.That(IdArray[12], Is.EqualTo(iCarId));
                Assert.That(IdArray[13], Is.EqualTo(iBarId));
                Assert.That(IdArray[14], Is.EqualTo(iFooId));
            });
        }

        [Test]
        public void TestScope_Two()
        {
            ///This Test Passes when we use wrapper mod to get same behaviour as TestScope_Zero but with openResolutionScope: true.
            var container = new Container();
            
            container.Register(typeof(WrapScopeInternally<>), reuse: Reuse.Scoped);
            container.Register<ICar, Car>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            container.Register<Bar>(setup: Setup.With(openResolutionScope: true), reuse: Reuse.Transient);
            
            container.Register<IBar>(made: Made.Of(r => ServiceInfo.Of<WrapScopeInternally<Bar>>(), f => f.Object()), reuse: Reuse.Transient);
            container.Register<IFoo, Foo>(Reuse.Singleton);

            var car = container.Resolve<ICar>();
            
            // Get guid id for each class and guid store array
            var iCarId = (nameof(ICar), car.GetId());
            var iBarId = (nameof(IBar), car.GetBarId());
            var iFooId = (nameof(IFoo), car.GetFooId());
            var IdArray = car.GetIdArray();
            Assert.Multiple(() =>
            {
                // Assert that each name id pair matches with that stored
                // in the first three elements of the array
                Assert.That(IdArray[0], Is.EqualTo(iCarId));
                Assert.That(IdArray[1], Is.EqualTo(iBarId));
                Assert.That(IdArray[2], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar and IFoo have been resolved from container in ICar class.
                // IBar should be the same : Success
                Assert.That(IdArray[3], Is.EqualTo(iCarId));
                Assert.That(IdArray[4], Is.EqualTo(iBarId));
                Assert.That(IdArray[5], Is.EqualTo(iFooId));
                // New Scope is opened in ICar. Check if ICar and IFoo is the Same
                // but IBar should be new ID. :Success
                Assert.That(IdArray[6], Is.EqualTo(iCarId));
                Assert.That(IdArray[7], !Is.EqualTo(iBarId));
                Assert.That(IdArray[8], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar should still be new ID. but should resolve twice
                // and be the same as the id from Array index 7 : Success
                Assert.That(IdArray[9], Is.EqualTo(iCarId));
                Assert.That(IdArray[10], !Is.EqualTo(iBarId));
                Assert.That(IdArray[10], Is.EqualTo(IdArray[7]));
                Assert.That(IdArray[11], Is.EqualTo(iFooId));
                // Assert that each name id pair matches with each again.
                // IBar Should match the original Ibar Id. : Success
                Assert.That(IdArray[12], Is.EqualTo(iCarId));
                Assert.That(IdArray[13], Is.EqualTo(iBarId));
                Assert.That(IdArray[14], Is.EqualTo(iFooId));
            });
        }
    }

    public interface IInternalWrapScope<out TSubObject>
    {
        public TSubObject Object();
    }

    public class WrapScopeInternally<TSubObject>
    {
        private readonly TSubObject _subObject;
        public WrapScopeInternally(TSubObject subObject) =>
            _subObject = subObject;
        public TSubObject Object() => _subObject;
    }

    public interface IFoo
    {
        public Guid GetId();
    }

    public interface IBar
    {
        public Guid GetId();
        public Guid GetFooId();
    }

    public interface ICar
    {
        public Guid GetId();
        public Guid GetBarId();
        public Guid GetFooId();
        public (string, Guid)[] GetIdArray();
    }


    public class Foo : IFoo
    {
        private readonly Guid _id;
        public Foo() => _id = Guid.NewGuid();
        public Guid GetId() => _id;
    }

    public class Bar : IBar
    {
        private readonly IFoo _foo;
        private readonly Guid _id;
        public Bar(IFoo foo)
        {
            _foo = foo;
            _id = Guid.NewGuid();
        }
        public Guid GetId() => _id;
        public Guid GetFooId() => _foo.GetId();
    }

    public class Car : ICar
    {
        private readonly Guid _id;
        private readonly (string, Guid)[] _guidArray;
        private readonly IBar _bar;
        public Car(IResolverContext container, IBar bar)
        {
            _id = Guid.NewGuid();
            _guidArray = new (string, Guid)[15];
            _bar = bar;
            /// Store id for each class five times
            /// ICar is this class and iFoo is singleton
            /// so both should always be the same id
            /// IBar is scoped so my expectiation is that is should produce 2 ids
            /// One in the main scope of ICar and the second one in the below new scope. 
            _guidArray[0] = (nameof(ICar), _id);
            _guidArray[1] = (nameof(IBar), _bar.GetId());
            _guidArray[2] = (nameof(IFoo), _bar.GetFooId());
            /// 2
            _guidArray[3] = (nameof(ICar), _id);
            _guidArray[4] = (nameof(IBar), container.Resolve<IBar>().GetId());
            _guidArray[5] = (nameof(IFoo), container.Resolve<IBar>().GetFooId());
            using (var scope = container.OpenScope())
            {
                //// 3 in new scope. IBar should have new id and be the same instance for each resolution.
                _guidArray[6] = (nameof(ICar), _id);
                _guidArray[7] = (nameof(IBar), scope.Resolve<IBar>().GetId());
                _guidArray[8] = (nameof(IFoo), scope.Resolve<IBar>().GetFooId());
                /// 4
                _guidArray[9] = (nameof(ICar), _id);
                _guidArray[10] = (nameof(IBar), scope.Resolve<IBar>().GetId());
                _guidArray[11] = (nameof(IFoo), scope.Resolve<IBar>().GetFooId());
            }
            /// 5 - IBar should be original instance scoped to ICar.
            _guidArray[12] = (nameof(ICar), _id);
            _guidArray[13] = (nameof(IBar), container.Resolve<IBar>().GetId());
            _guidArray[14] = (nameof(IFoo), container.Resolve<IBar>().GetFooId());
        }
        public Guid GetId() => _id;
        public Guid GetBarId() => _bar.GetId();
        public Guid GetFooId() => _bar.GetFooId();
        public (string, Guid)[] GetIdArray() => _guidArray;
    }
}
