using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class PrimitiveValueInjectionTests : ITest
    {
        public int Run()
        {
            Can_inject_value_into_parameter();
            Can_inject_value_into_parameter_using_condition();
            Throws_on_injecting_value_of_different_type();
            Can_inject_primitive_value_and_resolve_the_rest_of_parameters();
            Can_inject_primitive_value_and_handle_ReturnDefault_for_the_rest_of_parameters();
            Able_to_inject_non_primitive_value();
            Can_inject_primitive_value_into_property();
            Can_inject_non_primitive_value_into_property();
            Can_specify_how_to_resolve_property_per_registration_without_strings();
            Should_throw_if_property_is_not_found();
            return 10;
        }

        [Test]
        public void Can_inject_value_into_parameter()
        {
            var container = new Container();
            container.Register<InjectionRulesTests.ClientWithStringParam>(made: Parameters.Of.Name("x", r => "aha"));

            var client = container.Resolve<InjectionRulesTests.ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("aha"));
        }

        [Test]
        public void Can_inject_value_into_parameter_using_condition()
        {
            var container = new Container();
            container.Register<InjectionRulesTests.ClientWithStringParam>(
                made: Parameters.Of.Details((r, p) => p.ParameterType != typeof(string) ? null : ServiceDetails.Of("aha")));

            var client = container.Resolve<InjectionRulesTests.ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("aha"));
        }

        [Test]
        public void Throws_on_injecting_value_of_different_type()
        {
            var container = new Container();
            container.Register<InjectionRulesTests.ClientWithStringParam>(made: Parameters.Of.Name("x", r => 500));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<InjectionRulesTests.ClientWithStringParam>());

            StringAssert.Contains("Injected value 500 is not assignable to String", ex.Message);
        }

        [Test]
        public void Can_inject_primitive_value_and_resolve_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<InjectionRulesTests.ClientWithServiceAndStringParam>(
                made: Parameters.Of.Details((r, p) => p.ParameterType != typeof(string) ? null : ServiceDetails.Of("aha")));
            container.Register<IService, Service>();

            var client = container.Resolve<InjectionRulesTests.ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("aha"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_inject_primitive_value_and_handle_ReturnDefault_for_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<InjectionRulesTests.ClientWithServiceAndStringParam>(
                made: Parameters.Of.Details((r, p) => ServiceDetails.IfUnresolvedReturnDefault).Name("x", r => "yoga"));

            var client = container.Resolve<InjectionRulesTests.ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("yoga"));
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void Able_to_inject_non_primitive_value()
        {
            var container = new Container();
            container.Register<Service>(serviceKey: "service");
            container.Register<InjectionRulesTests.ClientWithServiceAndStringParam>(
                made: Parameters.Of
                    .Name("service", r => r.Container.Resolve<IService>("service", requiredServiceType: typeof(Service)))
                    .Name("x", _ => "blah"));

            var client = container.Resolve<InjectionRulesTests.ClientWithServiceAndStringParam>();

            Assert.IsNotNull(client.Service);
        }

        [Test]
        public void Can_inject_primitive_value_into_property()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "dependency");
            container.Register<InjectionRulesTests.ClientWithServiceAndStringProperty>(
                made: PropertiesAndFields.Of.Name("Message", r => "raven"));

            var client = container.Resolve<InjectionRulesTests.ClientWithServiceAndStringProperty>();

            Assert.AreEqual("raven", client.Message);
        }

        [Test]
        public void Can_inject_non_primitive_value_into_property()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "dependency");
            container.Register<InjectionRulesTests.ClientWithServiceAndStringProperty>(
                made: PropertiesAndFields.Of.Name("Service", r => r.Container.Resolve<IService>("dependency")));

            var client = container.Resolve<InjectionRulesTests.ClientWithServiceAndStringProperty>();

            Assert.IsInstanceOf<Service>(client.Service);
        }

        [Test]
        public void Can_specify_how_to_resolve_property_per_registration_without_strings()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "dependency");
            container.Register<InjectionRulesTests.ClientWithServiceAndStringProperty>(
                made: PropertiesAndFields.Of
                    .Name("Message", r => "hell")
                    .Details("Service", r => ServiceDetails.Of(serviceKey: "dependency")));

            var client = container.Resolve<InjectionRulesTests.ClientWithServiceAndStringProperty>();

            Assert.That(client.Message, Is.EqualTo("hell"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Should_throw_if_property_is_not_found()
        {
            var container = new Container();
            container.Register<InjectionRulesTests.ClientWithServiceAndStringProperty>(
                made: PropertiesAndFields.Of.Name("WrongName", r => "wrong name"));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<InjectionRulesTests.ClientWithServiceAndStringProperty>());

            StringAssert.Contains("Unable to find writable property or field \"WrongName\" when resolving", ex.Message);
        }
    }
}
