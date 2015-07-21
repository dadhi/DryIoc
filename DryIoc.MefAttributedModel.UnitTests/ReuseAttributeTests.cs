using System;
using System.ComponentModel.Composition;
using DryIocAttributes;
using NUnit.Framework;

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
            using (var container = new Container().WithMefAttributedModel().OpenScope()) 
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
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(ServiceWithResolutionScopeReuse), typeof(UserOfServiceWithResolutionScopeReuse));

            var one = container.Resolve<ServiceWithResolutionScopeReuse>();
            var user = container.Resolve<UserOfServiceWithResolutionScopeReuse>();

            Assert.That(one, Is.Not.SameAs(user.One));
            Assert.That(user.One, Is.SameAs(user.Another));
        }

        [Test]
        public void Can_specify_reuse_wrapper_for_exported_service()
        {
            var container = new Container().WithMefAttributedModel();
            container.RegisterExports(typeof(SharedWithReuseWrapper));

            var serviceRef = container.Resolve<ReuseWeakReference>(typeof(SharedWithReuseWrapper));

            Assert.That(serviceRef.Target, Is.InstanceOf<SharedWithReuseWrapper>());

            GC.KeepAlive(serviceRef);
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

                Assert.AreEqual(DryIoc.Error.UnableToResolveFromRegisteredServices, ex.Error);
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

        [Export, TransientReuse]
        public class ServiceWithReuseAttribute { }

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

        [Export, ReuseWrappers(typeof(ReuseWeakReference))]
        public class SharedWithReuseWrapper { }

        [Export, CurrentScopeReuse("ScopeA")]
        public class WithNamedCurrentScope { }
    }
}
