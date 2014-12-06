using System;
using System.Collections.Generic;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.Memory
{
    [TestFixture]
    [Explicit(@"
        Tests are passing, verified on .NET 3.5, 4.0. 
        If tests are passes at least once, then it is enough to prove that Container is GC collected.")]
    public class NoMemoryLeaksTests
    {
        [Test]
        public void Registration_with_resolver_as_param_should_NOT_hold_reference_to_container_when_it_is_GCed()
        {
            var container = new Container();
            var containerWeakRef = new WeakReference(container);

            container.Register<IDependency, Dependency>();
            container.RegisterDelegate(r => new ServiceWithDependency(r.Resolve<IDependency>()));
            container.Resolve<ServiceWithDependency>();
            container.Dispose();
            // ReSharper disable RedundantAssignment
            container = null;
            // ReSharper restore RedundantAssignment

            GC.Collect();

            Assert.That(containerWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Resolved_wrapper_should_NOT_hold_reference_to_container()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service), setup: Setup.With(metadata: "007"));
            var servicesOne = container.Resolve<IEnumerable<Meta<Lazy<IService>, string>>>();
            var servicesTwo = container.Resolve<IEnumerable<Meta<Lazy<IService>, string>>>();

            var containerWeakRef = new WeakReference(container);
            // ReSharper disable RedundantAssignment
            container = null;
            // ReSharper restore RedundantAssignment
            GCFullCollect();
            GC.KeepAlive(servicesOne); 
            GC.KeepAlive(servicesTwo); 
            Assert.That(containerWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void When_request_is_saved_outside_and_container_disposed_Then_no_reference_to_Container_should_be_hold()
        {
            var container = new Container();

            IResolver savedResolver = null;
            container.RegisterDelegate(r =>
            {
                savedResolver = r;
                return new Service();
            });

            container.Resolve<Service>();
            container.Resolve<Service>();

            var containerWeakRef = new WeakReference(container);
            // ReSharper disable RedundantAssignment
            container = null;
            // ReSharper restore RedundantAssignment
            GCFullCollect();
            GC.KeepAlive(savedResolver);
            Assert.That(containerWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void After_disposing_container_there_should_be_No_reference_to_registered_instance()
        {
            var container = new Container();

            var service = new Service();
            container.RegisterInstance(service);

            var serviceRef = new WeakReference(service);
            container.Dispose();
            service = null;

            GCFullCollect();
            Assert.That(serviceRef.IsAlive, Is.False);
        }

        [Test]
        public void After_disposing_container_there_should_be_No_reference_to_registered_instance_if_it_was_resolved_already()
        {
            var container = new Container();

            var service = new Service();
            container.RegisterInstance(service);
            container.Resolve<Service>();

            var serviceRef = new WeakReference(service);
            container.Dispose();
            service = null;

            GCFullCollect();
            Assert.That(serviceRef.IsAlive, Is.False);
        }

        [Test]
        public void After_disposing_container_there_should_be_No_reference_to_registered_delegate_if_it_was_resolved_already()
        {
            var container = new Container();

            var service = new Service();
            container.RegisterDelegate<IService>(_ => service);
            container.Register<ServiceClient>();

            container.Resolve<ServiceClient>();

            var serviceRef = new WeakReference(service);
            container.Dispose();
            service = null;

            GCFullCollect();
            Assert.That(serviceRef.IsAlive, Is.False);
        }
    
        private static void GCFullCollect()
        {
            GC.Collect(Int32.MaxValue);
            GC.WaitForPendingFinalizers();
            GC.Collect(Int32.MaxValue);
        }
    }
}
