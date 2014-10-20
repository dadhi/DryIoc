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

        [Test, Explicit]
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
            container.Register<Service>(ReuseIt.InThread);

            var one = container.Resolve<Service>();
            var another = container.Resolve<Service>();

            Assert.That(one, Is.SameAs(another));
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_same_thread_should_be_the_same()
        {
            var container = new Container();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(ReuseIt.InThread);

            var one = container.Resolve<ServiceWithDependency>();
            var another = container.Resolve<ServiceWithDependency>();
            Assert.That(one.Dependency, Is.SameAs(another.Dependency));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_different_thread_should_be_the_different()
        {
            var container = new Container();
            container.Register<Service>(ReuseIt.InThread);

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
            container.Register<IDependency, Dependency>(ReuseIt.InThread);

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
        public IScope GetScope(Request request)
        {
            return _scope;
        }

        private readonly ThreadScope _scope = new ThreadScope();

        class ThreadScope : IScope
        {
            public object GetOrAdd(int id, Func<object> factory)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;

                var threadScope = _threadScopes.Value.GetValueOrDefault(threadId);
                if (threadScope == null)
                    _threadScopes.Swap(s => s.AddOrUpdate(threadId, threadScope = new Scope(), 
                        (oldValue, newValue) => threadScope = oldValue)); // if Scope is already added in between then use it.
                return threadScope.GetOrAdd(id, factory);
            }

            private readonly Ref<HashTree<int, Scope>> _threadScopes = Ref.Of(HashTree<int, Scope>.Empty);
        }
    }

    public static partial class ReuseIt
    {
        public static ThreadReuse InThread = new ThreadReuse();
        public static HttpContextReuse InRequest = new HttpContextReuse();
    }

    public sealed class HttpContextReuse : IReuse
    {
        public static readonly HttpContextReuse Instance = new HttpContextReuse();

        public IScope GetScope(Request request)
        {
            if (HttpContext.Current == null)
                return _contextNullScope ?? CreateNewScope();

            var items = HttpContext.Current.Items;
            lock (_singleScopeLocker)
                if (!items.Contains(_reuseScopeKey))
                    items[_reuseScopeKey] = _contextNullScope ?? new Scope();

            return (Scope)items[_reuseScopeKey];
        }

        private IScope CreateNewScope()
        {
            lock (_singleScopeLocker)
                return _contextNullScope = _contextNullScope ?? new Scope();
        }

        private IScope _contextNullScope;
        private readonly object _singleScopeLocker = new object();
        private static readonly string _reuseScopeKey = typeof(HttpContextReuse).Name;
    }

    // Old example v1.3.1 
    //public sealed class HttpContextReuse : IReuse
    //{
    //    public static readonly HttpContextReuse Instance = new HttpContextReuse();

    //    public static T GetOrAddToContext<T>(int factoryID, Func<T> factory)
    //    {
    //        var key = KEY_UNIQUE_PREFIX + factoryID;
    //        var items = HttpContext.Current.Items;
    //        lock (_locker)
    //            if (!items.Contains(key))
    //                items[key] = factory();
    //        return (T)items[key];
    //    }

    //    public Expression Of(Request request, IRegistry registry, int factoryID, Expression factoryExpr)
    //    {
    //        return Expression.Call(_getOrAddToContextMethod.MakeGenericMethod(factoryExpr.Type),
    //            Expression.Constant(factoryID),        // use factoryID (unique per Container) as service ID.
    //            Expression.Lambda(factoryExpr, null)); // pass Func<TService> to create service only when not found in context.
    //    }

    //    private static readonly object _locker = new object();
    //    private readonly MethodInfo _getOrAddToContextMethod = typeof(HttpContextReuse).GetMethod("GetOrAddToContext");
    //    private const string KEY_UNIQUE_PREFIX = "DryIocHCR#";
    //}

    #region CUT

    public class Soose
    {
        public int Scopes;

        public Soose(int scopes)
        {
            Scopes = scopes;
        }
    }

    public class ServiceWithResolutionAndSingletonDependencies
    {
        public SingletonDep SingletonDep { get; set; }
        public ResolutionScopeDep ResolutionScopeDep { get; set; }

        public ServiceWithResolutionAndSingletonDependencies(SingletonDep singletonDep, ResolutionScopeDep resolutionScopeDep)
        {
            SingletonDep = singletonDep;
            ResolutionScopeDep = resolutionScopeDep;
        }
    }

    public class ResolutionScopeDep { }

    public class SingletonDep
    {
        public ResolutionScopeDep ResolutionScopeDep { get; set; }

        public SingletonDep(ResolutionScopeDep resolutionScopeDep)
        {
            ResolutionScopeDep = resolutionScopeDep;
        }
    }

    #endregion
}