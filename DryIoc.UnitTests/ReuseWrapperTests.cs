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
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseHiddenDisposable).One()));

            var service = container.Resolve<DisposableService>();

            container.Dispose();

            Assert.That(service.IsDisposed, Is.False);
        }

        [Test]
        public void Can_specify_do_Not_dispose_disposable_scoped_object()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: typeof(ReuseHiddenDisposable).One()));

                var service = container.Resolve<IService>();

                container.Dispose();

                Assert.That(((DisposableService)service).IsDisposed, Is.False);
            }
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_service_and_then_dispose_it()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: typeof(ReuseHiddenDisposable).One()));

                var disposable = container.Resolve<ReuseHiddenDisposable>(typeof(IService));
                disposable.Dispose();

                Assert.That(disposable.IsDisposed, Is.True);
                object result = null;
                var targetEx = Assert.Throws<ContainerException>(() => result = disposable.Target);

                Assert.That(targetEx.Message, Is.StringContaining(
                    "Target DryIoc.UnitTests.CUT.DisposableService was already disposed in DryIoc.ReuseHiddenDisposable wrapper."));

                var resolveEx = Assert.Throws<ContainerException>(() =>
                    container.Resolve<IService>());
                Assert.That(resolveEx.Message, Is.EqualTo(targetEx.Message));
                GC.KeepAlive(result);
            }
        }

        [Test]
        public void Can_resolve_explicitly_disposed_singleton_and_then_dispose_it()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseHiddenDisposable).One()));

            var disposable = container.Resolve<ReuseHiddenDisposable>(typeof(IService));
            disposable.Dispose();

            Assert.That(disposable.IsDisposed, Is.True);
            object result = null;
            var targetEx = Assert.Throws<ContainerException>(() => result = disposable.Target);
            Assert.That(targetEx.Message, Is.StringContaining(
                "Target DryIoc.UnitTests.CUT.DisposableService was already disposed in DryIoc.ReuseHiddenDisposable wrapper."));

            var resolveEx = Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
            Assert.That(resolveEx.Message, Is.EqualTo(targetEx.Message));
            GC.KeepAlive(result);
        }

        [Test, Explicit]
        public void Can_store_reused_object_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseWeakReference).One()));

            var serviceWeakRef = container.Resolve<ReuseWeakReference>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.Ref.IsAlive, Is.False);
        }

        [Test]
        public void Can_resolve_reused_object_as_weak_reference_typed_proxy()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseWeakReference).One()));

            var serviceWeakRef = container.Resolve<ReuseWeakReference>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.KeepAlive(serviceWeakRef.Target);
        }

        [Test]
        public void Can_resolve_different_reused_services_as_reused_Ref()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));
            container.Register<Dependency>(Reuse.InResolutionScope,
                setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));
            container.Register<ServiceClient>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));

            using (var scoped = container.OpenScope())
            {
                var service = scoped.Resolve<ReuseSwapable>(typeof(IService));
                var dependency = scoped.Resolve<ReuseSwapable>(typeof(Dependency));
                var client = scoped.Resolve<ReuseSwapable>(typeof(ServiceClient));

                Assert.That(service.Target, Is.InstanceOf<Service>());
                Assert.That(dependency.Target, Is.InstanceOf<Dependency>());
                Assert.That(client.Target, Is.InstanceOf<ServiceClient>());
            }
        }

        [Test, Explicit]
        public void Can_nest_Disposable_into_Weak_Referenced_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] {typeof(ReuseHiddenDisposable), typeof(ReuseWeakReference)}));

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
                setup: Setup.With(reuseWrappers: new[] {typeof(ReuseHiddenDisposable), typeof(ReuseWeakReference)}));

            var serviceWeakRef = container.Resolve<ReuseWeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ReuseHiddenDisposable>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
            GC.KeepAlive(serviceWeakRef.Target);
        }

        [Test]
        public void CanNot_nest_WeakRef_in_ExplicitlyDisposable_because_ref_is_not_disposable()
        {
            var container = new Container();
            var service = new DisposableService();
            container.RegisterDelegate<IService>(r => service, Reuse.Singleton,
                Setup.With(reuseWrappers: new[] {typeof(ReuseWeakReference), typeof(ReuseHiddenDisposable)}));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ReuseWeakReference>(typeof(IService)));
        }

        [Test]
        public void Cannot_resolve_non_reused_service_as_WeakReference_without_required_type()
        {
            var container = new Container();
            container.Register<IService, Service>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ReuseWeakReference>());
        }

        [Test]
        public void Should_throw_if_reused_wrappers_specified_without_reuse()
        {
            var container = new Container();
            container.Register<IService, Service>(
                setup: Setup.With(reuseWrappers: typeof(ReuseWeakReference).One()));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ReuseWeakReference>(typeof(IService)));

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve reuse wrapper DryIoc.ReuseWeakReference"));
        }

        [Test]
        public void CanNot_resolve_non_reused_service_as_ExplicitlyDisposable()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ReuseHiddenDisposable>(typeof(IService)));

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve reuse wrapper DryIoc.ReuseHiddenDisposable"));
        }

        [Test]
        public void Can_resolve_func_with_args_of_reuse_wrapper()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Service>();
                container.Register<ServiceWithParameterAndDependency>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: typeof(ReuseWeakReference).One()));

                var func = container.Resolve<Func<bool, ReuseWeakReference>>(typeof(ServiceWithParameterAndDependency));
                var service = func(true).Target;
                Assert.That(service, Is.InstanceOf<ServiceWithParameterAndDependency>());
            }
        }

        [Test]
        public void Can_resolve_service_as_Ref()
        {
            var container = new Container();

            container.Register<Service>();
            container.Register<ServiceWithParameterAndDependency>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));

            var func = container.Resolve<Func<bool, ReuseSwapable>>(typeof(ServiceWithParameterAndDependency));
            var service = func(true);
            var service2 = func(true);

            Assert.That(service.Target, Is.InstanceOf<ServiceWithParameterAndDependency>());
            Assert.That(service, Is.SameAs(service2));
        }

        [Test]
        public void Can_resolve_service_as_Ref_of_IDisposable()
        {
            var container = new Container();

            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));

            IService service;
            using (var scope = container.OpenScope())
                service = scope.Resolve<IService>();

            Assert.IsTrue(((DisposableService)service).IsDisposed);
        }

        [Test]
        public void Can_resolve_service_as_swapable()
        {
            var container = new Container();

            container.Register<Service>();
            container.Register<ServiceWithParameterAndDependency>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));

            var func = container.Resolve<Func<bool, ReuseSwapable>>(typeof(ServiceWithParameterAndDependency));
            var service = func(true);
            var service2 = func(true);

            Assert.That(service.Target, Is.InstanceOf<ServiceWithParameterAndDependency>());
            Assert.That(service.Target, Is.SameAs(service2.Target));

            var newService = new ServiceWithParameterAndDependency(new Service(), false);
            service.Swap(newService);
            var getNew = container.Resolve<Func<bool, ServiceWithParameterAndDependency>>();
            Assert.That(getNew(true), Is.SameAs(newService));
        }

        [Test]
        public void Can_resolve_as_swapable_and_swap_based_on_current_value()
        {
            var container = new Container();
            container.Register<Service>(Reuse.Singleton, setup: Setup.With(reuseWrappers: typeof(ReuseSwapable).One()));

            var service = container.Resolve<ReuseSwapable>(typeof(Service));

            var another = new Service();
            service.Swap(current => another);

            Assert.AreSame(another, container.Resolve<Service>());
        }

        [Test]
        public void Can_resolve_both_wrappers_separately()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] {typeof(ReuseHiddenDisposable), typeof(ReuseSwapable)}));

            var explicitlyDisposable = container.Resolve<ReuseHiddenDisposable>(typeof(DisposableService));
            var refReused = container.Resolve<ReuseSwapable>(typeof(DisposableService));

            Assert.AreSame(
                explicitlyDisposable.TargetOrDefault<DisposableService>(),
                refReused.TargetOrDefault<DisposableService>());
        }
    }
}