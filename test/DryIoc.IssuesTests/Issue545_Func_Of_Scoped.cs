using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue545_Func_Of_Scoped : ITest
    {
        public int Run()
        {
            Func_of_scoped_thing_should_return_object_from_a_new_scope_when_singleton_is_resolved_within_the_scope();
            Func_of_scoped_thing_should_return_object_from_a_new_scope_when_singleton_is_resolved_out_of_the_scope();
            return 2;
        }

        public interface IScopedComponent { }

        public class ScopedComponent : IScopedComponent { }

        public interface ISingletonComponent
        {
            IScopedComponent Scoped { get; }
        }

        public class SingletonComponentWithScopedFunc : ISingletonComponent
        {
            private Func<IScopedComponent> _getScoped;
            public SingletonComponentWithScopedFunc(Func<IScopedComponent> getScoped)
            {
                _getScoped = getScoped;
            }

            public IScopedComponent Scoped => _getScoped();
        }

        [Test]
        public void Func_of_scoped_thing_should_return_object_from_a_new_scope_when_singleton_is_resolved_within_the_scope()
        {
            var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            c.Register<ISingletonComponent, SingletonComponentWithScopedFunc>(Reuse.Singleton);

            var scopeName = "myScope";
            c.Register<IScopedComponent, ScopedComponent>(Reuse.ScopedTo(scopeName));

            IScopedComponent scoped1a, scoped1b, scoped2;

            using (var scope = c.OpenScope(scopeName))
            {
                var singleton = scope.Resolve<ISingletonComponent>();
                scoped1a = singleton.Scoped;
                scoped1b = singleton.Scoped;
            }

            using (var scope = c.OpenScope(scopeName))
            {
                var singleton = scope.Resolve<ISingletonComponent>();
                scoped2 = singleton.Scoped;
            }

            Assert.AreSame(scoped1b, scoped1a);
            Assert.AreNotSame(scoped1a, scoped2);
        }

        [Test]
        public void Func_of_scoped_thing_should_return_object_from_a_new_scope_when_singleton_is_resolved_out_of_the_scope()
        {
            var c = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            c.Register<ISingletonComponent, SingletonComponentWithScopedFunc>(Reuse.Singleton);
            var scopeName = "myScope";
            c.Register<IScopedComponent, ScopedComponent>(Reuse.InCurrentNamedScope(scopeName));

            var singleton = c.Resolve<ISingletonComponent>();
            IScopedComponent scoped1a, scoped1b, scoped2;

            using (c.OpenScope(scopeName))
            {
                scoped1a = singleton.Scoped;
                scoped1b = singleton.Scoped;
            }

            using (c.OpenScope(scopeName))
            {
                scoped2 = singleton.Scoped;
            }

            Assert.AreSame(scoped1b, scoped1a);
            Assert.AreNotSame(scoped1a, scoped2);
        }
    }
}