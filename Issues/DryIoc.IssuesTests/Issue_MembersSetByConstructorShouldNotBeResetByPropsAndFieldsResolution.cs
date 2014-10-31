using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue_MembersSetByConstructorShouldNotBeResetByPropsAndFieldsResolution
    {
        [Test]
        [Ignore("Not supported cause properties are set as: new Blah { Prop1 = x, Prop2 = y }")]
        public void Only_not_assigned_properies_and_fields_should_be_resolved_So_that_assigned_field_value_should_Not_change()
        {
            var container = new Container();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "another");
            container.Register<ClientWithAssignedProperty>(setup: Setup.With(
                propertiesAndFields: PropertiesAndFields.None.And("Service", serviceKey: "another")));

            var client = container.Resolve<ClientWithAssignedProperty>();

            Assert.That(client.Dep, Is.InstanceOf<Service>());
        }

        internal interface IService {}

        internal class Service : IService {}
        internal class AnotherService : IService {}

        public class ClientWithAssignedProperty
        {
            public IService Dep { get; set; }

            public ClientWithAssignedProperty(IService dep)
            {
                Dep = dep;
            }
        }
    }
}
