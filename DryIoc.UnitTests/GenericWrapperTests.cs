using System;
using System.Collections.Generic;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class GenericWrapperTests
    {
        [Test]
        public void IsRegistered_for_Func_of_registered_service_should_return_true_Same_as_for_open_generics()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));

            var registered = container.IsRegistered<Func<IService>>();

            Assert.That(registered, Is.True);
        }

        [Test]
        public void When_registered_both_named_and_default_service_Then_resolving_Lazy_with_the_name_should_return_named_service()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService), named: "named");

            var service = container.Resolve<Lazy<IService>>("named");

            Assert.That(service.Value, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Given_the_same_type_service_and_wrapper_registered_When_resolved_Then_service_will_preferred_over_wrapper()
        {
            var container = new Container();
            container.Register<Lazy<IService>>();
            container.Register<IService, Service>();

            var service = container.Resolve<Lazy<IService>>();

            Assert.That(service.Value, Is.InstanceOf<Service>());
        }

        [Test]
        public void Given_the_same_type_service_and_wrapper_registered_Wrapper_will_be_used_to_for_names_other_than_one_with_registered_service()
        {
            var container = new Container();
            container.Register<Lazy<IService>>();
            container.Register<IService, Service>();
            container.Register<IService, AnotherService>(named: "named");

            var service = container.Resolve<Lazy<IService>>("named");

            Assert.That(service.Value, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Resolved_wrapper_should_NOT_hold_reference_to_container()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service), setup: ServiceSetup.WithMetadata("007"));
            var services = container.Resolve<IEnumerable<Meta<Lazy<IService>, string>>>();

            var containerWeakRef = new WeakReference(container);
            // ReSharper disable RedundantAssignment
            container = null;
            // ReSharper restore RedundantAssignment
            GC.Collect(Int32.MaxValue);
            GC.WaitForFullGCComplete();
            GC.Collect(Int32.MaxValue);
            GC.KeepAlive(services);
            Assert.That(containerWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Wrapper_is_only_working_if_used_in_enumerable_or_other_wrapper_It_means_that_directly_resolving_multiple_wrapper_would_NOT_throw()
        {
            var container = new Container();
            container.Register(typeof(WrapperWithTwoArgs<,>), setup: GenericWrapperSetup.Default);
            container.Register<Service>();
            container.Register<AnotherService>();

            var wrapper = container.Resolve<WrapperWithTwoArgs<Service, AnotherService>>();

            Assert.That(wrapper.Arg0, Is.InstanceOf<Service>());
            Assert.That(wrapper.Arg1, Is.InstanceOf<AnotherService>());
        }

        [Test]
        public void Wrapper_is_only_working_if_used_in_enumerable_or_other_wrapper_It_means_that_resolving_array_of_multiple_wrapper_should_throw()
        {
            var container = new Container();
            container.Register(typeof(WrapperWithTwoArgs<,>), setup: GenericWrapperSetup.Default);
            container.Register<Service>();
            container.Register<AnotherService>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<WrapperWithTwoArgs<Service, AnotherService>[]>());
        }
    }

    public class WrapperWithTwoArgs<T0, T1>
    {
        public T0 Arg0 { get; set; }
        public T1 Arg1 { get; set; }

        public WrapperWithTwoArgs(T0 arg0, T1 arg1)
        {
            Arg0 = arg0;
            Arg1 = arg1;
        }
    }
}
