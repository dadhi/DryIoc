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

            container.Register<ServiceResolver>(reuse: Reuse.Transient);
            
            container.Register<ServiceSingleton>(reuse: Reuse.Singleton);
            container.Register<ServiceTransient>(reuse: Reuse.Transient);
            container.Register<ServiceScoped>(reuse: Reuse.Scoped);

            using var scope = container.OpenScope();

            var transient = scope.Resolve<ServiceTransient>();
            transient.Do();
        }

        [Test]
        public void Test_scope_inside_the_transient_dependency_of_the_scoped_service_But_injected_first_in_singleton()
        {
            var container = new Container();

            container.Register<ServiceResolver>(setup: Setup.With(useParentReuse: true)); // THIS IS THE FIX!

            container.Register<ServiceSingleton>(reuse: Reuse.Singleton);
            container.Register<ServiceTransient>(reuse: Reuse.Transient);
            container.Register<ServiceScoped>(reuse: Reuse.Scoped);

            // resolving singleton first breaks the resolution in the Do below
            container.Resolve<ServiceSingleton>();

            using var scope = container.OpenScope();

            var transient = scope.Resolve<ServiceTransient>();
            transient.Do();
        }

        public class ServiceSingleton
        {
            public readonly ServiceResolver ServiceResolver;

            public ServiceSingleton(ServiceResolver serviceResolver) =>
                ServiceResolver = serviceResolver;
        }

        public class ServiceTransient
        {
            public readonly ServiceResolver ServiceResolver;

            public ServiceTransient(ServiceResolver serviceResolver) =>
                ServiceResolver = serviceResolver;

            public void Do()
            {
                _ = ServiceResolver.GetService<ServiceScoped>();
            }
        }

        public class ServiceResolver
        {
            public readonly IResolver Resolver;

            public ServiceResolver(IResolver resolver) =>
                Resolver = resolver;

            public TService GetService<TService>() =>
                Resolver.Resolve<TService>();
        }

        public class ServiceScoped
        {
        }
    }
}
