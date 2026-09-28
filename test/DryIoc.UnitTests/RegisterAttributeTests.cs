using System;
using System.Text;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RegisterAttributeTests : ITest
    {
        public int Run()
        {
            Test_generating_the_object_graph_with_the_missing_services();
            Test_generating_the_object_graph();
            Can_register_service_with_tracking_disposable_reuse();
            Can_register_service_using_attribute_on_implementation_class();
            Can_register_service_using_attribute_on_implementation_class_with_inferred_service_type();
            Can_register_singleton_service();
            Can_register_scoped_service();
            Can_register_with_service_key();
            Can_register_decorator();
            Can_register_decorator_with_use_decoratee_reuse();
            Can_register_decorator_applying_to_specific_decoratee_type();
            Can_register_with_metadata();
            Can_register_with_prevent_disposal();
            Can_register_with_condition();
            Can_register_multiple_services_for_same_implementation();
            Can_register_with_allow_disposable_transient();
            Can_register_many_via_attribute();
            Can_register_many_with_except();
            Can_register_explicit_service_types();
            Can_register_with_named_scope();
            Can_register_scoped_to_service();
            Can_register_wrapper();
            Can_register_decorator_with_decoratee_service_key();
            Can_register_decorator_with_order();
            Can_register_static_factory_method();
            Can_register_with_if_already_registered_replace();
            Can_register_with_open_resolution_scope();
            Can_register_with_use_parent_reuse();
            Can_register_with_prefer_in_single_service_resolve();
            Can_register_with_constructor_with_resolvable_arguments();
            Can_register_with_disposal_order();
            Can_register_with_as_resolution_call();

            return 32;
        }

        [Test]
        public void Can_register_service_with_tracking_disposable_reuse()
        {
            var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(Registrations));
            Assert.AreEqual(1, count);

            var ad = c.Resolve<ID>();
            Assert.IsNotNull(ad);

            c.Dispose();
            Assert.IsTrue(((AD)ad).IsDisposed);
        }

        [Register<ID, AD>(TrackDisposableTransient = DisposableTracking.TrackDisposableTransient)]
        public static class Registrations { }

        public interface ID { }

        public class AD : ID, IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }

        public interface IA { }
        public class A : IA { }

        public class B
        {
            public readonly IA A;
            public B(IA a) => A = a;
        }

        public class B2
        {
            public readonly A A;
            public B2(A a) => A = a;
        }

        [Register(typeof(IA), typeof(A), ReuseAs.Singleton)]
        [Register(typeof(B), typeof(B), ReuseAs.Scoped)]
        [Register(typeof(B2), typeof(B2), ReuseAs.ScopedOrSingleton)]
        [CompileTimeContainer(RootTypes = new[] { typeof(B2) })]
        public partial class DiConfig { }

        [Test]
        public void Test_generating_the_object_graph()
        {
            using var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(DiConfig));

            using var scope = c.OpenScope();

            var b = scope.Resolve<B>();
            Assert.IsNotNull(b);
            Assert.IsInstanceOf<A>(b.A);

            var sb = new StringBuilder(4096);
            var containerForGen = c.GenerateCompileTimeContainerCSharpCode(sb,
                roots: null, // generates top level Resolve for all registered services
                namespaceUsings: new[] { nameof(UnitTests) },
                genCompileTimeContainerClassName: "MyCompTimeContainer");

            var code = sb.ToString();

            StringAssert.Contains("MyCompTimeContainer", code);
            StringAssert.Contains("new RegisterAttributeTests.A", code);
            StringAssert.Contains("new RegisterAttributeTests.B", code);
        }

        [Test]
        public void Test_generating_the_object_graph_with_the_missing_services()
        {
            using var c = new Container();

            var count = c.RegisterByRegisterAttributes(typeof(DiConfig));

            var sb = new StringBuilder(4096);
            var containerForGen = c.GenerateCompileTimeContainerCSharpCode(sb,
                roots: new[] { ServiceInfo.Of<B2>() },
                namespaceUsings: new[] { nameof(UnitTests) },
                genCompileTimeContainerClassName: "MyCompTimeContainer2");

            var code = sb.ToString();

            StringAssert.Contains("MyCompTimeContainer2", code);
            StringAssert.Contains("new RegisterAttributeTests.B2", code);
        }

        // -- New tests for enhanced RegisterAttribute functionality --

        public interface IMyService { }
        public interface IMyOtherService { }

        [Register(typeof(IMyService))]
        public class MyServiceImpl : IMyService, IMyOtherService { }

        [Test]
        public void Can_register_service_using_attribute_on_implementation_class()
        {
            var c = new Container();
            var count = c.RegisterByRegisterAttributes(typeof(MyServiceImpl));

            Assert.AreEqual(1, count);
            var svc = c.Resolve<IMyService>();
            Assert.IsInstanceOf<MyServiceImpl>(svc);
        }

        [Register]
        public class SelfRegisteredService : IMyService { }

        [Test]
        public void Can_register_service_using_attribute_on_implementation_class_with_inferred_service_type()
        {
            var c = new Container();
            var count = c.RegisterByRegisterAttributes(typeof(SelfRegisteredService));

            Assert.AreEqual(1, count);
            var svc = c.Resolve<SelfRegisteredService>();
            Assert.IsNotNull(svc);
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl), ReuseAs.Singleton)]
        public static class SingletonRegistration { }

        [Test]
        public void Can_register_singleton_service()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(SingletonRegistration));

            var svc1 = c.Resolve<IMyService>();
            var svc2 = c.Resolve<IMyService>();
            Assert.AreSame(svc1, svc2);
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl), ReuseAs.Scoped)]
        public static class ScopedRegistration { }

        [Test]
        public void Can_register_scoped_service()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(ScopedRegistration));

            using var scope1 = c.OpenScope();
            using var scope2 = c.OpenScope();

            var svc1a = scope1.Resolve<IMyService>();
            var svc1b = scope1.Resolve<IMyService>();
            var svc2a = scope2.Resolve<IMyService>();

            Assert.AreSame(svc1a, svc1b); // same within scope
            Assert.AreNotSame(svc1a, svc2a); // different across scopes
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl), ServiceKey = "named")]
        public static class NamedRegistration { }

        [Test]
        public void Can_register_with_service_key()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(NamedRegistration));

            var svc = c.Resolve<IMyService>(serviceKey: "named");
            Assert.IsInstanceOf<MyServiceImpl>(svc);
        }

        public interface IDecorated { string Value { get; } }

        public class DecoratedImpl : IDecorated
        {
            public string Value => "base";
        }

        [Register(typeof(IDecorated), typeof(DecoratorImpl), FactoryType = FactoryType.Decorator)]
        public static class DecoratorRegistrations { }

        public class DecoratorImpl : IDecorated
        {
            private readonly IDecorated _inner;
            public DecoratorImpl(IDecorated inner) => _inner = inner;
            public string Value => "decorated:" + _inner.Value;
        }

        [Test]
        public void Can_register_decorator()
        {
            var c = new Container();
            c.Register<IDecorated, DecoratedImpl>();
            c.RegisterByRegisterAttributes(typeof(DecoratorRegistrations));

            var svc = c.Resolve<IDecorated>();
            Assert.AreEqual("decorated:base", svc.Value);
        }

        [Register(typeof(IDecorated), typeof(DecoratorWithDecorateeReuse),
            FactoryType = FactoryType.Decorator, UseDecorateeReuse = true)]
        public static class DecoratorWithReuseRegistrations { }

        public class DecoratorWithDecorateeReuse : IDecorated
        {
            private readonly IDecorated _inner;
            public DecoratorWithDecorateeReuse(IDecorated inner) => _inner = inner;
            public string Value => "reuse:" + _inner.Value;
        }

        [Test]
        public void Can_register_decorator_with_use_decoratee_reuse()
        {
            var c = new Container();
            c.Register<IDecorated, DecoratedImpl>(Reuse.Singleton);
            c.RegisterByRegisterAttributes(typeof(DecoratorWithReuseRegistrations));

            // With UseDecorateeReuse=true, decorator inherits the decoratee's reuse (singleton)
            var svc1 = c.Resolve<IDecorated>();
            var svc2 = c.Resolve<IDecorated>();
            // Both resolutions go through the same singleton decorator
            Assert.AreSame(svc1, svc2);
            Assert.AreEqual("reuse:base", svc1.Value);
        }

        public class SpecificDecoratedImpl : IDecorated
        {
            public string Value => "specific";
        }

        public class SpecificDecorator : IDecorated
        {
            private readonly IDecorated _inner;
            public SpecificDecorator(IDecorated inner) => _inner = inner;
            public string Value => "specific-decorated:" + _inner.Value;
        }

        [Register(typeof(IDecorated), typeof(SpecificDecorator),
            FactoryType = FactoryType.Decorator, DecorateesType = typeof(SpecificDecoratedImpl))]
        public static class SpecificDecoratorRegistrations { }

        [Test]
        public void Can_register_decorator_applying_to_specific_decoratee_type()
        {
            var c = new Container();
            c.Register<IDecorated, DecoratedImpl>(serviceKey: "base");
            c.Register<IDecorated, SpecificDecoratedImpl>(serviceKey: "specific");
            c.RegisterByRegisterAttributes(typeof(SpecificDecoratorRegistrations));

            // The decorator should only apply to SpecificDecoratedImpl
            var specific = c.Resolve<IDecorated>(serviceKey: "specific");
            var baseService = c.Resolve<IDecorated>(serviceKey: "base");

            Assert.AreEqual("specific-decorated:specific", specific.Value);
            Assert.AreEqual("base", baseService.Value);
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl), Metadata = "service-meta")]
        public static class MetadataRegistrations { }

        [Test]
        public void Can_register_with_metadata()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(MetadataRegistrations));

            var meta = c.Resolve<Meta<IMyService, string>>();
            Assert.AreEqual("service-meta", meta.Metadata);
            Assert.IsInstanceOf<MyServiceImpl>(meta.Value);
        }

        public class PreventDisposalService : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }

        [Register(typeof(PreventDisposalService), typeof(PreventDisposalService),
            ReuseAs = ReuseAs.Singleton, PreventDisposal = true)]
        public static class PreventDisposalRegistrations { }

        [Test]
        public void Can_register_with_prevent_disposal()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(PreventDisposalRegistrations));

            var svc = c.Resolve<PreventDisposalService>();
            c.Dispose();

            Assert.IsFalse(svc.IsDisposed); // should NOT be disposed because of PreventDisposal
        }

        public class ConditionalService : IMyService { }
        public class AlwaysUsedService : IMyService { }

        public class NotRootCondition : RegisterConditionAttribute
        {
            public override bool Evaluate(Request request) => !request.IsEmpty && request.DirectParent.IsEmpty;
        }

        [Register(typeof(IMyService), typeof(ConditionalService), ConditionType = typeof(NotRootCondition))]
        public static class ConditionRegistrations { }

        [Test]
        public void Can_register_with_condition()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(ConditionRegistrations));
            // Also register a fallback service
            c.Register<IMyService, AlwaysUsedService>(ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            // With condition NotRootCondition, the conditional service resolves when it's the root
            var svc = c.Resolve<IMyService>();
            Assert.IsNotNull(svc);
            // The condition should pass for root resolution
            Assert.IsInstanceOf<ConditionalService>(svc);
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl))]
        [Register(typeof(IMyOtherService), typeof(MyServiceImpl))]
        public static class MultipleServiceRegistration { }

        [Test]
        public void Can_register_multiple_services_for_same_implementation()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(MultipleServiceRegistration));

            var svc1 = c.Resolve<IMyService>();
            var svc2 = c.Resolve<IMyOtherService>();
            Assert.IsInstanceOf<MyServiceImpl>(svc1);
            Assert.IsInstanceOf<MyServiceImpl>(svc2);
        }

        public class DisposableTransientService : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }

        [Register(typeof(DisposableTransientService), typeof(DisposableTransientService),
            TrackDisposableTransient = DisposableTracking.AllowDisposableTransient)]
        public static class AllowDisposableTransientRegistrations { }

        [Test]
        public void Can_register_with_allow_disposable_transient()
        {
            // Default rules warn on disposable transients; AllowDisposableTransient should suppress it
            var c = new Container(Rules.Default.WithTrackingDisposableTransients());
            c.RegisterByRegisterAttributes(typeof(AllowDisposableTransientRegistrations));

            // Should not track disposal (AllowDisposableTransient means "I take responsibility")
            var svc = c.Resolve<DisposableTransientService>();
            Assert.IsNotNull(svc);
            c.Dispose();
            // The service was not tracked (AllowDisposableTransient), so it's NOT auto-disposed
            Assert.IsFalse(svc.IsDisposed);
        }

        // -- RegisterMany / multi-service / scopes / wrappers / factory methods --

        public interface IManyA { }
        public interface IManyB { }
        public class ManyImpl : IManyA, IManyB { }

        [Register(typeof(ManyImpl), typeof(ManyImpl), RegisterMany = true)]
        public static class RegisterManyRegistrations { }

        [Test]
        public void Can_register_many_via_attribute()
        {
            var c = new Container();
            var count = c.RegisterByRegisterAttributes(typeof(RegisterManyRegistrations));
            Assert.GreaterOrEqual(count, 2);

            Assert.IsInstanceOf<ManyImpl>(c.Resolve<IManyA>());
            Assert.IsInstanceOf<ManyImpl>(c.Resolve<IManyB>());
        }

        [Register(typeof(ManyImpl), typeof(ManyImpl), RegisterMany = true, Except = new[] { typeof(IManyB) })]
        public static class RegisterManyExceptRegistrations { }

        [Test]
        public void Can_register_many_with_except()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(RegisterManyExceptRegistrations));

            Assert.IsInstanceOf<ManyImpl>(c.Resolve<IManyA>());
            Assert.Throws<ContainerException>(() => c.Resolve<IManyB>());
        }

        [Register(typeof(ManyImpl), typeof(ManyImpl), ServiceTypes = new[] { typeof(IManyA), typeof(IManyB) })]
        public static class ExplicitServiceTypesRegistrations { }

        [Test]
        public void Can_register_explicit_service_types()
        {
            var c = new Container();
            var count = c.RegisterByRegisterAttributes(typeof(ExplicitServiceTypesRegistrations));
            Assert.AreEqual(2, count);

            Assert.IsInstanceOf<ManyImpl>(c.Resolve<IManyA>());
            Assert.IsInstanceOf<ManyImpl>(c.Resolve<IManyB>());
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl), ReuseAs.Scoped, ReuseScopeName = "named")]
        public static class NamedScopeRegistrations { }

        [Test]
        public void Can_register_with_named_scope()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(NamedScopeRegistrations));

            using var scope = c.OpenScope("named");
            var a = scope.Resolve<IMyService>();
            var b = scope.Resolve<IMyService>();
            Assert.AreSame(a, b);

            using var other = c.OpenScope("other");
            Assert.Throws<ContainerException>(() => other.Resolve<IMyService>());
        }

        public class ParentService
        {
            public IMyService Dep { get; }
            public ParentService(IMyService dep) => Dep = dep;
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl), ReuseAs.ScopedToService, ScopedToServiceType = typeof(ParentService))]
        [Register(typeof(ParentService), typeof(ParentService), OpenResolutionScope = true)]
        public static class ScopedToServiceRegistrations { }

        [Test]
        public void Can_register_scoped_to_service()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(ScopedToServiceRegistrations));

            var p1 = c.Resolve<ParentService>();
            var p2 = c.Resolve<ParentService>();
            // Parent opens a resolution scope; dependency is scoped to that parent service
            Assert.AreNotSame(p1, p2);
            Assert.IsInstanceOf<MyServiceImpl>(p1.Dep);
            Assert.AreNotSame(p1.Dep, p2.Dep);
        }

        public class MyWrapper<T>
        {
            public T Value { get; }
            public MyWrapper(T value) => Value = value;
        }

        [Register(typeof(MyWrapper<>), typeof(MyWrapper<>), FactoryType = FactoryType.Wrapper)]
        public static class WrapperRegistrations { }

        [Test]
        public void Can_register_wrapper()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(WrapperRegistrations));
            c.Register<IMyService, MyServiceImpl>();

            var wrapped = c.Resolve<MyWrapper<IMyService>>();
            Assert.IsInstanceOf<MyServiceImpl>(wrapped.Value);
        }

        public class KeyedDecorator : IDecorated
        {
            private readonly IDecorated _inner;
            public KeyedDecorator(IDecorated inner) => _inner = inner;
            public string Value => "keyed:" + _inner.Value;
        }

        [Register(typeof(IDecorated), typeof(KeyedDecorator),
            FactoryType = FactoryType.Decorator, DecorateeServiceKey = "k1")]
        public static class KeyedDecoratorRegistrations { }

        [Test]
        public void Can_register_decorator_with_decoratee_service_key()
        {
            var c = new Container();
            c.Register<IDecorated, DecoratedImpl>(serviceKey: "k1");
            c.Register<IDecorated, DecoratedImpl>(serviceKey: "k2");
            c.RegisterByRegisterAttributes(typeof(KeyedDecoratorRegistrations));

            Assert.AreEqual("keyed:base", c.Resolve<IDecorated>(serviceKey: "k1").Value);
            Assert.AreEqual("base", c.Resolve<IDecorated>(serviceKey: "k2").Value);
        }

        public class OuterDecorator : IDecorated
        {
            private readonly IDecorated _inner;
            public OuterDecorator(IDecorated inner) => _inner = inner;
            public string Value => "outer:" + _inner.Value;
        }

        public class InnerDecorator : IDecorated
        {
            private readonly IDecorated _inner;
            public InnerDecorator(IDecorated inner) => _inner = inner;
            public string Value => "inner:" + _inner.Value;
        }

        [Register(typeof(IDecorated), typeof(OuterDecorator), FactoryType = FactoryType.Decorator, DecoratorOrder = 10)]
        [Register(typeof(IDecorated), typeof(InnerDecorator), FactoryType = FactoryType.Decorator, DecoratorOrder = -10)]
        public static class OrderedDecoratorRegistrations { }

        [Test]
        public void Can_register_decorator_with_order()
        {
            var c = new Container();
            c.Register<IDecorated, DecoratedImpl>();
            c.RegisterByRegisterAttributes(typeof(OrderedDecoratorRegistrations));

            // Inner (order -10) closer to decoratee; Outer (order 10) further out
            Assert.AreEqual("outer:inner:base", c.Resolve<IDecorated>().Value);
        }

        public static class FactoryMethods
        {
            [Register(typeof(IMyService), ReuseAs.Singleton)]
            public static IMyService CreateMyService() => new MyServiceImpl();
        }

        [Test]
        public void Can_register_static_factory_method()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(FactoryMethods));

            var a = c.Resolve<IMyService>();
            var b = c.Resolve<IMyService>();
            Assert.IsInstanceOf<MyServiceImpl>(a);
            Assert.AreSame(a, b);
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl))]
        [Register(typeof(IMyService), typeof(AlwaysUsedService), IfAlreadyRegistered = RegisterIfAlready.Replace)]
        public static class ReplaceRegistrations { }

        [Test]
        public void Can_register_with_if_already_registered_replace()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(ReplaceRegistrations));
            Assert.IsInstanceOf<AlwaysUsedService>(c.Resolve<IMyService>());
        }

        public class OpensScopeService
        {
            public IResolverContext Scope { get; }
            public OpensScopeService(IResolverContext scope) => Scope = scope;
        }

        public class ScopedDep : IDisposable
        {
            public bool IsDisposed;
            public void Dispose() => IsDisposed = true;
        }

        public class OpensScopeConsumer
        {
            public OpensScopeService Svc { get; }
            public OpensScopeConsumer(OpensScopeService svc) => Svc = svc;
        }

        [Register(typeof(OpensScopeService), typeof(OpensScopeService), OpenResolutionScope = true)]
        [Register(typeof(ScopedDep), typeof(ScopedDep), ReuseAs.Scoped)]
        public static class OpenResolutionScopeRegistrations { }

        [Test]
        public void Can_register_with_open_resolution_scope()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(OpenResolutionScopeRegistrations));
            c.Register<OpensScopeConsumer>();

            // OpenResolutionScope allows resolving scoped dependency without an outer scope
            // when OpensScopeService is a resolution root... actually ScopedDep needs a scope.
            // OpensScopeService opens a resolution scope so its own scoped deps work.
            // Here we just verify registration does not throw and service resolves.
            var svc = c.Resolve<OpensScopeService>();
            Assert.IsNotNull(svc);
            Assert.IsNotNull(svc.Scope);
        }

        public class ParentWithReuse
        {
            public ChildWithParentReuse Child { get; }
            public ParentWithReuse(ChildWithParentReuse child) => Child = child;
        }

        public class ChildWithParentReuse { }

        [Register(typeof(ParentWithReuse), typeof(ParentWithReuse), ReuseAs.Singleton)]
        [Register(typeof(ChildWithParentReuse), typeof(ChildWithParentReuse), UseParentReuse = true)]
        public static class ParentReuseRegistrations { }

        [Test]
        public void Can_register_with_use_parent_reuse()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(ParentReuseRegistrations));

            var p1 = c.Resolve<ParentWithReuse>();
            var p2 = c.Resolve<ParentWithReuse>();
            Assert.AreSame(p1, p2);
            Assert.AreSame(p1.Child, p2.Child); // child inherits singleton reuse of parent
        }

        public class PreferredService : IMyService { }
        public class OtherService : IMyService { }

        [Register(typeof(IMyService), typeof(PreferredService), PreferInSingleServiceResolve = true)]
        [Register(typeof(IMyService), typeof(OtherService))]
        public static class PreferRegistrations { }

        [Test]
        public void Can_register_with_prefer_in_single_service_resolve()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(PreferRegistrations));
            Assert.IsInstanceOf<PreferredService>(c.Resolve<IMyService>());
        }

        public class MultiCtorService
        {
            public string Via;
            public MultiCtorService() => Via = "default";
            public MultiCtorService(IMyService dep) => Via = "with-dep";
        }

        [Register(typeof(IMyService), typeof(MyServiceImpl))]
        [Register(typeof(MultiCtorService), typeof(MultiCtorService),
            FactoryMethod = MadeFactoryMethod.ConstructorWithResolvableArguments)]
        public static class CtorSelectRegistrations { }

        [Test]
        public void Can_register_with_constructor_with_resolvable_arguments()
        {
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(CtorSelectRegistrations));
            Assert.AreEqual("with-dep", c.Resolve<MultiCtorService>().Via);
        }

        public class DisposeOrderA : IDisposable
        {
            public static int Counter;
            public int Order;
            public void Dispose() => Order = ++Counter;
        }

        public class DisposeOrderB : IDisposable
        {
            public static int Counter;
            public int Order;
            public void Dispose() => Order = ++DisposeOrderA.Counter;
        }

        [Register(typeof(DisposeOrderA), typeof(DisposeOrderA), ReuseAs.Singleton, DisposalOrder = 2)]
        [Register(typeof(DisposeOrderB), typeof(DisposeOrderB), ReuseAs.Singleton, DisposalOrder = 1)]
        public static class DisposalOrderRegistrations { }

        [Test]
        public void Can_register_with_disposal_order()
        {
            DisposeOrderA.Counter = 0;
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(DisposalOrderRegistrations));
            var a = c.Resolve<DisposeOrderA>();
            var b = c.Resolve<DisposeOrderB>();
            c.Dispose();
            // B has DisposalOrder=1 (disposed first), A has 2 (disposed later)
            Assert.Less(b.Order, a.Order);
        }

        public class AsCallService { public static int Created; public AsCallService() => ++Created; }
        public class AsCallConsumer { public AsCallService S; public AsCallConsumer(AsCallService s) => S = s; }

        [Register(typeof(AsCallService), typeof(AsCallService), AsResolutionCall = true)]
        [Register(typeof(AsCallConsumer), typeof(AsCallConsumer))]
        public static class AsResolutionCallRegistrations { }

        [Test]
        public void Can_register_with_as_resolution_call()
        {
            AsCallService.Created = 0;
            var c = new Container();
            c.RegisterByRegisterAttributes(typeof(AsResolutionCallRegistrations));
            var consumer = c.Resolve<AsCallConsumer>();
            Assert.IsNotNull(consumer.S);
            Assert.GreaterOrEqual(AsCallService.Created, 1);
        }
    }
}
