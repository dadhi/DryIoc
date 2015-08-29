using System.Linq;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterInstanceTests
    {
        [Test]
        public void Can_replace_instance_without_replacing_factory_and_without_exceptions()
        {
            var container = new Container();

            container.RegisterInstance("a", Reuse.Singleton, IfAlreadyRegistered.Replace);
            container.RegisterInstance("z", Reuse.Singleton, IfAlreadyRegistered.Replace);
        }

        [Test]
        public void Registering_instance_with_not_available_reuse_should_throw_meaningful_error()
        {
            var container = new Container();
            
            var ex = Assert.Throws<ContainerException>(() => 
                container.RegisterInstance("a", Reuse.InCurrentNamedScope("b")));

            Assert.AreEqual(Error.NoMatchingScopeWhenRegisteringInstance, ex.Error);
        }

        [Test]
        public void Can_reregister_instance_with_different_reuse()
        {
            var container = new Container();

            container.RegisterInstance("a", Reuse.Singleton, IfAlreadyRegistered.Replace);

            using (var scope = container.OpenScope())
            {
                scope.RegisterInstance("bbbb", Reuse.InCurrentScope, IfAlreadyRegistered.Replace);
                Assert.AreEqual("bbbb", scope.Resolve<string>());
            }

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<string>());
            Assert.AreEqual(ex.Error, Error.NoCurrentScope);
        }

        [Test]
        public void Possible_to_Register_pre_created_instance_of_runtime_service_type()
        {
            var container = new Container();

            container.RegisterInstance(typeof(string), "ring", serviceKey: "MyPrecious");

            var ring = container.Resolve<string>("MyPrecious");
            Assert.That(ring, Is.EqualTo("ring"));
        }

        [Test]
        public void Registering_pre_created_instance_not_assignable_to_runtime_service_type_should_Throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.RegisterInstance(typeof(int), "ring", serviceKey: "MyPrecious"));

            Assert.AreEqual(ex.Error, Error.RegisteringInstanceNotAssignableToServiceType);
        }

        [Test]
        public void Wiping_cache_should_not_delete_current_instance_value()
        {
            var container = new Container();
            container.RegisterInstance("mine", Reuse.Singleton);

            var mine = container.WithoutCache().Resolve<string>();
            Assert.AreEqual("mine", mine);
        }

        [Test]
        public void Register_instance_with_different_if_already_registered_policies()
        {
            var container = new Container();
            container.RegisterInstance("nya", Reuse.Singleton);
            Assert.AreEqual("nya", container.Resolve<string>());

            container.RegisterInstance("yours", Reuse.Singleton);

            CollectionAssert.AreEquivalent(new[] { "nya", "yours" },
                container.Resolve<string[]>());
        }

        [Test]
        public void Register_instance_in_resolution_scope_does_not_make_sense_and_should_throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() => 
            container.RegisterInstance("xxx", Reuse.InResolutionScope));
        
            Assert.AreEqual(Error.ResolutionScopeIsNotSupportedForRegisterInstance, ex.Error);
        }

        [Test]
        public void Register_instance_with_replace_option_will_replace_registered_instance_in_place()
        {
            var container = new Container();

            container.RegisterInstance("hey");
            var regBefore = container.GetServiceRegistrations().Single();

            container.RegisterInstance("nah", ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var regAfter = container.GetServiceRegistrations().Single();

            Assert.AreEqual(regBefore.Factory.FactoryID, regAfter.Factory.FactoryID);
        }
    }
}
