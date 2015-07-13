using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class PubSub
    {
        [Test]
        public void Can_subscribe_to_Hub_based_on_implemented_interface()
        {
            var container = new Container();
            container.Register<PubSubHub>(Reuse.Singleton);
            container.RegisterInitializer<ISubsriber>((s, r) => r.Resolve<PubSubHub>().Subscribe(s.Receive));
            
            container.Register<Subscriber>();

            var subscriber = container.Resolve<Subscriber>();
            var hub = container.Resolve<PubSubHub>();
            hub.PingSubscribers();

            Assert.That(subscriber.LastMessage, Is.EqualTo("ping"));
        }

        [Test]
        public void Can_subscribe_attributed_subscriber()
        {
            var container = new Container();
            container.Register<PubSubHub>(Reuse.Singleton);
            container.RegisterInitializer<object>(
                (s, r) =>
                {
                    var receive = s.GetType().GetMethods()
                        .FirstOrDefault(m => m.GetAttributes(typeof(ReceiveAttribute)).Count() == 1);
                    r.Resolve<PubSubHub>().Subscribe(message => receive.ThrowIfNull().Invoke(s, new[] { message }));
                }, 
                r => r.ImplementationType != null 
                  && r.ImplementationType.GetAttributes(typeof(SubscriberAttribute)).Count() == 1);
            
            container.Register<AttributedSubscriber>();

            var subscriber = container.Resolve<AttributedSubscriber>();
            var hub = container.Resolve<PubSubHub>();
            hub.PingSubscribers();

            Assert.That(subscriber.LastMessage, Is.EqualTo("ping"));
        }

        [Test, Ignore]
        public void Can_subscribe_attributed_subscriber_with_decorator()
        {
            var container = new Container();
            container.Register<PubSubHub>(Reuse.Singleton);
            container.Register(Made.Of<object>(() => Subscribe(default(ISubsriber), default(PubSubHub))),
                setup: Setup.DecoratorWith(r => r.ImplementationType != null
                  && r.ImplementationType.GetAttributes(typeof(SubscriberAttribute)).Count() == 1));

            container.Register<AttributedSubscriber>();

            var subscriber = container.Resolve<AttributedSubscriber>();
            var hub = container.Resolve<PubSubHub>();
            hub.PingSubscribers();

            Assert.That(subscriber.LastMessage, Is.EqualTo("ping"));
        }

        public static ISubsriber Subscribe(ISubsriber s, PubSubHub hub)
        {
            hub.Subscribe(s.Receive);
            return s;
        }
    }

    public class PubSubHub
    {
        public void Subscribe(Action<object> subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void PingSubscribers()
        {
            for (var i = 0; i < _subscribers.Count; i++)
                _subscribers[i]("ping");
        }

        private readonly List<Action<object>> _subscribers = new List<Action<object>>();
    }

    public interface ISubsriber
    {
        void Receive(object message);
    }

    public class Subscriber : ISubsriber
    {
        public object LastMessage;

        public void Receive(object message)
        {
            LastMessage = message;
        }
    }

    [Subscriber]
    public class AttributedSubscriber
    {
        public object LastMessage;

        [Receive]
        public void Handle(object message)
        {
            LastMessage = message;
        }
    }

    public class ReceiveAttribute : Attribute {}

    public class SubscriberAttribute : Attribute {}
}
