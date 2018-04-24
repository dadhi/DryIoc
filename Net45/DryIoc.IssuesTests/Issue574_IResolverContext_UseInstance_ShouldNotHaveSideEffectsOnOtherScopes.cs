using NUnit.Framework;
using System;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue574_IResolverContext_UseInstance_ShouldNotHaveSideEffectsOnOtherScopes
    {
        [Test]
        public void ScopedFactory_ShouldResolveItselfWithinSelfScope_EvenIfThereAreParallelScopes()
        {
            var container = new Container();

            container.Register<IScopedFactory, ScopedFactory>(Reuse.Transient);

            var scopedFactory1 = container.Resolve<IScopedFactory>();
            var scopedFactory2 = scopedFactory1.Resolve<IScopedFactory>();
            Assert.AreSame(scopedFactory1, scopedFactory2);

            //doing the same once again
            var scopedFactory3 = container.Resolve<IScopedFactory>();
            var scopedFactory4 = scopedFactory3.Resolve<IScopedFactory>();
            Assert.AreNotSame(scopedFactory1, scopedFactory3);
            Assert.AreSame(scopedFactory3, scopedFactory4);
        }

        [Test]
        public void ScopedFactory_ShouldResolveItselfWithinSelfScope_EvenIfThereAreParallelScopesAndNullArgsProvided()
        {
            var container = new Container();

            container.Register<IScopedFactory, ScopedFactory>(Reuse.Transient);

            var scopedFactory1 = container.Resolve<IScopedFactory>(args: null);
            var scopedFactory2 = scopedFactory1.Resolve<IScopedFactory>();
            Assert.AreSame(scopedFactory1, scopedFactory2);

            var scopedFactory3 = container.Resolve<IScopedFactory>(args: null);
            var scopedFactory4 = scopedFactory3.Resolve<IScopedFactory>();
            Assert.AreNotSame(scopedFactory1, scopedFactory3);
            Assert.AreSame(scopedFactory3, scopedFactory4);
        }

        interface IScopedFactory
        {
            T Resolve<T>();
        }

        class ScopedFactory : IScopedFactory
        {
            private IResolverContext myScope;
            public ScopedFactory(IContainer container)
            {
                myScope = container.OpenScope();

                //use this factory within it`s scope
                myScope.UseInstance<IScopedFactory>(this);
            }

            public T Resolve<T>()
            {
                return myScope.Resolve<T>();
            }
        }
    }
}
