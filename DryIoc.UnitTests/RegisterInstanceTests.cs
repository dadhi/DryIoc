using System.Linq;
using System.Linq.Expressions;
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

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<string>());

            Assert.AreEqual(Error.UnableToResolveFromRegisteredServices, ex.Error);
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

        interface I { }
        class C : I { }
        class D { public D(I i) { } }

        [Test]
        public void Should_throw_on_reuse_mismatch()
        {
            var c = new Container();

            c.RegisterInstance<I>(new C(), reuse: new ShortReuse());
            c.Register<D>(Reuse.Singleton);

            var ex = Assert.Throws<ContainerException>(() =>
                c.Resolve<D>());

            Assert.AreEqual(Error.DependencyHasShorterReuseLifespan, ex.Error);
        }

        class ShortReuse : IReuse
        {
            public int Lifespan { get { return 50; } }

            public IScope GetScopeOrDefault(Request request)
            {
                return request.Scopes.SingletonScope;
            }

            public Expression GetScopeExpression(Request request)
            {
                return Expression.Property(Container.ScopesExpr, "SingletonScope");
            }

            public int GetScopedItemIdOrSelf(int factoryID, Request request)
            {
                return request.Scopes.SingletonScope.GetScopedItemIdOrSelf(factoryID);
            }
        }

        [Test]
        public void Can_register_instance_with_keep_option()
        {
            var container = new Container();
            container.RegisterInstance("a");

            container.RegisterInstance("x", ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var s = container.Resolve<string>();
            Assert.AreEqual("a", s);
        }

        [Test]
        public void Given_multiple_already_registered_services_When_registering_with_keep_option_Then_no_exception_should_be_thrown()
        {
            var container = new Container();

            container.RegisterInstance("a");
            container.RegisterInstance("b");

            Assert.DoesNotThrow(() => 
            container.RegisterInstance("x", ifAlreadyRegistered: IfAlreadyRegistered.Keep));
        }

        [Test]
        public void Given_multiple_already_registered_services_When_registering_with_Replace_option_Then_no_exception_should_be_thrown()
        {
            var container = new Container();

            container.RegisterInstance("a");
            container.RegisterInstance("b");

            Assert.DoesNotThrow(() =>
            container.RegisterInstance("x", ifAlreadyRegistered: IfAlreadyRegistered.Replace));

            var strings = container.Resolve<string[]>();
            CollectionAssert.AreEqual(new[] { "a", "x" }, strings);
        }
    }
}
