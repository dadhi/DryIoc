using NUnit.Framework;

namespace DryIoc.SpeedTestApp.Net40
{
    [TestFixture]
    public class FastSingletonTests 
    {
        [Test]
        public void When_resolving_singleton_Then_used_expression_should_contain_singleton_variable()
        {
            var container = new Container();
            container.Register<SingletonService>(Reuse.Singleton);

            var service = container.Resolve<SingletonService>();

            Assert.That(service, Is.Not.Null.And.SameAs(container.Resolve<SingletonService>()));
        }

        [Test]
        public void When_resolving_singleton_consumer_Then_used_expression_should_contain_singleton_variable()
        {
            var container = new Container();
            container.Register<SingletonService>();
            container.Register<DirectSingletonConsumer>();

            var consumer = container.Resolve<DirectSingletonConsumer>();
        }

        [Test]
        public void When_singleton_is_appears_multiple_times_in_factory_expression_Then_it_should_be_assigned_to_local_var_and_reused()
        {
            var container = new Container();
            container.Register<SingletonService>();
            container.Register<DirectSingletonConsumer>();
            container.Register<SingletonConsumer>();

            var consumer = container.Resolve<SingletonConsumer>();
        }
    }

    public class SingletonService {}

    public class DirectSingletonConsumer
    {
        public SingletonService Singleton { get; set; }

        public DirectSingletonConsumer(SingletonService singleton)
        {
            Singleton = singleton;
        }
    }

    public class SingletonConsumer
    {
        public SingletonService Singleton { get; set; }
        public DirectSingletonConsumer Consumer { get; set; }

        public SingletonConsumer(SingletonService singleton, DirectSingletonConsumer consumer)
        {
            Singleton = singleton;
            Consumer = consumer;
        }
    }
}
