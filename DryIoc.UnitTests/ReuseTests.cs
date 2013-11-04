using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ReuseTests
    {
        [Test]
        public void When_container_disposed_Then_factory_call_should_throw()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(DisposableService), Reuse.Singleton);

            var factory = container.Resolve<Func<IService>>();
            container.Dispose();

            Assert.Throws<ContainerException>(() => factory());
        }

        [Test]
        public void When_container_disposed_Then_factory_call_should_throw2()
        {
            var container = new Container();
            container.Register(typeof(IService), typeof(DisposableService), Reuse.Singleton);

            var service = (DisposableService)container.Resolve<IService>();
            container.Dispose();

            Assert.That(service.IsDisposed, Is.True);
        }

        [Test]
        public void When_registring_external_instance_and_disposing_container_Then_instance_should_not_be_disposed()
        {
            var container = new Container();
            var service = new DisposableService();
            container.RegisterInstance<IService>(service);

            container.Dispose();

            Assert.That(service.IsDisposed, Is.False);
        }

        [Test]
        public void Scopes_parameter_with_the_same_name_as_Container_scope_wont_collide()
        {
            var container = new Container();
            container.Register<Soose>(Reuse.Singleton);

            var factory = container.Resolve<Func<int, Soose>>();

            Assert.That(factory(1).Scopes, Is.EqualTo(1));
        }

        [Test]
        public void In_same_resolution_scope_Log_instances_should_be_same()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>();
            container.Register<Log>(Reuse.DuringResolution);

            var consumer = container.Resolve<Consumer>();
            //var consumerExpr = container.Resolve<Container.DebugExpression<Consumer>>();

            Assert.That(consumer.Log, Is.Not.Null.And.SameAs(consumer.Account.Log));
        }

        [Test]
        public void In_different_resolution_scopes_Log_instances_should_be_different()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>();
            container.Register<Log>(Reuse.DuringResolution);

            var consumer = container.Resolve<Consumer>();
            var account = container.Resolve<Account>();

            Assert.That(consumer.Log, Is.Not.Null.And.Not.SameAs(account.Log));
        }

        [Test]
        public void Can_reuse_instances_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var log = container.Resolve<Log>();
            using (var containerWithNewScope = container.OpenScope())
            {
                var logScoped1 = containerWithNewScope.Resolve<Log>();
                var logScoped2 = containerWithNewScope.Resolve<Log>();

                Assert.That(logScoped1, Is.SameAs(logScoped2).And.Not.SameAs(log));
            }
        }

        [Test]
        public void Can_reuse_dependencies_in_new_open_scope()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>(Reuse.Singleton);
            container.Register<Log>(Reuse.InCurrentScope);

            var consumer = container.Resolve<Consumer>();
            using (var containerWithNewScope = container.OpenScope())
            {
                var consumerScoped1 = containerWithNewScope.Resolve<Consumer>();
                var consumerScoped2 = containerWithNewScope.Resolve<Consumer>();

                Assert.That(consumerScoped1.Log, Is.SameAs(consumerScoped2.Log).And.Not.SameAs(consumer.Log));
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

            Assert.Throws<ContainerException>(() => getLog());
            Assert.Throws<ContainerException>(() => getLog());
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
        public void Nested_scope_disposition_should_not_affect_outer_scope_factories()
        {
            var container = new Container();
            container.Register<Log>(Reuse.InCurrentScope);

            var getLog = container.Resolve<Func<Log>>();
            using (container.OpenScope()) { }

            Assert.DoesNotThrow(() => getLog());
        }

        [Test]
        public void Nested_scope_disposition_should_not_affect_singleton_resolution_for_parent_container()
        {
            var container = new Container();
            container.Register<IService, Service>(Reuse.Singleton);

            IService serviceInNestedScope;
            using (container.OpenScope())
                serviceInNestedScope = container.Resolve<IService>();

            var serviceInOuterScope = container.Resolve<IService>();

            Assert.That(serviceInNestedScope, Is.SameAs(serviceInOuterScope));
        }

        [Test]
        public void For_signleton_injected_as_Func_and_as_instance_only_one_instance_should_be_created()
        {
            var container = new Container();
            ServiceWithInstanceCountWithStringParam.InstanceCount = 0;
            container.Register<ClientWithFuncAndInstanceDependency>();
            container.Register<IService, ServiceWithInstanceCountWithStringParam>(Reuse.Singleton);
            container.RegisterInstance("I am a string");

            container.Resolve<ClientWithFuncAndInstanceDependency>();

            Assert.That(ServiceWithInstanceCountWithStringParam.InstanceCount, Is.EqualTo(1));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_same_thread_should_be_the_same()
        {
            var container = new Container();
            container.Register<Service>(CustomReuse.InThread);

            var one = container.Resolve<Service>();
            var another = container.Resolve<Service>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_same_thread_should_be_the_same()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(CustomReuse.InThread);

            var one = container.Resolve<ServiceWithDependency>();
            var another = container.Resolve<ServiceWithDependency>();
            Assert.That(one.Dependency, Is.SameAs(another.Dependency));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_different_thread_should_be_the_different()
        {
            var container = new Container();
            container.Register<Service>(CustomReuse.InThread);

            Service one = null;
            var threadOne = new Thread(() => one = container.Resolve<Service>()) { IsBackground = true };
            threadOne.Start();

            var another = container.Resolve<Service>();

            threadOne.Join();
            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_different_thread_should_be_different()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(CustomReuse.InThread);

            ServiceWithDependency one = null;
            var threadOne = new Thread(() => one = container.Resolve<ServiceWithDependency>());
            threadOne.Start();

            var another = container.Resolve<ServiceWithDependency>();

            threadOne.Join();
            Assert.That(one.Dependency, Is.Not.SameAs(another.Dependency));
        }
    }

    public static class CustomReuse
    {
        public static ThreadReuse InThread = new ThreadReuse();
    }

    public class ThreadReuse : IReuse
    {
        public Expression Of(Request _, IRegistry __, int factoryID, Expression factoryExpr)
        {
            return Reuse.GetScopedServiceExpression(_scopeExpr, factoryID, factoryExpr);
        }

        [ThreadStatic]
        private static Scope _scope;
        private static readonly Expression _scopeExpr;

        // ReSharper disable UnusedMember.Local
        private static Scope GetScope()
        {
            return _scope ?? (_scope = new Scope());
        }
        // ReSharper restore UnusedMember.Local

        static ThreadReuse()
        {
            _scopeExpr = Expression.Call(typeof(ThreadReuse).GetMethod("GetScope", BindingFlags.Static | BindingFlags.NonPublic));
        }
    }

    #region CUT

    public class Soose
    {
        public int Scopes;

        public Soose(int scopes)
        {
            Scopes = scopes;
        }
    }

    public class DisposableService : IService, IDisposable
    {
        public bool IsDisposed;

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class Consumer
    {
        public Account Account { get; set; }
        public Log Log { get; set; }

        public Consumer(Account account, Log log)
        {
            Account = account;
            Log = log;
        }
    }

    public class Account
    {
        public Log Log { get; set; }

        public Account(Log log)
        {
            Log = log;
        }
    }

    public class Log
    {
    }

    #endregion
}