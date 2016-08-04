using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.Syntax.Autofac.UnitTests
{
    [TestFixture]
    public class BasicTests
    {
        public abstract class Metadata
        {
            public class AutoActivated : Metadata
            {
                public static readonly AutoActivated It = new AutoActivated();
            }
        }

        public interface ISpecific { }
        public interface INormal { }

        public class Foo : ISpecific { }
        public class Bar : INormal { }

        public interface IFoo { }
        public interface IBar { }
        public class FooBar : IFoo, IBar, IDisposable
        {
            public void Dispose()
            {
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

        [Test]
        public void How_Autofac_IEnumerable_handles_service_with_missing_dependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<NoDep>();
            var container = builder.Build();

            Assert.Throws<DependencyResolutionException>(() => 
                container.Resolve<IEnumerable<NoDep>>());
        }

        public class NoDep
        {
            public NoDep(ISomeDep dep) {}
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

        public class AutofacModule : Module
        {
            protected override void Load(ContainerBuilder moduleBuilder)
            {
                moduleBuilder.RegisterType<BB>().SingleInstance();
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

        public class C {}

        public class A
        {
            public bool IsCreatedWithB { get; private set; }
            public bool IsCreatedWithC { get; private set; }

            public A(B b)
            {
                IsCreatedWithB = true;
            }

            public A(C c)
            {
                IsCreatedWithC = true;
            }
        }
    }
}
