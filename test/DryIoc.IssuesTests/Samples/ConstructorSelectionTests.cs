using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class ConstructorSelectionTests : ITest
    {
        public int Run()
        {
            When_registering_service_with_multiple_constructors_you_Should_specify_what_constructor_to_use();
            Registering_service_with_many_public_constructors_without_constructor_selector_will_throw();
            It_is_possible_to_register_and_resolve_service_with_internal_constructor();
            return 3;
        }

        [Test]
        public void When_registering_service_with_multiple_constructors_you_Should_specify_what_constructor_to_use()
        {
            var container = new Container();
            container.Register<IService, SomeService>();

            container.Register<ClassWithMultipleConstructors>(
                made: Made.Of(t => t.GetConstructor(new[] { typeof(IService) })));

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

        [Test]
        public void It_is_possible_to_register_and_resolve_service_with_internal_constructor()
        {
            var container = new Container();
            container.Register<IService, SomeService>();

            container.Register<ClassWithInternalConstructor>(made: Made.Of(t => t.SingleConstructor(true)));

            var obj = container.Resolve<ClassWithInternalConstructor>();
            Assert.IsNotNull(obj);
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

        public class ClassWithInternalConstructor
        {
            public readonly IService Service;

            internal ClassWithInternalConstructor(IService service)
            {
                Service = service;
            }
        }
    }
}
