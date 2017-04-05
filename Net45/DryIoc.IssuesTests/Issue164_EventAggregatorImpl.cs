using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue164_EventAggregatorImpl
    {
        [Test]
        public void Able_to_handle_multiple_events_being_singleton()
        {
            var container = new Container();

            container.Register<IEventDispatcher, DryIocEventDispatcher>();
            container.RegisterMany<ScoringService>(Reuse.Singleton);
            container.RegisterMany<RenderingService>();

            var eventDispatcher = container.Resolve<IEventDispatcher>();

            eventDispatcher.Dispatch(new ZoneDestroyedEventArgs());
            eventDispatcher.Dispatch(new ZoneCreatedEventArgs());
        }

        public interface IEventDispatcher 
        {
            void Dispatch<TEvent>(TEvent eventToDispatch) where TEvent : EventArgs;
        }

        public class DryIocEventDispatcher : IEventDispatcher
        {
            private readonly IResolver _resolver;

            public DryIocEventDispatcher(IResolver resolver)
            {
                _resolver = resolver;
            }

            public void Dispatch<TEvent>(TEvent eventToDispatch) where TEvent : EventArgs
            {
                foreach (var handler in _resolver.ResolveMany<IHandles<TEvent>>())
                {
                    handler.Handle(eventToDispatch);
                }
            }
        }

        public interface IHandles<T> where T : EventArgs
        {
            void Handle(T args);
        }

        public class ZoneDestroyedEventArgs : EventArgs { }
        public class ZoneCreatedEventArgs : EventArgs { }

        public class ScoringService : IHandles<ZoneDestroyedEventArgs>, IHandles<ZoneCreatedEventArgs>
        {
            public void Handle(ZoneDestroyedEventArgs args)
            {
                Console.WriteLine("Scoring Service Handled ZoneDestroyed Event");
            }

            public void Handle(ZoneCreatedEventArgs args)
            {
                Console.WriteLine("Scoring Service Handled ZoneCreated Event");
            }
        }

        public class RenderingService : IHandles<ZoneDestroyedEventArgs>, IHandles<ZoneCreatedEventArgs>
        {
            public void Handle(ZoneDestroyedEventArgs args)
            {
                Console.WriteLine("Rendering Service Handled ZoneDestroyed Event");
            }

            public void Handle(ZoneCreatedEventArgs args)
            {
                Console.WriteLine("Rendering Service Handled ZoneCreated Event");
            }
        }
    }

}
