using System;
using System.Linq;
using NUnit.Framework;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue107_NamedScopesDependingOnResolvedTypes
    {
        public interface ITwoVariants { }
        internal class FirstVariant : ITwoVariants { }
        internal class SecondVariant : ITwoVariants { }

        public interface IDatabase { }
        internal class Database : IDatabase { }

        public interface IComponent
        {
            IArea Area1 { get; set; }
            IArea Area2 { get; set; }
        }

        public class Component : IComponent
        {
            public IArea Area1 { get; set; }
            public IArea Area2 { get; set; }

            public Component(IArea1 area1, IArea2 area2)
            {
                Area1 = area1;
                Area2 = area2;
            }
        }

        public interface IArea
        {
            IDatabase Database { get; set; }
            IMainViewModel1 MainViewModel1 { get; set; }
            ITwoVariants OneVariant { get; set; }
        }

        public interface IArea1 : IArea
        {

        }

        public interface IArea2 : IArea
        {

        }

        public class Area
        {
            public IDatabase Database { get; set; }
            public IMainViewModel1 MainViewModel1 { get; set; }
            public ITwoVariants OneVariant { get; set; }

            public Area(IDatabase database, IMainViewModel1 mainViewModel1, ITwoVariants oneVariant)
            {
                Database = database;
                MainViewModel1 = mainViewModel1;
                OneVariant = oneVariant;
            }
        }

        public class Area1 : Area, IArea1
        {
            public Area1(IDatabase database, IMainViewModel1 mainViewModel1, ITwoVariants oneVariant)
                : base(database, mainViewModel1, oneVariant)
            {
            }
        }

        public class Area2 : Area, IArea2
        {
            public Area2(IDatabase database, IMainViewModel1 mainViewModel1, ITwoVariants oneVariant)
                : base(database, mainViewModel1, oneVariant)
            {
            }
        }

        public interface IViewModelPresenter { }
        internal class ViewModelPresenter : IViewModelPresenter { }

        public interface IMainViewModel { }

        public interface IMainViewModel1 : IMainViewModel
        {
            IViewModelPresenter ViewModelPresenter { get; set; }
            IDatabase Database { get; set; }
            IChildViewModelSimple Simple { get; set; }
            IChildViewModelWithChildren WithChildren { get; set; }
            IChildViewModelSimple CreateDynamicChild();
            ITwoVariants OneVariant { get; set; }
        }

        internal class MainViewModel1 : IMainViewModel1
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public ITwoVariants OneVariant { get; set; }
            public IChildViewModelSimple Simple { get; set; }
            public IChildViewModelWithChildren WithChildren { get; set; }
            public Func<IChildViewModelSimple> ChildResolver { get; set; }

            public MainViewModel1(IViewModelPresenter viewModelPresenter, IDatabase database, ITwoVariants oneVariant, IChildViewModelSimple simple, IChildViewModelWithChildren withChildren,
                Func<IChildViewModelSimple> childResolver)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
                OneVariant = oneVariant;
                Simple = simple;
                WithChildren = withChildren;
                ChildResolver = childResolver;
            }

            public IChildViewModelSimple CreateDynamicChild()
            {
                return ChildResolver();
            }
        }

        public interface IChildViewModelSimple
        {
            IViewModelPresenter ViewModelPresenter { get; set; }
            IDatabase Database { get; set; }
        }

        internal class ChildViewModelSimple : IChildViewModelSimple
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }

            public ChildViewModelSimple(IViewModelPresenter viewModelPresenter, IDatabase database)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
            }
        }

        public interface IChildViewModelWithChildren
        {
            IViewModelPresenter ViewModelPresenter { get; set; }
            IDatabase Database { get; set; }
            IChildViewModelSimple Simple { get; set; }
            IChildViewModelWithMainViewModel ChildWithMainViewModel { get; set; }
        }

        internal class ChildViewModelWithChildren : IChildViewModelWithChildren
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public IChildViewModelSimple Simple { get; set; }
            public IChildViewModelWithMainViewModel ChildWithMainViewModel { get; set; }

            public ChildViewModelWithChildren(IViewModelPresenter viewModelPresenter, IDatabase database, IChildViewModelSimple simple, IChildViewModelWithMainViewModel childWithMainViewModel)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
                Simple = simple;
                ChildWithMainViewModel = childWithMainViewModel;
            }
        }

        public interface IChildViewModelWithMainViewModel
        {
            IViewModelPresenter ViewModelPresenter { get; set; }
            IDatabase Database { get; set; }
            IMainViewModel2 MainViewModel { get; set; }
        }

        internal class ChildViewModelWithMainViewModel : IChildViewModelWithMainViewModel
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public IMainViewModel2 MainViewModel { get; set; }

            public ChildViewModelWithMainViewModel(IViewModelPresenter viewModelPresenter, IDatabase database, IMainViewModel2 mainViewModel)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
                MainViewModel = mainViewModel;
            }
        }

        public interface IMainViewModel2 : IMainViewModel
        {
            IViewModelPresenter ViewModelPresenter { get; set; }
            IDatabase Database { get; set; }
            IChildViewModelSimple Simple { get; set; }
        }

        internal class MainViewModel2 : IMainViewModel2
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public IChildViewModelSimple Simple { get; set; }

            public MainViewModel2(IViewModelPresenter viewModelPresenter, IDatabase database, IChildViewModelSimple simple)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
                Simple = simple;
            }
        }

        internal enum Areas { First, Second }

        [Test, Ignore]
        public void Can_reuse_and_locate_based_on_object_graph_itself()
        {
            var container = new Container();

            container.Register<IComponent, Component>();

            container.Register<IArea1, Area1>(setup: Setup.With(openResolutionScope: true));
            container.Register<IArea2, Area2>(setup: Setup.With(openResolutionScope: true));

            container.Register<IMainViewModel1, MainViewModel1>(
                setup: Setup.With(openResolutionScope: true));

            container.Register<IDatabase, Database>(
                Reuse.InResolutionScopeOf<IArea>());

            container.Register<ITwoVariants, FirstVariant>(
                Reuse.InResolutionScopeOf<IArea1>(),
                setup: Setup.With(openResolutionScope: true));
            
            container.Register<ITwoVariants, SecondVariant>(
                Reuse.InResolutionScopeOf<IArea2>(),
                setup: Setup.With(openResolutionScope: true));

            container.Register<IViewModelPresenter, ViewModelPresenter>(
                Reuse.InResolutionScopeOf<IMainViewModel>());

            container.Register<IChildViewModelSimple, ChildViewModelSimple>();
            container.Register<IChildViewModelWithChildren, ChildViewModelWithChildren>();
            container.Register<IChildViewModelWithMainViewModel, ChildViewModelWithMainViewModel>();

            container.Register<IMainViewModel2, MainViewModel2>(
                setup: Setup.With(openResolutionScope: true));

            var component = container.Resolve<IComponent>();

            // Database: Same in Area1 and Area2
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.Simple.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.Simple.Database, "Inside of area always the same database");
            Assert.AreNotSame(component.Area1.Database, component.Area2.Database, "Each area with own database");

            // ViewModelPrsenter (LifestyleBoundToNearest): Same in Area1 and Area 2
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreNotSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area2.MainViewModel1.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            Assert.AreNotSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");

            // Dynamic: Same in Area1 and Area2
            var child = component.Area1.MainViewModel1.CreateDynamicChild();
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, child.ViewModelPresenter, "Also dynamic created objects should follow the normal rules");
            Assert.AreSame(component.Area1.Database, child.Database, "Also dynamic created objects should follow the normal rules");

            // Area1 should use FirstVariant as ITwoVariants, Area2 SecondVariant
            Assert.IsInstanceOf<FirstVariant>(component.Area1.OneVariant);
            Assert.IsInstanceOf<FirstVariant>(component.Area1.MainViewModel1.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.MainViewModel1.OneVariant);
        }

        [Test]
        public void Can_reuse_based_on_object_graph_itself()
        {
            var container = new Container();

            container.Register<IComponent, Component>(
                made: Parameters.Of
                    .Name("area1", serviceKey: Areas.First)
                    .Name("area2", serviceKey: Areas.Second));

            container.Register<IArea1, Area1>(serviceKey: Areas.First,
                setup: Setup.With(openResolutionScope: true),
                made: Parameters.Of
                    .Type<ITwoVariants>(serviceKey: Areas.First)
                    .Type<IMainViewModel1>(serviceKey: Areas.First));

            container.Register<IArea2, Area2>(serviceKey: Areas.Second,
                setup: Setup.With(openResolutionScope: true),
                made: Parameters.Of
                    .Type<ITwoVariants>(serviceKey: Areas.Second)
                    .Type<IMainViewModel1>(serviceKey: Areas.Second));

            container.Register<IDatabase, Database>(Reuse.InResolutionScopeOf<IArea>());

            container.Register<IMainViewModel1, MainViewModel1>(serviceKey: Areas.First,
                setup: Setup.With(openResolutionScope: true),
                made: Parameters.Of.Type<ITwoVariants>(serviceKey: Areas.First));

            container.Register<IMainViewModel1, MainViewModel1>(serviceKey: Areas.Second,
                setup: Setup.With(openResolutionScope: true),
                made: Parameters.Of.Type<ITwoVariants>(serviceKey: Areas.Second));

            container.Register<ITwoVariants, FirstVariant>(serviceKey: Areas.First);
            container.Register<ITwoVariants, SecondVariant>(serviceKey: Areas.Second);

            container.Register<IViewModelPresenter, ViewModelPresenter>(Reuse.InResolutionScopeOf<IMainViewModel>());

            container.Register<IChildViewModelSimple, ChildViewModelSimple>();

            container.Register<IChildViewModelWithChildren, ChildViewModelWithChildren>();

            container.Register<IChildViewModelWithMainViewModel, ChildViewModelWithMainViewModel>();

            container.Register<IMainViewModel2, MainViewModel2>(
                setup: Setup.With(openResolutionScope: true));

            var component = container.Resolve<IComponent>();

            // Database: Same in Area1 and Area2
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.Simple.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel1.WithChildren.Simple.Database, "Inside of area always the same database");
            Assert.AreNotSame(component.Area1.Database, component.Area2.Database, "Each area with own database");

            // ViewModelPrsenter (LifestyleBoundToNearest): Same in Area1 and Area 2
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreNotSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area2.MainViewModel1.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            Assert.AreNotSame(component.Area1.MainViewModel1.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel1.WithChildren.ChildWithMainViewModel.MainViewModel.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");

            // Dynamic: Same in Area1 and Area2
            var child = component.Area1.MainViewModel1.CreateDynamicChild();
            Assert.AreSame(component.Area1.MainViewModel1.ViewModelPresenter, child.ViewModelPresenter, "Also dynamic created objects should follow the normal rules");
            Assert.AreSame(component.Area1.Database, child.Database, "Also dynamic created objects should follow the normal rules");

            // Area1 should use FirstVariant as ITwoVariants, Area2 SecondVariant
            Assert.IsInstanceOf<FirstVariant>(component.Area1.OneVariant);
            Assert.IsInstanceOf<FirstVariant>(component.Area1.MainViewModel1.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.MainViewModel1.OneVariant);
        }

        internal interface ICar { }
        internal class FastCar : ICar { }

        internal class SomeTool
        {
            public ICar Car { get; private set; }
            public SomeTool(ICar car)
            {
                Car = car;
            }
        }

        internal class AreaWithOneCar : IDisposable
        {
            public ICar Car { get; private set; }
            public SomeTool Tool { get; private set; }

            public AreaWithOneCar(ICar car, SomeTool tool, IDisposable scope)
            {
                _scope = scope;
                Car = car;
                Tool = tool;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }

            private readonly IDisposable _scope;
        }

        internal class AreaManager
        {
            public AreaWithOneCar[] OneCarAreas { get; private set; }
            public ICar ReferenceCar { get; private set; }

            public AreaManager(AreaWithOneCar[] oneCarAreas, ICar referenceCar)
            {
                OneCarAreas = oneCarAreas;
                ReferenceCar = referenceCar;
            }
        }

        [Test]
        public void Achievable_with_dynamic_dependency_and_resolution_scope()
        {
            var container = new Container();

            container.Register<ICar, FastCar>(Reuse.InResolutionScope);

            // isResolutionRoot: true means that service will be injected as: `r => new Client(r.Resolve<Service>())`
            // rather then inline creation expression (which is default): `r => new Client(new Service(...))`
            // Direct use of `Resolve` method means that dependency treated by container as new resolution root,
            // and therefore has its own ResolutionScope! So every dependency (e.g. ICar) down the resolve method 
            // registered with `Reuse.InResolutionScope` will reside in the new resolution scope. 
            container.Register<AreaWithOneCar>(
                setup: Setup.With(openResolutionScope: true)); // NOTE: remove setup parameter to see what happens

            container.Register<SomeTool>();

            container.Register<AreaManager>();

            var manager = container.Resolve<AreaManager>();
            var area = manager.OneCarAreas[0];

            Assert.AreSame(area.Car, area.Tool.Car);
            Assert.AreNotSame(manager.ReferenceCar, area.Car);
        }

        internal class SmallCar : ICar, IDisposable
        {
            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        internal class CarefulAreaManager : IDisposable
        {
            public AreaWithOneCar[] Areas { get; private set; }
            public ICar ReferenceCar { get; private set; }

            public CarefulAreaManager(AreaWithOneCar[] areas, ICar referenceCar)
            {
                Areas = areas;
                ReferenceCar = referenceCar;
            }

            public void Dispose()
            {
                foreach (var area in Areas)
                    area.Dispose();
            }
        }

        [Test]
        public void Service_scoped_to_dynamic_dependency_could_be_disposed()
        {
            var container = new Container();

            container.Register<ICar, SmallCar>(Reuse.InResolutionScope);
            container.Register<AreaWithOneCar>(setup: Setup.With(openResolutionScope: true));
            container.Register<SomeTool>();
            container.Register<CarefulAreaManager>();

            var manager = container.Resolve<CarefulAreaManager>();
            var area = manager.Areas.First();

            Assert.AreSame(area.Car, area.Tool.Car);
            Assert.AreNotSame(manager.ReferenceCar, area.Car);

            manager.Dispose();
            Assert.IsTrue(((SmallCar)area.Car).IsDisposed);
            Assert.IsTrue(((SmallCar)area.Tool.Car).IsDisposed);
            Assert.IsFalse(((SmallCar)manager.ReferenceCar).IsDisposed);
        }

        [Test]
        public void SingletonWithSeveralInterfaces_ResolveEachInterface_EachResolveReturnsSameInstance()
        {
            var container = new Container();

            container.RegisterMany<SingletonWithTwoInterfaces>(Reuse.Singleton);

            var singletonViaFirstInterface = container.Resolve<ISingletonFirstInterface>();
            var singletonViaSecondInterface = container.Resolve<ISingletonSecondInterface>();

            Assert.AreSame(singletonViaFirstInterface, singletonViaSecondInterface, "Singleton resolved via independent interfaces shall still a Singleton");
        }

        public interface ISingletonFirstInterface {}

        public interface ISingletonSecondInterface {}

        public interface ISingletonCombinedInterface : ISingletonFirstInterface, ISingletonSecondInterface { }

        public class SingletonWithTwoInterfaces : ISingletonFirstInterface, ISingletonSecondInterface, ISingletonCombinedInterface {}

        [Test]
        public void Can_inject_property_of_larger_interface_without_register_delegate()
        {
            var container = new Container();

            container.Register<IBigInterfaceWhichProvidesSmallerInterfaces, BigInterfaceWhichProvidesSmallerInterfaces>(Reuse.Singleton);

            container.Register(Made.Of(
                r => ServiceInfo.Of<IBigInterfaceWhichProvidesSmallerInterfaces>(),
                b => b.Small));

            var smallInterface = container.Resolve<ISmallInterface>();
            Assert.NotNull(smallInterface);
        }

        public interface ISmallInterface { }

        public interface IBigInterfaceWhichProvidesSmallerInterfaces
        {
           ISmallInterface Small { get; }
        }

        internal class BigInterfaceWhichProvidesSmallerInterfaces : IBigInterfaceWhichProvidesSmallerInterfaces
        {
            public ISmallInterface Small { get; private set; }

            public BigInterfaceWhichProvidesSmallerInterfaces()
            {
                Small = new LimitedFunctional();
            }

            class LimitedFunctional : ISmallInterface { }
        }
    }
}
