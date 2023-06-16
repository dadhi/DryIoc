using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue580_Scope_is_lost_in_IResolver_inside_scope_because_of_singleton : ITest
    {
        public int Run()
        {
            Test_scope_inside_the_transient_dependency_of_the_scoped_service();
            Test_scope_inside_the_transient_dependency_of_the_scoped_service_But_injected_first_in_singleton();
            return 2;
        }

        [Test]
        public void Test_scope_inside_the_transient_dependency_of_the_scoped_service()
        {
            var container = new Container();

            container.Register<ServiceSingleton>(reuse: Reuse.Singleton);
            container.Register<ServiceTransient>(reuse: Reuse.Transient);
            container.Register<ServiceResolver>(reuse: Reuse.Transient);
            container.Register<ServiceScoped>(reuse: Reuse.Scoped);

            using var scope = container.OpenScope();

            var transient = scope.Resolve<ServiceTransient>();
            transient.Do();
        }

        // [Test] // todo: @fixme
        public void Test_scope_inside_the_transient_dependency_of_the_scoped_service_But_injected_first_in_singleton()
        {
            var container = new Container();

            container.Register<ServiceSingleton>(reuse: Reuse.Singleton);
            container.Register<ServiceTransient>(reuse: Reuse.Transient);
            container.Register<ServiceResolver>(reuse: Reuse.Transient);
            container.Register<ServiceScoped>(reuse: Reuse.Scoped);

            // resolving singleton first breaks the resolution in the Do below
            var singleton = container.Resolve<ServiceSingleton>();

            using var scope = container.OpenScope();

            var transient = scope.Resolve<ServiceTransient>();
            transient.Do();
        }

        public class ServiceSingleton
        {
            private readonly ServiceResolver _serviceResolver;

            public ServiceSingleton(ServiceResolver serviceResolver) =>
                _serviceResolver = serviceResolver;
        }

        public class ServiceTransient
        {
            private readonly ServiceResolver _serviceResolver;

            public ServiceTransient(ServiceResolver serviceResolver) =>
                _serviceResolver = serviceResolver;

            public void Do()
            {
                _ = _serviceResolver.GetService<ServiceScoped>();
            }
        }

        public class ServiceResolver
        {
            private readonly IResolver _resolver;

            public ServiceResolver(IResolver resolver) =>
                _resolver = resolver;

            public TService GetService<TService>() =>
                _resolver.Resolve<TService>();
        }

        public class ServiceScoped
        {
        }
    }
}
