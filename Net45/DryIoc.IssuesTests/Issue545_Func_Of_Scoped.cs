﻿using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue545_Func_Of_Scoped
    {
        public interface IScopedComponent {}

        public class ScopedComponent : IScopedComponent {}

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
            var c = new Container();

            c.Register<ISingletonComponent, SingletonComponentWithScopedFunc>(Reuse.Singleton);
            var scopeName = "myScope";
            c.Register<IScopedComponent, ScopedComponent>(Reuse.InCurrentNamedScope(scopeName));

            IScopedComponent scoped1, scoped2;

            using (var scope = c.OpenScope(scopeName))
            {
                var singleton = scope.Resolve<ISingletonComponent>();
                scoped1 = singleton.Scoped;
            }

            using (var scope = c.OpenScope(scopeName))
            {
                var singleton = scope.Resolve<ISingletonComponent>();
                scoped2 = singleton.Scoped;
            }

            Assert.AreNotSame(scoped1, scoped2);
        }

        [Test]
        public void Func_of_scoped_thing_should_return_object_from_a_new_scope_when_singleton_is_resolved_out_of_the_scope()
        {
            var c = new Container();

            c.Register<ISingletonComponent, SingletonComponentWithScopedFunc>(Reuse.Singleton);
            var scopeName = "myScope";
            c.Register<IScopedComponent, ScopedComponent>(Reuse.InCurrentNamedScope(scopeName));

            var singleton = c.Resolve<ISingletonComponent>();
            IScopedComponent scoped1, scoped2;

            using (c.OpenScope(scopeName))
            {
                scoped1 = singleton.Scoped;
            }

            using (c.OpenScope(scopeName))
            {
                scoped2 = singleton.Scoped;
            }

            Assert.AreNotSame(scoped1, scoped2);
        }
    }
}