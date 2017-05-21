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

            container.UseInstance("x", IfAlreadyRegistered: IfAlreadyRegistered.Keep);

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
        public void UseInstance_by_default_should_replace_previous_typed_registration()
        {
            var container = new Container();
            container.Register<AA>();

            var aa = new AA();
            container.UseInstance(aa);

            var x = container.Resolve<AA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_append_to_previous_typed_registration()
        {
            var container = new Container();
            container.Register<AA>();

            var aa = new AA();
            container.UseInstance(aa, IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

            var aas = container.ResolveMany<AA>();
            Assert.AreEqual(2, aas.Count());
        }

        [Test]
        public void Can_append_multiple_and_then_replace_all()
        {
            var container = new Container();
            container.Register<AA>();

            var aa = new AA();
            container.UseInstance(aa, IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            container.UseInstance<AA>(new AAA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            var aas = container.ResolveMany<AA>();
            Assert.AreEqual(3, aas.Count());

            container.UseInstance<AA>(new AAAA());
            var aaaa = container.Resolve<AA>();
            Assert.IsInstanceOf<AAAA>(aaaa);
        }

        [Test]
        public void Can_append_multiple_and_then_replace_all_in_presence_of_keyed()
        {
            var container = new Container();

            container.UseInstance<AA>(new AAA(), serviceKey: "aaa");
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

            container.UseInstance<AA>(new AAAA());
            Assert.IsInstanceOf<AAAA>(container.Resolve<AA>());
            Assert.IsInstanceOf<AAA>(container.Resolve<AA>("aaa"));
        }

        [Test]
        public void Can_have_both_default_and_keyed_instances_and_keyed_ifAlreadyRegistered_does_not_apply()
        {
            var container = new Container();

            container.UseInstance(new AA());
            container.UseInstance(new AA(), serviceKey: "keyed", IfAlreadyRegistered: IfAlreadyRegistered.Throw);

            var aas = container.ResolveMany<AA>();
            Assert.AreEqual(2, aas.Count());
        }

        [Test]
        public void Can_append_new_default_with_presence_of_keyyed()
        {
            var container = new Container();

            container.UseInstance(new AA(), serviceKey: "aa");
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNotKeyed);

            var aas = container.ResolveMany<AA>();
            Assert.AreEqual(3, aas.Count());
        }

        [Test]
        public void Can_keep_previous_default()
        {
            var container = new Container();

            var aa = new AA();
            container.UseInstance(aa);
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.Keep);

            var x = container.Resolve<AA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_keep_previous_default_with_presence_of_keyed()
        {
            var container = new Container();

            var aa = new AA();
            container.UseInstance(aa);
            container.UseInstance(aa, serviceKey: "aa");
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.Keep);

            var x = container.Resolve<AA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_throw_on_previous_default()
        {
            var container = new Container();

            var aa = new AA();
            container.UseInstance(aa);

            var ex = Assert.Throws<ContainerException>(() => 
            container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(
                Error.NameOf(Error.UnableToRegisterDuplicateDefault),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Can_throw_on_previous_default_with_presence_of_keyed()
        {
            var container = new Container();

            var aa = new AA();
            container.UseInstance(aa);
            container.UseInstance(aa, serviceKey: "aa");

            var ex = Assert.Throws<ContainerException>(() =>
                container.UseInstance(new AA(), IfAlreadyRegistered: IfAlreadyRegistered.Throw));

            Assert.AreEqual(
                Error.NameOf(Error.UnableToRegisterDuplicateDefault),
                Error.NameOf(ex.Error));
        }


        [Test]
        public void Can_append_new_default_implementation_And_ignore_the_duplicate_implementation()
        {
            var container = new Container();

            var aa = new AA();
            container.UseInstance(aa);
            container.UseInstance<AA>(new AAA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            Assert.AreEqual(2, container.ResolveMany<AA>().Count());

            var newaaa = new AAA();
            container.UseInstance<AA>(newaaa, IfAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);

            container.UseInstance<AA>(new AAAA(), IfAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation);
            Assert.AreEqual(3, container.ResolveMany<AA>().Count());
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

        public class AAA : AA { }
        public class AAAA : AA { }

        public class BB
        {
            public readonly AA A;
            public BB(AA a) { A = a; }
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
    }
}
