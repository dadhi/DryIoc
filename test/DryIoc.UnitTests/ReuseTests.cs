using System;
using System.Collections.Generic;
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

            Assert.AreEqual(
                Error.NameOf(Error.ScopeIsDisposed),
                Error.NameOf(ex.Error));
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
        public void When_registering_external_instance_and_disposing_container_Then_instance_should_be_disposed()
        {
            var container = new Container();
            var service = new DisposableService();
            container.RegisterInstance<IService>(service);

            container.Dispose();

            Assert.That(service.IsDisposed, Is.True);
        }

        [Test]
        public void When_registering_external_instance_with_prevent_disposal_parameter_Then_instance_should_Not_be_disposed()
        {
            var container = new Container();
            container.RegisterInstance<IService>(new DisposableService(), setup: Setup.With(preventDisposal: true));
            var service = container.Resolve<IService>();

            container.Dispose();

            Assert.IsFalse(((DisposableService)service).IsDisposed);
        }

        [Test, Explicit]
        public void Registering_instance_as_weak_reference_will_prevent_the_disposal()
        {
            var container = new Container();
            var instance = new DisposableService();
            container.RegisterInstance(instance, setup: Setup.With(weaklyReferenced: true));
            instance = container.Resolve<DisposableService>();

            container.Dispose();

            Assert.IsFalse(instance.IsDisposed);
            GC.KeepAlive(instance);
        }

        [Test]
        public void Int32_parameter_with_the_same_name_as_Container_scope_wont_collide()
        {
            var container = new Container();
            container.Register<Soose>(Reuse.Singleton);

            var factory = container.Resolve<Func<int, Soose>>();

            Assert.AreEqual(1, factory(1).Scopes);
        }

        [Test]
        public void In_same_resolution_scope_Log_instances_should_be_same()
        {
            var container = new Container();
            container.Register<Consumer>();
            container.Register<Account>();
            container.Register<Log>(Reuse.Scoped);

            using (var scope = container.OpenScope())
            {
                var consumer = scope.Resolve<Consumer>();

                Assert.That(consumer.Log, Is.Not.Null);
                Assert.That(consumer.Log, Is.SameAs(consumer.Account.Log));
            }
        }

        [Test]
        public void In_different_resolution_scopes_Log_instances_should_be_different()
        {
            var container = new Container();
            container.Register<Consumer>(setup: Setup.With(openResolutionScope: true));
            container.Register<Account>(setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.Scoped);

            var consumer = container.Resolve<Consumer>();
            var account = container.Resolve<Account>();

            Assert.That(consumer.Log, Is.Not.Null);
            Assert.That(consumer.Log, Is.Not.SameAs(account.Log));
        }

        [Test]
        public void If_not_fully_matched_resolution_scope_found_Then_the_top_scope_will_be_used()
        {
            var container = new Container();

            container.Register<AccountUser>();
            container.Register<Account>(setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.ScopedTo<Account>("account"));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<AccountUser>());

            Assert.AreEqual(
                Error.NameOf(Error.NoMatchedScopeFound),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Resolve_succeed_only_if_fully_matched_resolution_scope_found()
        {
            var container = new Container();

            container.Register<AccountUser>(made: Parameters.Of.Type<Account>(serviceKey: "account"));
            container.Register<Account>(serviceKey: "account", setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.ScopedTo<Account>("account"));

            var user = container.Resolve<AccountUser>();

            Assert.IsNotNull(user.Account.Log);
        }

        [Test]
        public void Resolve_service_reused_in_resolution_scope_succeed_if_key_matched()
        {
            var container = new Container();

            container.Register<AccountUser>(made: Parameters.Of.Type<Account>(serviceKey: "account"));
            container.Register<Account>(serviceKey: "account", setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.ScopedTo(serviceKey: "account"));

            var user = container.Resolve<AccountUser>();

            Assert.IsNotNull(user.Account.Log);
        }

        [Test]
        public void Resolve_service_reused_in_resolution_scope_of_open_generic_service()
        {
            var container = new Container();

            container.Register(typeof(Aaa<>), setup: Setup.With(openResolutionScope: true));
            container.Register<Bbb>(Reuse.ScopedTo(typeof(Aaa<>)));

            var aaa = container.Resolve<Aaa<Bbb>>();

            Assert.IsNotNull(aaa);
        }

        public class Bbb { }

        public class Aaa<T>
        {
            public T Ttt { get; }

            public Aaa(T t)
            {
                Ttt = t;
            }
        }

        [Test]
        public void Can_control_disposing_of_matching_resolution_scope_with_wrapper()
        {
            var container = new Container();

            container.Register<AccountUser>();
            container.Register<Account, CarefulAccount>(Reuse.Singleton, setup: Setup.With(openResolutionScope: true));
            container.Register<Log, DisposableLog>(Reuse.ScopedTo<Account>());

            var user = container.Resolve<AccountUser>();

            Assert.IsNotNull(user.Account.Log);
            ((IDisposable)user.Account).Dispose();

            Assert.IsTrue(((DisposableLog)user.Account.Log).IsDisposed);
        }

        [Test]
        public void Can_use_resolution_scope_reuse_bound_to_resolution_root()
        {
            var container = new Container();
            container.Register<ViewModel2>(setup: Setup.With(openResolutionScope: true));
            container.Register<Log>(Reuse.ScopedTo<ViewModel2>());

            var vm = container.Resolve<ViewModel2>();

            Assert.IsNotNull(vm.Log);
        }

        [Test]
        public void Resolution_scope_can_be_tracked_as_disposable_transient_when_injected_as_IDisposable()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());

            container.Register<K>(Reuse.Scoped);
            container.Register<L>(setup: Setup.With(openResolutionScope: true));

            K k;
            using (var scope = container.OpenScope())
            {
                var l = scope.Resolve<L>();
                k = l.K;
            }

            Assert.IsTrue(k.IsDisposed);
        }

        public class K : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class L
        {
            public K K { get; }
            public IDisposable Scope { get; }

            public L(K k, IResolverContext scope)
            {
                K = k;
                Scope = scope;
            }
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

        internal interface IViewModel { }

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
            public Account Account { get; }
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
            private readonly IDisposable _scope;

            public CarefulAccount(Log log, IResolverContext scope) : base(log)
            {
                _scope = scope;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }
        }

        [Test]
        public void Given_Thread_reuse_and_no_open_scope_Exception_should_be_thrown()
        {
            var container = new Container();
            container.Register<Service>(Reuse.InThread);

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<Service>());

            Assert.AreEqual(
                Error.NameOf(Error.NoCurrentScope),
                Error.NameOf(ex.Error));
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_same_thread_should_be_the_same()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            using (container.OpenScope())
            {
                container.Register<Service>(Reuse.InThread);

                var one = container.Resolve<Service>();
                var another = container.Resolve<Service>();

                Assert.AreSame(one, another);
            }
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_same_thread_should_be_the_same_In_nested_scope_too()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            using (var scope = container.OpenScope("root"))
            {
                container.Register<Service>(Reuse.ScopedTo("root"));

                var one = scope.Resolve<Service>();

                using (var nested = scope.OpenScope())
                {
                    var two = nested.Resolve<Service>();
                    Assert.AreSame(one, two);
                }

                var another = container.Resolve<Service>();
                Assert.AreSame(one, another);
            }
        }

        [Test]
        public void Given_Thread_reuse_Dependencies_injected_in_same_thread_should_be_the_same()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            container.Register<ServiceWithDependency>();
            container.Register<IDependency, Dependency>(Reuse.InThread);

            using (var scope = container.OpenScope())
            {
                var one = scope.Resolve<ServiceWithDependency>();
                var another = scope.Resolve<ServiceWithDependency>();
                Assert.AreSame(one.Dependency, another.Dependency);
            }
        }

        [Test]
        public void Given_Thread_reuse_Services_resolved_in_different_thread_should_be_the_different()
        {
            var container = new Container(scopeContext: new ThreadScopeContext());
            var mainThread = container.OpenScope();
            container.Register<Service>(Reuse.InThread);

            Service one = null;
            var threadOne = new Thread(() =>
            {
                using (var threadLocal = container.OpenScope())
                    one = threadLocal.Resolve<Service>();
            })
            { IsBackground = true };

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
            parent.Register<ServiceWithDependency>();
            parent.Register<IDependency, Dependency>(Reuse.InThread);

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

            container.Register<ServiceWithResolutionAndSingletonDependencies>(
                setup: Setup.With(openResolutionScope: true));

            container.Register<SingletonDep>(Reuse.Scoped);
            container.Register<ResolutionScopeDep>(Reuse.Singleton);

            var service = container.Resolve<ServiceWithResolutionAndSingletonDependencies>();

            Assert.AreSame(service.ResolutionScopeDep, service.SingletonDep.ResolutionScopeDep);
        }

        [Test]
        public void Should_throw_if_rule_specified_and_dependency_lifespan_is_less_than_parents()
        {
            var container = new Container();
            container.Register<Client>(Reuse.Singleton);
            container.Register<ILogger, FastLogger>(Reuse.InCurrentScope);

            using (var scope = container.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() =>
                    scope.Resolve<Client>());

                Assert.That(ex.Error, Is.EqualTo(Error.DependencyHasShorterReuseLifespan));
            }
        }

        [Test]
        public void Should_Not_throw_if_rule_is_off_and_dependency_lifespan_is_less_than_parents()
        {
            var container = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());
            container.Register<Client>(Reuse.Singleton, setup: Setup.With(openResolutionScope: true));
            container.Register<ILogger, FastLogger>(Reuse.Scoped);

            var client = container.Resolve<Client>();

            Assert.That(client.Logger, Is.InstanceOf<FastLogger>());
        }

        [Test]
        public void Can_dispose_resolution_reused_services()
        {
            var container = new Container();

            container.Register<SomeDep>(Reuse.Scoped);
            container.Register<SomeRoot>(Reuse.Singleton, setup: Setup.With(openResolutionScope: true));

            var root = container.Resolve<SomeRoot>();
            root.Dispose();

            Assert.That(root.Dep.IsDisposed, Is.True);
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
            public Y Y { get; }
            public X(Y y)
            {
                Y = y;
            }
        }
        internal class Y
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public Lazy<X> X { get; }
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
            container.Use(container.TrackDisposable(service)); // he-he, hacking it a bit

            container.Dispose();
            Assert.IsTrue(service.IsDisposed);
        }

        [Test]
        public void Dispose_should_happen_in_reverse_resolution_order()
        {
            var container = new Container();
            container.Register<F>(Reuse.Singleton);
            container.Register<S>(Reuse.Singleton);

            var fst = container.Resolve<F>();
            container.Dispose();

            Assert.AreEqual("fs", fst.S.Order);
        }

        class F : IDisposable
        {
            public readonly S S;

            public F(S s)
            {
                S = s;
            }

            public void Dispose()
            {
                S.Order += "f";
            }
        }

        class S : IDisposable
        {
            public string Order = String.Empty;

            public void Dispose()
            {
                Order += "s";
            }
        }

        internal class SomethingDisposable : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() { IsDisposed = true; }
        }

        internal class A : IDisposable
        {
            public void Dispose()
            {
                throw new DivideByZeroException();
            }
        }
        internal class B : IDisposable
        {
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

        internal class SomeRoot : IDisposable
        {
            public SomeDep Dep { get; }

            public SomeRoot(SomeDep dep, IResolverContext scope)
            {
                _scope = scope;
                Dep = dep;
            }

            public void Dispose()
            {
                _scope.Dispose();
            }

            private readonly IDisposable _scope;
        }

        [Test]
        public void Can_specify_default_reuse_per_Container_different_from_Transient()
        {
            var container = new Container(r => r.WithDefaultReuse(Reuse.Scoped));

            container.Register<Abc>();

            using (var scope = container.OpenScope())
            {
                var abc = scope.Resolve<Abc>();
                Assert.AreSame(abc, scope.Resolve<Abc>());
            }
        }

        [Test]
        public void Resolution_scope_should_be_propagated_through_resolution_call_intermediate_dependencies()
        {
            var container = new Container();

            container.Register<AD>(Reuse.ScopedTo<AResolutionScoped>());
            container.Register<ADConsumer>(setup: Setup.With(asResolutionCall: true));
            container.Register<AResolutionScoped>(setup: Setup.With(openResolutionScope: true));

            var scoped = container.Resolve<AResolutionScoped>();
            Assert.IsNotNull(scoped);
            Assert.AreSame(scoped.Consumer.Ad, scoped.Consumer.Ad2);
        }

        [Test]
        public void Resolution_call_should_not_create_resolution_scope()
        {
            var container = new Container();

            container.Register<AD>(Reuse.ScopedTo<ADConsumer>());
            container.Register<ADConsumer>(setup: Setup.With(asResolutionCall: true));
            container.Register<AResolutionScoped>(setup: Setup.With(openResolutionScope: true));

            Assert.Throws<ContainerException>(() =>
            container.Resolve<AResolutionScoped>());
        }

        [Test]
        public void Resolve_a_big_amount_of_singletons()
        {
            var container = new Container();

            const int count = 32 * 2 + 1;
            for (var i = 0; i < count; i++)
            {
                var serviceKey = new IntKey(i);
                container.Register<AD>(Reuse.Singleton, serviceKey: serviceKey);
            }

            var services = container.Resolve<KeyValuePair<IntKey, AD>[]>();
            for (var i = 0; i < count; i++)
            {
                var pair = services[i];
                Assert.IsNotNull(pair.Value);
                Assert.AreEqual(i, pair.Key.Index);
            }

            container.Dispose();
            for (var i = 0; i < count; ++i)
                Assert.IsTrue(services[i].Value.IsDisposed);
        }

        public class IntKey
        {
            public readonly int Index;

            public IntKey(int i)
            {
                Index = i;
            }

            public override bool Equals(object obj)
            {
                var intKey = obj as IntKey;
                return intKey != null && Index.Equals(intKey.Index);
            }

            public override int GetHashCode()
            {
                return Index.GetHashCode();
            }
        }

        public class AD : IDisposable
        {
            public bool IsDisposed { get; set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public class ADConsumer
        {
            public AD Ad { get; }

            public AD Ad2 { get; }

            public ADConsumer(AD ad, AD ad2)
            {
                Ad = ad;
                Ad2 = ad2;
            }
        }

        public class AResolutionScoped
        {
            public ADConsumer Consumer { get; }

            public AResolutionScoped(ADConsumer consumer)
            {
                Consumer = consumer;
            }
        }

        public class Abc { }

        [Test]
        public void Resolving_the_object_should_use_required_service_type_for_matching_the_resolution_scope()
        {
            var container = new Container();

            container.Register<O>(setup: Setup.With(openResolutionScope: true));
            container.Register<Ho>(Reuse.ScopedTo<O>());

            container.Resolve<object>(typeof(O));
        }

        [Test]
        public void Scoped_service_can_open_resolution_scope()
        {
            var container = new Container();

            container.Register<O>(Reuse.Scoped, setup: Setup.With(openResolutionScope: true));
            container.Register<Ho>(Reuse.ScopedTo<O>());

            using (var scope = container.OpenScope())
            {
                var o = scope.Resolve<O>();
                var o2 = scope.Resolve<O>();
                Assert.AreNotSame(o, o2);
                Assert.AreSame(o.Ho, o.Ho2);
            }
        }

        public class Ho { }

        public class O
        {
            public Ho Ho { get; }
            public Ho Ho2 { get; }

            public O(Ho ho, Ho ho2)
            {
                Ho = ho;
                Ho2 = ho2;
            }
        }

        [Test]
        public void Singleton_with_all_singleton_dependencies_will_be_evaluated_to_constant_expression()
        {
            var container = new Container();

            container.Register<O>(Reuse.Singleton);
            container.Register<Ho>(Reuse.Singleton);
            container.Register<P>(Reuse.Singleton);

            var p = container.Resolve<P>();

            Assert.IsNotNull(p);
        }

        public class P
        {
            public P(Ho ho, O o)
            {

            }
        }

        [Test]
        public void Can_specify_multiple_scope_names_in_one_reuse()
        {
            var c = new Container();

            c.Register<Go>(Reuse.ScopedTo("a", "b"));

            using (var a = c.OpenScope("a"))
            {
                var goA = a.Resolve<Go>();
                using (var noname = a.OpenScope())
                {
                    var goNN = noname.Resolve<Go>();
                    Assert.AreSame(goNN, goA);

                    using (var b = noname.OpenScope("b"))
                    {
                        var goB = b.Resolve<Go>();
                        Assert.AreNotSame(goB, goA); // different name
                        Assert.AreSame(goB, b.Resolve<Go>());
                    }
                }
            }
        }

        class Go { }

        [Test]
        public void Can_specify_to_dispose_something_earlier()
        {
            var c = new Container();

            c.Register<Duck>(Reuse.Singleton, setup: Setup.With(disposalOrder: 1));
            c.Register<Quack>(Reuse.Singleton);

            var d = c.Resolve<Duck>();
            var q = c.Resolve<Quack>();

            Assert.AreSame(q, d.Quack.Value); // extract lazy value
            c.Dispose();

            Assert.IsTrue(d.IsDisposed);
            Assert.IsTrue(q.IsDisposed);

            Assert.IsTrue(q.LastTimeQuacked); // !!! here indication that dependency disposed before consumer
        }

        [Test]
        public void Can_specify_to_dispose_something_later()
        {
            var c = new Container();

            c.Register<Duck>(Reuse.Singleton, setup: Setup.With(disposalOrder: -1));
            c.Register<Quack>(Reuse.Singleton, setup: Setup.With(disposalOrder: -2));

            var d = c.Resolve<Duck>();
            var q = c.Resolve<Quack>();

            Assert.AreSame(q, d.Quack.Value); // extract lazy value
            c.Dispose();

            Assert.IsTrue(d.IsDisposed);
            Assert.IsTrue(q.IsDisposed);

            Assert.IsFalse(q.LastTimeQuacked); // !!! here indication that dependency disposed before consumer
        }

        public class Duck : IDisposable
        {
            public Lazy<Quack> Quack { get; private set; }

            public Duck(Lazy<Quack> quack)
            {
                Quack = quack;
            }

            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                Quack.Value.LastTime();
                IsDisposed = true;
            }
        }

        public class Quack : IDisposable
        {
            public bool LastTimeQuacked { get; private set; }
            public void LastTime()
            {
                if (!IsDisposed)
                    LastTimeQuacked = true;
            }

            public bool IsDisposed { get; private set; }
            public void Dispose()
            {
                IsDisposed = true;
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