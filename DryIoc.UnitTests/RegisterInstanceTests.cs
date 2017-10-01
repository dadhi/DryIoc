using System;
using System.Collections.Generic;
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
        public void Register_instance_with_replace_option_will_replace_registered_instance_in_place()
        {
            var container = new Container();

            container.UseInstance("hey");
            var regBefore = container.GetServiceRegistrations().Single();

            container.UseInstance("nah");
            var regAfter = container.GetServiceRegistrations().Single();

            Assert.AreEqual(regBefore.Factory.FactoryID, regAfter.Factory.FactoryID);
        }

        [Test]
        public void Can_register_instance_with_keep_option()
        {
            var container = new Container();
            container.UseInstance("a");

            container.UseInstance("x", ifAlreadyRegistered: IfAlreadyRegistered.Keep);

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
            Assert.AreEqual(Error.NameOf(Error.UnableToUseInstanceForExistingNonInstanceFactory), Error.NameOf(ex.Error));
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
        public void UseInstance_by_default_should_replace_previous_typed_registration()
        {
            var container = new Container();
            container.Register<DependencyA>();

            var aa = new DependencyA();
            container.UseInstance(aa);

            var x = container.Resolve<DependencyA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_append_to_previous_typed_registration()
        {
            var container = new Container();
            container.Register<DependencyA>();

            var aa = new DependencyA();
            container.UseInstance(aa, ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(2, aas.Count());
        }

        [Test]
        public void Can_append_multiple_and_then_replace_all()
        {
            var container = new Container();
            container.Register<DependencyA>();

            var aa = new DependencyA();
            container.UseInstance(aa, ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            container.UseInstance<DependencyA>(new AAA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(3, aas.Count());

            container.UseInstance<DependencyA>(new AAAA());
            var aaaa = container.Resolve<DependencyA>();
            Assert.IsInstanceOf<AAAA>(aaaa);
        }

        [Test]
        public void Can_append_multiple_and_then_replace_all_in_presence_of_keyed()
        {
            var container = new Container();

            container.UseInstance<DependencyA>(new AAA(), serviceKey: "aaa");
            container.UseInstance(new DependencyA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            container.UseInstance(new DependencyA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

            container.UseInstance<DependencyA>(new AAAA());
            Assert.IsInstanceOf<AAAA>(container.Resolve<DependencyA>());
            Assert.IsInstanceOf<AAA>(container.Resolve<DependencyA>("aaa"));
        }

        [Test]
        public void Can_have_both_default_and_keyed_instances_and_keyed_ifAlreadyRegistered_does_not_apply()
        {
            var container = new Container();

            container.UseInstance(new DependencyA());
            container.UseInstance(new DependencyA(), serviceKey: "keyed", ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(2, aas.Count());
        }

        [Test]
        public void Can_append_new_default_with_presence_of_keyyed()
        {
            var container = new Container();

            container.UseInstance(new DependencyA(), serviceKey: "aa");
            container.UseInstance(new DependencyA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            container.UseInstance(new DependencyA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(3, aas.Count());
        }

        [Test]
        public void Can_keep_previous_default()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.UseInstance(aa);
            container.UseInstance(new DependencyA(), ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var x = container.Resolve<DependencyA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_keep_previous_default_with_presence_of_keyed()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.UseInstance(aa);
            container.UseInstance(aa, serviceKey: "aa");
            container.UseInstance(new DependencyA(), ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            var x = container.Resolve<DependencyA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_throw_on_previous_default()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.UseInstance(aa);

            var ex = Assert.Throws<ContainerException>(() => 
            container.UseInstance(new DependencyA(), IfAlreadyRegistered.Throw));

            Assert.AreEqual(
                Error.NameOf(Error.UnableToRegisterDuplicateDefault),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Can_throw_on_previous_default_with_presence_of_keyed()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.UseInstance(aa);
            container.UseInstance(aa, serviceKey: "aa");

            var ex = Assert.Throws<ContainerException>(() =>
                container.UseInstance(new DependencyA(), IfAlreadyRegistered.Throw));

            Assert.AreEqual(
                Error.NameOf(Error.UnableToRegisterDuplicateDefault),
                Error.NameOf(ex.Error));
        }


        [Test]
        public void Can_append_new_default_implementation_And_ignore_the_duplicate_implementation()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.UseInstance(aa);
            container.UseInstance<DependencyA>(new AAA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            Assert.AreEqual(2, container.ResolveMany<DependencyA>().Count());

            var newaaa = new AAA();
            container.UseInstance<DependencyA>(newaaa, ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            container.UseInstance<DependencyA>(new AAAA(), ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);
            Assert.AreEqual(3, container.ResolveMany<DependencyA>().Count());
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
        public void Should_use_correct_instance_in_collection_in_and_out_of_scope()
        {
            var container = new Container();
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");

            using (var scope = container.OpenScope())
            {
                scope.UseInstance<int>(45);
                CollectionAssert.AreEquivalent(new[] { 42, 44, 45 }, scope.Resolve<int[]>());
            }

            CollectionAssert.AreEquivalent(new[] { 42, 43, 44 }, container.Resolve<int[]>());
        }

        [Test]
        public void Should_fallback_to_singleton_in_collection_if_no_scoped_instance()
        {
            var container = new Container();
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");

            using (var scope = container.OpenScope())
            {
                CollectionAssert.AreEquivalent(new[] { 42, 43, 44 }, scope.Resolve<int[]>());
            }

            CollectionAssert.AreEquivalent(new[] { 42, 43, 44 }, container.Resolve<int[]>());
        }

        [Test]
        public void Should_use_correct_instance_in_lazy_collection_in_and_out_of_scope()
        {
            var container = new Container(rules => rules.WithResolveIEnumerableAsLazyEnumerable());
            container.UseInstance<int>(42, serviceKey: "nice number");
            container.UseInstance<int>(43);
            container.UseInstance<int>(44, serviceKey: "another nice number");

            using (var scope = container.OpenScope())
            {
                scope.UseInstance<int>(45);
                CollectionAssert.AreEquivalent(new[] { 42, 44, 45 }, scope.Resolve<IEnumerable<int>>().ToArray());
            }

            CollectionAssert.AreEquivalent(new[] { 42, 43, 44 }, container.Resolve<IEnumerable<int>>().ToArray());
        }

        [Test]
        public void The_used_instance_dependency_should_be_independent_of_scope()
        {
            var container = new Container();
            container.Register<ServiceB>();

            using (var scope = container.OpenScope())
            {
                var dep = new DependencyA();
                scope.UseInstance(dep);
        
                var service = scope.Resolve<ServiceB>(); // will inject `a`
                Assert.AreSame(dep, service.Dep);
            }

            var anotherDep = new DependencyA();
            container.UseInstance(anotherDep);
            var anotherBB = container.Resolve<ServiceB>(); // will inject `anotherA`
            Assert.AreSame(anotherDep, anotherBB.Dep);
        }

        [Test, Ignore("Case relates to #494. Does not work. Fix is hard with current UseInstance impl or likely is impossible.")]
        public void Replacing_typed_registration_via_scoped_instance_should_do_WHAT()
        {
            var container = new Container();
            container.Register<ServiceB>();
            container.Register<DependencyA>();

            var dependencyA = new DependencyA();
            ServiceB service;

            using (var scope = container.OpenScope())
            {
                scope.UseInstance(dependencyA);
                service = scope.Resolve<ServiceB>();
                Assert.AreSame(dependencyA, service.Dep);
            }

            service = container.Resolve<ServiceB>();
            Assert.IsNotNull(service.Dep);
            Assert.AreNotSame(dependencyA, service.Dep);
        }

        public class DependencyA { }

        public class AAA : DependencyA { }
        public class AAAA : DependencyA { }

        public class ServiceB
        {
            public readonly DependencyA Dep;
            public ServiceB(DependencyA dep) { Dep = dep; }
        }

        [Test]
        public void Can_apply_decorator_to_resolved_used_instance()
        {
            var container = new Container();

            container.UseInstance("x");
            container.Register<string>(Made.Of(() => AdjustString(Arg.Of<string>())), setup: Setup.Decorator);

            var xy = container.Resolve<string>();

            Assert.AreEqual("xy", xy);
        }

        [Test]
        public void Can_apply_decorator_to_injected_used_instance()
        {
            var container = new Container();

            container.Register<XUser>();
            container.UseInstance("x");
            container.Register(Made.Of(() => AdjustString(Arg.Of<string>())), setup: Setup.Decorator);

            var user = container.Resolve<XUser>();

            Assert.AreEqual("xy", user.Name);
        }

        public static string AdjustString(string m)
        {
            return m + "y";
        }

        public class XUser
        {
            public string Name { get; private set; }

            public XUser(string name)
            {
                Name = name;
            }
        }

        [Test]
        public void Can_use_instance_in_upper_scope_and_resolve_it_in_nested_scope()
        {
            var container = new Container();
            var a = new ClassA();

            using (var scope1 = container.OpenScope("1"))
            {
                scope1.UseInstance(a);

                using (var scope2 = scope1.OpenScope("2"))
                {
                    var resolvedA = scope2.Resolve<ClassA>();

                    if (resolvedA == null) // it's true
                    {
                        throw new NullReferenceException();
                    }
                }
            }
        }

        class ClassA
        {
        }

        [Test]
        public void Can_resolve_collection_of_open_generic_and_instance_in_order_of_registration()
        {
            var c = new Container();

            c.Register(typeof(G<>), Reuse.Singleton);
            c.Register(typeof(G<int>), Reuse.Singleton);

            var instance = new G<int>();
            c.RegisterDelegate(_ => instance, Reuse.Singleton);

            var items = c.Resolve<IEnumerable<G<int>>>().ToArray();

            Assert.AreEqual(3, items.Length);
            Assert.AreSame(instance, items[2]);
        }

        class G<T> { }
    }
}
