using System;
using System.ComponentModel.Composition;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class ReuseAttributeTests
    {
        [Test]
        public void Can_specify_any_supported_Reuse_using_attribute()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(ServiceWithReuseAttribute));

            var service = container.Resolve<ServiceWithReuseAttribute>();
            var otherService = container.Resolve<ServiceWithReuseAttribute>();

            Assert.That(service, Is.Not.SameAs(otherService));
        }

        [Test]
        public void Can_specify_singleton_reuse()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(ServiceWithSingletonReuse));

            var one = container.Resolve<ServiceWithSingletonReuse>();
            var another = container.Resolve<ServiceWithSingletonReuse>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Can_specify_current_scope_reuse()
        {
            using (var container = new Container().WithAttributedModel().OpenScope()) 
            { 
                container.RegisterExports(typeof(ServiceWithCurrentScopeReuse));

                var one = container.Resolve<ServiceWithCurrentScopeReuse>();
                using (var scope = container.OpenScope())
                {
                    var oneInScope = scope.Resolve<ServiceWithCurrentScopeReuse>();
                    var anotherInScope = scope.Resolve<ServiceWithCurrentScopeReuse>();

                    Assert.That(one, Is.Not.SameAs(oneInScope));
                    Assert.That(oneInScope, Is.SameAs(anotherInScope));
                }
            }
        }

        [Test]
        public void Can_specify_resolution_scope_reuse()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(ServiceWithResolutionScopeReuse), typeof(UserOfServiceWithResolutionScopeReuse));

            var one = container.Resolve<ServiceWithResolutionScopeReuse>();
            var user = container.Resolve<UserOfServiceWithResolutionScopeReuse>();

            Assert.That(one, Is.Not.SameAs(user.One));
            Assert.That(user.One, Is.SameAs(user.Another));
        }

        [Test]
        public void Can_specify_reuse_wrapper_for_exported_service()
        {
            var container = new Container().WithAttributedModel();
            container.RegisterExports(typeof(SharedWithReuseWrapper));

            var serviceRef = container.Resolve<WeakReference>(typeof(SharedWithReuseWrapper));

            Assert.That(serviceRef.Target, Is.InstanceOf<SharedWithReuseWrapper>());

            GC.KeepAlive(serviceRef);
        }
    }

    [Export, TransientReuse]
    public class ServiceWithReuseAttribute {}

    [Export, SingletonReuse]
    public class ServiceWithSingletonReuse { }

    [Export, CurrentScopeReuse]
    public class ServiceWithCurrentScopeReuse { }

    [Export, ResolutionScopeReuse]
    public class ServiceWithResolutionScopeReuse { }

    [Export, ResolutionScopeReuse]
    public class UserOfServiceWithResolutionScopeReuse
    {
        public ServiceWithResolutionScopeReuse One { get; set; }
        public ServiceWithResolutionScopeReuse Another { get; set; }

        public UserOfServiceWithResolutionScopeReuse(
            ServiceWithResolutionScopeReuse one,
            ServiceWithResolutionScopeReuse another)
        {
            One = one;
            Another = another;
        }
    }

    [Export, ReuseWrappers(typeof(WeakReference))]
    public class SharedWithReuseWrapper {}
}
