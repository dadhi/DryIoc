using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue343_Scope_validation_for_Transient_does_not_work_as_expected
    {
        [Test]
        public void I_should_be_able_to_specify_Transient_as_captive_dependency()
        {
            var container = new Container(rules => rules.WithThrowIfScopedOrSingletonHasTransientDependency());

            container.Register<HttpContextAccessor>(Reuse.Transient);
            container.RegisterDelegate<HttpContextAccessor, ILog>(acc => new ScopedLog(acc), Reuse.Singleton);

            var ex = Assert.Throws<ContainerException>(() =>
            container.ValidateAndThrow(typeof(ILog)));

            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.CollectedExceptions[0].ErrorName);
        }

        [Test]
        public void By_default_the_Transient_is_not_a_captive_dependency()
        {
            var container = new Container();

            container.Register<HttpContextAccessor>(Reuse.Transient);
            container.RegisterDelegate<HttpContextAccessor, ILog>(acc => new ScopedLog(acc), Reuse.Singleton);

            Assert.DoesNotThrow(() =>
            container.ValidateAndThrow(typeof(ILog)));
        }

        [Test]
        public void The_Scoped_is_still_a_captive_dependency_in_Singleton()
        {
            var container = new Container(rules => rules.WithThrowIfScopedOrSingletonHasTransientDependency());

            container.Register<HttpContextAccessor>(Reuse.Scoped);
            container.RegisterDelegate<HttpContextAccessor, ILog>(acc => new ScopedLog(acc), Reuse.Singleton);

            var ex = Assert.Throws<ContainerException>(() =>
            container.ValidateAndThrow(typeof(ILog)));

            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.CollectedExceptions[0].ErrorName);
        }

        [Test]
        public void The_switching_off_shorter_reuse_lifespan_error_doesnot_switch_off_the_Transient_error_you_need_to_specify_this()
        {
            IContainer container = new Container(rules => rules
                .WithThrowIfScopedOrSingletonHasTransientDependency()
                .WithoutThrowIfDependencyHasShorterReuseLifespan());

            container.Register<HttpContextAccessor>(); // transient by default
            container.RegisterDelegate<HttpContextAccessor, ILog>(acc => new ScopedLog(acc), Reuse.Singleton);

            var ex = Assert.Throws<ContainerException>(() =>
            container.ValidateAndThrow(typeof(ILog)));

            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.CollectedExceptions[0].ErrorName);

            container = container.With(rules => rules.WithoutThrowIfScopedOrSingletonHasTransientDependency());
            Assert.DoesNotThrow(() =>
            container.ValidateAndThrow(typeof(ILog)));
        }

        interface ILog { }
        class HttpContextAccessor { }
        class ScopedLog : ILog
        {
            public ScopedLog(HttpContextAccessor accessor) { }
        }
    }
}
