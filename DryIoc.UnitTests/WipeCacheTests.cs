using System;
using NUnit.Framework;
using DryIoc.UnitTests.CUT;

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
            
            container.Register<IDependency, Foo1>(ifAlreadyRegistered: IfAlreadyRegistered.UpdateRegistered);
            service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());
        }

        [Test]
        public void When_resolution_cache_is_wiped_Then_resolving_service_after_updating_depenency_registration_will_return_New_dependency()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>();
            var service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Dependency>());

            container = container.WipeCache();
            container.Register<IDependency, Foo1>(ifAlreadyRegistered: IfAlreadyRegistered.UpdateRegistered);
            service = container.Resolve<ServiceWithDependency>();
            Assert.That(service.Dependency, Is.InstanceOf<Foo1>());
        }
    }
}
