using System;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue496_Provide_builtin_method_to_post_initialize_instance_after_it_is_registered
    {
        [Test]
        public void Test()
        {
            var c = new Container();

            c.Register<A>(Reuse.Singleton);
            c.Register<B>(Reuse.Singleton);

            c.Register<object>(
                made: Made.Of(r => FactoryMethod.Of(
                    r.GetKnownImplementationOrServiceType().GetMethod("Init"),
                    ServiceInfo.Of(r.ServiceType, serviceKey: r.ServiceKey))),
                setup: Setup.DecoratorOf<IInitializable>());

            var a = c.Resolve<A>();
            Assert.AreEqual("A is initialized", a.Name);
            Assert.AreEqual("B is initialized", a.B.Name);
        }

        public interface IInitializable { }

        public class A : IInitializable
        {
            public string Name = "A";
            public B B;

            public A Init(B b)
            {
                Name += " is initialized";
                B = b;
                return this;
            }
        }

        public class B : IInitializable
        {
            public string Name = "B";

            public B Init(A a)
            {
                Name += " is initialized";
                return this;
            }
        }
    }
}
