using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class InjectionRulesTests
    {
        [Test]
        public void Specify_property_selector_when_registering_service()
        {
            var container = new Container();

            container.Register<SomeBlah>(setup: Setup.With(propertiesAndFields:
                (type, request, registry) => type.GetProperties().Select(PropertyOrFieldServiceInfo.Of)));
            container.Register<IService, Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        [Test]
        public void Specify_property_selector_with_custom_service_type_when_registering_service()
        {
            var container = new Container();

            container.Register<SomeBlah>(setup: Setup.With(propertiesAndFields:
                (type, request, registry) => type.GetProperties().Select(p =>
                    p.Name.Equals("Uses") ? PropertyOrFieldServiceInfo.Of(p).With(ServiceInfoDetails.Of(typeof(Service)), request, registry) : null)));
            container.Register<Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_inject_primitive_value()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(setup: Setup.With(parameters: Parameters.Of.Name("x", "hola")));

            var client = container.Resolve<ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void CanNot_inject_primitive_value_of_different_type()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(setup: Setup.With(parameters: Parameters.Of.Name("x", 500)));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithStringParam>());
            
            Assert.That(ex.Message, Is.StringContaining("Injected value 500 is not assignable to String parameter \"x\""));
        }

        [Test]
        public void Can_inject_primitive_value_and_resolve_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(setup: Setup.With(parameters: Parameters.Of.Name("x", "hola")));
            container.Register<IService, Service>();

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_inject_primitive_value_and_handle_ReturnDefault_for_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(
                setup: Setup.With(parameters: Parameters.AllowDefault.Name("x", "hola")));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void Can_nicely_specify_required_type_and_key_per_parameter()
        {
            var container = new Container();
            container.Register<Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringParam>(setup: Setup.With(
                parameters: Parameters.Of.Name("x", "hola").Name("service", ServiceInfoDetails.Of(typeof(Service), "dependency"))));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void Can_specify_how_to_resolve_property_per_registration()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.Of
                    .Name("Message", "hell")
                    .Name("Service", r => r.Resolve<IService>("dependency"))));

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Message, Is.EqualTo("hell"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Should_throw_if_property_is_not_found()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.Of.Name("WrongName", "wrong name")));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithServiceAndStringProperty>());

            Assert.That(ex.Message, Is.StringContaining("Unable to find property \"WrongName\" when resolving"));
        }

        #region CUT

        public class SomeBlah
        {
            public IService Uses { get; set; }
        }

        public class ClientWithStringParam
        {
            public string X { get; private set; }

            public ClientWithStringParam(string x)
            {
                X = x;
            }
        }

        public class ClientWithServiceAndStringParam
        {
            public IService Service { get; private set; }
            public string X { get; private set; }

            public ClientWithServiceAndStringParam(IService service, string x)
            {
                Service = service;
                X = x;
            }
        }

        public class ClientWithServiceAndStringProperty
        {
            public IService Service { get; set; }
            public string Message { get; set; }
        }

        #endregion
    }
}
