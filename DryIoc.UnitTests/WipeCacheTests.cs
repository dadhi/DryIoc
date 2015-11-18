using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class WipeCacheTests
    {
        [Test]
        public void Resolving_service_after_updating_depenency_registration_will_return_old_dependency_due_Resolution_Cache()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>();
            var service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());
            
            container.Register<IDependency, Foo1>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());
        }

        [Test]
        public void When_resolution_cache_is_wiped_Then_resolving_service_after_updating_depenency_registration_will_return_New_dependency()
        {
            IContainer container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>();
            var service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());

            container = container.WithoutCache();
            container.Register<IDependency, Foo1>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Foo1>());
        }

        [Test]
        public void Should_throw_for_second_default_registration()
        {
            IContainer container = new Container();

            container.Register<IService, Service>();
            var service = container.Resolve<IService>();
            Assert.That(service, Is.InstanceOf<Service>());

            container.Register<IService, AnotherService>();
            container = container.WithoutCache();
            Assert.Throws<ContainerException>(() => 
                container.Resolve<IService>());
        }

        [Test]
        public void Should_return_updated_registration()
        {
            IContainer container = new Container();

            container.Register<IService, Service>();
            var service = container.Resolve<IService>();
            Assert.That(service, Is.InstanceOf<Service>());

            container.Register<IService, AnotherService>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            container = container.WithoutCache();
            service = container.Resolve<IService>();
            Assert.That(service, Is.InstanceOf<AnotherService>());
        }
    }
}
