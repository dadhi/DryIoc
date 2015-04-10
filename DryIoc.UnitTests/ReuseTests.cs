using System;
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

            var ex = Assert.Throws<ContainerException>(() => factory());
            Assert.That(ex.Message, Is.StringContaining("is disposed and its operations are no longer available"));
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

        [Test]
        public void If_not_fully_matched_resolution_scope_found_Then_the_top_scope_will_be_used()
        {
            var container = new Container();

            container.Register<AccountUser>();
            container.Register<Account>(setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.InResolutionScopeOf<Account>("account"));

            var ex = Assert.Throws<ContainerException>(() => 
                container.Resolve<AccountUser>());

            Assert.AreEqual(ex.Error, Error.UNABLE_TO_RESOLVE_SERVICE);
        }

        [Test]
        public void Resolve_succeed_only_if_fully_matched_resolution_scope_found()
        {
            var container = new Container();

            container.Register<AccountUser>(made: Parameters.Of.Type<Account>(serviceKey: "account"));
            container.Register<Account>(serviceKey: "account", setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.InResolutionScopeOf<Account>("account"));

            var user = container.Resolve<AccountUser>();

            Assert.NotNull(user.Account.Log);
        }

        [Test]
        public void Resolve_service_reused_in_resolution_scope_succeed_if_key_matched()
        {
            var container = new Container();

            container.Register<AccountUser>(made: Parameters.Of.Type<Account>(serviceKey: "account"));
            container.Register<Account>(serviceKey: "account", setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.InResolutionScopeOf(serviceKey: "account"));

            var user = container.Resolve<AccountUser>();

            Assert.NotNull(user.Account.Log);
        }

        [Test]
        public void Can_control_disposing_of_matching_resolution_scope_with_wrapper()
        {
            var container = new Container();

            container.Register<AccountUser>();
            container.Register<Account, CarefulAccount>(setup: Setup.With(openResolutionScope: true));
            container.Register<Log, DisposableLog>(Reuse.InResolutionScopeOf<Account>());

            var user = container.Resolve<AccountUser>();

            Assert.NotNull(user.Account.Log);
            ((IDisposable)user.Account).Dispose();

            Assert.IsTrue(((DisposableLog)user.Account.Log).IsDisposed);
        }

        [Test]
        public void Can_use_resolution_scope_reuse_bound_to_resolution_root()
        {
            var container = new Container();
            container.Register<ViewModel2>();
            container.Register<Log>(Reuse.InResolutionScopeOf<ViewModel2>());

            var vm = container.Resolve<ViewModel2>();

            Assert.IsNotNull(vm.Log);
        }

        [Test]
        public void Can_specify_to_reuse_in_outermost_resolution_scope()
        {
            var container = new Container();
            container.Register<ViewModel1Presenter>();
            container.Register<ViewModel1>(setup: Setup.With(openResolutionScope: true));
            container.Register<ViewModel2>(setup: Setup.With(openResolutionScope: true));

            container.Register<Log>(Reuse.Transient,
                setup: Setup.With(condition: request => request.IsResolutionRoot));

            var logReuse = Reuse.InResolutionScopeOf<IViewModel>(outermost: true);
            container.Register<Log>(logReuse,
                setup: Setup.With(condition: request => logReuse.GetScopeOrDefault(request) != null));

            var presenter = container.Resolve<ViewModel1Presenter>();

            Assert.AreSame(presenter.VM1.Log, presenter.VM1.VM2.Log);
        }

        [Test]
        public void Can_correctly_handle_resolution_scope_condition()
        {
            var container = new Container();
            container.Register<ViewModel2>(made: Made.Of(() => new ViewModel2(Arg.Of<Log>(IfUnresolved.ReturnDefault))));

            var logReuse = Reuse.InResolutionScopeOf<ViewModel1>();
            container.Register<Log>(logReuse, setup: Setup.With(condition: request => logReuse.GetScopeOrDefault(request) != null));

            var vm = container.Resolve<ViewModel2>();

            Assert.IsNull(vm.Log);
        }

        [Test]
        public void Can_specify_to_resolve_corresponding_log_in_resolution_scope_automagically_Without_condition()
        {
            var container = new Container();
            container.Register<ViewModel1Presenter>();
            container.Register<ViewModel1>(setup: Setup.With(openResolutionScope: true));
            container.Register<ViewModel2>(setup: Setup.With(openResolutionScope: true));

            container.Register<Log>(Reuse.InResolutionScopeOf<IViewModel>(outermost: true));

            container.Register<Log>(setup: Setup.With(condition: request => request.IsResolutionRoot));

            var presenter = container.Resolve<ViewModel1Presenter>();

            Assert.AreSame(presenter.VM1.Log, presenter.VM1.VM2.Log);
        }

        internal class ViewModel1Presenter
        {
            public ViewModel1 VM1 { get; set; }
            public Log Log { get; set; }

            public ViewModel1Presenter(ViewModel1 vm1, Log log)
            {
                VM1 = vm1;
                Log = log;
            }
        }

        internal interface IViewModel {}

        internal class ViewModel1 : IViewModel
        {
            public ViewModel2 VM2 { get; set; }
            public Log Log { get; set; }
            public ViewModel1(ViewModel2 vm2, Log log)
            {
                VM2 = vm2;
                Log = log;
            }
        }

        internal class ViewModel2 : IViewModel
        {
            public Log Log { get; set; }
            public ViewModel2(Log log)
            {
                Log = log;
            }
        }

        internal class AccountUser
        {
            public Account Account { get; private set; }
            public AccountUser(Account account)
            {
                Account = account;
            }
        }

        internal class DisposableLog : Log, IDisposable 
        {
            public void Dispose()
            {
                IsDisposed = true;
            }

            public bool IsDisposed { get; private set; }
        }

        internal class CarefulAccount : Account, IDisposable
        {
            private readonly CaptureResolutionScope<Log> _disposableLogAndEverything;

            public CarefulAccount(CaptureResolutionScope<Log> log) : base(log.Value)
            {
                _disposableLogAndEverything = log;
            }

            public void Dispose()
            {
                _disposableLogAndEverything.Dispose();
            }
        }

        [Test]
        public void Given_Thread_reuse_and_no_open_scope_Exception_should_be_thrown()
        {
            var container = new Container();
            container.Register<Service>(Reuse.InThreadScope);

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<Service>());

            Assert.That(ex.Message, Is.StringContaining("Unable to resolve DryIoc.UnitTests.CUT.Service"));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_same_thread_should_be_the_same()
        {
            using (var container = new Container(scopeContext: new ThreadScopeContext()).OpenScope())
            {
                container.Register<Service>(Reuse.InThreadScope);

                var one = container.Resolve<Service>();
                var another = container.Resolve<Service>();

                Assert.That(one, Is.SameAs(another));
            }
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_same_thread_should_be_the_same_In_nested_scope_too()
        {
            using (var container = new Container(scopeContext: new ThreadScopeContext()).OpenScope())
            {
                container.Register<Service>(Reuse.InThreadScope);

                var one = container.Resolve<Service>();

                using (var nested = container.OpenScope())
                {
                    var two = nested.Resolve<Service>();
                    Assert.That(one, Is.SameAs(two));
                }

                var another = container.Resolve<Service>();
                Assert.That(one, Is.SameAs(another));
            }
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_same_thread_should_be_the_same()
        {
            var container = new Container(scopeContext: new ThreadScopeContext()).OpenScope();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(Reuse.InThreadScope);

            var one = container.Resolve<ServiceWithDependency>();
            var another = container.Resolve<ServiceWithDependency>();
            Assert.That(one.Dependency, Is.SameAs(another.Dependency));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_different_thread_should_be_the_different()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            var mainThread = container.OpenScope();
            mainThread.Register<Service>(Reuse.InThreadScope);

            Service one = null;
            var threadOne = new Thread(() =>
            {
                using (var threadLocal = container.OpenScope())
                    one = threadLocal.Resolve<Service>();
            }) { IsBackground = true };

            threadOne.Start();

            var another = mainThread.Resolve<Service>();

            threadOne.Join();
            Assert.That(one, Is.Not.SameAs(another));
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_different_thread_should_be_different()
        {
            var parent = new Container(scopeContext: new ThreadScopeContext());
            var container = parent.OpenScope();
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(Reuse.InThreadScope);

            ServiceWithDependency one = null;
            var threadOne = new Thread(() =>
            {
                using (var otherThread = parent.OpenScope())
                    one = otherThread.Resolve<ServiceWithDependency>();
            });
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
            container.Register<SingletonDep>(Reuse.InResolutionScope);
            container.Register<ResolutionScopeDep>(Reuse.Singleton);

            var service = container.Resolve<ServiceWithResolutionAndSingletonDependencies>();

            Assert.That(service.ResolutionScopeDep, Is.SameAs(service.SingletonDep.ResolutionScopeDep));
        }

        [Test]
        public void Should_throw_if_rule_specified_and_dependency_lifespan_is_less_than_parents()
        {
            var container = new Container();
            container.Register<Client>(Reuse.Singleton);
            container.Register<ILogger, FastLogger>(Reuse.InResolutionScope);

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Client>());

            Assert.That(ex.Error, Is.EqualTo(Error.DEPENDENCY_HAS_SHORTER_REUSE_LIFESPAN));
        }

        [Test]
        public void Should_Not_throw_if_rule_is_off_and_dependency_lifespan_is_less_than_parents()
        {
            var container = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());
            container.Register<Client>(Reuse.Singleton);
            container.Register<ILogger, FastLogger>(Reuse.InResolutionScope);

            var client = container.Resolve<Client>();

            Assert.That(client.Logger, Is.InstanceOf<FastLogger>());
        }

        [Test]
        public void Can_replace_singleton_reuse_with_transient_in_container()
        {
            var container = new Container(rules => rules
                .WithReuseMapping((reuse, _) => reuse is SingletonReuse ? Reuse.Transient : reuse));

            container.Register<Service>(Reuse.Singleton);

            var one = container.Resolve<Service>();
            var two = container.Resolve<Service>();

            Assert.That(one, Is.Not.SameAs(two));
        }

        [Test]
        public void Can_disposed_resolution_reused_services()
        {
            var container = new Container();
            container.Register<SomeDep>(Reuse.InResolutionScope);
            container.Register<SomeRoot>();

            var service = container.Resolve<CaptureResolutionScope<SomeRoot>>();
            using (service)
                Assert.That(service.Scope, Is.Not.Null);

            Assert.That(service.Value.Dep.IsDisposed, Is.True);
        }

        [Test]
        public void Possible_to_reuse_item_multiple_times_down_in_object_subgraph_Only_as_lazy_dependency()
        {
            var container = new Container();
            container.Register<X>(Reuse.Singleton);
            container.Register<Y>();

            Assert.DoesNotThrow(() => container.Resolve<X>());
        }

        internal class X
        {
            public Y Y { get; private set; }
            public X(Y y)
            {
                Y = y;
            }
        }
        internal class Y
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public Lazy<X> X { get; private set; }
            public Y(Lazy<X> x)
            {
                X = x;
            }
        }

        [Test]
        public void Should_dispose_all_singletons_despite_exception_in_first_dispose()
        {
            IContainer container = new Container();
            container.Register<A>(Reuse.Singleton);
            container.Register<B>(Reuse.Singleton);

            container.Resolve<A>();
            var b = container.Resolve<B>();

            container.Dispose();
            Assert.IsTrue(b.IsDisposed);
        }

        [Test]
        public void Disposing_container_should_dispose_the_registered_instance_even_not_resolved()
        {
            var container = new Container();
            var service = new SomethingDisposable();
            container.RegisterInstance(service, Reuse.Singleton);

            container.Dispose();
            Assert.IsTrue(service.IsDisposed);
        }

        internal class SomethingDisposable : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() { IsDisposed = true; }
        }

        internal class A : IDisposable {
            public void Dispose()
            {
                throw new DivideByZeroException();
            }
        }
        internal class B : IDisposable {
            public bool IsDisposed;
            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        internal class SomeDep : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        internal class SomeRoot
        {
            public SomeDep Dep { get; private set; }
            public SomeRoot(SomeDep dep)
            {
                Dep = dep;
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
}