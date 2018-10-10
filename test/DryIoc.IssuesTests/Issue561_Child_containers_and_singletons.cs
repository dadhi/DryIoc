using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue561_Child_containers_and_singletons
    {
        public interface IService { }
        public class Service : IService { }
        public class TestService : IService { }
        public class Concrete
        {
            public IService Service { get; }
            public Concrete(IService service)
            {
                Service = service;
            }
        }

        [Test]
        public void should_resolve_unregistered_concrete_class()
        {
            var parent = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());
            parent.Register<IService, Service>();
            Assert.IsInstanceOf<Concrete>(parent.Resolve<Concrete>());

            var child = parent.WithRegistrationsCopy();
            Assert.IsInstanceOf<Concrete>(child.Resolve<Concrete>());
        }

        [Test]
        public void child_can_override_parent_singleton_instances_in_isolation()
        {
            var parent = new Container();
            var service = new Service();
            parent.RegisterDelegate<IService>(_ => service);
            parent.Register<Concrete>();

            // first child can override parent registrations
            var child = parent.WithRegistrationsCopy();
            child.RegisterDelegate<IService>(_ => new TestService(), ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var concrete = child.Resolve<Concrete>();
            Assert.IsInstanceOf<TestService>(concrete.Service);

            // parent container should not be changed. 
            concrete = parent.Resolve<Concrete>();
            Assert.IsInstanceOf<Service>(concrete.Service);

            // and subsequent child containers should get original parent registrations
            var child2 = parent.WithRegistrationsCopy();
            concrete = child2.Resolve<Concrete>();
            Assert.IsInstanceOf<Service>(concrete.Service);
        }

        [Test]
        public void child_can_override_parent_transient_registrations_in_isolation()
        {
            var parent = new Container();
            parent.Register<IService, Service>();
            parent.Register<Concrete>();

            // first child can override parent registrations
            var child = parent.WithRegistrationsCopy();
            child.RegisterDelegate<IService>(_ => new TestService(), ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var concrete = child.Resolve<Concrete>();
            Assert.IsInstanceOf<TestService>(concrete.Service);

            // parent container should not be changed. 
            concrete = parent.Resolve<Concrete>();
            Assert.IsInstanceOf<Service>(concrete.Service);

            // and subsequent child containers should get original parent registrations
            var child2 = parent.WithRegistrationsCopy();
            concrete = child2.Resolve<Concrete>();
            Assert.IsInstanceOf<Service>(concrete.Service);
        }
    }
}
