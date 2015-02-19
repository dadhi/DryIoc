using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue107_NamedScopesDependingOnResolvedTypes
    {
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
            public AreaWithOneCar OneCarArea { get { return _disposableArea.Area; } }
            public ICar ReferenceCar { get; private set; }

            private readonly DisposableArea<AreaWithOneCar> _disposableArea;
            public CarefulAreaManager(DisposableArea<AreaWithOneCar> disposableArea, ICar referenceCar)
            {
                _disposableArea = disposableArea;
                ReferenceCar = referenceCar;
            }

            public void Dispose()
            {
                _disposableArea.Dispose();
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
            var area = manager.OneCarArea;

            Assert.AreSame(area.Car, area.Tool.Car);
            Assert.AreNotSame(manager.ReferenceCar, area.Car);

            manager.Dispose();
            Assert.IsTrue(((SmallCar)area.Car).IsDisposed);
            Assert.IsTrue(((SmallCar)area.Tool.Car).IsDisposed);
            Assert.IsFalse(((SmallCar)manager.ReferenceCar).IsDisposed);
        }
    }
}
