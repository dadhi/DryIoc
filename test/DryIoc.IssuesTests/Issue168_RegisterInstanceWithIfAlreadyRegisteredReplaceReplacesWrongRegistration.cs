using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue168_RegisterInstanceWithIfAlreadyRegisteredReplaceReplacesWrongRegistration
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.RegisterInstance<A>(new A());
            container.RegisterInstance<B>(new B());
            container.Register<C>(Reuse.Transient);

            container.Resolve<C>();
        }

        public class A
        {
        }
        public class B
        {
        }
        public class C
        {
            public C(A a, B b)
            {
            }
        }
    }
}
