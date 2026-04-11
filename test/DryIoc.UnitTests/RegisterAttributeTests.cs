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

            return 16;
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
    }
}
