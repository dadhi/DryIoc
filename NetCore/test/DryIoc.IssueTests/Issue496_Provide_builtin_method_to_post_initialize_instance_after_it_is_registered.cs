using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue496_Provide_builtin_method_to_post_initialize_instance_after_it_is_registered
    {
        [Test]
        public void Try_decorator_factory()
        {
            var c = new Container();

            c.Register<A>(Reuse.Singleton);
            c.Register<B>(Reuse.Singleton);

            c.Register<object>(
                made: Made.Of(
                    r => r.GetKnownImplementationOrServiceType().GetMethod("Init"), 
                    r => ServiceInfo.Of(r.ServiceType, serviceKey: r.ServiceKey)),
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

        [Test]
        public void Try_initializer()
        {
            var c = new Container();

            c.Register<M>(Reuse.Singleton);

            var mInitialized = false;
            c.RegisterInitializer<M>((m, r) =>
            {
                if (mInitialized)
                    return;
                mInitialized = true;

                m.S += "i";
                var n = r.Resolve<N>();
                Assert.AreEqual(">i", n.S);
            });

            c.Register<N>(Reuse.Singleton);

            var nInitialized = false;
            c.RegisterInitializer<N>((n, r) =>
            {
                if (nInitialized)
                    return;
                nInitialized = true;

                n.S += "i";
                var m = r.Resolve<M>();
                Assert.AreEqual(">i", m.S);
            });

            var mm = c.Resolve<M>();
            Assert.AreEqual(">i", mm.S);
            Assert.AreEqual(">i", c.Resolve<M>().S); // does not change because singleton

            var nn = c.Resolve<N>();
            Assert.AreEqual(">i", nn.S);
            Assert.AreEqual(">i", c.Resolve<N>().S); // does not change because singleton
        }

        class M
        {
            public string S = ">";
        }

        class N
        {
            public string S = ">";
        }
    }
}
