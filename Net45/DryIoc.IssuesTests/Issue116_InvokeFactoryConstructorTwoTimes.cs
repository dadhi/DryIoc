using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue116_InvokeFactoryConstructorTwoTimes
    {
        class SomeSingleton
        {
        }

        class A
        {
            public A(SomeSingleton someSingleton)
            {
            }
        }

        class Factory
        {
            public static int ConstructorInvoked = 0;

            public Factory(Func<A> factory)
            {
                // important!
                var a = factory();
                ConstructorInvoked++;
            }
        }

        [Test]
        public void While_resolve_singleton_should_invoke_constructor_1_time()
        {
            var container = new Container();
            container.Register<SomeSingleton>(Reuse.Singleton);
            container.Register<A>();
            container.Register<Factory>(Reuse.Singleton);

            var fact = container.Resolve<Factory>();
            Assert.That(Factory.ConstructorInvoked, Is.EqualTo(1));
        }
    }
}
