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
            container.Register<ClientWithStringParam>(setup: Setup.With(parameters: Parameters.Default.With("x", "hola")));

            var client = container.Resolve<ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void CanNot_inject_primitive_value_of_different_type()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(setup: Setup.With(parameters: Parameters.Default.With("x", 500)));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithStringParam>());
            
            Assert.That(ex.Message, Is.StringContaining("Injected value 500 is not assignable to String parameter \"x\""));
        }

        [Test]
        public void Can_inject_primitive_value_and_resolve_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(setup: Setup.With(parameters: Parameters.Default.With("x", "hola")));
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
                setup: Setup.With(parameters: Parameters.With(IfUnresolved.ReturnDefault).With("x", "hola")));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.Null);
        }

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
    }

    public static class Parameters
    {
        public static ParameterSelector Default = (parameter, request, registry) => ParameterServiceInfo.Of(parameter);

        public static ParameterSelector With(IfUnresolved ifUnresolved)
        {
            return ifUnresolved == IfUnresolved.Throw ? Default
                : ((parameter, req, reg) => ParameterServiceInfo.Of(parameter).With(ServiceInfoDetails.Of(ifUnresolved: ifUnresolved), req, reg));
        }

        public static ParameterSelector With<T>(this ParameterSelector source, string name, T value)
        {
            name.ThrowIfNull();
            return (parameter, req, reg) => !parameter.Name.Equals(name) ? source(parameter, req, reg)
                : ParameterServiceInfo.Of(parameter).With(ServiceInfoDetails.Of(() => value), req, reg);
        }
    }
}
