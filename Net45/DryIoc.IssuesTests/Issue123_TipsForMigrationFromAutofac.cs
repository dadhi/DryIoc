using System;
using System.Linq;
using Autofac;
using Autofac.Features.OwnedInstances;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue123_TipsForMigrationFromAutofac
    {
        [Test]
        public void How_to_get_all_registrations_in_registration_order()
        {
            var container = new Container();
            container.Register<IFoo, FooBar>();
            container.Register<IBar, FooBar>(serviceKey: 1);
            container.Register<IBar, FooBar>();

            var regsInOrder = container.GetServiceRegistrations()
                .OrderBy(factory => factory.FactoryRegistrationOrder)
                .ToArray();

            Assert.AreEqual(null, regsInOrder[0].OptionalServiceKey);
            Assert.AreEqual(1, regsInOrder[1].OptionalServiceKey);
            Assert.AreEqual(DefaultKey.Value, regsInOrder[2].OptionalServiceKey);
        }

        public abstract class Metadata
        {
            public class AutoActivated : Metadata
            {
                public static readonly AutoActivated It = new AutoActivated();
            }
        }

        [Test]
        public void Auto_activated_with_metadata()
        {
            var container = new Container();
            container.Register<ISpecific, Foo>(setup: Setup.With(metadata: Metadata.AutoActivated.It));
            container.Register<INormal, Bar>();

            var registrations = container.GetServiceRegistrations()
                .Where(r => r.Factory.Setup.Metadata is Metadata.AutoActivated)
                .OrderBy(r => r.FactoryRegistrationOrder)
                .GroupBy(r => r.FactoryRegistrationOrder, (f, r) => r.First())
                .Select(r => container.Resolve(r.ServiceType, r.OptionalServiceKey));
            
            Assert.IsInstanceOf<ISpecific>(registrations.First());
        }

        public interface ISpecific { }
        public interface INormal { }

        public class Foo : ISpecific { }
        public class Bar : INormal { }

        [Test]
        public void AsImplementedInterfaces()
        {
            var container = new Container();

            container.RegisterMany<FooBar>(serviceTypeCondition: type => type.IsInterface);

            Assert.NotNull(container.Resolve<IFoo>());
            Assert.NotNull(container.Resolve<IBar>());
            
            Assert.Null(container.Resolve<FooBar>(IfUnresolved.ReturnDefault));
        }

        [Test]
        public void IDisposable_should_be_excluded_from_register_many()
        {
            var container = new Container();
            container.RegisterMany<FooBar>(serviceTypeCondition: type => type.IsInterface);

            var fooBar = container.Resolve<IDisposable>(IfUnresolved.ReturnDefault);

            Assert.IsNull(fooBar);
        }

        public interface IFoo { }
        public interface IBar { }
        public class FooBar : IFoo, IBar, IDisposable
        {
            public void Dispose()
            {
            }
        }

        [Test]
        public void Can_register_many_services_produced_by_factory()
        {
            var builder = new Container();

            builder.Register<HubConnectionFactory>();

            builder.RegisterMany(
                Made.Of(r => ServiceInfo.Of<HubConnectionFactory>(), f => f.CreateHubConnection<IAssetHub>()),
                Reuse.Singleton);

            builder.RegisterMany(
                Made.Of(r => ServiceInfo.Of<HubConnectionFactory>(), f => f.CreateHubConnection<INotificationHub>()),
                Reuse.Singleton);

            builder.Resolve<IAssetHub>();
            builder.Resolve<INotificationHub>();

            var hubs = builder.Resolve<IStatefulHub[]>();
            Assert.AreEqual(hubs.Length, 2);
        }

        public interface IStatefulHub { }

        public interface IAssetHub : IStatefulHub { }
        public class AssetHub : IAssetHub {}

        public interface INotificationHub : IStatefulHub { }
        public class NotificationHub : INotificationHub { }

        public class HubConnectionFactory
        {
            public T CreateHubConnection<T>() where T : class
            {
                if (typeof(T) == typeof(IAssetHub))
                    return new AssetHub() as T;
                if (typeof(T) == typeof(INotificationHub))
                    return new NotificationHub() as T;
                return null;
            }
        }

        [Test]
        public void How_Autofac_Owned_works()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AClient>();
            builder.RegisterType<AService>();
            builder.RegisterType<ADependency>();
            builder.RegisterType<ANestedDep>().InstancePerOwned<AService>();
            var container = builder.Build();

            var c1 = container.Resolve<AClient>();
            var c2 = container.Resolve<AClient>();
            Assert.AreNotSame(c2, c1);
            Assert.AreNotSame(c2.Service, c1.Service);

            var dep = c1.Service.Value.Dep;
            Assert.AreNotSame(c2.Service.Value.Dep, dep);
            Assert.AreSame(c1.Service.Value.NestedDep, dep.NestedDep);

            c1.Dispose();
            Assert.IsTrue(dep.IsDisposed);
            Assert.IsTrue(dep.NestedDep.IsDisposed);
        }

        public class ANestedDep : IDisposable 
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class ADependency : IDisposable 
        {
            public ANestedDep NestedDep { get; private set; }

            public bool IsDisposed { get; private set; }

            public ADependency(ANestedDep nestedDep)
            {
                NestedDep = nestedDep;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class AService
        {
            public ADependency Dep { get; private set; }
            public ANestedDep NestedDep { get; private set; }

            public AService(ADependency dep, ANestedDep nestedDep)
            {
                Dep = dep;
                NestedDep = nestedDep;
            }
        }

        public class AClient : IDisposable
        {
            public Owned<AService> Service { get; private set; }
            public AClient(Owned<AService> service)
            {
                Service = service;
            }

            public void Dispose()
            {
                Service.Dispose();
            }
        }

        [Test]
        public void Implement_Owned_in_DryIoc()
        {
            var container = new Container();
            container.Register(typeof(DryIocOwned<>), setup: Setup.Wrapper);
            container.Register<My>();
            container.Register<Cake>(Reuse.InResolutionScopeOf<My>());

            var my = container.Resolve<My>();
            my.Dispose();

            Assert.IsTrue(my.OwnedCake.Value.IsDisposed);
        }

        public class Cake : IDisposable 
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class My : IDisposable
        {
            public DryIocOwned<Cake> OwnedCake { get; private set; }

            public My(DryIocOwned<Cake> ownedCake)
            {
                OwnedCake = ownedCake;
            }

            public void Dispose()
            {
                OwnedCake.Dispose();
            }
        }

        public class DryIocOwned<TService> : IDisposable
        {
            public TService Value { get; private set; }
            private readonly IDisposable _scope;

            public DryIocOwned(TService value, IDisposable scope)
            {
                Value = value;
                _scope = scope;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }
        }

        [Test]
        public void Autofac_default_constructor_selection_policy()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<A>();
            builder.RegisterType<B>();
            var container = builder.Build();

            container.Resolve<A>();
        }

        [Test]
        public void DryIoc_constructor_selection_plus_specs()
        {
            var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments));

            container.Register(Made.Of(() => new A(Arg.Of<C>(IfUnresolved.ReturnDefault))));
            container.Register<B>();

            var a = container.Resolve<A>();
            Assert.IsTrue(a.IsCreatedWithC);
        }

        public class B {}
        public class C {}

        public class A
        {
            public bool IsCreateWithB { get; private set; }
            public bool IsCreatedWithC { get; private set; }

            public A(B b)
            {
                IsCreateWithB = true;
            }

            public A(C c)
            {
                IsCreatedWithC = true;
            }
        }
    }
}
