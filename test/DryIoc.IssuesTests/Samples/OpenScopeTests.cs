using NUnit.Framework;

namespace DryIoc.IssuesTests.Samples
{
    [TestFixture]
    public class OpenScopeTests : ITest
    {
        public int Run()
        {
            Session_example_of_scope_usage();
            Session_example_of_scope_usage_when_factory_is_not_resolution_root();
            Session_example_of_scope_usage_using_factory_method();
            return 3;
        }

        [Test]
        public void Session_example_of_scope_usage()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<ISessionFactory, TestSessionFactory>();
            container.Register(Made.Of(r => ServiceInfo.Of<ISessionFactory>(), f => f.OpenSession()), Reuse.InCurrentScope);

            ISession scopeOneSession;
            using (var scoped = container.OpenScope())
            {
                scopeOneSession = scoped.Resolve<ISession>();
                Assert.AreSame(scopeOneSession, scoped.Resolve<ISession>());
            }

            using (var scoped = container.OpenScope())
            {
                var scopeTwoSession = scoped.Resolve<ISession>();
                Assert.AreNotSame(scopeOneSession, scopeTwoSession);
                Assert.AreSame(scopeTwoSession, scoped.Resolve<ISession>());
            }
        }

        [Test]
        public void Session_example_of_scope_usage_when_factory_is_not_resolution_root()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<SessionClient>();
            container.Register<ISessionFactory, TestSessionFactory>();
            container.Register(Made.Of(r => ServiceInfo.Of<ISessionFactory>(), f => f.OpenSession()), Reuse.InCurrentScope);

            SessionClient client;
            using (var scoped = container.OpenScope())
            {
                client = scoped.Resolve<SessionClient>();
                Assert.AreSame(client.Session, scoped.Resolve<SessionClient>().Session);
            }

            using (var scoped = container.OpenScope())
            {
                var clientScope2 = scoped.Resolve<SessionClient>();
                Assert.AreNotSame(client.Session, clientScope2.Session);
                Assert.AreSame(clientScope2.Session, scoped.Resolve<SessionClient>().Session);
            }
        }

        [Test]
        public void Session_example_of_scope_usage_using_factory_method()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<ISessionFactory, TestSessionFactory>();

            container.Register<ISession>(Reuse.InCurrentScope,
                Made.Of(r => ServiceInfo.Of<ISessionFactory>(), f => f.OpenSession()));

            ISession scopeOneSession;
            using (var scoped = container.OpenScope())
            {
                scopeOneSession = scoped.Resolve<ISession>();
                Assert.AreSame(scopeOneSession, scoped.Resolve<ISession>());
            }

            using (var scoped = container.OpenScope())
            {
                var scopeTwoSession = scoped.Resolve<ISession>();
                Assert.AreNotSame(scopeOneSession, scopeTwoSession);
                Assert.AreSame(scopeTwoSession, scoped.Resolve<ISession>());
            }
        }

        #region Session example CUT

        internal interface ISession {}

        private class TestSession : ISession {}

        internal interface ISessionFactory
        {
            ISession OpenSession();
        }

        internal class TestSessionFactory : ISessionFactory
        {
            public ISession OpenSession()
            {
                return new TestSession();
            }
        }

        internal class SessionClient
        {
            public ISession Session { get; private set; }

            public SessionClient(ISession session)
            {
                Session = session;
            }
        }

        #endregion
    }
}
