using System.Threading;
using System.Threading.Tasks;
using ImTools;
using MediatR;
using NUnit.Framework;
// ReSharper disable RedundantCast
// ReSharper disable RedundantExtendsListEntry

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue37_MediatR_Polymorphic_Notification
    {
        [Test]
        public void Publish_Notification_with_MediatR()
        {
            var container = new Container();

            // MediatR setup
            container.RegisterDelegate<ServiceFactory>(r => r.Resolve);
            container.RegisterMany(typeof(IMediator).GetAssembly().One(), Registrator.Interfaces);

            // User code
            container.RegisterMany<InventoryDenormalizer>(Reuse.Singleton, serviceTypeCondition: Registrator.Interfaces);

            var mediator = container.Resolve<IMediator>();
            mediator.Publish(new InventoryNotificationReceived());

            var inventoryDenormalizer = (InventoryDenormalizer)container.Resolve<INotificationHandler<InventoryNotificationReceived>>();
            Assert.AreEqual(2, inventoryDenormalizer.HandledInventoryNotificationReceived);
        }

        [Test]
        public void Publish_Notification_with_DryIoc_alone()
        {
            var container = new Container();

            container.RegisterMany<InventoryDenormalizer>(Reuse.Singleton, serviceTypeCondition: Registrator.Interfaces);

            var handlers = container.Resolve<INotificationHandler<InventoryNotificationReceived>[]>();
            Assert.AreEqual(2, handlers.Length);

            foreach (var h in handlers)
                h.Handle(new InventoryNotificationReceived(), CancellationToken.None);

            var inventoryDenormalizer = (InventoryDenormalizer)handlers[0];
            Assert.AreEqual(2, inventoryDenormalizer.HandledInventoryNotificationReceived);
        }

        [Test]
        public void Publish_Notification_manually()
        {
            var inventoryDenormalizer = new InventoryDenormalizer();
            var handlers = new[]
            {
                (INotificationHandler<InventoryNotificationReceived>)inventoryDenormalizer,
                (INotificationHandler<InventoryNotificationBase>)inventoryDenormalizer
            };

            foreach (var h in handlers)
                h.Handle(new InventoryNotificationReceived(), CancellationToken.None);

            Assert.AreEqual(2, inventoryDenormalizer.HandledInventoryNotificationReceived);
        }
    }

    public class InventoryDenormalizer :
        INotificationHandler<InventoryNotificationBase>,
        INotificationHandler<InventoryNotificationReceived>
    {
        public int HandledInventoryNotificationReceived;
        public int HandleInventoryNotificationBase;

        public Task Handle(InventoryNotificationReceived notification, CancellationToken cancellationToken)
        {
            HandledInventoryNotificationReceived++;
            return Task.CompletedTask;  // This is called twice.
        }

        public Task Handle(InventoryNotificationBase notification, CancellationToken cancellationToken)
        {
            HandleInventoryNotificationBase++;
            return Task.CompletedTask;  // This is never called.
        }
    }

    public class InventoryNotificationBase : INotification { }

    public class InventoryNotificationReceived : InventoryNotificationBase, INotification {}
}
