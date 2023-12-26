using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue550_Use_not_working_for_scoped_type_after_having_resolved_it_in_another_scope : ITest
    {
        public int Run()
        {
            Resolve_WithoutResolveInAnotherScopeBefore_Succeed();
            Resolve_WithResolveInAnotherScopeBefore_Fail();
            return 2;
        }

        private interface IScopedTypeDependency { }
        private class DefaultScopedTypeDependency : IScopedTypeDependency { }
        private class AnotherScopedTypeDependency : IScopedTypeDependency { }
        private class ScopedType
        {
            public IScopedTypeDependency Dependency { get; set; }
            public ScopedType(IScopedTypeDependency dependency)
            {
                Dependency = dependency;
            }
        }

        [Test]
        public void Resolve_WithoutResolveInAnotherScopeBefore_Succeed()
        {
            var container = new Container();
            container.Register<ScopedType>(Reuse.Scoped);
            container.Register<IScopedTypeDependency, DefaultScopedTypeDependency>(Reuse.Scoped);

            ScopedType scopedTypeObj;
            var dependency = new AnotherScopedTypeDependency();
            using (var scope = container.OpenScope())
            {
                scope.Use<IScopedTypeDependency>(dependency);
                scopedTypeObj = scope.Resolve<ScopedType>();
            }

            Assert.AreSame(scopedTypeObj.Dependency, dependency);
        }

        [Test]
        public void Resolve_WithResolveInAnotherScopeBefore_Fail()
        {
            var container = new Container();
            container.Register<ScopedType>(Reuse.Scoped);
            container.Register<IScopedTypeDependency, DefaultScopedTypeDependency>(
                Reuse.Scoped,
                setup: Setup.With(asResolutionCall: true)); // fixes the thing

            ScopedType scopedTypeObj;
            using (var scope1 = container.OpenScope())
                scope1.Resolve<ScopedType>();

            var dependency = new AnotherScopedTypeDependency();
            using (var scope2 = container.OpenScope())
            {
                scope2.Use<IScopedTypeDependency>(dependency);
                scopedTypeObj = scope2.Resolve<ScopedType>();
            }

            Assert.AreSame(scopedTypeObj.Dependency, dependency);
        }
    }
}
