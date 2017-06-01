using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue123_TipsForMigrationFromAutofac
    {
        [Test]
        public void Transient_disposable_is_tracked_in_container()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FooBar>().As<IFoo>();

            var container = builder.Build();

            var f = container.Resolve<IFoo>();
            Assert.AreNotSame(f, container.Resolve<IFoo>());

            container.Dispose();
            Assert.IsTrue(((FooBar)f).IsDisposed);
        }

        [Test]
        public void Transient_disposable_is_tracked_in_open_scope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FooBar>().As<IFoo>();

            var container = builder.Build();

            IFoo f;
            using (var scope = container.BeginLifetimeScope())
            {
                f = scope.Resolve<IFoo>();
                Assert.AreNotSame(f, scope.Resolve<IFoo>());
            }

            Assert.IsTrue(((FooBar)f).IsDisposed);
        }

        [Test]
        public void How_to_get_all_registrations_in_registration_order()
        {
            var container = new Container();
            container.Register<IFoo, FooBar>(Reuse.Singleton);
            container.Register<IBar, FooBar>(Reuse.Singleton, serviceKey: 1);
            container.Register<IBar, FooBar>(Reuse.Singleton);

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
            container.Register<ISpecific, Foo>(setup: Setup.With(metadataOrFuncOfMetadata: Metadata.AutoActivated.It));
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

            container.RegisterMany<FooBar>(serviceTypeCondition: type => type.IsInterface, reuse: Reuse.Singleton);

            Assert.IsNotNull(container.Resolve<IFoo>());
            Assert.IsNotNull(container.Resolve<IBar>());
            
            Assert.Null(container.Resolve<FooBar>(IfUnresolved.ReturnDefault));
        }

        [Test]
        public void IDisposable_should_be_excluded_from_register_many()
        {
            var container = new Container();
            container.RegisterMany<FooBar>(serviceTypeCondition: type => type.IsInterface, reuse: Reuse.Singleton);

            var fooBar = container.Resolve<IDisposable>(IfUnresolved.ReturnDefault);

            Assert.IsNull(fooBar);
        }

        public interface IFoo { }
        public interface IBar { }
        public class FooBar : IFoo, IBar, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Test]
        public void AutofacDisposalOrder()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Y>().SingleInstance();
            builder.RegisterType<X>().SingleInstance();
            var container = builder.Build();

            var x = container.Resolve<X>();
            var y = container.Resolve<Y>();

            var order = string.Empty;
            x.Disposed = () => order += "x";
            y.Disposed = () => order += "y";

            container.Dispose();

            Assert.AreEqual("yx", order);
        }

        public class X : IDisposable
        {
            public Action Disposed;

            public void Dispose()
            {
                Disposed();
            }
        }

        public class Y : IDisposable
        {
            public Action Disposed;

            public void Dispose()
            {
                Disposed();
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
            builder.RegisterType<ADependency>().InstancePerOwned<AService>();
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

        [Test]
        public void How_Autofac_IEnumerable_handles_service_with_missing_dependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<NoDep>();
            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => 
                container.Resolve<IEnumerable<NoDep>>());
        }

        [Test]
        public void How_DryIoc_IEnumerable_handles_service_with_missing_dependency()
        {
            var container = new Container();
            container.Register<NoDep>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<IEnumerable<NoDep>>());
        }

        public class NoDep
        {
            public ISomeDep Dep { get; private set; }
            public NoDep(ISomeDep dep)
            {
                Dep = dep;
            }
        }

        public interface ISomeDep { }

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
            container.Register<My>(Reuse.InResolutionScope);
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

        [Test]
        public void Module_Autofac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<AutofacModule>();
            builder.RegisterType<B>();

            var container = builder.Build();

            var bb = container.Resolve<BB>();
            Assert.IsInstanceOf<B>(bb.B);
        }

        [Test]
        public void Module_DryIoc()
        {
            var container = new Container();

            container.RegisterMany<DryIocModule>();
            container.Register<B>();

            foreach (var module in container.ResolveMany<IModule>())
                module.Load(container);

            var bb = container.Resolve<BB>();
            Assert.IsInstanceOf<B>(bb.B);
        }

        [Test]
        public void Single_implementation_multiple_interfaces_dont_share_lifetime()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<AB>().As<IA>().SingleInstance();
            builder.RegisterType<AB>().As<IB>().SingleInstance();

            var c = builder.Build();

            var a = c.Resolve<IA>();
            var b = c.Resolve<IB>();

            Assert.AreNotSame(a, b);
        }

        public interface IA { }
        public interface IB { }
        public class AB : IA, IB { }
        public class AA : IA { }

        [Test]
        public void Resolve_all_services_implementing_the_interface()
        {
            var container = new Container();
            container.Register<IB, AB>();
            container.Register<AA>();

            var aas = container.GetServiceRegistrations()
                .Where(r => typeof(IA).IsAssignableFrom(r.Factory.ImplementationType ?? r.ServiceType))
                .Select(r => (IA)container.Resolve(r.ServiceType))
                .ToList();

            Assert.AreEqual(2, aas.Count);
        }

        [Test]
        public void Resolve_all_registered_interface_services()
        {
            var container = new Container();

            // changed to RegisterMany to register implemented interfaces as services
            container.RegisterMany<AB>();
            container.RegisterMany<AA>();

            // simple resolve
            var aas = container.Resolve<IList<IA>>();

            Assert.AreEqual(2, aas.Count);
        }

        [Test]
        public void Autofac_Lazy_of_non_registered_dependency_should_throw()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<BLazy>();

            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => 
            container.Resolve<BLazy>());
        }

        [Test]
        public void DryIoc_Lazy_of_non_registered_dependency_should_throw()
        {
            var container = new Container(rules => rules.With(FactoryMethod.ConstructorWithResolvableArguments));

            container.Register<BLazy>();

            Assert.Throws<ContainerException>(() =>
            container.Resolve<BLazy>());
        }

        public class AutofacModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<BB>().SingleInstance();
            }
        }

        public interface IModule
        {
            void Load(IRegistrator builder);
        }

        public class DryIocModule : IModule
        {
            public void Load(IRegistrator builder)
            {
                builder.Register<BB>(Reuse.Singleton);
            }
        }

        public class B {}

        public class BB
        {
            public B B { get; private set; }

            public BB(B b)
            {
                B = b;
            }
        }

        public class BLazy
        {
            public Lazy<B> LazyB { get; private set; }

            public BLazy(Lazy<B> lazyB)
            {
                LazyB = lazyB;
            }
        }

        public class C {}

        public class A
        {
            public B B { get; private set; }
            public C C { get; private set; }
            public bool IsCreatedWithB { get; private set; }
            public bool IsCreatedWithC { get; private set; }

            public A(B b)
            {
                B = b;
                IsCreatedWithB = true;
            }

            public A(C c)
            {
                C = c;
                IsCreatedWithC = true;
            }
        }
    }
}
