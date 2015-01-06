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
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));

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
                    setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));

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
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));

            var wrapperProxy = container.Resolve<ExplicitlyDisposableReused<IService>>();
            var wrapper = container.Resolve<ExplicitlyDisposableReused>(typeof(IService));

            Assert.That(wrapperProxy.Target, Is.InstanceOf<DisposableService>());
            Assert.That(wrapperProxy.Target, Is.SameAs(wrapper.Target));
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_service_and_then_dispose_it()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                    setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));

                var disposable = container.Resolve<ExplicitlyDisposableReused<object>>(typeof(IService));
                disposable.Dispose();

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
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));

            var disposable = container.Resolve<ExplicitlyDisposableReused<IService>>();
            disposable.Dispose();

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
                    setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));

                container.Register(typeof(ExplicitlyDisposableReused<>), setup: SetupWrapper.Default);

                var disposable = container.Resolve<ExplicitlyDisposableReused<IService>>(typeof(DisposableService));

                Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
            }
        }

        [Test]
        public void Can_resolve_explicitly_disposed_singleton_with_required_type()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused)));


            container.Register(typeof(ExplicitlyDisposableReused<>), setup: SetupWrapper.Default);

            var disposable = container.Resolve<ExplicitlyDisposableReused<IService>>(typeof(DisposableService));

            Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
        }

        [Test, Explicit]
        public void Can_store_reused_object_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.Default.WithReuseWrappers(typeof(ReusedWeakRef)));

            var serviceWeakRef = container.Resolve<ReusedWeakRef>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.WeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Can_resolve_reused_object_as_weak_reference_typed_proxy()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.Default.WithReuseWrappers(typeof(ReusedWeakRef)));

            var serviceWeakRef = container.Resolve<ReusedWeakRef<IService>>();
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_different_reused_services_as_reused_Ref()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.Default.WithReuseWrappers(typeof(RefReused)));
            container.Register<Dependency>(Reuse.InResolutionScope,
                setup: Setup.Default.WithReuseWrappers(typeof(RefReused)));
            container.Register<ServiceClient>(Reuse.InCurrentScope,
                setup: Setup.Default.WithReuseWrappers(typeof(RefReused)));

            using (var scoped = container.OpenScope())
            {
                var service = scoped.Resolve<ReusedRef<IService>>();
                var dependency = scoped.Resolve<ReusedRef<Dependency>>();
                var client = scoped.Resolve<ReusedRef<ServiceClient>>();

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
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused), typeof(ReusedWeakRef)));

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
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused), typeof(ReusedWeakRef)));

            var serviceWeakRef = container.Resolve<ReusedWeakRef>(typeof(IService));
            var serviceDisposable = container.Resolve<ExplicitlyDisposableReused>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
        }

        [Test]
        public void CanNot_nest_WeakRef_in_ExplicitlyDisposable_because_ref_is_not_disposable()
        {
            var container = new Container();
            var service = new DisposableService();
            container.RegisterInstance<IService>(service, Reuse.Singleton,
                Setup.Default.WithReuseWrappers(typeof(ReusedWeakRef), typeof(ExplicitlyDisposableReused)));

            Assert.Throws<ContainerException>(() => 
            container.Resolve<ReusedWeakRef>(typeof(IService)));
        }

        [Test]
        public void Cannot_resolve_non_reused_service_as_WeakReference_without_required_type()
        {
            var container = new Container();
            container.Register<IService, Service>();

            Assert.Throws<ContainerException>(() =>
                container.Resolve<ReusedWeakRef>());
        }

        [Test]
        public void CanNot_resolve_non_reused_service_as_WeakRef()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ReusedWeakRef>(typeof(IService)));

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve reuse wrapper DryIoc.ReusedWeakRef"));
        }

        [Test]
        public void CanNot_resolve_non_reused_service_as_ExplicitlyDisposable()
        {
            var container = new Container();
            container.Register<IService, Service>();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<ExplicitlyDisposableReused>(typeof(IService)));

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve reuse wrapper DryIoc.ExplicitlyDisposableReused"));
        }

        [Test]
        public void Can_resolve_func_with_args_of_reuse_wrapper()
        {
            using (var container = new Container().OpenScope())
            {
                container.Register<Service>();
                container.Register<ServiceWithParameterAndDependency>(Reuse.InCurrentScope,
                    setup: Setup.Default.WithReuseWrappers(typeof(ReusedWeakRef)));

                var func = container.Resolve<Func<bool, ReusedWeakRef>>(typeof(ServiceWithParameterAndDependency));
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
                setup: Setup.Default.WithReuseWrappers(typeof(RefReused)));

            var func = container.Resolve<Func<bool, RefReused>>(typeof(ServiceWithParameterAndDependency));
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
                setup: Setup.Default.WithReuseWrappers(typeof(RefReused)));

            IService service;
            using (container.OpenScope())
                service = container.Resolve<IService>();

            Assert.IsTrue(((DisposableService)service).IsDisposed);
        }

        [Test]
        public void Can_resolve_service_as_typed_Ref_proxy()
        {
            var container = new Container();

            container.Register<Service>();
            container.Register<ServiceWithParameterAndDependency>(Reuse.Singleton,
                setup: Setup.Default.WithReuseWrappers(typeof(RefReused)));

            var func = container.Resolve<Func<bool, ReusedRef<ServiceWithParameterAndDependency>>>();
            var service = func(true);
            var service2 = func(true);

            Assert.That(service.Target, Is.InstanceOf<ServiceWithParameterAndDependency>());
            Assert.That(service.Target, Is.SameAs(service2.Target));

            var newService = new ServiceWithParameterAndDependency(new Service(), false);
            service.Swap(_ => newService);
            var getNew = container.Resolve<Func<bool, ServiceWithParameterAndDependency>>();
            Assert.That(getNew(true), Is.SameAs(newService));
        }

        [Test]
        public void May_resolve_two_nested_wrappers()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.Default.WithReuseWrappers(typeof(ExplicitlyDisposableReused), typeof(RefReused)));

            var service = container.Resolve<ReusedRef<ExplicitlyDisposableReused<DisposableService>>>();
            service.Target.Dispose();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<DisposableService>());

            Assert.That(ex.Message, Is.StringContaining("Target of type WeakReference was already disposed in DryIoc.ExplicitlyDisposable"));
        }

        //[Test]
        //public void Can_renew_shared_instance_with_ExplicitlyRenewable_shared_in_current_scope()
        //{
        //    using (var container = new Container().OpenScope())
        //    {
        //        container.Register<Service>(Shared.InCurrentScope,
        //            setup: Setup.Default.WithReuseWrappers(typeof(Disposable)));

        //        var renewable = container.Resolve<Disposable<Service>>();
        //        var service = renewable.Target;
        //        renewable.MarkForRenew();

        //        var newService = container.Resolve(typeof(Service));
        //        Assert.That(newService, Is.Not.SameAs(service));
        //    }
        //}
    }
}
