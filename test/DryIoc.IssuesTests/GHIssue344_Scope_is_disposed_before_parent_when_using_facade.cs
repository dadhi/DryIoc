using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue344_Scope_is_disposed_before_parent_when_using_facade
    {
        [Test]
        public void The_test()
        {
            IContainer root = new Container();

            root.Register<MyDisposable>(Reuse.Scoped);

            // this happens within the DryIoc-MsDI adapter when ASP handles an http request 
            using (var scope = root.OpenScope())
            {
                // some service is resolved, that requires the disposable
                scope.Resolve<MyDisposable>();

                // another service is resolved, that depends on `IContainer` which it will receive from the context given to ASP
                // now, `container == ctx` which should be fine since the `WithCurrentScope` call returns an instance of `Container`
                var container = scope.Resolve<IContainer>();

                // the service now creates a facade for and works with it. After it's done, the facade is disposed.
                using (container.CreateFacade())
                {
                    // do anything at all, it doesn't matter
                }

                scope.Resolve<MyDisposable>(); // should not throw that the scope is disposed
            }
        }

        class MyDisposable : IDisposable
        {
            public void Dispose() {}
        }
    }
}
