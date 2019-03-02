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
        // ReSharper disable RedundantAssignment

        [Test]
        public void Registration_with_resolver_as_param_should_NOT_hold_reference_to_container_when_it_is_GCed()
        {
            var container = new Container();

            container.Register<IDependency, Dependency>();
            container.RegisterDelegate(r => new ServiceWithDependency(r.Resolve<IDependency>()));
            container.Resolve<ServiceWithDependency>();

            var containerWeakRef = new WeakReference(container);
            container.Dispose();
            container = null;
            GCFullCollect();

            Assert.IsFalse(containerWeakRef.IsAlive);
        }

        [Test]
        public void Resolved_wrapper_should_NOT_hold_reference_to_container()
        {
            var container = new Container();

            container.Register(typeof(IService), typeof(Service), setup: Setup.With(metadataOrFuncOfMetadata: "007"));
            container.Resolve<IEnumerable<Meta<Lazy<IService>, string>>>();
            container.Resolve<IEnumerable<Meta<Lazy<IService>, string>>>();

            var containerWeakRef = new WeakReference(container);
            container.Dispose();
            container = null;
            GCFullCollect();

            Assert.False(containerWeakRef.IsAlive);
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
            container.Dispose();
            container = null;
            GCFullCollect();

            Assert.IsFalse(containerWeakRef.IsAlive);
        }

        public void After_disposing_container_there_should_be_No_reference_to_registered_instance()
        {
            var container = new Container();

            var service = new Service();
            container.RegisterInstance(service);

            var serviceRef = new WeakReference(service);
            container.Dispose();
            container = null;
            service = null;
            GCFullCollect();

            Assert.False(serviceRef.IsAlive);
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
            container = null;
            service = null;
            GCFullCollect();

            Assert.IsFalse(serviceRef.IsAlive);
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
            container = null;
            service = null;
            GCFullCollect();

            Assert.IsFalse(serviceRef.IsAlive);
        }
    
        private static void GCFullCollect()
        {
            GC.Collect(int.MaxValue);
            GC.WaitForPendingFinalizers();
            GC.Collect(int.MaxValue);
        }
    }
}
