using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class ConstructorSelectionTests
    {
        [Test]
        public void When_registering_service_with_multiple_constructors_you_Should_specify_what_constructor_to_use()
        {
            var container = new Container();
            container.Register<IService, SomeService>();

            container.Register<ClassWithMultipleConstructors>(
                withConstructor: t => t.GetConstructor(new[] { typeof(IService) }));

            var service = container.Resolve<ClassWithMultipleConstructors>();
            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void Registering_service_with_many_public_constructors_without_constructor_selector_will_throw()
        {
            var container = new Container();

            Assert.Throws<ContainerException>(() =>
                container.Register<ClassWithMultipleConstructors>());
        }
    }

    public class ClassWithMultipleConstructors
    {
        public readonly string Parameter;
        public readonly IService Service;

        public ClassWithMultipleConstructors(string parameter)
        {
            Parameter = parameter;
        }

        public ClassWithMultipleConstructors(IService service)
        {
            Service = service;
        }
    }
}
