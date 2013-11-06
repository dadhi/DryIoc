using System;
using System.Collections.Generic;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests.Memory
{
    [TestFixture]
    [Ignore(@"
        Tests are passing, verified on .NET 3.5, 4.0. 
        If tests are passes at least once, then it is enough to prove that Container is GC collected.")]
    public class MemoryLeaksTests
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
    }
}
