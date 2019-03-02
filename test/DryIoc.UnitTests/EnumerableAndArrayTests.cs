using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class EnumerableAndArrayTests
    {
        [Test]
        public void Resolving_array_with_default_and_one_named_service_will_return_both_services()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService), serviceKey: "another");

            var services = container.Resolve<Func<IService>[]>();

            Assert.That(services.Length, Is.EqualTo(2));
        }

        [Test]
        public void I_can_resolve_array_of_singletons()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), Reuse.Singleton);

            var services = container.Resolve<IService[]>();

            Assert.That(services.Length, Is.EqualTo(1));
        }

        [Test]
        public void I_can_resolve_mixed_array_of_singletons_and_transients()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), Reuse.Singleton);
            container.Register(typeof(IService), typeof(AnotherService), serviceKey: "another");

            var services = container.Resolve<IService[]>();

            Assert.That(services.Length, Is.EqualTo(2));
        }

        [Test]
        public void Resolving_enumerable_of_service_should_return_enumerable_type()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var services = container.Resolve<IEnumerable<IService>>();

            Assert.That(services, Is.InstanceOf<IEnumerable<IService>>());
        }

        [Test]
        public void Resolving_enumerable_with_default_and_one_named_service_will_return_both_services()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService), serviceKey: "AnotherService");

            var services = container.Resolve<IEnumerable<IService>>();

            Assert.That(services.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Resolving_enumerable_of_service_registered_with_func_should_return_enumerable_with_single_service()
        {
            var container = new Container();
            container.RegisterDelegate<IService<string>>(_ => new ClosedGenericClass());

            var services = container.Resolve<IEnumerable<IService<string>>>();

            Assert.That(services.Single(), Is.Not.Null);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void I_can_resolve_array_of_open_generics(bool lazyEnumerable)
        {
            IContainer container = new Container();
            if (lazyEnumerable)
                container = container.With(r => r.WithResolveIEnumerableAsLazyEnumerable());

            container.Register(typeof(IService<>), typeof(Service<>), Reuse.Singleton);

            var services = container.Resolve<IEnumerable<IService<int>>>();

            Assert.That(services.Single(), Is.InstanceOf<Service<int>>());
        }

        [Test]
        public void I_can_resolve_array_of_lazy_singletons()
        {
            var container = new Container();
            ServiceWithInstanceCount.InstanceCount = 0;

            container.Register(typeof(ServiceWithInstanceCount), Reuse.Singleton);

            var services = container.Resolve<IEnumerable<Lazy<ServiceWithInstanceCount>>>();
            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(0));

            var service = services.First().Value;
            Assert.That(service, Is.Not.Null);

            Assert.That(ServiceWithInstanceCount.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void I_can_inject_enumerable_as_dependency()
        {
            var container = new Container();
            container.Register(typeof(IDependency), typeof(Dependency));
            container.Register(typeof(IDependency), typeof(Dependency), serviceKey: "Foo2");
            container.Register(typeof(IService), typeof(ServiceWithEnumerableDependencies));

            var service = (ServiceWithEnumerableDependencies)container.Resolve<IService>();

            Assert.That(service.Foos, Is.InstanceOf<IEnumerable<IDependency>>());
        }

        [Test]
        public void Resolving_array_of_not_registered_services_should_return_empty_array()
        {
            var container = new Container();

            var services = container.Resolve<IService[]>();

            Assert.That(services.Length, Is.EqualTo(0));
        }

        [Test]
        public void When_enumerable_is_injected_it_will_Not_change_after_registering_of_new_service()
        {
            var container = new Container();
            container.Register<ServiceAggregator>();

            container.Register(typeof(IService), typeof(Service));
            var servicesBefore = container.Resolve<ServiceAggregator>().Services;
            Assert.AreEqual(1, servicesBefore.Count());

            container.Register(typeof(IService), typeof(AnotherService), serviceKey: "another");

            var servicesAfter = container.Resolve<ServiceAggregator>().Services;
            Assert.AreEqual(1, servicesAfter.Count());
        }

        internal class ServiceAggregator
        {
            public IEnumerable<IService> Services { get; private set; }
            public ServiceAggregator(IEnumerable<IService> services)
            {
                Services = services;
            }
        }

        [Test]
        public void When_enumerable_dependency_is_reresolved_after_registering_another_service_Then_enumerable_should_NOT_contain_that_service()
        {
            var container = new Container();
            container.Register<ServiceWithEnumerableDependencies>();
            container.Register<IDependency, Foo1>();

            var service = container.Resolve<ServiceWithEnumerableDependencies>();
            Assert.That(service.Foos.Count(), Is.EqualTo(1));

            container.Register<IDependency, Foo2>();
            var serviceAfter = container.Resolve<ServiceWithEnumerableDependencies>();

            Assert.That(serviceAfter.Foos.Count(), Is.EqualTo(1));
        }

        [Test]
        public void I_should_be_able_to_resolve_Lazy_of_Func_of_IEnumerable()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service), serviceKey: "blah");
            container.Register(typeof(IService), typeof(Service), serviceKey: "crew");

            var result = container.Resolve<Lazy<Func<IEnumerable<IService>>>>();

            Assert.That(result, Is.InstanceOf<Lazy<Func<IEnumerable<IService>>>>());
            Assert.That(result.Value.Invoke().Count(), Is.EqualTo(2));
        }

        [Test]
        public void If_some_item_is_not_resolved_then_it_will_return_empty_collection()
        {
            var container = new Container();
            container.Register<Service>(setup: Setup.With(metadataOrFuncOfMetadata: 1));

            var items = container.Resolve<IEnumerable<Meta<Service, bool>>>();

            Assert.That(items.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Resove_func_of_default_service_then_array_of_default_and_named_service_should_Succeed()
        {
            var container = new Container();
            container.Register<IService, Service>(serviceKey: 1);
            container.Register<IService, Service>();

            container.Resolve<Func<IService>>();
            var funcs = container.Resolve<Func<IService>[]>();

            Assert.That(funcs.Length, Is.EqualTo(2));
        }

        [Test]
        public void Resolve_generic_wrappers()
        {
            var container = new Container();
            container.Register<ICmd, X>();
            container.Register<ICmd, Y>();
            container.Register(typeof(MenuItem<>), setup: Setup.Wrapper);

            var items = container.Resolve<MenuItem<ICmd>[]>();

            Assert.AreEqual(2, items.Length);
        }

        [Test]
        public void Skip_resolution_on_missing_dependency_when_resolved_with_ReturnDefault()
        {
            var container = new Container();
            container.Register<A>();

            var items = container.Resolve<A[]>(IfUnresolved.ReturnDefault);

            Assert.AreEqual(0, items.Length);
        }

        [Test]
        public void Lazy_enumerable_should_throw_on_missing_dependency()
        {
            var container = new Container(rules =>
                rules.WithResolveIEnumerableAsLazyEnumerable());
            container.Register<A>();

            var items = container.Resolve<IEnumerable<A>>();


            var ex = Assert.Throws<ContainerException>(
                () => items.Count());

            Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), Error.NameOf(ex.Error));
        }

        [Test]
        public void Lazy_enumerable_should_Not_throw_on_missing_dependency_when_resolved_as_ResolveDefault()
        {
            var container = new Container(rules =>
                rules.WithResolveIEnumerableAsLazyEnumerable());
            container.Register<A>();

            var items = container.Resolve<IEnumerable<A>>(IfUnresolved.ReturnDefault);

            Assert.AreEqual(0, items.Count());
        }

        [Test]
        public void Lazy_enumerable_of_enumerable_should_throw_for_nested_enumarable_of_ResolveThrow()
        {
            var container = new Container(rules =>
                rules.WithResolveIEnumerableAsLazyEnumerable());

            container.Register<A>();
            container.Register(Made.Of(() => new AA(Arg.Of<IEnumerable<A>>(IfUnresolved.Throw))));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<IEnumerable<AA>>().ToArray());

            Assert.AreEqual(
                Error.NameOf(Error.UnableToResolveUnknownService),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Lazy_enumerable_of_enumerable_of_meta_dep_should_throw_for_nested_enumerable_of_ResolveThrow()
        {
            var container = new Container(rules =>
                rules.WithResolveIEnumerableAsLazyEnumerable());

            container.Register<D>();
            container.Register(Made.Of(() => new AD(Arg.Of<IEnumerable<D>>(IfUnresolved.Throw))));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<IEnumerable<AD>>().ToArray());

            Assert.AreEqual(Error.NameOf(Error.UnableToResolveUnknownService), ex.ErrorName);
        }

        [Test]
        public void Lazy_enumerable_of_enumerable_of_array_dep_should_throw_for_nested_enumerable_of_ResolveThrow()
        {
            var container = new Container(rules =>
                rules.WithResolveIEnumerableAsLazyEnumerable());

            container.Register<E>();
            container.Register(Made.Of(() => new AE(Arg.Of<IEnumerable<E>>(IfUnresolved.Throw))));

            var aes = container.Resolve<IEnumerable<AD>>().ToArray();
        }

        [Test]
        public void Should_include_closed_and_open_generic_version_of_service_in_order_of_registration()
        {
            var container = new Container();

            container.Register(typeof(IFoo<>), typeof(OpenFoo<>));
            container.Register(typeof(IFoo<int>), typeof(ClosedFoo));

            var fooTypes = container.Resolve<IFoo<int>[]>().Select(f => f.GetType());
            CollectionAssert.AreEqual(new[] { typeof(OpenFoo<int>), typeof(ClosedFoo) }, fooTypes);
        }

        [Test]
        public void LazyEnumerable_should_include_closed_and_open_generic_version_of_service_in_order_of_registration()
        {
            var container = new Container();

            container.Register(typeof(IFoo<>), typeof(OpenFoo<>));
            container.Register(typeof(IFoo<int>), typeof(ClosedFoo));

            var fooTypes = container.Resolve<LazyEnumerable<IFoo<int>>>().Select(f => f.GetType());
            CollectionAssert.AreEqual(new[] { typeof(OpenFoo<int>), typeof(ClosedFoo) }, fooTypes);
        }

        public interface IFoo<T> { }

        public class ClosedFoo : IFoo<int> { }
        public class OpenFoo<T> : IFoo<T> { }

        public class B { }

        public class A
        {
            public A(B b) { }
        }

        public class AA
        {
            public AA(IEnumerable<A> aas)
            {
                aas.ToArray();
            }
        }

        public class D
        {
            public D(Meta<B, string> b) { }
        }

        public class AD
        {
            public AD(IEnumerable<D> dds)
            {
                dds.ToArray();
            }
        }

        public class E
        {
            public E(B[] b) { }
        }

        public class AE
        {
            public AE(IEnumerable<E> dds)
            {
                dds.ToArray();
            }
        }

        public interface ICmd { }
        public class X : ICmd { }
        public class Y : ICmd { }
        public class MenuItem<T> where T : ICmd { }

        [Test]
        public void Can_resolve_array_of_strings()
        {
            var container = new Container();
            var arr = new[] { "str" };
            container.RegisterInstance(arr, serviceKey: "key");
            var inst = container.Resolve<string[]>("key");
        }

        [Test]
        public void Can_resolve_array_of_asResolutionCall_service()
        {
            var c = new Container();
            c.Register<N>(setup: Setup.With(asResolutionCall: true));

            var ns = c.Resolve<N[]>();
            Assert.AreEqual(1, ns.Length);
        }

        class N { }
    }
}