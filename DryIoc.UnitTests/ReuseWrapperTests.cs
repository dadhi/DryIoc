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
            container.RegisterInstance(new DisposableService(), Reuse.InCurrentScope);
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
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            var service = container.Resolve<IService>();

            container.Dispose();

            Assert.That(((DisposableService)service).IsDisposed, Is.False);
        }

        [Test]
        public void Can_resolve_explicitly_disposed_the_same_way_as_generic_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var wrapper = container.Resolve<ExplicitlyDisposable>(typeof(IService));

            Assert.That(wrapper.Target, Is.InstanceOf<DisposableService>());
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_service_and_then_dispose_it()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var disposable = container.Resolve<ExplicitlyDisposable>(typeof(IService));
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
        public void Can_resolve_explicitly_disposed_singleton_and_then_dispose_it()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));
            container.Register(typeof(ExplicitlyDisposable<>), setup: WrapperSetup.Default);

            var disposable = container.Resolve<ExplicitlyDisposable<IService>>();
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
            var container = new Container();
            container.Register<DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));
            container.Register(typeof(ExplicitlyDisposable<>), setup: WrapperSetup.Default);

            var disposable = container.Resolve<ExplicitlyDisposable<IService>>(typeof(DisposableService));

            Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
        }

        [Test]
        public void Can_resolve_explicitly_disposed_singleton_with_required_type()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            container.Register(typeof(ExplicitlyDisposable<>), setup: WrapperSetup.Default);

            var disposable = container.Resolve<ExplicitlyDisposable<IService>>(typeof(DisposableService));

            Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
        }

        [Test]
        public void Should_throw_on_unknown_reuse_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable }));

            container.Register(typeof(UnknownWrapper), 
                CreateReuseWrapperFactory(_ => typeof(WeakReference), typeof(WeakReference)));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<UnknownWrapper>());

            Assert.That(ex.Message, Is
                .StringContaining("Unable to resolve WeakReference").And
                .StringContaining("in wrapper DryIoc.UnitTests.ReuseWrapperTests.UnknownWrapper"));
        }

        [Test, Explicit]
        public void Can_store_reused_object_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(WeakReference), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Can_resolve_different_reused_services_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeaklyReferenced }));
            container.Register<Dependency>(Reuse.InResolutionScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeaklyReferenced }));
            container.Register<ServiceClient>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(WeakReference), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var service = container.Resolve<WeakReference>(typeof(IService));
            var dependency = container.Resolve<WeakReference>(typeof(Dependency));
            var client = container.Resolve<WeakReference>(typeof(ServiceClient));

            Assert.That(service.Target, Is.InstanceOf<Service>());
            Assert.That(dependency.Target, Is.InstanceOf<Dependency>());
            Assert.That(client.Target, Is.InstanceOf<ServiceClient>());
        }

        [Test, Explicit]
        public void Can_nest_Disposable_into_Weak_Referenced_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable, ReuseWrapper.WeaklyReferenced }));

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
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable, ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            container.Register(typeof(WeakReference), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ExplicitlyDisposable>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
        }

        [Test]
        public void Can_nest_Disposable_into_WeakReference_reused_in_singleton_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExplicitlyDisposable, ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            container.Register(typeof(WeakReference), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ExplicitlyDisposable>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
        }

        [Test]
        public void Cannot_resolve_non_reused_service_as_WeakReference_without_required_type()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Register(typeof(WeakReference), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<WeakReference>());
        }

        [Test]
        public void Can_resolve_non_reused_service_as_WeakReference()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Register(typeof(WeakReference), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());
        }

        [Test]
        public void Can_resolve_non_reused_service_as_ExplicitlyDisposable()
        {
            var container = new Container();
            container.Register<IService, Service>();

            container.Register(typeof(ExplicitlyDisposable), CreateReuseWrapperFactory(_ => typeof(object), typeof(object)));

            var serviceWeakRef = container.Resolve<ExplicitlyDisposable>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());
        }

        private static Factory CreateReuseWrapperFactory(
            Func<Type, Type> getWrappedServiceType, 
            params Type[] wrapperConstructorArgs)
        {
            return new ExpressionFactory(request =>
            {
                var wrapperType = request.ServiceType;
                var serviceType = request.Registry.GetWrappedServiceType(request.RequiredServiceType ?? wrapperType);
                var serviceRequest = request.Push(serviceType);
                var serviceFactory = request.Registry.ResolveFactory(serviceRequest);
                if (serviceFactory == null)
                    return null;

                if (serviceFactory.Reuse != null && !serviceFactory.Setup.ReuseWrappers.IsNullOrEmpty() &&
                    serviceFactory.Setup.ReuseWrappers.IndexOf(w => w.WrapperType == wrapperType) != -1)
                    return serviceFactory.GetExpressionOrDefault(serviceRequest, wrapperType);

                // otherwise resolve wrapper using reflection factory.
                var reflectionFactory = new ReflectionFactory(wrapperType, setup: WrapperSetup.With(
                    (type, _) => type.GetConstructorOrNull(args: wrapperConstructorArgs),
                    getWrappedServiceType: getWrappedServiceType));

                return reflectionFactory.GetExpressionOrDefault(request);
            }, setup: WrapperSetup.With(getWrappedServiceType));
        }

        public class UnknownWrapper
        {
            public WeakReference WekRef { get; set; }

            public UnknownWrapper(WeakReference wekRef)
            {
                WekRef = wekRef;
            }
        }
    }
}
