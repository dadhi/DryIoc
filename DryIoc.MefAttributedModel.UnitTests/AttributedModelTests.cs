using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DryIoc.MefAttributedModel.UnitTests.CUT;
using DryIocAttributes;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests
{
    [TestFixture]
    public class AttributedModelTests
    {
        [Test]
        public void I_can_resolve_service_with_dependencies()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<DependentService>();

            Assert.That(service.TransientService, Is.Not.Null);
            Assert.That(service.SingletonService, Is.Not.Null);
            Assert.That(service.TransientOpenGenericService, Is.Not.Null);
            Assert.That(service.OpenGenericServiceWithTwoParameters, Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_transient_service()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<ITransientService>();
            var anotherService = _container.Resolve<ITransientService>();

            Assert.That(service, Is.Not.Null.And.Not.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_singleton_service()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<ISingletonService>();
            var anotherService = _container.Resolve<ISingletonService>();

            Assert.That(service, Is.Not.Null.And.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_singleton_open_generic_service()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<IOpenGenericService<int>>();
            var anotherService = _container.Resolve<IOpenGenericService<int>>();

            Assert.That(service, Is.Not.Null.And.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_transient_open_generic_service()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<TransientOpenGenericService<object>>();
            var anotherService = _container.Resolve<TransientOpenGenericService<object>>();

            Assert.That(service, Is.Not.Null.And.Not.SameAs(anotherService));
        }

        [Test]
        public void I_can_resolve_open_generic_service_with_two_parameters()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<OpenGenericServiceWithTwoParameters<int, string>>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_service_factory()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var serviceFactory = _container.Resolve<Func<ITransientService>>();

            Assert.That(serviceFactory(), Is.Not.Null);
        }

        [Test]
        public void I_can_resolve_array_of_func_with_one_parameter()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var factories = _container.Resolve<Func<string, IServiceWithMultipleImplentations>[]>();
            Assert.That(factories.Length, Is.EqualTo(2));

            var oneService = factories[0].Invoke("0");
            Assert.That(oneService.Message, Is.EqualTo("0"));

            var anotherService = factories[1].Invoke("1");
            Assert.That(anotherService.Message, Is.EqualTo("1"));
        }

        [Test]
        public void I_can_resolve_meta_factory_many()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var factories = _container.Resolve<Meta<Func<IServiceWithMetadata>, IViewMetadata>[]>();
            Assert.That(factories.Length, Is.EqualTo(3));

            var factory = factories.First(meta => meta.Metadata.DisplayName.Equals("Down"));
            var service = factory.Value();
            Assert.That(service, Is.InstanceOf<AnotherServiceWithMetadata>());

            var anotherService = factory.Value();
            Assert.That(anotherService, Is.Not.SameAs(service));
        }

        [Test]
        public void Container_can_be_setup_to_select_one_constructor_based_on_attribute()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var service = _container.Resolve<ServiceWithMultipleCostructorsAndOneImporting>();

            Assert.That(service.Transient, Is.Not.Null);
            Assert.That(service.Singleton, Is.Null);

        }

        [Test]
        public void Service_with_metadata_can_be_resolved_without_name()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            Assert.DoesNotThrow(
                () => _container.Resolve<SingleServiceWithMetadata>());
        }

        [Test]
        public void Registering_service_with_metadata_provided_with_multiple_attributes_should_fail_Cause_of_underministic_behavior()
        {
            var container = new Container();

            var ex = Assert.Throws<AttributedModelException>(() =>
                container.RegisterExports(typeof(OneWithManyMeta)));
            Assert.AreEqual(ex.Error, Error.UnsupportedMultipleMetadata);

        }

        [Test]
        public void Resolving_service_with_multiple_constructors_without_importing_attribute_should_fail()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            var ex = Assert.Throws<AttributedModelException>(
                () => _container.Resolve<ServiceWithMultipleCostructors>());
            Assert.AreEqual(ex.Error, Error.NoSingleCtorWithImportingAttr);
        }

        [Test]
        public void Importing_LazyEnumerable()
        {
            var container = new Container(r => r.WithMefAttributedModel().WithResolveIEnumerableAsLazyEnumerable());
            container.RegisterExports(typeof(UseLazyEnumerable), typeof(Me));

            Assert.IsInstanceOf<LazyEnumerable<Me>>(
                container.Resolve<UseLazyEnumerable>().Mes);
        }

        [Test]
        public void Export_as_new_resolution_scope_dependency()
        {
            var container = new Container(r => r.WithMefAttributedModel());
            container.RegisterExports(typeof(LazyDepClient), typeof(LazyDep));

            var clientExpr = container.Resolve<LambdaExpression>(typeof(LazyDepClient));

            StringAssert.Contains(".Resolve", clientExpr.ToString());
        }

        [Test]
        public void Export_condition_should_be_evaluated()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            Assert.IsInstanceOf<ExportConditionalObject1>(_container.Resolve<ImportConditionObject1>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject2>(_container.Resolve<ImportConditionObject2>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject3>(_container.Resolve<ImportConditionObject3>().ExportConditionInterface);
        }

        [Test]
        public void I_can_mark_exports_to_be_injected_as_resolution_roots()
        {
            var container = new Container(r => r.WithMefAttributedModel());
            container.RegisterExports(typeof(A), typeof(B));

            var b = container.Resolve<LambdaExpression>(typeof(B));

            Assert.That(b.ToString(), Is.StringContaining("Resolve(DryIoc.MefAttributedModel.UnitTests.CUT.A"));
        }

        [Test, Explicit("Exporting nested open-generic class - not supported from the get go. Requires evaluation.")]
        public void Can_register_open_generic_returned_by_factory_method_nested_in_open_generic_class()
        {
            GivenAssemblyWithExportedTypes();
            WhenIRegisterAllExportedTypes();

            _container.Resolve<Aaa<A2>>();
        }

        #region Implementation

        private void WhenIRegisterAllExportedTypes()
        {
            _container = new Container().WithMefAttributedModel();
            _container.RegisterExports(new[] { _assembly });
        }

        private void GivenAssemblyWithExportedTypes()
        {
            _assembly = typeof(DependentService).GetAssembly();
        }

        private Assembly _assembly;

        private IContainer _container;

        #endregion
    }

    [ExportWithDisplayName("blah"), WithMetadata("hey")]
    public class OneWithManyMeta { }
}

