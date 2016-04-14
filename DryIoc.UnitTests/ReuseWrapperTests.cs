using System;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ReuseWrapperTests
    {
        [Test]
        public void Can_specify_to_dispose_registered_instance()
        {
            var container = new Container();
            container.RegisterInstance(new DisposableService(), Reuse.Singleton);
            var service = container.Resolve<DisposableService>();

            container.Dispose();

            Assert.That(service.IsDisposed, Is.True);
        }

        [Test]
        public void Can_specify_do_Not_dispose_disposable_singleton_object()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton, setup: Setup.With(preventDisposal: true));

            var service = container.Resolve<DisposableService>();

            container.Dispose();

            Assert.That(service.IsDisposed, Is.False);
        }

        [Test]
        public void Can_specify_do_Not_dispose_disposable_scoped_object()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<IService, DisposableService>(Reuse.InCurrentScope, setup: Setup.With(preventDisposal: true));

                var service = container.Resolve<IService>();

                container.Dispose();

                Assert.That(((DisposableService)service).IsDisposed, Is.False);
            }
        }

        [Explicit]
        public void Can_store_reused_object_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton, setup: Setup.With(weaklyReferenced: true));

            var serviceWeakRef = new WeakReference(container.Resolve<IService>());
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Can_store_as_WeakReference_in_current_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope, setup: Setup.With(weaklyReferenced: true));

            using (var scope = container.OpenScope())
            {
                var service = scope.Resolve<IService>();
                Assert.IsInstanceOf<DisposableService>(service);
            }
        }

        [Test]
        public void Can_store_disposable_as_WeakReference_and_dispose_it_if_alive()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.InCurrentScope, setup: Setup.With(weaklyReferenced: true));
            var scope = container.OpenScope();

            var service = scope.Resolve<DisposableService>();

            scope.Dispose();
            Assert.IsTrue(service.IsDisposed);
        }

        [Explicit]
        public void Can_nest_Disposable_into_Weak_Referenced_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(preventDisposal: true, weaklyReferenced: true));

            using (container.OpenScope())
            {
                var service = container.Resolve<IService>();
                var serviceWeakRef = new WeakReference(service);
                // ReSharper disable RedundantAssignment
                service = null;
                // ReSharper restore RedundantAssignment

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.KeepAlive(container);

                Assert.That(serviceWeakRef.IsAlive, Is.False);
            }
        }

        [Test]
        public void Can_nest_Disposable_into_WeakReference_reused_in_current_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(preventDisposal: true, weaklyReferenced: true));

            var service = container.Resolve<IService>();

            Assert.IsInstanceOf<DisposableService>(service);
            GC.KeepAlive(service);
        }

        [Test]
        public void Should_Not_throw_if_reused_wrappers_registered_without_reuse_because_of_possible_ReuseMapping_rules()
        {
            var container = new Container();

            Assert.DoesNotThrow(() =>
            container.Register<IService, Service>(setup: Setup.With(weaklyReferenced: true)));
        }

        [Test]
        public void Can_resolve_func_with_args_of_reuse_wrapper()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Service>();
                container.Register<ServiceWithParameterAndDependency>(
                    Reuse.InCurrentScope, setup: Setup.With(weaklyReferenced: true));

                var func = container.Resolve<Func<bool, ServiceWithParameterAndDependency>>();
                var service = func(true);

                Assert.That(service, Is.InstanceOf<ServiceWithParameterAndDependency>());
            }
        }
    }
}