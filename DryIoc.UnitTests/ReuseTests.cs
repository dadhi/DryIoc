using System;
using System.Threading;
using System.Web;
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
            container.Register<Log>(Reuse.InResolutionScope);

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
            container.Register<Log>(Reuse.InResolutionScope);

            var consumer = container.Resolve<Consumer>();
            var account = container.Resolve<Account>();

            Assert.That(consumer.Log, Is.Not.Null.And.Not.SameAs(account.Log));
        }

        [Test][Ignore]
        public void For_signleton_injected_as_Func_and_as_instance_only_one_instance_should_be_created()
        {
            ServiceWithInstanceCountWithStringParam.InstanceCount = 0;
            try
            {
                var container = new Container();
                container.Register<ClientWithFuncAndInstanceDependency>();
                container.Register<IService, ServiceWithInstanceCountWithStringParam>(Reuse.Singleton);
                container.RegisterInstance("I am a string");

                container.Resolve<ClientWithFuncAndInstanceDependency>();

                Assert.That(ServiceWithInstanceCountWithStringParam.InstanceCount, Is.EqualTo(1));
            }
            finally
            {
                ServiceWithInstanceCountWithStringParam.InstanceCount = 0;
            }
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_same_thread_should_be_the_same()
        {
            var container = new Container();
            container.Register<Service>(CustomReuse.InThreadScope);

            var one = container.Resolve<Service>();
            var another = container.Resolve<Service>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_same_thread_should_be_the_same()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(CustomReuse.InThreadScope);

            var one = container.Resolve<ServiceWithDependency>();
            var another = container.Resolve<ServiceWithDependency>();
            Assert.That(one.Dependency, Is.SameAs(another.Dependency));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_different_thread_should_be_the_different()
        {
            var container = new Container();
            container.Register<Service>(CustomReuse.InThreadScope);

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
            container.Register<IDependency, Dependency>(CustomReuse.InThreadScope);

            ServiceWithDependency one = null;
            var threadOne = new Thread(() => one = container.Resolve<ServiceWithDependency>());
            threadOne.Start();

            var another = container.Resolve<ServiceWithDependency>();

            threadOne.Join();
            Assert.That(one.Dependency, Is.Not.SameAs(another.Dependency));
        }

        [Test]
        public void Can_use_both_resolution_scope_and_singleton_reuse_in_same_resolution_root()
        {
            var container = new Container();
            container.Register<ServiceWithResolutionAndSingletonDependencies>();
            container.Register<SingletonDep>(Reuse.Singleton);
            container.Register<ResolutionScopeDep>(Reuse.InResolutionScope);

            var service = container.Resolve<ServiceWithResolutionAndSingletonDependencies>();

            Assert.That(service.ResolutionScopeDep, Is.SameAs(service.SingletonDep.ResolutionScopeDep));
        }
    }

    public class ThreadReuse : IReuse
    {
        public bool IsEager { get { return false; } }

        public object Of(int factoryID, FactoryDelegate factoryDelegate, ref ResolutionState state)
        {
            var s = state;
            return GetScope().GetOrAdd(factoryID, () => factoryDelegate(s));
        }

        private static Scope GetScope()
        {
            return _scope ?? (_scope = new Scope());
        }

        [ThreadStatic]
        private static Scope _scope;
    }

    public static class CustomReuse
    {
        public static ThreadReuse InThreadScope = new ThreadReuse();
        public static HttpContextReuse InHttpContext = new HttpContextReuse();
    }

    public class HttpContextReuse : IReuse
    {
        public bool IsEager { get { return false; } }

        public object Of(int factoryID, FactoryDelegate factoryDelegate, ref ResolutionState state)
        {
            var s = state;
            return GetOrAddToContext(factoryID, () => factoryDelegate(s));
        }

        public static T GetOrAddToContext<T>(int factoryID, Func<T> factory)
        {
            var key = "IoC." + factoryID;
            if (HttpContext.Current.Items[key] == null)
                HttpContext.Current.Items[key] = factory();
            return (T)HttpContext.Current.Items[key];
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

    class ServiceWithResolutionAndSingletonDependencies
    {
        public SingletonDep SingletonDep { get; set; }
        public ResolutionScopeDep ResolutionScopeDep { get; set; }

        public ServiceWithResolutionAndSingletonDependencies(SingletonDep singletonDep, ResolutionScopeDep resolutionScopeDep)
        {
            SingletonDep = singletonDep;
            ResolutionScopeDep = resolutionScopeDep;
        }
    }

    internal class ResolutionScopeDep {}

    internal class SingletonDep
    {
        public ResolutionScopeDep ResolutionScopeDep { get; set; }

        public SingletonDep(ResolutionScopeDep resolutionScopeDep)
        {
            ResolutionScopeDep = resolutionScopeDep;
        }
    }

    #endregion
}