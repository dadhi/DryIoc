using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GH_Issue6_Open_generic_singleton_service_registration_that_satisfies_multiple_interfaces
    {
        [Test, Ignore("to fix")]
        public void Register_mapping_should_work()
        {
            var container = new Container();

            container.Register(typeof(EventHub<>), Reuse.Singleton);
            container.RegisterMapping(typeof(IPublisher<>), typeof(EventHub<>));
            container.RegisterMapping(typeof(ISubscriber<>), typeof(EventHub<>));

            var pub = container.Resolve<IPublisher<string>>();
            var sub = container.Resolve<ISubscriber<string>>();
            var hub = container.Resolve<EventHub<string>>();

            Assert.AreSame(pub, sub);
            Assert.AreSame(pub, hub);
        }

        public class EventHub<T> : IPublisher<T>, ISubscriber<T>
        {
            private readonly List<Action<T>> listeners = new List<Action<T>>();

            public void Publish(T data)
            {
                foreach (var listener in listeners)
                {
                    listener(data);
                }
            }

            public void Register(Action<T> listener)
            {
                listeners.Add(listener);
            }

            public void Unregister(Action<T> listener)
            {
                listeners.Remove(listener);
            }
        }

        public interface IPublisher<T>
        {
            void Publish(T data);
        }

        public interface ISubscriber<T>
        {
            void Register(Action<T> listener);

            void Unregister(Action<T> listener);
        }
    }
}
