using System;
using System.Linq.Expressions;
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
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            var service = container.Resolve<DisposableService>();

            container.Dispose();

            Assert.That(service.IsDisposed, Is.False);
        }

        [Test]
        public void Can_specify_do_Not_dispose_disposable_scoped_object()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            var service = container.Resolve<IService>();

            container.Dispose();

            Assert.That(((DisposableService)service).IsDisposed, Is.False);
        }

        [Test]
        public void Can_resolve_explicitly_disposed_the_same_way_as_generic_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            var wrapper = container.Resolve<ExternallyDisposable>(typeof(IService));

            Assert.That(wrapper.Target, Is.InstanceOf<DisposableService>());
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_service_and_then_dispose_it()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression, 
                    setup: WrapperSetup.With(_ => typeof(object))));

            var disposable = container.Resolve<ExternallyDisposable>(typeof(IService));
            disposable.DisposeTarget();

            Assert.That(disposable.IsDisposed, Is.True);
            var targetEx = Assert.Throws<ContainerException>(() => { var _ = disposable.Target; });
            Assert.That(targetEx.Message, Is.StringContaining(
                "Target of type DryIoc.UnitTests.CUT.DisposableService was already disposed in DryIoc.ExternallyDisposable"));

            var resolveEx = Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
            Assert.That(resolveEx.Message, Is.EqualTo(targetEx.Message));
        }

        [Test]
        public void Can_resolve_explicitly_disposed_singleton_and_then_dispose_it()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            container.Register(typeof(ExternallyDisposable<>), setup: WrapperSetup.Default);

            var disposable = container.Resolve<ExternallyDisposable<IService>>();
            disposable.Dispose();

            Assert.That(disposable.IsDisposed, Is.True);
            var targetEx = Assert.Throws<ContainerException>(() => { var _ = disposable.Target; });
            Assert.That(targetEx.Message, Is.StringContaining(
                "Target of type DryIoc.UnitTests.CUT.DisposableService was already disposed in DryIoc.ExternallyDisposable"));

            var resolveEx = Assert.Throws<ContainerException>(() =>
                container.Resolve<IService>());
            Assert.That(resolveEx.Message, Is.EqualTo(targetEx.Message));
        }

        [Test]
        public void Can_resolve_explicitly_disposed_scoped_with_required_type()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            container.Register(typeof(ExternallyDisposable<>), setup: WrapperSetup.Default);

            var disposable = container.Resolve<ExternallyDisposable<IService>>(typeof(DisposableService));

            Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
        }

        [Test]
        public void Can_resolve_explicitly_disposed_singleton_with_required_type()
        {
            var container = new Container();
            container.Register<DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            container.Register(typeof(ExternallyDisposable<>), setup: WrapperSetup.Default);

            var disposable = container.Resolve<ExternallyDisposable<IService>>(typeof(DisposableService));

            Assert.That(disposable.Target, Is.InstanceOf<DisposableService>());
        }

        [Test]
        public void Should_throw_on_unknown_reuse_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable }));

            container.Register(typeof(UnknownWrapper<>),
                new FactoryProvider(_ => new ExpressionFactory(CreateReuseWrapperExpression), 
                    WrapperSetup.Default));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<UnknownWrapper<IService>>());

            Assert.That(ex.Message, Is.StringContaining(
                "Unable to resolve DryIoc.UnitTests.ReuseWrapperTests.UnknownWrapper<DryIoc.UnitTests.CUT.IService>"));
        }

        [Test, Explicit]
        public void Can_store_reused_object_as_weak_reference()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(WeakReference),
                new ExpressionFactory(CreateReuseWrapperExpression, 
                    setup: WrapperSetup.With(_ => typeof(object))));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            Assert.That(serviceWeakRef.Target, Is.InstanceOf<Service>());

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.IsAlive, Is.False);
        }

        [Test, Explicit]
        public void Can_nest_Disposable_into_Weak_Referenced_wrapper()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable, ReuseWrapper.WeaklyReferenced }));

            var service = container.Resolve<IService>();
            var serviceWeakRef = new WeakReference(service);
            service = null;

            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            GC.KeepAlive(container);

            Assert.That(serviceWeakRef.IsAlive, Is.False);
        }

        [Test]
        public void Can_nest_Disposable_into_WeakReference_reused_in_current_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.InCurrentScope,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable, ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            container.Register(typeof(WeakReference),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ExternallyDisposable>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
        }

        [Test]
        public void Can_nest_Disposable_into_WeakReference_reused_in_singleton_scope()
        {
            var container = new Container();
            container.Register<IService, DisposableService>(Reuse.Singleton,
                setup: Setup.With(reuseWrappers: new[] { ReuseWrapper.ExternallyDisposable, ReuseWrapper.WeaklyReferenced }));

            container.Register(typeof(ExternallyDisposable),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            container.Register(typeof(WeakReference),
                new ExpressionFactory(CreateReuseWrapperExpression,
                    setup: WrapperSetup.With(_ => typeof(object))));

            var serviceWeakRef = container.Resolve<WeakReference>(typeof(IService));
            var serviceDisposable = container.Resolve<ExternallyDisposable>(typeof(IService));

            Assert.That(serviceWeakRef.Target, Is.SameAs(serviceDisposable));
        }

        private static Expression CreateReuseWrapperExpression(Request request)
        {
            var wrapperType = request.ServiceType;
            var serviceType = request.Registry.GetWrappedServiceType(request.RequiredServiceType ?? wrapperType);
            var serviceRequest = request.Push(serviceType);
            var serviceFactory = request.Registry.ResolveFactory(serviceRequest);
            var serviceExpr = serviceFactory == null ? null : serviceFactory.GetExpressionOrDefault(serviceRequest, wrapperType);
            return serviceExpr;
        }

        public class UnknownWrapper<T>
        {
            public UnknownWrapper(WeakReference weakRef) { }
        }
    }
}
