using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    class OpenScopeTests
    {
        [Test]
        public void Test()
        {
            var container = new Container();
            container.Register<ISessionFactory, TestSessionFactory>();
            container.RegisterDelegate(r => r.Resolve<ISessionFactory>().OpenSession(), Reuse.InCurrentScope);

            ISession scopeOneSession;
            using (var scopedContainer = container.OpenScope())
            {
                scopeOneSession = scopedContainer.Resolve<ISession>();
                Assert.AreSame(scopeOneSession, scopedContainer.Resolve<ISession>());
            }

            using (var scopedContainer = container.OpenScope())
            {
                var scopeTwoSession = scopedContainer.Resolve<ISession>();
                Assert.AreNotSame(scopeOneSession, scopeTwoSession);
                Assert.AreSame(scopeTwoSession, scopedContainer.Resolve<ISession>());
            }
        }
    }

    public interface ISession { }

    class TestSession : ISession { }

    public interface ISessionFactory
    {
        ISession OpenSession();
    }

    class TestSessionFactory : ISessionFactory
    {
        public ISession OpenSession()
        {
            return new TestSession();
        }
    }
}
