using System;
using System.Linq.Expressions;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue417_Performance_issue_with_Func : ITest
    {
        public int Run()
        {
            Can_inject_singleton_as_resolve_call_inside_func_with_args();
            return 1;
        }

        [Test]
        public void Can_inject_singleton_as_resolve_call_inside_func_with_args()
        {
            var container = new Container();

            container.Register<ResolvedSingleton>(Reuse.Singleton, setup: Setup.With(asResolutionCall: true));
            container.Register<Dependency1>(Reuse.Transient);
            container.Register<Dependency2>(Reuse.Transient);
            container.Register<SingletonDependant>(Reuse.Transient);

            container.Resolve<ResolvedSingleton>();//I expect that container will remember this singleton

            var factoryExprString = container.Resolve<LambdaExpression, Func<string, SingletonDependant>>().ToString();

            StringAssert.DoesNotContain("Dependency1", factoryExprString);
            StringAssert.DoesNotContain("Dependency2", factoryExprString);

            var factory = container.Resolve<Func<string, SingletonDependant>>();
            Assert.IsNotNull(factory("value"));
        }

        class SingletonDependant
        {
            public SingletonDependant(string someParameter, ResolvedSingleton resolvedSingleton)
            {
            }
        }

        class ResolvedSingleton
        {
            public ResolvedSingleton(Dependency1 dependency1, Dependency2 dependency2)
            {
            }
        }

        internal class Dependency2
        {
        }

        internal class Dependency1
        {
        }
    }
}
