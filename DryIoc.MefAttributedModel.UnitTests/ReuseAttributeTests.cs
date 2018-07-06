using System;
using NUnit.Framework;
using DryIoc.MefAttributedModel.UnitTests.CUT;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ReuseAttributeTests
    {
        [Test]
        public void Can_specify_any_supported_Reuse_using_attribute()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(ServiceWithReuseAttribute));

            var service = container.Resolve<ServiceWithReuseAttribute>();
            var otherService = container.Resolve<ServiceWithReuseAttribute>();

            Assert.That(service, Is.Not.SameAs(otherService));
        }

        [Test]
        public void Can_specify_singleton_reuse()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(ServiceWithSingletonReuse));

            var one = container.Resolve<ServiceWithSingletonReuse>();
            var another = container.Resolve<ServiceWithSingletonReuse>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Can_specify_current_scope_reuse()
        {
            var container = new Container().WithMefAttributedModel();
            using (var scope = container.OpenScope()) 
            { 
                container.RegisterExports(typeof(ServiceWithCurrentScopeReuse));

                var one = scope.Resolve<ServiceWithCurrentScopeReuse>();
                using (var scope2 = scope.OpenScope())
                {
                    var oneInScope = scope2.Resolve<ServiceWithCurrentScopeReuse>();
                    var anotherInScope = scope2.Resolve<ServiceWithCurrentScopeReuse>();

                    Assert.That(one, Is.Not.SameAs(oneInScope));
                    Assert.That(oneInScope, Is.SameAs(anotherInScope));
                }
            }
        }

        [Test]
        public void Can_specify_resolution_scope_reuse()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(
                typeof(ServiceWithResolutionScopeReuse), 
                typeof(UserOfServiceWithResolutionScopeReuse));

            var user = container.Resolve<UserOfServiceWithResolutionScopeReuse>();

            Assert.AreSame(user.One, user.Another);
        }

        [Test, Explicit("Deals with WeakReference and may fail on CI")]
        public void Can_specify_to_store_reused_instance_as_weak_reference()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(WeaklyReferencedService));

            var service = container.Resolve<WeaklyReferencedService>();

            Assert.IsInstanceOf<WeaklyReferencedService>(service);
            GC.KeepAlive(service);
        }

        [Test]
        public void Can_specify_to_prevent_disposal_for_reused_instance()
        {
            var container = new Container().WithMefAttributedModel();

            container.RegisterExports(typeof(PreventDisposalService));

            var service = container.Resolve<PreventDisposalService>();
            container.Dispose();

            Assert.IsFalse(service.IsDisposed);
        }

        [Test]
        public void Allows_disposable_transient()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DT), typeof(DT2), typeof(DTUser));

            var user = container.Resolve<DTUser>();

            container.Dispose();
            Assert.IsFalse(user.Dt.IsDisposed);
            Assert.IsTrue(user.Dt2.IsDisposed);
        }

        [Test]
        public void Allows_tracking_disposable_transient_in_singleton_scope()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(DT2));

            var disposable = container.Resolve<DT2>();

            container.Dispose();
            Assert.IsTrue(disposable.IsDisposed);
        }

        [Test]
        public void Can_track_disposable_transient_but_export_option_still_override_the_container_option()
        {
            var container = new Container(rules => rules
                .WithTrackingDisposableTransients())
                .WithMefAttributedModel();

            container.RegisterExports(typeof(DT), typeof(DT2), typeof(DTUser));

            var user = container.Resolve<DTUser>();

            container.Dispose();
            Assert.IsFalse(user.Dt.IsDisposed);
            Assert.IsTrue(user.Dt2.IsDisposed);
        }

        [Test]
        public void When_no_named_current_scope_reuse_Then_it_should_throw()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(WithNamedCurrentScope));

            using (var scoped = container.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() => 
                    scoped.Resolve<WithNamedCurrentScope>());

                Assert.AreEqual(DryIoc.Error.NoMatchedScopeFound, ex.Error);
            }
        }

        [Test]
        public void When_there_is_corresponding_named_current_scope_Then_it_should_resolve()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(WithNamedCurrentScope));

            using (var scoped = container.OpenScope("ScopeA"))
            {
                var service = scoped.Resolve<WithNamedCurrentScope>();
                Assert.AreSame(service, scoped.Resolve<WithNamedCurrentScope>());
            }
        }
    }
}
