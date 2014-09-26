using System.Linq;
using System.Reflection;
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
            container.Register<ClientWithStringParam>(setup: Setup.With(parameters: Parameters.All.With("x", "hola")));

            var client = container.Resolve<ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void CanNot_inject_primitive_value_of_different_type()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(setup: Setup.With(parameters: Parameters.All.With("x", 500)));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithStringParam>());

            Assert.That(ex.Message, Is.StringContaining("Injected value 500 is not assignable to String parameter \"x\""));
        }

        [Test]
        public void Can_inject_primitive_value_and_resolve_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(setup: Setup.With(parameters: Parameters.All.With("x", "hola")));
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
                setup: Setup.With(parameters: Parameters.AllDefaultIfUnresolved.With("x", "hola")));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void Can_inject_resolved_value_per_parameter()
        {
            var container = new Container();
            container.Register<Service>(named: "service");
            container.Register<ClientWithServiceAndStringParam>(setup: Setup.With(parameters:
                Parameters.All.With("x", "hola").With("service", r => r.Resolve<IService>("service", requiredServiceType: typeof(Service)))));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_nicely_specify_required_type_and_key_per_parameter()
        {
            var container = new Container();
            container.Register<Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringParam>(setup: Setup.With(
                parameters: Parameters.All.With("x", "hola").With("service", typeof(Service), "dependency")));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void Can_setup_parameter_base_on_its_type()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(
                setup: Setup.With(parameters: Parameters.AllDefaultIfUnresolved.With("x", "hola")));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void Can_specify_how_to_resolve_property_per_registration()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None
                    .With("Message", "hell")
                    .With("Service", r => r.Resolve<IService>("dependency"))));

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Message, Is.EqualTo("hell"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Should_throw_if_property_is_not_found()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.With("WrongName", "wrong name")));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithServiceAndStringProperty>());

            Assert.That(ex.Message, Is.StringContaining("Unable to find property or field \"WrongName\" when resolving"));
        }

        [Test]
        public void Should_simply_specify_that_all_assignable_properties_and_fiels_should_be_resolved()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.AllPublicNonPrimitive));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.F, Is.InstanceOf<Service>());
            Assert.That(client.P, Is.InstanceOf<Service>());
            Assert.That(client.PNonResolvable, Is.Null);
        }

        [Test]
        public void I_can_resolve_property_with_private_setter()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.With("PWithPrivateSetter")));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithPrivateSetter, Is.InstanceOf<Service>());
        }

        [Test]
        public void I_can_resolve_private_property()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.With("_pPrivate")));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingPrivateProperty, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_private_field()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.With("_fPrivate")));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingField, Is.InstanceOf<Service>());
        }

        [Test]
        public void When_resolving_readonly_field_it_should_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.With("FReadonly")));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ClientWithPropsAndFields>());

            Assert.That(ex.Message, Is.StringContaining("Specified field \"FReadonly\" is readonly when resolving"));
        }

        [Test][Ignore]
        public void Only_not_assigned_properies_and_fields_should_be_resolved_So_that_assgined_field_value_should_Not_change()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "another");
            container.Register<ClientWithAssignedProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.With("Service", serviceKey: "another")));

            var client = container.Resolve<ClientWithAssignedProperty>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Only_non_primitive_properies_and_fields_should_be_resolved()
        {
            var container = new Container();

            container.Register<IService, Service>();
            container.RegisterInstance("Hello string!");
            container.Register<ClientWithServiceAndStringProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.AllPublicNonPrimitive));

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.Message, Is.Null);
        }

        [Test]
        public void Can_specify_all_to_throw_if_Any_property_is_unresolved()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.All(IfUnresolved.Throw)));

            Assert.Throws<ContainerException>(() => 
                container.Resolve<ClientWithServiceAndStringProperty>());
        }

        [Test]
        public void I_can_easily_exclude_fields_from_resolution()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.All(except: _ => _.MemberType == MemberTypes.Field)));

            var client = container.Resolve<ClientWithPropsAndFields>();
            Assert.That(client.F, Is.Null);
            Assert.That(client.P, Is.InstanceOf<Service>());
        }

        [Test]
        public void I_can_easily_exclude_properties_from_resolution()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.All(except: _ => _.MemberType == MemberTypes.Property)));

            var client = container.Resolve<ClientWithPropsAndFields>();
            Assert.That(client.F, Is.InstanceOf<Service>());
            Assert.That(client.P, Is.Null);
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

        public class ClientWithPropsAndFields
        {
            public IService F;
            // ReSharper disable UnassignedReadonlyField
            public readonly IService FReadonly;
            public IService PWithBackingField { get { return _fPrivate; } }
            // ReSharper restore UnassignedReadonlyField

            // ReSharper disable UnassignedField.Compiler
            private IService _fPrivate;
            // ReSharper restore UnassignedField.Compiler

            public IService P { get; set; }
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public IService PWithPrivateSetter { get; private set; }
            private IService _pPrivate { get; set; }
            public IService PWithBackingPrivateProperty { get { return _pPrivate; } }

            // ReSharper restore UnusedAutoPropertyAccessor.Local

            public AnotherService PNonResolvable { get; set; }
        }

        public class ClientWithAssignedProperty
        {
            public IService Service { get; set; }

            public ClientWithAssignedProperty(IService service)
            {
                Service = service;
            }
        }

        #endregion
    }
}