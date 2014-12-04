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
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

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
                    setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

                var service = container.Resolve<IService>();

                container.Dispose();

                Assert.That(((DisposableService)service).IsDisposed, Is.False);
            }
        }

        [Test]
        public void Can_resolve_explicitly_disposable_proxy_with_compile_time_service_type()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            var wrapperProxy = container.Resolve<ExplicitlyDisposableProxy<IService>>();
            var wrapper = container.Resolve<ExplicitlyDisposable>(typeof(IService));

            Assert.That(wrapperProxy.Target, Is.InstanceOf<DisposableService>());
            Assert.That(wrapperProxy.Target, Is.SameAs(wrapper.Target));
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_service_and_then_dispose_it()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

                var disposable = container.Resolve<ExplicitlyDisposableProxy<object>>(typeof(IService));
                disposable.DisposeTarget();

                Assert.That(disposable.IsDisposed, Is.True);
                object result = null;
                var targetEx = Assert.Throws<ContainerException>(() => result = disposable.Target);

                Assert.That(targetEx.Message, Is.StringContaining(
                    "Target of type DryIoc.UnitTests.CUT.DisposableService was already disposed in DryIoc.ExplicitlyDisposable"));

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
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            var disposable = container.Resolve<ExplicitlyDisposableProxy<IService>>();
            disposable.DisposeTarget();

            Assert.That(disposable.IsDisposed, Is.True);
            object result = null;
            var targetEx = Assert.Throws<ContainerException>(() => result = disposable.Target);
            Assert.That(targetEx.Message, Is.StringContaining(
                "Target of type DryIoc.UnitTests.CUT.DisposableService was already disposed in DryIoc.ExplicitlyDisposable"));

            var resolveEx = Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
            Assert.That(resolveEx.Message, Is.EqualTo(targetEx.Message));
            GC.KeepAlive(result);
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_with_required_type()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<DisposableService>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

                container.Register(typeof(ExplicitlyDisposableProxy<>), setup: SetupWrapper.Default);

                var disposable = container.Resolve<ExplicitlyDisposableProxy<IService>>(typeof(DisposableService));

                Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
            }
        }

        [Test]
        public void Can_resolve_explicitly_disposed_singleton_with_required_type()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));


            container.Register(typeof(ExplicitlyDisposableProxy<>), setup: SetupWrapper.Default);

            var disposable = container.Resolve<ExplicitlyDisposableProxy<IService>>(typeof(DisposableService));

            Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
        }

        [Test, Explicit]
        public void Can_store_reused_object_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeakReference }));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Can_resolve_reused_object_as_weak_reference_typed_proxy()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeakReference }));

            var serviceWeakRef = container.Resolve<WeakReferenceProxy<IService>>();
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_different_reused_services_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));
            container.Register<Dependency>(Reuse.InResolutionScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));
            container.Register<ServiceClient>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            using (var scoped = container.OpenScope())
            {
                var service = scoped.Resolve<ExplicitlyDisposableProxy<IService>>();
                var dependency = scoped.Resolve<ExplicitlyDisposableProxy<Dependency>>();
                var client = scoped.Resolve<ExplicitlyDisposableProxy<ServiceClient>>();

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
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable, ReuseWrapper.WeakReference }));

            var service = container.Resolve<IService>();
            var serviceWeakRef = new WeakReference(service);
            // ReSharper disable RedundantAssignment
            service = null;
            // ReSharper restore RedundantAssignment

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Can_nest_Disposable_into_WeakReference_reused_in_current_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable, ReuseWrapper.WeakReference }));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ExplicitlyDisposable>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
        }

        [Test]
        public void Can_nest_WeakReference_in_ExplicitlyDisposable_reused_in_singleton_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeakReference, ReuseWrapper.ExplicitlyDisposable }));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ExplicitlyDisposableProxy<IService>>();

            Assert.That(serviceDisposable.Target, Is.SameAs(serviceWeakRef.Target));
        }

        [Test]
        public void Cannot_resolve_non_reused_service_as_WeakReference_without_required_type()
        {
            var container = new Container();
            container.Register<IService, Service>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<WeakReference>());
        }

        [Test]
        public void CanNot_resolve_non_reused_service_as_WeakReference()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<WeakReference>(typeof(IService)));

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve reuse wrapper WeakReference"));
        }

        [Test]
        public void CanNot_resolve_non_reused_service_as_ExplicitlyDisposable()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ExplicitlyDisposable>(typeof(IService)));

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve reuse wrapper DryIoc.ExplicitlyDisposable"));
        }

        [Test]
        public void Can_resolve_func_with_args_of_reuse_wrapper()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Service>();
                container.Register<ServiceWithParameterAndDependency>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeakReference }));

                var func = container.Resolve<Func<bool, WeakReference>>(typeof(ServiceWithParameterAndDependency));
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
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.Ref }));

            var func = container.Resolve<Func<bool, Ref<object>>>(typeof(ServiceWithParameterAndDependency));
            var service = func(true);
            var service2 = func(true);

            Assert.That(service.Value, Is.InstanceOf<ServiceWithParameterAndDependency>());
            Assert.That(service, Is.SameAs(service2));
        }

        [Test]
        public void Can_resolve_service_as_typed_Ref_proxy()
        {
            var container = new Container();

            container.Register<Service>();
            container.Register<ServiceWithParameterAndDependency>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.Ref }));

            var func = container.Resolve<Func<bool, RefProxy<ServiceWithParameterAndDependency>>>();
            var service = func(true);
            var service2 = func(true);

            Assert.That(service.Value, Is.InstanceOf<ServiceWithParameterAndDependency>());
            Assert.That(service.Value, Is.SameAs(service2.Value));

            var newService = new ServiceWithParameterAndDependency(new Service(), false);
            service.Swap(_ => newService);
            var getNew = container.Resolve<Func<bool, ServiceWithParameterAndDependency>>();
            Assert.That(getNew(true), Is.SameAs(newService));
        }

        [Test]
        public void Service_wrapped_in_WeakReference_and_in_ExplicitlyDisposable_may_be_disposed()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeakReference, ReuseWrapper.ExplicitlyDisposable }));

            var service = container.Resolve<ExplicitlyDisposableProxy<DisposableService>>();
            service.DisposeTarget();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<DisposableService>());

            Assert.That(ex.Message, Is.StringContaining("Target of type WeakReference was already disposed in DryIoc.ExplicitlyDisposable"));
        }

        [Test]
        public void Disposable_reuse_wrapper_allows_to_check_when_service_is_disposed()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Service>(Reuse.InCurrentScope,
                    setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.Disposable }));

                var service = container.Resolve<DisposableProxy<Service>>();

                container.Dispose();

                Assert.That(service.IsDisposed, Is.True);
            }
        }

        [Test]
        public void Can_renew_shared_instance_with_ExplicitlyRenewable_shared_in_current_scope()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Service>(Shared.InCurrentScope,
                    setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.Disposable }));

                var renewable = container.Resolve<DisposableProxy<Service>>();
                var service = renewable.Target;
                renewable.Renew();

                var newService = container.Resolve(typeof(Service));
                Assert.That(newService, Is.Not.SameAs(service));
            }
        }
    }
}
