using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DryIoc.UnitTests.CUT;
using NUnit.Framework;
using static FastExpressionCompiler.LightExpression.Expression;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class RulesTests
    {
        [Test]
        public void Given_service_with_two_ctors_I_can_specify_what_ctor_to_choose_for_resolve()
        {
            var container = new Container();

            container.Register(typeof(Bla<>), made: Made.Of(
                t => t.GetConstructorOrNull(typeof(Func<>).MakeGenericType(t.GetGenericParamsAndArgs()[0]))));

            container.Register(typeof(SomeService), typeof(SomeService));

            var bla = container.Resolve<Bla<SomeService>>();

            Assert.That(bla.Factory(), Is.InstanceOf<SomeService>());
        }

        [Test]
        public void I_should_be_able_to_add_rule_to_resolve_not_registered_service()
        {
            var container = new Container(Rules.Default.WithUnknownServiceResolvers(request =>
                !request.ServiceType.IsValueType() && !request.ServiceType.IsAbstract()
                    ? new ReflectionFactory(request.ServiceType)
                    : null));

            var service = container.Resolve<NotRegisteredService>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void I_can_remove_rule_to_resolve_not_registered_service()
        {
            Rules.UnknownServiceResolver unknownServiceResolver = request =>
                !request.ServiceType.IsValueType() && !request.ServiceType.IsAbstract()
                    ? new ReflectionFactory(request.ServiceType)
                    : null;

            IContainer container = new Container(Rules.Default.WithUnknownServiceResolvers(unknownServiceResolver));
            Assert.IsNotNull(container.Resolve<NotRegisteredService>());

            container = container
                .With(rules => rules.WithoutUnknownServiceResolver(unknownServiceResolver))
                .WithoutCache(); // Important to remove cache

            Assert.IsNull(container.Resolve<NotRegisteredService>(IfUnresolved.ReturnDefault));
        }

        [Test]
        public void I_can_add_rule_to_resolve_any_unknown_concrete_type()
        {
            IContainer container = new Container(
                rules => rules.WithConcreteTypeDynamicRegistrations());

            var x = container.Resolve<X>();

            Assert.IsInstanceOf<X>(x);
            Assert.IsInstanceOf<Y>(x.Dep);
        }

        [Test]
        public void I_can_Not_resolve_unknown_concrete_type_on_specific_condition()
        {
            IContainer container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations((serviceType, _) => serviceType != typeof(X)));

            Assert.Throws<ContainerException>(() =>
                container.Resolve<X>());
        }

        [Test]
        public void I_can_resolve_unknown_concrete_type_on_specific_condition()
        {
            IContainer container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations((serviceType, _) => serviceType != typeof(X)));

            container.Register<X>(Reuse.Singleton);
            var x = container.Resolve<X>();

            Assert.IsInstanceOf<X>(x);
            Assert.IsInstanceOf<Y>(x.Dep);
        }

        [Test]
        public void Can_resolve_unknown_concrete_type_as_Func_with_required_type()
        {
            var container = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());

            var getX = container.Resolve<Func<Y, object>>(typeof(X));

            var x = getX(new Y());
            Assert.IsInstanceOf<X>(x);
        }

        [Test]
        public void Cannot_resolve_unknown_concrete_type_as_Func_with_required_type()
        {
            var container = new Container(rules => rules.WithConcreteTypeDynamicRegistrations());

            var getX = container.Resolve<Func<Y, object>>(typeof(A));

            var x = getX(new Y());
            Assert.IsInstanceOf<A>(x);
        }

        [Test]
        public void Can_collect_unresolved_concrete_type_dependency()
        {
            var unresolvedTypes = new List<Type>();

            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations()
                .WithUnknownServiceHandler(req => unresolvedTypes.Add(req.ServiceType)));

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<Xx>());

            Assert.AreEqual(Error.NameOf(Error.UnableToResolveFromRegisteredServices), Error.NameOf(ex.Error));

            CollectionAssert.AreEqual(new[] { typeof(IMissing), typeof(Xx) }, unresolvedTypes);
        }

        [Test]
        public void Can_fallback_to_next_rule_if_AutoConcreteResolution_is_unable_to_resolve_concrete_type()
        {
            var container = new Container(rules => rules
                .WithConcreteTypeDynamicRegistrations()
                .WithUnknownServiceResolvers(r => r.ServiceType == typeof(Xx) ?
                    new DelegateFactory(_ => new Xx(null)) : null));

            var xx = container.Resolve<Xx>();

            Assert.IsInstanceOf<Xx>(xx);
        }

        public class X
        {
            public Y Dep { get; }

            public X(Y dep)
            {
                Dep = dep;
            }
        }

        public interface IMissing { }
        public class Xx
        {
            public IMissing Dep { get; set; }
            public Xx(IMissing dep)
            {
                Dep = dep;
            }
        }

        public class Y { }

        [Test]
        public void When_service_registered_with_name_Then_it_could_be_resolved_with_ctor_parameter_ImportAttribute()
        {
            var container = new Container(rules => rules.With(parameters: GetServiceInfoFromImportAttribute));

            container.Register(typeof(INamedService), typeof(NamedService));
            container.Register(typeof(INamedService), typeof(AnotherNamedService), serviceKey: "blah");
            container.Register(typeof(ServiceWithImportedCtorParameter));

            var service = container.Resolve<ServiceWithImportedCtorParameter>();

            Assert.That(service.NamedDependency, Is.InstanceOf<AnotherNamedService>());
        }

        [Test]
        public void I_should_be_able_to_import_single_service_based_on_specified_metadata()
        {
            var container = new Container(rules => rules.With(parameters: GetServiceFromWithMetadataAttribute));

            container.Register(typeof(IFooService), typeof(FooHey), setup: Setup.With(metadataOrFuncOfMetadata: FooMetadata.Hey));
            container.Register(typeof(IFooService), typeof(FooBlah), setup: Setup.With(metadataOrFuncOfMetadata: FooMetadata.Blah));
            container.Register(typeof(FooConsumer));

            var service = container.Resolve<FooConsumer>();

            Assert.That(service.Foo.Value, Is.InstanceOf<FooBlah>());
            GC.KeepAlive(container);
        }

        [Test]
        public void Can_specify_rules_to_resolve_last_registration_from_multiple_available()
        {
            var container = new Container(Rules.Default.WithFactorySelector(Rules.SelectLastRegisteredFactory()));

            container.Register(typeof(IService), typeof(Service));
            container.Register(typeof(IService), typeof(AnotherService));
            var service = container.Resolve(typeof(IService));

            Assert.IsInstanceOf<AnotherService>(service);
        }

        [Test]
        public void Can_specify_rules_to_resolve_last_registration_from_mixed_open_and_closed_generics()
        {
            var container = new Container(r => r.WithFactorySelector(Rules.SelectLastRegisteredFactory()));

            container.Register(typeof(IService<>), typeof(Service<>));
            container.Register(typeof(IService<int>), typeof(IntService));
            var service = container.Resolve<IService<int>>();

            Assert.IsInstanceOf<IntService>(service);
        }

        class IntService : IService<int>
        {
            public int Dependency { get; set; }
        }

        [Test]
        public void You_can_specify_rules_to_disable_registration_based_on_reuse_type()
        {
            var container = new Container(Rules.Default.WithFactorySelector(
                (request, factories) => factories.FirstOrDefault(f => f.Key.Equals(request.ServiceKey) && !(f.Value.Reuse is SingletonReuse)).Value));

            container.Register<IService, Service>(Reuse.Singleton);
            var service = container.Resolve(typeof(IService), IfUnresolved.ReturnDefault);

            Assert.IsNull(service);
        }

        public static Func<ParameterInfo, ParameterServiceInfo> GetServiceInfoFromImportAttribute(Request request)
        {
            return parameter =>
            {
                var import = (ImportAttribute)parameter.GetAttributes(typeof(ImportAttribute)).FirstOrDefault();
                var details = import == null ? ServiceDetails.Default
                    : ServiceDetails.Of(import.ContractType, import.ContractName);
                return ParameterServiceInfo.Of(parameter).WithDetails(details);
            };
        }

        public static Func<ParameterInfo, ParameterServiceInfo> GetServiceFromWithMetadataAttribute(Request request)
        {
            return parameter =>
            {
                var import = (ImportWithMetadataAttribute)parameter.GetAttributes(typeof(ImportWithMetadataAttribute))
                    .FirstOrDefault();
                if (import == null)
                    return null;

                var registry = request.Container;
                var serviceType = parameter.ParameterType;
                serviceType = registry.GetWrappedType(serviceType, request.RequiredServiceType);
                var metadata = import.Metadata;
                var factory = registry.GetAllServiceFactories(serviceType)
                    .FirstOrDefault(kv => metadata.Equals(kv.Value.Setup.Metadata))
                    .ThrowIfNull();

                return ParameterServiceInfo.Of(parameter).WithDetails(ServiceDetails.Of(serviceType, factory.Key));
            };
        }

        [Test]
        public void Can_turn_Off_singleton_optimization()
        {
            var container = new Container(r => r.WithoutEagerCachingSingletonForFasterAccess());
            container.Register<FooHey>(Reuse.Singleton);

            var singleton = container.Resolve<LambdaExpression>(typeof(FooHey));

            // expression contains item creation delegate / lambda
            StringAssert.Contains("r => r.SingletonScope.GetOrAddViaFactoryDelegate", singleton.ToString());
        }

        internal class XX { }
        internal class YY { }
        internal class ZZ { }

        [Test]
        public void AutoFallback_resolution_rule_should_respect_IfUnresolved_policy_in_case_of_multiple_registrations()
        {
            var container = new Container()
                .WithAutoFallbackDynamicRegistrations(typeof(Me), typeof(MiniMe));

            var me = container.Resolve<IMe>(IfUnresolved.ReturnDefault);

            Assert.IsNull(me);
        }

        [Test]
        public void AutoFallback_resolution_rule_should_respect_IfUnresolved_policy_in_case_of_multiple_registrations_from_assemblies()
        {
            var container = new Container()
                .WithAutoFallbackDynamicRegistrations(typeof(Me).GetAssembly());

            var me = container.Resolve<IMe>(IfUnresolved.ReturnDefault);

            Assert.IsNull(me);
        }

        [Test]
        public void Can_specify_condition_to_exclude_unwanted_services_from_AutoFallback_resolution_rule()
        {
            var container = new Container()
                .WithAutoFallbackDynamicRegistrations(Reuse.Singleton,
                    Setup.With(condition: req => req.Parent.ImplementationType.Name.Contains("Green")),
                    typeof(Me));

            container.Register<RedMe>();

            var redMe = container.Resolve<RedMe>(IfUnresolved.ReturnDefault);
            Assert.IsNull(redMe);
        }

        public interface IMe { }
        internal class Me : IMe { }
        internal class MiniMe : IMe { }
        internal class GreenMe
        {
            public IMe Mee { get; set; }
            public GreenMe(IMe mee)
            {
                Mee = mee;
            }
        }

        internal class RedMe
        {
            public IMe Mee { get; set; }
            public RedMe(IMe mee)
            {
                Mee = mee;
            }
        }

        [Test]
        public void Exist_support_for_non_primitive_value_injection_via_container_rule()
        {
            var container = new Container(rules => rules.WithItemToExpressionConverter(
                (item, type) => type == typeof(ConnectionString)
                ? New(type.SingleConstructor(), Constant(((ConnectionString)item).Value))
                : null));

            var s = new ConnectionString("aaa");
            container.Register(Made.Of(() => new ConStrUser(Arg.Index<ConnectionString>(0)), r => s));

            var user = container.Resolve<ConStrUser>();
            Assert.AreEqual("aaa", user.S.Value);
        }

        [Test]
        public void Container_rule_for_serializing_custom_value_to_expression_should_throw_proper_exception_for_not_supported_type()
        {
            var container = new Container(rules => rules.WithThrowIfRuntimeStateRequired());

            var s = new ConnectionString("aaa");
            container.Register(Made.Of(() => new ConStrUser(Arg.Index<ConnectionString>(0)), r => s));

            var ex = Assert.Throws<ContainerException>(() => container.Resolve<ConStrUser>());
            Assert.AreEqual(Error.StateIsRequiredToUseItem, ex.Error);
        }

        public class ConnectionString
        {
            public string Value;
            public ConnectionString(string value)
            {
                Value = value;
            }
        }

        public class ConStrUser
        {
            public ConnectionString S { get; set; }
            public ConStrUser(ConnectionString s)
            {
                S = s;
            }
        }

        [Test]
        public void Container_should_throw_on_registering_disposable_transient()
        {
            var container = new Container();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Register<AD>());

            Assert.AreEqual(Error.RegisteredDisposableTransientWontBeDisposedByContainer, ex.Error);
        }

        [Test]
        public void I_can_silence_throw_on_registering_disposable_transient_for_specific_registration()
        {
            var container = new Container();

            Assert.DoesNotThrow(() =>
            container.Register<AD>(setup: Setup.With(allowDisposableTransient: true)));
        }

        [Test]
        public void I_can_silence_throw_on_registering_disposable_transient_for_whole_container()
        {
            var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());

            Assert.DoesNotThrow(() =>
            container.Register<AD>());
        }

        [Test]
        public void Should_track_transient_disposable_dependency_in_singleton_scope()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());

            container.Register<AD>();
            container.Register<ADConsumer>(Reuse.Singleton);
            var singleton = container.Resolve<ADConsumer>();

            container.Dispose();

            Assert.IsTrue(singleton.Ad.IsDisposed);
        }

        [Test]
        public void Should_not_track_func_of_transient_disposable_dependency_in_singleton_scope()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());

            container.Register<AD>();
            container.Register<ADFuncConsumer>(Reuse.Singleton);
            var singleton = container.Resolve<ADFuncConsumer>();

            container.Dispose();

            Assert.IsFalse(singleton.Ad.IsDisposed);
        }

        [Test]
        public void Should_track_lazy_of_transient_disposable_dependency_in_singleton_scope()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());

            container.Register<AD>();
            container.Register<ADLazyConsumer>(Reuse.Singleton);
            var singleton = container.Resolve<ADLazyConsumer>();

            container.Dispose();

            Assert.IsTrue(singleton.Ad.IsDisposed);
        }

        [Test]
        public void Should_track_transient_disposable_dependency_in_current_scope()
        {
            var container = new Container(rules => rules.WithTrackingDisposableTransients());

            container.Register<AD>();
            container.Register<ADConsumer>(Reuse.InCurrentScope);

            ADConsumer scoped;
            using (var scope = container.OpenScope())
            {
                scoped = scope.Resolve<ADConsumer>();
            }

            Assert.IsTrue(scoped.Ad.IsDisposed);
        }

        [Test]
        public void Should_track_transient_service_in_open_scope_if_present()
        {
            var container = new Container();
            container.Register<AD>(setup: Setup.With(trackDisposableTransient: true));

            AD ad;
            using (var scope = container.OpenScope())
                ad = scope.Resolve<AD>();

            Assert.IsTrue(ad.IsDisposed);
        }

        [Test]
        public void Tracked_disposables_should_be_different()
        {
            var container = new Container();
            container.Register<AD>(setup: Setup.With(trackDisposableTransient: true));

            using (var scope = container.OpenScope())
            {
                var ad = scope.Resolve<AD>();
                Assert.AreNotSame(ad, scope.Resolve<AD>());
            }
        }

        [Test]
        public void Should_track_transient_service_in_open_scope_of_any_name_if_present()
        {
            var container = new Container();
            container.Register<AD>(setup: Setup.With(trackDisposableTransient: true));

            AD ad;
            using (var scope = container.OpenScope("hey"))
                ad = scope.Resolve<AD>();

            Assert.IsTrue(ad.IsDisposed);
        }

        [Test]
        public void Should_track_transient_service_in_singleton_scope_if_no_open_scope()
        {
            var container = new Container();
            container.Register<AD>(setup: Setup.With(trackDisposableTransient: true));

            var ad = container.Resolve<AD>();

            container.Dispose();
            Assert.IsTrue(ad.IsDisposed);
        }

        public class AD : IDisposable
        {
            public bool IsDisposed { get; private set; }

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

        public class ADFuncConsumer
        {
            public AD Ad { get; }

            public ADFuncConsumer(Func<AD> ad)
            {
                Ad = ad();
            }
        }

        public class ADLazyConsumer
        {
            public AD Ad { get; }

            public ADLazyConsumer(Lazy<AD> ad)
            {
                Ad = ad.Value;
            }
        }

        [Test]
        public void Can_specify_IfAlreadyRegistered_per_Container()
        {
            var container = new Container(rules => rules
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Keep));

            container.Register<I, A>();
            container.Register<I, B>();

            var i = container.Resolve<I>();

            Assert.IsInstanceOf<A>(i);
        }

        [Test]
        public void If_IfAlreadyRegistered_per_Container_is_overriden_by_individual_registrations()
        {
            var container = new Container(rules => rules
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Keep));

            container.Register<I, A>();
            container.Register<I, B>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            var i = container.Resolve<I>();

            Assert.IsInstanceOf<B>(i);
        }

        [Test]
        public void If_IfAlreadyRegistered_per_Container_affects_RegisterMany_as_expected()
        {
            var container = new Container(rules => rules
                .WithDefaultIfAlreadyRegistered(IfAlreadyRegistered.Keep));

            container.RegisterMany(new[] { typeof(A), typeof(B) });

            var i = container.Resolve<I>();

            Assert.IsInstanceOf<A>(i);
        }

        [Test]
        public void Can_specify_to_capture_stack_trace_and_display_it_disposed_exception()
        {
            var container = new Container(rules => rules
                .WithCaptureContainerDisposeStackTrace());

            var scope = container.OpenScope();
            scope.Dispose();

            var ex = Assert.Throws<ContainerException>(() =>
                scope.Resolve<string>());

            Assert.AreEqual(Error.NameOf(Error.ContainerIsDisposed), Error.NameOf(ex.Error));

            StringAssert.Contains("stack-trace", ex.Message);
        }

        [Test]
        public void DisposedContainer_error_message_should_include_tip_how_to_enable_stack_trace()
        {
            var container = new Container();
            container.Dispose();

            var ex = Assert.Throws<ContainerException>(() =>
                container.Resolve<string>());

            Assert.AreEqual(Error.NameOf(Error.ContainerIsDisposed), Error.NameOf(ex.Error));
            StringAssert.Contains("WithCaptureContainerDisposeStackTrace", ex.Message);
        }

        #region CUT

        public interface I { }

        public class A : I { }

        public class B : I { }

        public class SomeService { }

        public class Bla<T>
        {
            public string Message { get; set; }
            public Func<T> Factory { get; set; }

            public Bla(string message)
            {
                Message = message;
            }

            public Bla(Func<T> factory)
            {
                Factory = factory;
            }
        }

        enum FooMetadata { Hey, Blah }

        public interface IFooService
        {
        }

        public class FooHey : IFooService
        {
        }

        public class FooBlah : IFooService
        {
        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class ImportWithMetadataAttribute : Attribute
        {
            public ImportWithMetadataAttribute(object metadata)
            {
                Metadata = metadata.ThrowIfNull();
            }

            public readonly object Metadata;
        }

        public class FooConsumer
        {
            public Lazy<IFooService> Foo { get; set; }

            public FooConsumer([ImportWithMetadata(FooMetadata.Blah)] Lazy<IFooService> foo)
            {
                Foo = foo;
            }
        }

        public class TransientOpenGenericService<T>
        {
            public T Value { get; set; }
        }

        public interface INamedService
        {
        }

        public class NamedService : INamedService
        {
        }

        public class AnotherNamedService : INamedService
        {
        }

        public class ServiceWithImportedCtorParameter
        {
            public INamedService NamedDependency { get; set; }

            public ServiceWithImportedCtorParameter([Import("blah")]INamedService namedDependency)
            {
                NamedDependency = namedDependency;
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class NotRegisteredService
        {
        }

        #endregion
    }
}