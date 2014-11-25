using System;
using NUnit.Framework;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace DryIoc.Samples
{
    [TestFixture]
    class OpenScopeTests
    {
        [Test]
        public void Session_example_of_scope_usage()
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

        #endregion

        [Test, Ignore]
        public void Can_override_registrations_in_open_scope()
        {
            var container = new Container();
            container.Register<IClient, Client>();
            container.Register<IDep, Dep>();
            container.Register<IServ, Serv>(Shared.InContainer);

            var client = container.Resolve<IClient>();
            Assert.That(client, Is.InstanceOf<Client>());
            Assert.That(client.Dep, Is.InstanceOf<Dep>());
            Assert.That(client.Serv, Is.InstanceOf<Serv>());

            using (var scoped = container.OpenScope())
            {
                scoped.Register<IClient, ClientScoped>(named: "scoped");
                scoped.Register<IDep, DepScoped>(named: "scoped");

                var scopedClient = scoped.Resolve<IClient>("scoped");
                Assert.That(scopedClient, Is.InstanceOf<ClientScoped>());
                Assert.That(scopedClient.Dep, Is.InstanceOf<DepScoped>());
                Assert.That(scopedClient.Serv, Is.InstanceOf<Serv>());
            }

            client = container.Resolve<IClient>();
            Assert.That(client, Is.InstanceOf<Client>());
        }

        [Test, Ignore]
        public void Can_override_registrations_in_open_scope_OLDONE()
        {
            var container = new Container();
            container.Register<IClient, Client>();
            container.Register<IDep, Dep>();
            container.Register<IServ, Serv>(Shared.InContainer);

            var client = container.Resolve<IClient>();
            Assert.That(client, Is.InstanceOf<Client>());
            Assert.That(client.Dep, Is.InstanceOf<Dep>());
            Assert.That(client.Serv, Is.InstanceOf<Serv>());

            using (var scoped = container.OpenScope())
            {
                scoped.Register<IClient, ClientScoped>();
                scoped.Register<IDep, DepScoped>();

                var scopedClient = scoped.Resolve<IClient>();
                Assert.That(scopedClient, Is.InstanceOf<ClientScoped>());
                Assert.That(scopedClient.Dep, Is.InstanceOf<DepScoped>());
                Assert.That(scopedClient.Serv, Is.InstanceOf<Serv>());
            }

            client = container.Resolve<IClient>();
            Assert.That(client, Is.InstanceOf<Client>());
        }

        [Test]
        public void Services_should_be_different_in_different_scopes()
        {
            var container = new Container();
            container.Register<IndependentService>(Shared.InCurrentScope);

            var scope = container.OpenScope();
            var first = scope.Resolve<IndependentService>();
            scope.Dispose();

            scope = container.OpenScope();
            var second = scope.Resolve<IndependentService>();
            scope.Dispose();

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void Factory_should_return_different_service_when_called_in_different_scopes()
        {
            var container = new Container();

            container.Register<IService, IndependentService>(Reuse.InCurrentScope);
            container.Register<ServiceWithFuncConstructorDependency>(Reuse.Singleton);

            var service = container.Resolve<ServiceWithFuncConstructorDependency>();

            var scope = container.BeginScope();
            var first = service.GetScopedService();
            scope.Dispose();

            scope = container.BeginScope();
            var second = service.GetScopedService();
            scope.Dispose();

            Assert.That(second, Is.Not.SameAs(first));
        }

        internal class IndependentService : IService { }

        internal class ServiceWithFuncConstructorDependency
        {
            public Func<IService> GetScopedService { get; private set; }

            public ServiceWithFuncConstructorDependency(Func<IService> getScopedService)
            {
                GetScopedService = getScopedService;
            }
        }

        internal interface IDep { }
        internal interface IServ { }
        internal interface IClient
        {
            IDep Dep { get; }
            IServ Serv { get; }
        }

        internal class Client : IClient
        {
            public IDep Dep { get; private set; }
            public IServ Serv { get; private set; }

            public Client(IDep dep, IServ serv)
            {
                Dep = dep;
                Serv = serv;
            }
        }

        internal class ClientScoped : IClient
        {
            public IDep Dep { get; private set; }
            public IServ Serv { get; private set; }

            public ClientScoped(IDep dep, IServ serv)
            {
                Dep = dep;
                Serv = serv;
            }
        }

        internal class Dep : IDep { }
        internal class DepScoped : IDep { }
        internal class Serv : IServ { }
    }
}
