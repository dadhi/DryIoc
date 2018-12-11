using System.Collections.Generic;

namespace DryIoc.MefAttributedModel.UnitTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using CUT;
    using NUnit.Framework;

    [TestFixture]
    public class AttributedModelTests : AttributedModelTestsBase
    {
        [Test]
        public void I_can_resolve_service_with_dependencies()
        {
            var service = _container.Resolve<DependentService>();

            Assert.That(service.TransientService, Is.Not.Null);
            Assert.That(service.SingletonService, Is.Not.Null);
            Assert.That(service.TransientOpenGenericService, Is.Not.Null);
            Assert.That(service.OpenGenericServiceWithTwoParameters, Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_transient_service()
        {
            var service = _container.Resolve<ITransientService>();
            var anotherService = _container.Resolve<ITransientService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.Not.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_singleton_service()
        {
            var service = _container.Resolve<ISingletonService>();
            var anotherService = _container.Resolve<ISingletonService>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_singleton_open_generic_service()
        {
            var service = _container.Resolve<IOpenGenericService<int>>();
            var anotherService = _container.Resolve<IOpenGenericService<int>>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_transient_open_generic_service()
        {
            var service = _container.Resolve<TransientOpenGenericService<object>>();
            var anotherService = _container.Resolve<TransientOpenGenericService<object>>();

            Assert.That(service, Is.Not.Null);
            Assert.That(service, Is.Not.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_open_generic_service_with_two_parameters()
        {
            var service = _container.Resolve<OpenGenericServiceWithTwoParameters<int, string>>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_service_factory()
        {
            var serviceFactory = _container.Resolve<Func<ITransientService>>();

            Assert.That(serviceFactory(), Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_array_of_func_with_one_parameter()
        {
            var factories = _container.Resolve<Func<string, IServiceWithMultipleImplementations>[]>();
            Assert.That(factories.Length, Is.EqualTo(2));

            var oneService = factories[0].Invoke("0");
            Assert.That(oneService.Message, Is.EqualTo("0"));

            var anotherService = factories[1].Invoke("1");
            Assert.That(anotherService.Message, Is.EqualTo("1"));
        }

        [Test]
        public void I_can_resolve_meta_factory_many()
        {
            var factories = _container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
            Assert.That(factories.Length, Is.EqualTo(3));

            var factory = factories.First(meta => meta.Metadata.DisplayName.Equals("Down"));
            var service = factory.Value();
            Assert.IsInstanceOf<AnotherServiceWithMetadata>(service);

            var anotherService = factory.Value();
            Assert.That(anotherService, Is.Not.SameAs(service));
        }

        [Test]
        public void Container_can_be_setup_to_select_one_constructor_based_on_attribute()
        {
            var service = _container.Resolve<ServiceWithMultipleCostructorsAndOneImporting>();

            Assert.That(service.Transient, Is.Not.Null);
            Assert.That(service.Singleton, Is.Null);

        }

        [Test]
        public void Service_with_metadata_can_be_resolved_without_name()
        {
            Assert.DoesNotThrow(
                () => _container.Resolve<SingleServiceWithMetadata>());
        }

        [Test]
        public void Resolving_service_with_multiple_constructors_without_importing_attribute_should_fail()
        {
            var ex = Assert.Throws<AttributedModelException>(
                () => _container.Resolve<ServiceWithMultipleCostructors>());
            Assert.AreEqual(ex.Error, Error.NoSingleCtorWithImportingAttr);
        }

        [Test]
        public void Export_as_new_resolution_scope_dependency()
        {
            var clientExpr = _container.Resolve<LambdaExpression>(typeof(LazyDepClient));

            StringAssert.Contains(".Resolve", clientExpr.ToString());
        }

        [Test]
        public void Export_condition_should_be_evaluated()
        {
            Assert.IsInstanceOf<ExportConditionalObject1>(_container.Resolve<ImportConditionObject1>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject2>(_container.Resolve<ImportConditionObject2>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject3>(_container.Resolve<ImportConditionObject3>().ExportConditionInterface);
        }

        [Test]
        public void I_can_mark_exports_to_be_injected_as_resolution_roots()
        {
            var b = _container.Resolve<LambdaExpression>(typeof(B));

            StringAssert.Contains("Resolve(DryIoc.MefAttributedModel.UnitTests.CUT.A", b.ToString());
        }

        [Test]
        public void Can_register_open_generic_returned_by_factory_method_nested_in_open_generic_class()
        {
            Assert.IsNotNull(_container.Resolve<Daah.Fooh<A1>>(serviceKey: "a"));
            Assert.IsNotNull(_container.Resolve<Daah.Fooh<A1>>(serviceKey: "b"));
        }

        [Test]
        public void Can_specify_all_Register_options_for_export()
        {
            IAllOpts opts;
            using (var s = _container.OpenScope("b"))
            {
                opts = s.Resolve<IAllOpts>(serviceKey: "a");
                Assert.IsNotNull(opts);
            }

            Assert.IsTrue(((AllOpts)opts).IsDisposed);
        }

        [Test]
        public void ExportedRegistrationInfo_has_non_null_metadata_provided_by_WithMetadataAttribute()
        {
            var regs = AttributedModel.Scan(new[] { typeof(WithWithMetadataOnlyKeyValue).GetAssembly() });
            var reg = regs.Single(r => r.ImplementationType == typeof(WithWithMetadataOnlyKeyValue));
            Assert.IsNotNull(reg.GetSetup().Metadata);
            Assert.IsTrue(reg.GetSetup().MatchesMetadata("a", 1));
        }

        [Test]
        public void ExportedRegistrationInfo_has_non_null_metadata_provided_by_ExportMetadataAttribute()
        {
            var regs = AttributedModel.Scan(new[] { typeof(WithExportMetadataOnlyKeyValue).GetAssembly() });
            var reg = regs.Single(r => r.ImplementationType == typeof(WithExportMetadataOnlyKeyValue));
            Assert.IsNotNull(reg.GetSetup().Metadata);
            Assert.IsTrue(reg.GetSetup().MatchesMetadata("b", 2));
        }

        [Test]
        public void Can_resolve_export_with_WithMetadata_only_metadata()
        {
            var it = _container.Resolve<Meta<WithWithMetadataOnlyKeyValue, IDictionary<string, object>>>();

            CollectionAssert.AreEquivalent(new[] { "a" }, it.Metadata.Keys);
            CollectionAssert.AreEquivalent(new[] { 1 }, it.Metadata.Values);
        }

        [Test]
        public void Can_resolve_export_with_ExportAttribute_only_metadata()
        {
            var it = _container.Resolve<Meta<WithExportMetadataOnlyKeyValue, IDictionary<string, object>>>();

            CollectionAssert.AreEquivalent(new[] { "b" }, it.Metadata.Keys);
            CollectionAssert.AreEquivalent(new[] { 2 }, it.Metadata.Values);
        }

        [Test]
        public void Can_resolve_export_with_multi_metadata()
        {
            var it = _container.Resolve<Meta<WithMetaKeyValue, IDictionary<string, object>>>();

            CollectionAssert.AreEquivalent(new[] { "a", "b" }, it.Metadata.Keys);
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, it.Metadata.Values);
        }
    }
}
