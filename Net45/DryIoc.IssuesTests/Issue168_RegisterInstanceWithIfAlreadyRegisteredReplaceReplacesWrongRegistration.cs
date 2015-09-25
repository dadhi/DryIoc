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
            container.RegisterInstance<A>(new A(), Reuse.Singleton, IfAlreadyRegistered.Replace);
            container.RegisterInstance<B>(new B(), Reuse.Singleton, IfAlreadyRegistered.Replace);
            container.Register<C>(Reuse.Transient);
            container.Resolve<C>(IfUnresolved.Throw);
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
