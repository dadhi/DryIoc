using System;
using System.Collections.Generic;
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

            container.UseInstance("a");
            container.UseInstance("z");
        }

        [Test]
        public void Can_reregister_instance_with_different_reuse()
        {
            var container = new Container();

            container.UseInstance("a");
            Assert.AreEqual("a", container.Resolve<string>());

            using (var scope = container.OpenScope())
            {
                scope.UseInstance("b");
                Assert.AreEqual("b", scope.Resolve<string>());
            }

            Assert.AreEqual("a", container.Resolve<string>());
        }

        [Test]
        public void Possible_to_Register_pre_created_instance_of_runtime_service_type()
        {
            var container = new Container();

            container.UseInstance(typeof(string), "ring", serviceKey: "MyPrecious");

            var ring = container.Resolve<string>("MyPrecious");
            Assert.That(ring, Is.EqualTo("ring"));
        }

        [Test]
        public void Registering_pre_created_instance_not_assignable_to_runtime_service_type_should_Throw()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.UseInstance(typeof(int), "ring", serviceKey: "MyPrecious"));

            Assert.AreEqual(ex.Error, Error.RegisteringInstanceNotAssignableToServiceType);
        }

        [Test]
        public void Wiping_cache_should_not_delete_current_instance_value()
        {
            var container = new Container();
            container.UseInstance("mine");

            var mine = container.WithoutCache().Resolve<string>();
            Assert.AreEqual("mine", mine);
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

            container.UseInstance("hey");
            var regBefore = container.GetServiceRegistrations().Single();

            container.UseInstance("nah");
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

            c.RegisterInstance<I>(new C(), new ShortReuse());
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
            container.UseInstance("a");

            container.RegisterInstance("x", ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var s = container.Resolve<string>();
            Assert.AreEqual("a", s);
        }

        [Test]
        public void For_multiple_registered_instances_Then_Replace_should_replace_them_all()
        {
            var container = new Container();

            container.UseInstance("a");
            container.UseInstance("b");
            container.UseInstance("x");

            var x = container.Resolve<string>();
            Assert.AreEqual("x", x);
        }

        [Test]
        public void Replace_keyed_instance_registration()
        {
            var container = new Container();

            container.UseInstance("a", serviceKey: "x");
            Assert.AreEqual("a", container.Resolve<string>(serviceKey: "x"));

            container.UseInstance("b", serviceKey: "x");
            Assert.AreEqual("b", container.Resolve<string>(serviceKey: "x"));
        }

        [Test]
        public void Throws_on_attempt_to_replace_keyed_instance_registration_of_non_instance_factory()
        {
            var container = new Container();

            container.RegisterDelegate(_ => "a", serviceKey: "x");
            Assert.AreEqual("a", container.Resolve<string>(serviceKey: "x"));

            var ex = Assert.Throws<ContainerException>(() => 
                container.UseInstance("b", serviceKey: "x"));
            //Assert.AreEqual(Error.NameOf(), Error.NameOf(ex.Error));
        }

        [Test]
        public void Can_register_with_and_then_without_PreventDisposal_behavior()
        {
            var container = new Container();

            container.UseInstance("a", preventDisposal: true);
            container.Resolve<string>();

            container.UseInstance("a");
            container.Resolve<string>();
        }

        public class Me : IDisposable
        {
            public void Dispose()
            {
            }
        }

        [Test]
        public void Can_use_intstance_of_Int_type()
        {
            var container = new Container();
            container.UseInstance<int>(42);

            var int42 = container.Resolve<int>();

            Assert.AreEqual(42, int42);
        }

        [Test]
        public void Can_work_with_multiple_keyed_and_default_instances()
        {
            var container = new Container();
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");

            var forties = container.Resolve<int[]>();

            Assert.AreEqual(3, forties.Length);
        }

        [Test]
        public void Can_reuse_the_default_factory()
        {
            var container = new Container();
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");
            container.UseInstance<int>(45); // will replace the 43

            var forties = container.Resolve<int[]>();

            CollectionAssert.AreEquivalent(new[] { 42, 45, 44 }, forties);
        }

        [Test]
        public void Wont_reuse_the_default_factory_if_reuse_is_different()
        {
            var container = new Container();
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");

            using (var scope = container.OpenScope())
            {
                scope.UseInstance<int>(45);
                CollectionAssert.AreEquivalent(new[] { 42, 43, 44, 45 }, scope.Resolve<int[]>());
            }

            CollectionAssert.AreEquivalent(new[] { 42, 43, 44 }, container.Resolve<int[]>());
        }

        [Test]
        public void Wont_reuse_the_default_factory_if_reuse_is_different_for_lazy_collection()
        {
            var container = new Container(rules => rules.WithResolveIEnumerableAsLazyEnumerable());
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");

            using (var scope = container.OpenScope())
            {
                scope.UseInstance<int>(45);
                CollectionAssert.AreEquivalent(new[] { 42, 43, 44, 45 }, scope.Resolve<IEnumerable<int>>().ToArray());
            }

            CollectionAssert.AreEquivalent(new[] { 42, 43, 44 }, container.Resolve<IEnumerable<int>>().ToArray());
        }

        [Test]
        public void The_used_instance_dependency_should_be_independent_of_scope()
        {
            var container = new Container();
            container.Register<BB>();

            using (var scope = container.OpenScope())
            {
                var a = new AA();
                scope.UseInstance(a);
        
                var bb = scope.Resolve<BB>(); // will inject `a`
                Assert.AreSame(a, bb.A);
            }

            var anotherA = new AA();
            container.UseInstance(anotherA);
            var anotherBB = container.Resolve<BB>(); // will inject `anotherA`
            Assert.AreSame(anotherA, anotherBB.A);
        }

        public class AA { }

        public class BB
        {
            public readonly AA A;
            public BB(AA a) { A = a; }
        }
    }
}
