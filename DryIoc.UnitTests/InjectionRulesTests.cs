using System.Linq;
using System.Reflection;
using DryIoc.UnitTests.CUT;
using Me;
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

            container.Register<SomeBlah>(made: Made.Of(propertiesAndFields:
                r => r.ImplementationType.GetTypeInfo().DeclaredProperties.Select(PropertyOrFieldServiceInfo.Of)));
            container.Register<IService, Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        [Test]
        public void Specify_property_selector_with_custom_service_type_when_registering_service()
        {
            var container = new Container();

            container.Register<SomeBlah>(made: Made.Of(propertiesAndFields:
                r => r.ImplementationType.GetTypeInfo().DeclaredProperties.Select(p =>
                    p.Name.Equals("Uses") ? PropertyOrFieldServiceInfo.Of(p)
                        .WithDetails(ServiceDetails.Of(typeof(Service)), r) : null)));
            container.Register<Service>();

            var blah = container.Resolve<SomeBlah>();
            Assert.That(blah.Uses, Is.InstanceOf<Service>());
        }

        [Test]
        public void Should_simply_specify_that_all_assignable_properties_and_fiels_should_be_resolved()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterMany<ClientWithPropsAndFields>(made: Made.Of(propertiesAndFields: PropertiesAndFields.Auto));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.F, Is.InstanceOf<Service>());
            Assert.That(client.P, Is.InstanceOf<Service>());
            Assert.That(client.PNonResolvable, Is.Null);
        }

        [Test]
        public void I_can_resolve_property_with_internal_setter()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: "key");
            container.RegisterMany(Made.Of(() => new ClientWithPropsAndFields { PWithInternalSetter = Arg.Of<IService>(IfUnresolved.Throw, "key") }));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithInternalSetter, Is.InstanceOf<Service>());
        }

        [Test]
        public void I_can_specify_to_throw_if_property_is_unresolved()
        {
            var container = new Container();
            container.RegisterMany(Made.Of(() => new ClientWithPropsAndFields { PWithInternalSetter = Arg.Of<IService>(IfUnresolved.Throw, "key") }));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ClientWithPropsAndFields>());

            Assert.AreEqual(ex.Error, Error.UnableToResolveUnknownService);
        }

        [Test]
        public void I_can_resolve_internal_property()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterMany(Made.Of(() => new ClientWithPropsAndFields { PInternal = default(IService) }));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingInternalProperty, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_private_field()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterMany<ClientWithPropsAndFields>(made: PropertiesAndFields.Of.Name("_fPrivate"));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.PWithBackingField, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_only_properties()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(serviceKey: "another");

            container.RegisterMany<ClientWithPropsAndFields>(
                made: PropertiesAndFields.All(withFields: false).And(PropertiesAndFields.Of.Name("PInternal", serviceKey: "another")));

            var client = container.Resolve<ClientWithPropsAndFields>();

            Assert.That(client.P, Is.InstanceOf<Service>());
            Assert.That(client.PWithBackingInternalProperty, Is.InstanceOf<AnotherService>());
            Assert.That(client.F, Is.Null);
            Assert.That(client.PWithBackingField, Is.Null);
            Assert.That(client.PNonResolvable, Is.Null);
        }

        [Test]
        public void When_resolving_readonly_field_it_should_throw()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.RegisterMany<ClientWithPropsAndFields>(made: PropertiesAndFields.Of.Name("FReadonly"));

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
            container.Register<ClientWithServiceAndStringProperty>(made: PropertiesAndFields.Auto);

            var client = container.Resolve<ClientWithServiceAndStringProperty>();

            Assert.That(client.Service, Is.InstanceOf<Service>());
            Assert.That(client.Message, Is.Null);
        }

        [Test]
        public void Can_specify_all_to_throw_if_Any_property_is_unresolved()
        {
            var container = new Container();
            container.Register<ClientWithServiceAndStringProperty>(made: PropertiesAndFields.All(ifUnresolved: IfUnresolved.Throw));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ClientWithServiceAndStringProperty>());
        }

        [Test]
        public void Indexer_properties_should_be_ignored_by_All_properties_discovery()
        {
            var container = new Container();
            container.Register<FooWithIndexer>(made:
                PropertiesAndFields.All(ifUnresolved: IfUnresolved.Throw));

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
            container.Register<Client>(made: Parameters.Of.Name("dep", defaultValue: defaultDep));

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.SameAs(defaultDep));
        }

        [Test]
        public void Can_specify_for_param_type_default_value_if_unresolved()
        {
            var container = new Container();
            var defaultDep = new Dep();
            container.Register<Client>(made: Parameters.Of.Type<Dep>(defaultValue: defaultDep));

            var client = container.Resolve<Client>();

            Assert.That(client.Dep, Is.SameAs(defaultDep));
        }

        [Test]
        public void Can_pass_parent_type_as_string_param_to_dependency()
        {
            var container = new Container();

            container.Register<MyService>();
            container.Register<ConnectionStringProvider>(Reuse.Singleton);
            container.Register<IConnectionStringProvider, ConnectionNamingConnectionStringProvider>(
                made: Made.Of(parameters: request => parameter =>
                {
                    if (parameter.ParameterType != typeof(string))
                        return null;
                    var targetType = request.Parent.ImplementationType;
                    var targetName = string.Format("{0}.{1}", targetType.Namespace, targetType.Name);
                    return ParameterServiceInfo.Of(parameter)
                        .WithDetails(ServiceDetails.Of(defaultValue: targetName), request);
                }));


            var service = container.Resolve<MyService>();

            var provider = service.ConnectionProvider as ConnectionNamingConnectionStringProvider;
            Assert.IsNotNull(provider);
            Assert.AreEqual("Me.MyService", provider.TargetName);
        }

        [Test]
        public void Can_pass_parent_type_as_string_param_to_dependency_using_factory_method()
        {
            var container = new Container();

            container.Register<MyService>();
            container.Register<ConnectionStringProvider>(Reuse.Singleton);
            container.Register<IConnectionStringProvider, ConnectionNamingConnectionStringProvider>(
                Made.Of(() => new ConnectionNamingConnectionStringProvider(default(ConnectionStringProvider), Arg.Of<string>("targetName"))));

            container.Register<string>(serviceKey: "targetName",
                made: Made.Of(r =>
                {
                    var method = GetType().GetMethodOrNull("GetTargetName");
                    var targetType = r.Parent.Parent.ImplementationType;
                    return FactoryMethod.Of(method.MakeGenericMethod(targetType));
                }));

            var service = container.Resolve<MyService>();

            var provider = service.ConnectionProvider as ConnectionNamingConnectionStringProvider;
            Assert.IsNotNull(provider);
            Assert.AreEqual("Me.MyService", provider.TargetName);
        }

        public static string GetTargetName<TTarget>()
        {
            return string.Format("{0}.{1}", typeof(TTarget).Namespace, typeof(TTarget).Name);
        }

        [Test]
        public void If_you_register_not_a_primitive_value_then_container_should_throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
                container.Register<A>(made: Parameters.Of.Type(_ => new B())));

            Assert.AreEqual(Error.RegisteringWithNotSupportedDepedendencyCustomValueType, ex.Error);
        }

        class A
        {
            public B B { get; private set; }

            public A(B b)
            {
                B = b;
            }
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
            public IService PWithInternalSetter { get; internal set; }
            internal IService PInternal { get; set; }
            public IService PWithBackingInternalProperty { get { return PInternal; } }

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

namespace Me
{
    public class ConnectionStringProvider { }

    public interface IConnectionStringProvider { }

    internal class ConnectionNamingConnectionStringProvider : IConnectionStringProvider
    {
        public ConnectionStringProvider ConnectionStringProvider { get; private set; }
        public string TargetName { get; private set; }

        public ConnectionNamingConnectionStringProvider(ConnectionStringProvider connectionStringProvider, string targetName)
        {
            ConnectionStringProvider = connectionStringProvider;
            TargetName = targetName;
        }
    }

    public class MyService
    {
        public IConnectionStringProvider ConnectionProvider { get; private set; }
        public MyService(IConnectionStringProvider connectionProvider)
        {
            ConnectionProvider = connectionProvider;
        }
    }
}