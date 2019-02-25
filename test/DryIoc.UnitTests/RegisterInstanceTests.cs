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
        public void Can_re_register_instance_with_different_reuse()
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
            container.UseInstance("mine");

            var mine = container.WithoutCache().Resolve<string>();
            Assert.AreEqual("mine", mine);
        }

        [Test]
        public void Register_instance_with_replace_option_will_replace_registered_instance_in_place()
        {
            var container = new Container();

            container.RegisterInstance("hey");
            var regBefore = container.GetServiceRegistrations();
            Assert.AreEqual(1, regBefore.Count());

            container.RegisterInstance("nah", IfAlreadyRegistered.Replace);
            var regAfter = container.GetServiceRegistrations();
            Assert.AreEqual(1, regAfter.Count());
        }

        [Test]
        public void Can_register_instance_with_keep_option()
        {
            var container = new Container();

            container.RegisterInstance("a");
            container.RegisterInstance("x", IfAlreadyRegistered.Keep);

            var s = container.Resolve<string>();
            Assert.AreEqual("a", s);
        }

        [Test]
        public void For_multiple_registered_instances_Then_Replace_should_replace_them_all()
        {
            var container = new Container();

            container.RegisterInstance("a");
            container.RegisterInstance("b");
            container.RegisterInstance("x", IfAlreadyRegistered.Replace);

            var x = container.Resolve<string>();
            Assert.AreEqual("x", x);
        }

        [Test]
        public void Replace_keyed_instance_registration()
        {
            var container = new Container();

            container.RegisterInstance("a", serviceKey: "x");
            Assert.AreEqual("a", container.Resolve<string>(serviceKey: "x"));

            container.RegisterInstance("b", serviceKey: "x", ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            Assert.AreEqual("b", container.Resolve<string>(serviceKey: "x"));
        }

        [Test]
        public void Can_replace_keyed_delegate_with_used_instance()
        {
            var container = new Container();

            container.RegisterDelegate(_ => "a", serviceKey: "x");
            Assert.AreEqual("a", container.Resolve<string>(serviceKey: "x"));

            container.RegisterInstance("b", serviceKey: "x", ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            Assert.AreEqual("b", container.Resolve<string>(serviceKey: "x"));
        }

        [Test]
        public void Can_register_with_and_then_without_PreventDisposal_behavior()
        {
            var container = new Container();

            container.RegisterInstance("a", setup: Setup.With(preventDisposal: true));
            container.Resolve<string>();

            container.RegisterInstance("a", IfAlreadyRegistered.Replace);
            container.Resolve<string>();
        }

        public class Me : IDisposable
        {
            public void Dispose()
            {
            }
        }

        [Test]
        public void Can_use_instance_of_Int_type()
        {
            var container = new Container();
            container.RegisterInstance<int>(42);

            var int42 = container.Resolve<int>();

            Assert.AreEqual(42, int42);
        }

        [Test]
        public void Can_work_with_multiple_keyed_and_default_instances()
        {
            var container = new Container();
            container.RegisterInstance<int>(42, serviceKey: "nice number");
            container.RegisterInstance<int>(43);
            container.RegisterInstance<int>(44, serviceKey: "another nice number");

            var forties = container.Resolve<int[]>();

            Assert.AreEqual(3, forties.Length);
        }

        [Test]
        public void The_Use_by_default_should_replace_previous_typed_registration()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.Use(aa);
            Assert.AreSame(aa, container.Resolve<DependencyA>());

            container.Use(new DependencyA());
            Assert.AreNotSame(aa, container.Resolve<DependencyA>());
        }

        [Test]
        public void Can_append_to_previous_typed_registration()
        {
            var container = new Container();
            container.Register<DependencyA>();

            var aa = new DependencyA();
            container.RegisterInstance(aa);

            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(2, aas.Count());
        }

        [Test]
        public void Can_append_multiple_and_then_replace_all()
        {
            var container = new Container();
            container.Register<DependencyA>();

            var aa = new DependencyA();
            container.RegisterInstance(aa, IfAlreadyRegistered.AppendNotKeyed);
            container.RegisterInstance<DependencyA>(new AAA(), IfAlreadyRegistered.AppendNotKeyed);
            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(3, aas.Count());

            container.RegisterInstance<DependencyA>(new AAAA(), IfAlreadyRegistered.Replace);
            var aaaa = container.Resolve<DependencyA>();
            Assert.IsInstanceOf<AAAA>(aaaa);
        }

        [Test]
        public void Can_append_multiple_and_then_replace_all_in_presence_of_keyed()
        {
            var container = new Container();

            container.RegisterInstance<DependencyA>(new AAA(), serviceKey: "aaa");
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.AppendNotKeyed);
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.AppendNotKeyed);

            container.RegisterInstance<DependencyA>(new AAAA(), IfAlreadyRegistered.Replace);
            Assert.IsInstanceOf<AAAA>(container.Resolve<DependencyA>());
            Assert.IsInstanceOf<AAA>(container.Resolve<DependencyA>("aaa"));
        }

        [Test]
        public void Can_have_both_default_and_keyed_instances_and_keyed_ifAlreadyRegistered_does_not_apply()
        {
            var container = new Container();

            container.RegisterInstance(new DependencyA());
            container.RegisterInstance(new DependencyA(), serviceKey: "keyed", ifAlreadyRegistered: IfAlreadyRegistered.Throw);

            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(2, aas.Count());
        }

        [Test]
        public void Can_append_new_default_with_presence_of_keyed()
        {
            var container = new Container();

            container.RegisterInstance(new DependencyA(), serviceKey: "aa");
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.AppendNotKeyed);
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.AppendNotKeyed);

            var aas = container.ResolveMany<DependencyA>();
            Assert.AreEqual(3, aas.Count());
        }

        [Test]
        public void Can_keep_previous_default()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.RegisterInstance(aa);
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.Keep);

            var x = container.Resolve<DependencyA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_keep_previous_default_with_presence_of_keyed()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.RegisterInstance(aa);
            container.RegisterInstance(aa, serviceKey: "aa");
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.Keep);

            var x = container.Resolve<DependencyA>();
            Assert.AreSame(aa, x);
        }

        [Test]
        public void Can_throw_on_previous_default()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.RegisterInstance(aa);

            var ex = Assert.Throws<ContainerException>(() => 
            container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.Throw));

            Assert.AreEqual(
                Error.NameOf(Error.UnableToRegisterDuplicateDefault),
                ex.ErrorName);
        }

        [Test]
        public void Can_throw_on_previous_default_with_presence_of_keyed()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.RegisterInstance(aa);
            container.RegisterInstance(aa, serviceKey: "aa");

            var ex = Assert.Throws<ContainerException>(() =>
                container.RegisterInstance(new DependencyA(), IfAlreadyRegistered.Throw));

            Assert.AreEqual(
                Error.NameOf(Error.UnableToRegisterDuplicateDefault),
                ex.ErrorName);
        }

        [Test]
        public void Can_append_new_default_implementation_And_ignore_the_duplicate_implementation()
        {
            var container = new Container();

            var aa = new DependencyA();
            container.RegisterInstance(aa);
            container.RegisterInstance<DependencyA>(new AAA(), IfAlreadyRegistered.AppendNewImplementation);

            Assert.AreEqual(2, container.ResolveMany<DependencyA>().Count());

            var newaaa = new AAA();
            container.RegisterInstance<DependencyA>(newaaa, IfAlreadyRegistered.AppendNewImplementation);

            container.RegisterInstance<DependencyA>(new AAAA(), IfAlreadyRegistered.AppendNewImplementation);
            Assert.AreEqual(3, container.ResolveMany<DependencyA>().Count());
        }

        [Test]
        public void Can_reuse_the_default_factory()
        {
            var container = new Container();
            container.RegisterInstance<int>(42, serviceKey: "nice number");
            container.RegisterInstance<int>(43, IfAlreadyRegistered.Replace);
            container.RegisterInstance<int>(44, serviceKey: "another nice number");
            container.RegisterInstance<int>(45, IfAlreadyRegistered.Replace); // will replace the 43

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

            var singletonDep = new DependencyA();
            container.UseInstance(singletonDep);

            using (var scope = container.OpenScope())
            {
                var scopedDep = new DependencyA();
                scope.UseInstance(scopedDep);
        
                var service = scope.Resolve<ServiceB>();
                Assert.AreSame(scopedDep, service.Dep);
            }

            var anotherBB = container.Resolve<ServiceB>();
            Assert.AreSame(singletonDep, anotherBB.Dep);
        }

        [Test]
        public void Singleton_service_should_consume_singleton_instance_despite_presence_of_scoped_instance()
        {
            var container = new Container();
            container.Register<ServiceB>(Reuse.Singleton);

            var singletonDep = new DependencyA();
            container.UseInstance(singletonDep);

            using (var scope = container.OpenScope())
            {
                var scopedDep = new DependencyA();
                scope.UseInstance(scopedDep);

                var service = scope.Resolve<ServiceB>();
                Assert.AreSame(singletonDep, service.Dep);
            }

            var anotherBB = container.Resolve<ServiceB>();
            Assert.AreSame(singletonDep, anotherBB.Dep);
        }

        [Test]
        public void UseInstance_wont_replace_existing_typed_registration_instead_will_append_to_it()
        {
            var container = new Container();
            container.Register<ServiceB>();

            container.Register<DependencyA>();

            var dependencyA = new DependencyA();
            ServiceB service;
            using (var scope = container.OpenScope())
            {
                scope.UseInstance(dependencyA); // what does it mean for the typed DependencyA

                service = scope.Resolve<ServiceB>();
                Assert.AreSame(dependencyA, service.Dep);
            }

            service = container.Resolve<ServiceB>();
            Assert.IsNotNull(service.Dep);
            Assert.AreNotSame(dependencyA, service.Dep);
        }

        [Test]
        public void UseInstance_wont_replace_existing_typed_registration_instead_will_append_to_it_In_presence_or_keyed_registration()
        {
            var container = new Container();
            container.Register<ServiceB>();

            container.Register<DependencyA>(serviceKey: "other");
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
