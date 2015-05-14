using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue123_TipsForMigrationFromAutofac
    {
        [Test]
        public void Analog_of_AsImplementedInterfaces()
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

            //builder.RegisterDelegate(r => r.Resolve<HubConnectionFactory>().CreateHubConnection<IAssetHub>(), Reuse.Singleton);
            //builder.RegisterDelegate<IStatefulHub>(r => r.Resolve<IAssetHub>());
            //builder.RegisterDelegate(r => r.Resolve<HubConnectionFactory>().CreateHubConnection<INotificationHub>(), Reuse.Singleton);
            //builder.RegisterDelegate<IStatefulHub>(r => r.Resolve<INotificationHub>());

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
