using System;
using System.Linq;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ReuseInCurrentScopeTests
    {
        [Test]
        public void Can_reuse_instances_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            using (var scope = container.OpenScope())
            {
                var outerLog = scope.Resolve<Log>();
                using (var scope2 = scope.OpenScope())
                {
                    var scopedLog1 = scope2.Resolve<Log>();
                    var scopedLog2 = scope2.Resolve<Log>();

                    Assert.AreSame(scopedLog1, scopedLog2);
                    Assert.AreNotSame(scopedLog1, outerLog);
                }
            }
        }

        [Test]
        public void Can_reuse_instances_in_three_level_nested_scope()
        {
            var container = new Container();
            using (var scope = container.OpenScope())
            {
                container.Register<Log>(Reuse.InCurrentScope);

                var outerLog = scope.Resolve<Log>();
                using (var scope2 = scope.OpenScope())
                {
                    var scopedLog = scope2.Resolve<Log>();

                    using (var deepScope = scope2.OpenScope())
                    {
                        var deepLog1 = deepScope.Resolve<Log>();
                        var deepLog2 = deepScope.Resolve<Log>();

                        Assert.That(deepLog1, Is.SameAs(deepLog2));
                        Assert.That(deepLog1, Is.Not.SameAs(scopedLog));
                        Assert.That(deepLog1, Is.Not.SameAs(outerLog));
                    }

                    Assert.That(scopedLog, Is.Not.SameAs(outerLog));
                }
            }
        }

        [Test]
        public void Can_reuse_injected_dependencies_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>();
            container.Register<Log>(Reuse.InCurrentScope);

            using (var scoped = container.OpenScope())
            {
                var outerConsumer = scoped.Resolve<Consumer>();

                using (var nestedScoped = scoped.OpenScope())
                {
                    var scopedConsumer1 = nestedScoped.Resolve<Consumer>();
                    var scopedConsumer2 = nestedScoped.Resolve<Consumer>();

                    Assert.That(scopedConsumer1.Log, Is.SameAs(scopedConsumer2.Log));
                    Assert.That(scopedConsumer1.Log, Is.Not.SameAs(outerConsumer.Log));
                }
            }
        }

        [Test]
        public void Can_open_many_parallel_scopes()
        {
            var container = new Container();
            using (var scope = container.OpenScope())
            {
                Assert.DoesNotThrow(() => scope.OpenScope().Dispose());
            }
        }

        [Test]
        public void Cannot_create_service_after_scope_is_disposed()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            Func<Log> getLog;
            using (var containerWithNewScope = container.OpenScope())
                getLog = containerWithNewScope.Resolve<Func<Log>>();

            Assert.AreEqual(
                Error.NameOf(Error.ScopeIsDisposed),
                Error.NameOf(Assert.Throws<ContainerException>(() => getLog()).Error));

            // the same error should be kept for further operations
            Assert.AreEqual(
                Error.NameOf(Error.ScopeIsDisposed),
                Error.NameOf(Assert.Throws<ContainerException>(() => getLog()).Error));
        }

        [Test]
        public void Scope_can_be_safely_disposed_multiple_times_It_does_NOT_throw()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var containerWithNewScope = container.OpenScope();
            containerWithNewScope.Resolve<Log>();
            containerWithNewScope.Dispose();

            Assert.DoesNotThrow(
                containerWithNewScope.Dispose);
        }

        [Test]
        public void Calling_Func_of_scoped_service_outside_of_scope_should_Throw()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<Log>(Reuse.InCurrentScope);

            var getLog = container.Resolve<Func<Log>>();
            using (container.OpenScope())
                Assert.That(getLog(), Is.InstanceOf<Log>());

            var ex = Assert.Throws<ContainerException>(() => getLog());
            Assert.AreEqual(
                Error.NameOf(Error.NoCurrentScope), 
                ex.ErrorName);
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_singleton_resolution_for_parent_container()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton);

            IService serviceInNestedScope;
            using (container.OpenScope())
                serviceInNestedScope = container.Resolve<Func<IService>>()();

            var serviceInOuterScope = container.Resolve<Func<IService>>()();

            Assert.That(serviceInNestedScope, Is.SameAs(serviceInOuterScope));
        }

        [Test]
        public void Can_override_registrations_in_open_scope()
        {
            var container = new Container();
            var scopeName = "blah";

            // two client versions: root and scoped
            container.Register<IClient, Client>();
            container.Register<IClient, ClientScoped>(serviceKey: scopeName);

            // uses
            container.Register<IServ, Serv>(Reuse.Singleton);

            // two dependency versions:
            container.Register<IDep, Dep>();
            container.Register<IDep, DepScoped>(serviceKey: scopeName);

            var client = container.Resolve<IClient>();

            Assert.That(client, Is.InstanceOf<Client>());
            Assert.That(client.Dep, Is.InstanceOf<Dep>());
            Assert.That(client.Serv, Is.InstanceOf<Serv>());

            using (var scoped = container.With(rules => rules
                .WithFactorySelector(Rules.SelectKeyedOverDefaultFactory(scopeName)))
                .OpenScope(scopeName))
            {
                var scopedClient = scoped.Resolve<IClient>(scopeName);

                Assert.That(scopedClient, Is.InstanceOf<ClientScoped>());
                Assert.That(scopedClient.Dep, Is.InstanceOf<DepScoped>());
                Assert.That(scopedClient.Serv, Is.InstanceOf<Serv>());
            }

            client = container.Resolve<IClient>();
            Assert.That(client, Is.InstanceOf<Client>());
        }

        [Test]
        public void Can_ResolveMany_and_filter_out_scoped_services()
        {
            var container = new Container();
            container.Register<IDep, Dep>();
            container.Register<IDep, DepScoped>(Reuse.InCurrentScope);

            var deps = container.Resolve<LazyEnumerable<IDep>>().ToArray();
            Assert.AreEqual(1, deps.Length);
            Assert.IsInstanceOf<Dep>(deps[0]);

            using (var scope = container.OpenScope())
            {
                var scopedDeps = scope.Resolve<LazyEnumerable<IDep>>().ToArray();
                Assert.AreEqual(2, scopedDeps.Length);
            }
        }

        [Test]
        public void Can_ResolveMany_without_filtering_out_scoped_services_with_ScopedOrSingleton_reuse()
        {
            var container = new Container();
            container.Register<IDep, Dep>();
            container.Register<IDep, DepScoped>(Reuse.ScopedOrSingleton);

            var deps = container.Resolve<LazyEnumerable<IDep>>().ToArray();
            Assert.AreEqual(2, deps.Length);
            Assert.IsInstanceOf<Dep>(deps[0]);

            using (var scope = container.OpenScope())
            {
                var scopedDeps = scope.Resolve<LazyEnumerable<IDep>>().ToArray();
                Assert.AreEqual(2, scopedDeps.Length);
            }
        }

        [Test]
        public void Can_switch_off_filtering_out_not_scoped_services()
        {
            var container = new Container(r => r.WithoutImplicitCheckForReuseMatchingScope());
            container.Register<IDep, DepScoped>(Reuse.InCurrentScope);

            var ex = Assert.Throws<ContainerException>(
                () => container.Resolve<IDep>());
            Assert.AreEqual(Error.NoCurrentScope, ex.Error);
        }

        [Test]
        public void Services_should_be_different_in_different_scopes()
        {
            var container = new Container();
            container.Register<IndependentService>(Reuse.InCurrentScope);

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
            var container = new Container(scopeContext: new ThreadScopeContext());

            container.Register<IService, IndependentService>(Reuse.InCurrentScope);
            container.Register<ServiceWithFuncConstructorDependency>(Reuse.Singleton);

            var service = container.Resolve<ServiceWithFuncConstructorDependency>();

            var scope = container.OpenScope();
            var first = service.GetScopedService();
            scope.Dispose();

            scope = container.OpenScope();
            var second = service.GetScopedService();
            scope.Dispose();

            Assert.That(second, Is.Not.SameAs(first));
            container.Dispose();
        }

        [Test]
        public void Open_context_independent_scope()
        {
            var container = new Container();
            container.Register<Blah>(Reuse.InCurrentScope);

            using (var scope = container.OpenScope())
            {
                var blah = scope.Resolve<Blah>();
                Assert.AreSame(blah, scope.Resolve<Blah>());

                using (var scope2 = ((Container)scope).OpenScope())
                    Assert.AreNotSame(blah, scope2.Resolve<Blah>());
            }
        }

        [Test]
        public void Open_context_independent_named_scope()
        {
            var container = new Container();
            container.Register<Blah>(Reuse.InCurrentNamedScope("hey"));

            using (var scope = container.OpenScope("hey"))
            {
                var blah = scope.Resolve<Blah>();
                Assert.AreSame(blah, scope.Resolve<Blah>());

                using (var scope2 = ((Container)scope).OpenScope())
                    Assert.AreSame(blah, scope2.Resolve<Blah>());
            }

            container.Dispose();
        }

        [Test]
        public void Open_multiple_context_independent_scopes()
        {
            var container = new Container();
            container.Register<Blah>(Reuse.InCurrentScope);

            var scope1 = container.OpenScope();
            var scope2 = container.OpenScope();

            var blah1 = scope1.Resolve<Blah>();
            var blah2 = scope2.Resolve<Blah>();

            Assert.AreSame(blah1, scope1.Resolve<Blah>());
            Assert.AreNotSame(blah1, blah2);

            scope1.Dispose();
            scope2.Dispose(); 
        }

        [Test]
        public void Can_resolve_service_as_scoped_or_singleton_depending_on_scope_availability()
        {
            var container = new Container();

            container.Register<Blah>(Reuse.ScopedOrSingleton);

            var singleton = container.Resolve<Blah>();

            using (var scope = container.OpenScope())
            {
                var scoped = scope.Resolve<Blah>();
                Assert.AreNotSame(singleton, scoped);
                Assert.AreSame(scoped, scope.Resolve<Blah>());

                using (var nestedScope = scope.OpenScope())
                    Assert.AreNotSame(scoped, nestedScope.Resolve<Blah>());

                Assert.AreSame(scoped, scope.Resolve<Blah>());
            }

            Assert.AreSame(singleton, container.Resolve<Blah>());

            container.Dispose();
            Assert.IsTrue(singleton.IsDisposed);
        }

        [Test]
        public void Can_inject_service_as_scoped_or_singleton_depending_on_scope_availability()
        {
            var container = new Container();

            container.Register<Blah>(Reuse.ScopedOrSingleton);

            container.Register<SingletonBlahUser>(Reuse.Singleton);
            container.Register<ScopedBlahUser>(Reuse.Scoped);

            var singleton = container.Resolve<SingletonBlahUser>();

            using (var scope = container.OpenScope())
            {
                var scoped = scope.Resolve<ScopedBlahUser>();

                Assert.AreNotSame(singleton.Blah, scoped.Blah);
                Assert.AreSame(scoped.Blah, scope.Resolve<ScopedBlahUser>().Blah);

                using (var nestedScope = scope.OpenScope())
                    Assert.AreNotSame(scoped.Blah, nestedScope.Resolve<ScopedBlahUser>().Blah);

                Assert.AreSame(scoped.Blah, scope.Resolve<ScopedBlahUser>().Blah);
            }

            Assert.AreSame(singleton.Blah, container.Resolve<SingletonBlahUser>().Blah);

            container.Dispose();
            Assert.IsTrue(singleton.Blah.IsDisposed);
        }

        public class SingletonBlahUser
        {
            public Blah Blah { get; }

            public SingletonBlahUser(Blah blah)
            {
                Blah = blah;
            }
        }

        public class ScopedBlahUser
        {
            public Blah Blah { get; }

            public ScopedBlahUser(Blah blah)
            {
                Blah = blah;
            }
        }

        public interface IAction { }
        public class ActionOne : IAction { }
        public class ActionTwo : IAction { }

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

        public class Blah : IDisposable
        {
            public void Dispose()
            {
                IsDisposed = true;
            }

            public bool IsDisposed { get; private set; }
        }
    }
}