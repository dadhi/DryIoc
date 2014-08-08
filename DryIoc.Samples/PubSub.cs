using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    [Ignore]
    public class PubSub
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.RegisterManyServicesWithOneImplementation<SomeHandler>();
            container.RegisterManyServicesWithOneImplementation<PubSubHub>(Reuse.Singleton);

            var sub = container.Resolve<ISub>();
            var handler = container.Resolve<SomeHandler>();

            Assert.That(handler, Is.Not.Null);
            CollectionAssert.Contains(sub.Handlers, handler);
        }
    }

    public class SomeHandler
    {
        public SomeHandler(ISub sub)
        {
            // remove that as infrastructure piece
            //sub.Subscribe(this); 
        }
    }

    public interface ISub
    {
        IEnumerable<object> Handlers { get; }

        void Subscribe<THandler>(THandler handler);
    }

    public class PubSubHub : ISub
    {
        public IEnumerable<object> Handlers
        {
            get { return _handlers.ToArray(); }
        }

        public void Subscribe<THandler>(THandler handler)
        {
            _handlers.Add(handler);
        }

        private readonly List<object> _handlers = new List<object>();
    }
}
