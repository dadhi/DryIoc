using System;
using System.Collections.Generic;
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

        internal class Component : IComponent
        {
            public IArea Area1 { get; set; }
            public IArea Area2 { get; set; }

            public Component(IArea area1, IArea area2)
            {
                Area1 = area1;
                Area2 = area2;
            }
        }

        internal class Component2 : IComponent
        {
            public IArea Area1 { get; set; }
            public IArea Area2 { get; set; }

            public Component2(
                Func<ITwoVariants, IMainViewModel, IArea> getArea,
                KeyValuePair<Areas, ITwoVariants>[] variants,
                KeyValuePair<Areas, Func<ITwoVariants, IMainViewModel>>[] mainViewModels)
            {
                var firstVariant = variants.Single(v => v.Key == Areas.First).Value;
                Area1 = getArea(firstVariant, mainViewModels.Single(v => v.Key == Areas.First).Value(firstVariant));

                var secondVariant = variants.Single(v => v.Key == Areas.Second).Value;
                Area2 = getArea(secondVariant, mainViewModels.Single(v => v.Key == Areas.Second).Value(secondVariant));
            }
        }

        public interface IArea
        {
            IDatabase Database { get; set; }
            IMainViewModel MainViewModel { get; set; } // NOTE: changed from IMainViewModel1 to IMainViewModel, that was probably the original desire.
            ITwoVariants OneVariant { get; set; }
        }

        public class Area : IArea
        {
            public IDatabase Database { get; set; }
            public IMainViewModel MainViewModel { get; set; }
            public ITwoVariants OneVariant { get; set; }

            public Area(IDatabase database, IMainViewModel mainViewModel, ITwoVariants oneVariant)
            {
                Database = database;
                MainViewModel = mainViewModel;
                OneVariant = oneVariant;
            }
        }

        public interface IViewModelPresenter { }
        internal class ViewModelPresenter : IViewModelPresenter { }

        public interface IMainViewModel
        {
            IViewModelPresenter ViewModelPresenter { get; set; }
            IDatabase Database { get; set; }
            IChildViewModelSimple Simple { get; set; }
            IChildViewModelWithChildren WithChildren { get; set; }
            IChildViewModelSimple CreateDynamicChild();
            ITwoVariants OneVariant { get; set; }
        }

        internal class MainViewModel1 : IMainViewModel
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public ITwoVariants OneVariant { get; set; }
            public IChildViewModelSimple Simple { get; set; }
            public IChildViewModelWithChildren WithChildren { get; set; }
            public Func<IChildViewModelSimple> ChildResolver { get; set; }

            public MainViewModel1(
                IViewModelPresenter viewModelPresenter, 
                IDatabase database, 
                ITwoVariants oneVariant, 
                IChildViewModelSimple simple, 
                IChildViewModelWithChildren withChildren,
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
            IMainViewModel MainViewModel { get; }
        }

        internal class ChildViewModelWithMainViewModel : IChildViewModelWithMainViewModel
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public IMainViewModel MainViewModel { get { return _mainviewModel.Value; } }

            public ChildViewModelWithMainViewModel(IViewModelPresenter viewModelPresenter, IDatabase database, Lazy<IMainViewModel> mainviewModel)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
                _mainviewModel = mainviewModel;
            }

            private Lazy<IMainViewModel> _mainviewModel;
        }

        internal class MainViewModel2 : IMainViewModel
        {
            public IViewModelPresenter ViewModelPresenter { get; set; }
            public IDatabase Database { get; set; }
            public ITwoVariants OneVariant { get; set; }
            public IChildViewModelSimple Simple { get; set; }
            public IChildViewModelWithChildren WithChildren { get; set; }
            public Func<IChildViewModelSimple> ChildResolver { get; set; }

            public IChildViewModelSimple CreateDynamicChild()
            {
                return ChildResolver();
            }

            public MainViewModel2(
                IViewModelPresenter viewModelPresenter, 
                IDatabase database,
                ITwoVariants oneVariant, 
                IChildViewModelSimple simple,
                IChildViewModelWithChildren withChildren,
                Func<IChildViewModelSimple> childResolver)
            {
                ViewModelPresenter = viewModelPresenter;
                Database = database;
                OneVariant = oneVariant;
                Simple = simple;
                WithChildren = withChildren;
                ChildResolver = childResolver;
            }
        }

        internal enum Areas { First, Second }

        [Test]
        public void Can_register_complex_graph_bound_to_context_area_Using_Func_with_Args()
        {
            var container = new Container();

            container.Register<IDatabase, Database>(Reuse.InResolutionScope);

            container.Register<ITwoVariants, FirstVariant>(named: Areas.First);  // For Area1
            container.Register<ITwoVariants, SecondVariant>(named: Areas.Second); // For Area2

            container.Register<IMainViewModel, MainViewModel1>(named: Areas.First, reuse: Reuse.InResolutionScope);
            container.Register<IMainViewModel, MainViewModel2>(named: Areas.Second, reuse: Reuse.InResolutionScope);

            container.Register<IArea, Area>(setup: Setup.With(isDynamicDependency: true));  // Making Area a resolution root 

            container.Register<IComponent, Component2>();

            container.Register<IViewModelPresenter, ViewModelPresenter>(Reuse.InResolutionScope);
            container.Register<IChildViewModelSimple, ChildViewModelSimple>();
            container.Register<IChildViewModelWithChildren, ChildViewModelWithChildren>();
            container.Register<IChildViewModelWithMainViewModel, ChildViewModelWithMainViewModel>();

            var component = container.Resolve<IComponent>();

            // Database: Same for Area1 and Area2
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.Simple.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.Database, "Inside of area always the same database");
            //Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.Simple.Database, "Inside of area always the same database");
            Assert.AreNotSame(component.Area1.Database, component.Area2.Database, "Each area with own database");

            // ViewModelPresnter (LifestyleBoundToNearest): Same for Area1 and Area 2
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreNotSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area2.MainViewModel.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            //Assert.AreNotSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            //Assert.AreSame(component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");

            // Dynamic: Same for Area1 and Area2
            var child = component.Area1.MainViewModel.CreateDynamicChild();
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, child.ViewModelPresenter, "Also dynamic created objects should follow the normal rules");
            Assert.AreSame(component.Area1.Database, child.Database, "Also dynamic created objects should follow the normal rules");

            // Area1 should use FirstVariant as ITwoVariants, Area2 SecondVariant
            Assert.IsInstanceOf<FirstVariant>(component.Area1.OneVariant);
            Assert.IsInstanceOf<FirstVariant>(component.Area1.MainViewModel.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.MainViewModel.OneVariant);
        }

        public static Rules.FactorySelectorRule PreferParentKeyOverDefault = (request, factories) =>
        {
            if (request.ServiceKey != null) // if key is specified, do not handle it
                return factories.FirstOrDefault(f => f.Key.Equals(request.ServiceKey)).Value;

            var parentWithKey = request.Parent.Enumerate().FirstOrDefault(p => p.ServiceKey != null);
            if (parentWithKey != null) // try find service with the same key as parent.
                return factories.FirstOrDefault(f => f.Key.Equals(parentWithKey.ServiceKey)).Value
                    ?? factories.FirstOrDefault(f => f.Key.Equals(null)).Value; // if not found, fallback to default key

            return factories.FirstOrDefault(f => f.Key.Equals(null)).Value; // if no parent with key, fallback to default key
        };

        [Test]
        public void Can_register_complex_graph_bound_to_context_area_Using_key_based_type_selection()
        {
            var container = new Container(rules => rules.WithFactorySelector(PreferParentKeyOverDefault));

            container.Register<IDatabase, Database>(Reuse.InResolutionScope);

            container.Register<ITwoVariants, FirstVariant>(named: Areas.First);  // For Area1
            container.Register<ITwoVariants, SecondVariant>(named: Areas.Second); // For Area2

            container.Register<IMainViewModel, MainViewModel1>(named: Areas.First, reuse: Reuse.InResolutionScope);
            container.Register<IMainViewModel, MainViewModel2>(named: Areas.Second, reuse: Reuse.InResolutionScope);

            container.Register<IArea, Area>(named: Areas.First, setup: Setup.With(isDynamicDependency: true));  // Making area1, area2 resolution roots 
            container.Register<IArea, Area>(named: Areas.Second, setup: Setup.With(isDynamicDependency: true)); // to start their own object sub-graphs.

            container.Register<IComponent, Component>(with: Parameters.Of
                .Name("area1", serviceKey: Areas.First)
                .Name("area2", serviceKey: Areas.Second));

            container.Register<IViewModelPresenter, ViewModelPresenter>(Reuse.InResolutionScope);
            container.Register<IChildViewModelSimple, ChildViewModelSimple>();
            container.Register<IChildViewModelWithChildren, ChildViewModelWithChildren>();
            container.Register<IChildViewModelWithMainViewModel, ChildViewModelWithMainViewModel>();

            var component = container.Resolve<IComponent>();

            // Database: Same for Area1 and Area2
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.Simple.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.Database, "Inside of area always the same database");
            //Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.Database, "Inside of area always the same database");
            Assert.AreSame(component.Area1.Database, component.Area1.MainViewModel.WithChildren.Simple.Database, "Inside of area always the same database");
            Assert.AreNotSame(component.Area1.Database, component.Area2.Database, "Each area with own database");

            // ViewModelPresnter (LifestyleBoundToNearest): Same for Area1 and Area 2
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");
            Assert.AreNotSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area2.MainViewModel.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            //Assert.AreNotSame(component.Area1.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, "Each MainViewModel has own ViewModelPresenter");
            //Assert.AreSame(component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.ViewModelPresenter, component.Area1.MainViewModel.WithChildren.ChildWithMainViewModel.MainViewModel.Simple.ViewModelPresenter, "All ViewModelChildren shares with the owning MainViewModel same ViewModelPresenter");

            // Dynamic: Same for Area1 and Area2
            var child = component.Area1.MainViewModel.CreateDynamicChild();
            Assert.AreSame(component.Area1.MainViewModel.ViewModelPresenter, child.ViewModelPresenter, "Also dynamic created objects should follow the normal rules");
            Assert.AreSame(component.Area1.Database, child.Database, "Also dynamic created objects should follow the normal rules");

            // Area1 should use FirstVariant as ITwoVariants, Area2 SecondVariant
            Assert.IsInstanceOf<FirstVariant>(component.Area1.OneVariant);
            Assert.IsInstanceOf<FirstVariant>(component.Area1.MainViewModel.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.OneVariant);
            Assert.IsInstanceOf<SecondVariant>(component.Area2.MainViewModel.OneVariant);
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

        internal class AreaWithOneCar
        {
            public ICar Car { get; private set; }
            public SomeTool Tool { get; private set; }

            public AreaWithOneCar(ICar car, SomeTool tool)
            {
                Car = car;
                Tool = tool;
            }
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

            // isDynamicDependency: true means that service will be injected as: `r => new Client(r.Resolve<Service>())`
            // rather then inline creation expression (which is default): `r => new Client(new Service(...))`
            // Direct use of `Resolve` method means that dependency treated by container as new resolution root,
            // and therefore has its own ResolutionScope! So every dependency (e.g. ICar) down the resolve method 
            // registered with `Reuse.InResolutionScope` will reside in the new resolution scope. 
            container.Register<AreaWithOneCar>(
                setup: Setup.With(isDynamicDependency: true)); // NOTE: remove setup parameter to see what happens

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

        /// <summary>Mediator to use as resolution root instead of Area, and resolve Area wrapped in DryIoc.ResolutionScoped disposable wrapper.
        /// The wrapper is taking care to dispose corresponding ResolutionScope.</summary>
        internal class DisposableArea<TArea> : IDisposable
        {
            public TArea Area { get { return _area.Value; } }

            private readonly ResolutionScoped<TArea> _area;
            public DisposableArea(ResolutionScoped<TArea> area)
            {
                _area = area;
            }

            public void Dispose()
            {
                _area.Dispose();
            }
        }

        internal class CarefulAreaManager : IDisposable
        {
            public IEnumerable<AreaWithOneCar> OneCarAreas { get { return _disposableAreas.Select(x => x.Area); } }
            public ICar ReferenceCar { get; private set; }

            private readonly DisposableArea<AreaWithOneCar>[] _disposableAreas;
            public CarefulAreaManager(DisposableArea<AreaWithOneCar>[] disposableAreas, ICar referenceCar)
            {
                _disposableAreas = disposableAreas;
                ReferenceCar = referenceCar;
            }

            public void Dispose()
            {
                foreach (var disposableArea in _disposableAreas)
                    disposableArea.Dispose();
            }
        }

        [Test]
        public void Service_scoped_to_dynamic_dependency_could_be_disposed()
        {
            var container = new Container();

            container.Register<ICar, SmallCar>(Reuse.InResolutionScope);

            container.Register(typeof(DisposableArea<>), setup: Setup.With(isDynamicDependency: true));
            container.Register<AreaWithOneCar>();
            container.Register<SomeTool>();

            container.Register<CarefulAreaManager>();

            var manager = container.Resolve<CarefulAreaManager>();
            var area = manager.OneCarAreas.First();

            Assert.AreSame(area.Car, area.Tool.Car);
            Assert.AreNotSame(manager.ReferenceCar, area.Car);

            manager.Dispose();
            Assert.IsTrue(((SmallCar)area.Car).IsDisposed);
            Assert.IsTrue(((SmallCar)area.Tool.Car).IsDisposed);
            Assert.IsFalse(((SmallCar)manager.ReferenceCar).IsDisposed);
        }
    }
}
