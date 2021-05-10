using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue399_Func_dependency_on_Singleton_resolved_under_scope_breaks_after_disposing_scope_when_WithFuncAndLazyWithoutRegistration : ITest
    {
        public int Run()
        {
            Test1();
            return 1;
        }

        [Test]
        public void Test1()
        {
            var container = new Container(rules => rules.WithFuncAndLazyWithoutRegistration());

            container.Register<DepFactory>(Reuse.Singleton);
            container.Register<Dep>(Reuse.Transient);

            DepFactory factory = null;

            using (var scope = container.OpenScope())
            {
                factory = scope.Resolve<DepFactory>();
            }

            var dep = factory.Create();
            Assert.IsNotNull(dep);

            container.Dispose();
        }

        class DepFactory
        {
            private readonly Func<Dep> _depFunc;
            public DepFactory(Func<Dep> depFunc) => _depFunc = depFunc;

            public Dep Create()
            {
                // DryIoc.ContainerException: 'code: Error.ContainerIsDisposed;
                // message: Container is disposed and should not be used: "container with scope {IsDisposed=true, Name=null}
                // with Rules with { FuncAndLazyWithoutRegistration} has been DISPOSED!
                return _depFunc.Invoke();
            }
        }

        class Dep { }
    }
}
