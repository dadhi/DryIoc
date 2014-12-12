using System;
using System.Linq;
using System.Reflection;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

#pragma warning disable 0649 // Field '...' is never assigned to, and will always have its default value null

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class InjectionRulesTests
    {
        [Test]
        public void Specify_property_selector_when_registering_service()
        {
            var container = new Container();

            container.Register<SomeBlah>(inject: InjectionRules.With(propertiesAndFields:
                r => r.ImplementationType.GetTypeInfo().DeclaredProperties.Select(PropertyOrFieldServiceInfo.Of)));
            container.Register<IService, Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        [Test]
        public void Specify_property_selector_with_custom_service_type_when_registering_service()
        {
            var container = new Container();

            container.Register<SomeBlah>(inject: InjectionRules.With(propertiesAndFields:
                r => r.ImplementationType.GetTypeInfo().DeclaredProperties.Select(p =>
                    p.Name.Equals("Uses") ? PropertyOrFieldServiceInfo.Of(p).WithDetails(ServiceInfoDetails.Of(typeof(Service)), r) : null)));
            container.Register<Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_inject_primitive_value()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(inject: Parameters.Of.Name("x", "hola"));

            var client = container.Resolve<ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void Can_inject_primitive_value_by_type()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(inject: Parameters.Of.Name("x", "hola"));

            var client = container.Resolve<ClientWithStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void CanNot_inject_primitive_value_of_different_type()
        {
            var container = new Container();
            container.Register<ClientWithStringParam>(inject: Parameters.Of.Name("x", 500));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithStringParam>());

            Assert.That(ex.Message, Is.StringContaining("Injected value 500 is not assignable to String {with custom value} as parameter \"x\""));
        }

        [Test]
        public void Can_inject_primitive_value_and_resolve_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(inject: Parameters.Of.Type(typeof(string), "hola"));
            container.Register<IService, Service>();

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_inject_primitive_value_and_handle_ReturnDefault_for_the_rest_of_parameters()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(inject: Parameters.DefaultIfUnresolved.Name("x", "hola"));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void Can_inject_resolved_value_per_parameter()
        {
            var container = new Container();
            container.Register<Service>(named: "service");
            container.Register<ClientWithServiceAndStringParam>(inject: Parameters.Of
                .Name("x", "hola")
                .Name("service", r => r.Resolve<IService>("service", requiredServiceType: typeof(Service))));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_inject_resolved_value_per_parameter_based_on_type()
        {
            var container = new Container();
            container.Register<Service>();
            container.Register<ClientWithServiceAndStringParam>(inject:
                Parameters.Of.Name("x", "hola").Type(typeof(Service), r => r.Resolve<IService>(typeof(Service))));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_nicely_specify_required_type_and_key_per_parameter()
        {
            var container = new Container();
            container.Register<Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringParam>(inject:
                Parameters.Of.Name("x", "hola").Name("service", typeof(Service), "dependency"));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void Can_nicely_specify_required_type_and_key_per_parameter_based_on_arbitrarily_condition()
        {
            var container = new Container();
            container.Register<Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringParam>(inject: Parameters.Of
                    .Name("x", "hola")
                    .Condition(p => typeof(IService).IsAssignableTo(p.ParameterType), typeof(Service), "dependency"));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.X, Is.EqualTo("hola"));
        }

        [Test]
        public void Can_setup_parameter_base_on_its_type()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringParam>(inject: Parameters.DefaultIfUnresolved.Name("x", "hola"));

            var client = container.Resolve<ClientWithServiceAndStringParam>();

            Assert.That(client.X, Is.EqualTo("hola"));
            Assert.That(client.Service, Is.Null);
        }

        [Test]
        public void Can_specify_how_to_resolve_property_per_registration()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringProperty>(inject: PropertiesAndFields.Of
                    .Name("Message", "hell")
                    .Name("Service", r => r.Resolve<IService>("dependency")));

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Message, Is.EqualTo("hell"));
            Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_specify_how_to_resolve_property_per_registration_based_on_its_type()
        {
            var container = new Container();
            container.Register<IService, Service>(named: "dependency");
            container.Register<ClientWithServiceAndStringProperty>(inject: PropertiesAndFields.Of
                .Type(typeof(string), "hell"));
                    //.Name("Service", r => r.Resolve<IService>("dependency"))));

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Message, Is.EqualTo("hell"));
            //Assert.That(client.Service, Is.InstanceOf<Service>());
        }

        [Test]
        public void Should_throw_if_property_is_not_found()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringProperty>(inject: 
                PropertiesAndFields.Of.Name("WrongName", "wrong name"));

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<ClientWithServiceAndStringProperty>());

            Assert.That(ex.Message, Is.StringContaining("Unable to find writable property or field \"WrongName\" when resolving"));
        }

        [Test]
        public void Should_simply_specify_that_all_assignable_properties_and_fiels_should_be_resolved()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields.PublicNonPrimitive);

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
            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields.Of.Name("PWithPrivateSetter"));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithPrivateSetter, Is.InstanceOf<Service>());
        }

        [Test]
        public void I_can_resolve_private_property()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields.Of.Name("_pInternal"));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingPrivateProperty, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_private_field()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields.Of.Name("_fPrivate"));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingField, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_fields()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields.Of.Condition(m => m is FieldInfo));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingField, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_fields_into_one_service_and_properties_to_another()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "another");

            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields
                    .All(PropertiesAndFields.Include.All)
                    .Condition(m => m is FieldInfo, serviceKey: "another"));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.P, Is.InstanceOf<Service>());
            Assert.That(client.PWithBackingPrivateProperty, Is.InstanceOf<Service>());
            Assert.That(client.F, Is.InstanceOf<AnotherService>());
            Assert.That(client.PWithBackingField, Is.InstanceOf<AnotherService>());
            Assert.That(client.PNonResolvable, Is.Null);
        }

        [Test]
        public void When_resolving_readonly_field_it_should_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: PropertiesAndFields.Of.Name("FReadonly"));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ClientWithPropsAndFields>());

            Assert.That(ex.Message, Is.StringContaining("Unable to find writable property or field \"FReadonly\" when resolving"));
        }

        [Test]
        public void Only_non_primitive_properies_and_fields_should_be_resolved()
        {
            var container = new Container();

            container.Register<IService, Service>();
            container.RegisterInstance("Hello string!");
            container.Register<ClientWithServiceAndStringProperty>(inject: PropertiesAndFields.PublicNonPrimitive);

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.Message, Is.Null);
        }

        [Test]
        public void Can_specify_all_to_throw_if_Any_property_is_unresolved()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringProperty>(inject: PropertiesAndFields.All(IfUnresolved.Throw));

            Assert.Throws<ContainerException>(() => 
                container.Resolve<ClientWithServiceAndStringProperty>());
        }

        [Test]
        public void I_can_easily_exclude_fields_from_resolution()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: 
                PropertiesAndFields.PublicNonPrimitive.Except(m => m is FieldInfo));

            var client = container.Resolve<ClientWithPropsAndFields>();
            Assert.That(client.F, Is.Null);
            Assert.That(client.P, Is.InstanceOf<Service>());
        }

        [Test]
        public void I_can_easily_exclude_properties_from_resolution()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterAll<ClientWithPropsAndFields>(rules: 
                PropertiesAndFields.PublicNonPrimitive.Except(m => m is PropertyInfo));

            var client = container.Resolve<ClientWithPropsAndFields>();
            Assert.That(client.F, Is.InstanceOf<Service>());
            Assert.That(client.P, Is.Null);
        }

        [Test]
        public void Resolve_parameter_customly_inside_Func_should_happen_when_calling_Func_not_on_resolve()
        {
            var container = new Container();

            var resolved = false;
            container.Register<ClientWithStringParam>(inject: 
                Parameters.Of.Name("x", _ =>
                {
                    resolved = true;
                    return "resolved";
                }));

            var getClient = container.Resolve<Func<ClientWithStringParam>>();

            Assert.That(resolved, Is.False);
            Assert.That(getClient().X, Is.EqualTo("resolved"));
            Assert.That(resolved, Is.True);
        }

        [Test]
        public void Indexer_properties_should_be_ignored_by_All_properties_discovery()
        {
            var container = new Container();
            container.Register<FooWithIndexer>(inject: 
                PropertiesAndFields.All(IfUnresolved.Throw, PropertiesAndFields.Include.All));

            Assert.DoesNotThrow(() => 
                container.Resolve<FooWithIndexer>());
        }

        [Test]
        public void Should_support_optional_constructor_parameters_if_dependency_registered_for_parameter()
        {
            var container = new Container();
            container.Register<Client>();
            container.Register<Dep>();

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.InstanceOf<Dep>());
        }

        [Test]
        public void Should_automatically_specify_IfUnresolvedReturnDefault_for_optional_parameters()
        {
            var container = new Container();
            container.Register<Client>();

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.Null);
        }

        [Test]
        public void Can_specify_for_param_name_default_value_if_unresolved()
        {
            var container = new Container();
            var defaultDep = new Dep();
            container.Register<Client>(inject: Parameters.Of.Name("dep", defaultValue: defaultDep));

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.SameAs(defaultDep));
        }

        [Test]
        public void Can_specify_for_param_type_default_value_if_unresolved()
        {
            var container = new Container();
            var defaultDep = new Dep();
            container.Register<Client>(inject: Parameters.Of.Type(typeof(Dep), defaultValue: defaultDep));

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.SameAs(defaultDep));
        }

        #region CUT

        internal class FooWithIndexer
        {
            public object this[int index]
            {
                get { return null; }
                // ReSharper disable ValueParameterNotUsed
                set { }
                // ReSharper restore ValueParameterNotUsed
            }
        }

        internal class SomeBlah
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

        internal class ClientWithServiceAndStringParam
        {
            public IService Service { get; private set; }
            public string X { get; private set; }

            public ClientWithServiceAndStringParam(IService service, string x)
            {
                Service = service;
                X = x;
            }
        }

        internal class ClientWithServiceAndStringProperty
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

            internal IService _fPrivate;

            public IService P { get; set; }
            public IService PWithPrivateSetter { get; internal set; }
            internal IService _pInternal { get; set; }
            public IService PWithBackingPrivateProperty { get { return _pInternal; } }

            public AnotherService PNonResolvable { get; set; }
        }

        internal class Client
        {
            public Dep Dep { get; set; }

            public Client(Dep dep = null)
            {
                Dep = dep;
            }
        }

        public class Dep { }

        #endregion
    }
}