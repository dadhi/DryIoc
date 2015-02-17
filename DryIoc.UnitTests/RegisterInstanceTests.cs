using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterInstanceTests
    {
        [Test]
        public void Can_replace_instance_without_replacing_factory()
        {
            var container = new Container();

            container.RegisterInstance("a", Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            container.RegisterInstance("b", Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        }

        [Test]
        public void Registering_instance_with_not_available_reuse_should_throw_meaningful_error()
        {
            var container = new Container();
            
            var ex = Assert.Throws<ContainerException>(() => 
                container.RegisterInstance("a", Reuse.InCurrentNamedScope("b")));

            Assert.AreEqual(ex.Error, Error.NO_CURRENT_SCOPE);
        }

        [Test]
        public void Can_reregister_instance_with_different_reuse()
        {
            var container = new Container();

            container.RegisterInstance("a", Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            using (container.OpenScope())
            {
                container.RegisterInstance("b", Reuse.InCurrentScope, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
                Assert.AreEqual("b", container.Resolve<string>());
            }

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<string>());
            Assert.AreEqual(ex.Error, Error.NO_CURRENT_SCOPE);
        }

        [Test]
        public void Possible_to_Register_pre_created_instance_of_runtime_service_type()
        {
            var container = new Container();

            container.RegisterInstance(typeof(string), "ring", named: "MyPrecious");

            var ring = container.Resolve<string>("MyPrecious");
            Assert.That(ring, Is.EqualTo("ring"));
        }

        [Test]
        public void Registering_pre_created_instance_not_assignable_to_runtime_service_type_should_Throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.RegisterInstance(typeof(int), "ring", named: "MyPrecious"));

            Assert.AreEqual(ex.Error, Error.REGED_OBJ_NOT_ASSIGNABLE_TO_SERVICE_TYPE);
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
        public void Wiping_singletons_should_not_delete_current_instance_value()
        {
            var container = new Container();
            container.RegisterInstance("mine", Reuse.Singleton);

            var ex = Assert.Throws<ContainerException>(() =>
                container.WithoutSingletonsAndCache().Resolve<string>());

            Assert.AreEqual(ex.Error, Error.UNABLE_TO_RESOLVE_SERVICE);
        }

        [Test]
        public void Register_instance_with_different_if_already_registered_policies()
        {
            var container = new Container();
            container.RegisterInstance("mine", Reuse.Singleton);
            Assert.AreEqual("mine", container.Resolve<string>());

            container.RegisterInstance("yours", Reuse.Singleton);

            CollectionAssert.AreEquivalent(new[] { "mine", "yours" },
                container.Resolve<string[]>());
        }

        [Test]
        public void Register_instance_in_resolution_scope_is_the_same_as_delegate()
        {
            var container = new Container();
            container.RegisterInstance("xxx", Reuse.InResolutionScope);

            Assert.NotNull(container.Resolve<string>());
        }
    }
}
