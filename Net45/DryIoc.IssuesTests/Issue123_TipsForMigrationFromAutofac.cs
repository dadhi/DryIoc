using System;
using System.Linq;
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
                .OrderBy(factory => factory.RegistrationOrder)
                .ToArray();

            Assert.AreEqual(null, regsInOrder[0].OptionalServiceKey);
            Assert.AreEqual(1, regsInOrder[1].OptionalServiceKey);
            Assert.AreEqual(DefaultKey.Value, regsInOrder[2].OptionalServiceKey);
        }

        [Flags]
        public enum Metadata { AutoActivated }

        [Test]
        public void Auto_activated_with_metadata()
        {
            var container = new Container();
            container.Register<ISpecific, Foo>(setup: Setup.With(metadata: Metadata.AutoActivated));
            container.Register<INormal, Bar>();

            var registrations = container.GetServiceRegistrations()
                .Where(r => r.Factory.Setup.Metadata as Metadata? == Metadata.AutoActivated)
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

        public interface IFoo { }
        public interface IBar { }
        public class FooBar : IFoo, IBar
        {
        }

        [Test]
        public void Can_register_many_services_produced_by_factory()
        {
            var builder = new Container();

            builder.Register<HubConnectionFactory>();

            builder.RegisterMany<IAssetHub>(Reuse.Singleton,
                Made.Of(r => ServiceInfo.Of<HubConnectionFactory>(), f => f.CreateHubConnection<IAssetHub>()));

            builder.RegisterMany<INotificationHub>(Reuse.Singleton,
                Made.Of(r => ServiceInfo.Of<HubConnectionFactory>(), f => f.CreateHubConnection<INotificationHub>()));

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
    }

}
