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
        public void If_reuse_type_does_not_implement_IReuse_it_should_Throw()
        {
            var container = new Container().WithAttributedModel();
            Assert.Throws<ContainerException>(() => 
                container.RegisterExports(typeof(ServiceWithBadReuseAttribute)));
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
            var container = new Container().WithAttributedModel();
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
    }

    [Export, TransientReuse]
    public class ServiceWithReuseAttribute {}

    [Export, Reuse(typeof(string))]
    public class ServiceWithBadReuseAttribute { }

    [Export, SingletonReuse]
    public class ServiceWithSingletonReuse { }

    [Export, CurrentScopeReuse]
    public class ServiceWithCurrentScopeReuse { }

    [Export, ResolutionScopeReuse]
    public class ServiceWithResolutionScopeReuse { }

    [Export]
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
}
