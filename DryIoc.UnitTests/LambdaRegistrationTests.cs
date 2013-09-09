using System;
using DryIoc.UnitTests.CUT;
using DryIoc.UnitTests.Playground;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class LambdaRegistrationTests
    {
        [Test]
        public void Given_Lambda_registration_Resolving_service_should_be_of_Lambda_provided_implementation()
        {
            var container = new Container();
            container.RegisterLambda<IService>(() => new Service());

            var service = container.Resolve<IService>();

            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_without_specified_service_type_should_use_implementation_type_as_service_type()
        {
            var container = new Container();
            container.RegisterLambda(() => new Service());

            Assert.Throws<ContainerException>(() => container.Resolve<IService>());

            var service = container.Resolve<Service>();
            Assert.That(service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Func()
        {
            var container = new Container();
            container.RegisterLambda<IService>(() => new Service());

            var func = container.Resolve<Func<IService>>();

            Assert.That(func(), Is.Not.Null.And.Not.SameAs(func()));
        }

        [Test]
        public void Lambda_registration_could_be_resolved_as_Lazy()
        {
            var container = new Container();
            container.RegisterLambda<IService>(() => new Service());

            var service = container.Resolve<Lazy<IService>>();

            Assert.That(service.Value, Is.Not.Null.And.SameAs(service.Value));
        }

        [Test]
        public void Given_lambda_registration_Injecting_it_as_dependency_should_work()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();

            var dependency = new Dependency();
            container.RegisterLambda<IDependency>(() => dependency);

            var service = container.Resolve<ServiceWithDependency>();

            Assert.That(service.Dependency, Is.SameAs(dependency));
        }
    }
}
