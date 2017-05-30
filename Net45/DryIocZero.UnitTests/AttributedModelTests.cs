namespace DryIocZero.UnitTests
{
    using DryIoc.MefAttributedModel.UnitTests.CUT;
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
            var ex = Assert.Throws<ContainerException>(
                () => _container.Resolve<ServiceWithMultipleCostructors>());

            Assert.AreEqual(DryIocZero.Error.UnableToResolveDefaultService, ex.Error);
        }

        [Test]
        public void Export_condition_should_be_evaluated()
        {
            Assert.IsInstanceOf<ExportConditionalObject1>(
                _container.Resolve<ImportConditionObject1>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject2>(
                _container.Resolve<ImportConditionObject2>().ExportConditionInterface);
            Assert.IsInstanceOf<ExportConditionalObject3>(
                _container.Resolve<ImportConditionObject3>().ExportConditionInterface);
        }

        [Test]
        public void Can_specify_all_Register_options_for_export()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());

            IAllOpts opts;
            using (var s = container.OpenScope("b"))
            {
                opts = s.Resolve<IAllOpts>(serviceKey: "a");
                Assert.IsNotNull(opts);
                Assert.AreSame(opts, s.Resolve<IAllOpts>(serviceKey: "a"));

                IAllOpts opts2;
                using (var ss = s.OpenScope())
                {
                    opts2 = ss.Resolve<IAllOpts>(serviceKey: "a");
                    Assert.AreNotSame(opts, opts2);
                }

                Assert.IsTrue(((AllOpts)opts2).IsDisposed);
            }

            Assert.IsTrue(((AllOpts)opts).IsDisposed);
            container.Dispose();
        }

        [Test]
        public void I_can_check_existense_of_open_scope()
        {
            using (var s = _container.OpenScope("a"))
            {
                Assert.IsNotNull(s.GetCurrentScope());
            }
        }

        [Test]
        public void Should_throw_if_no_open_scope()
        {
            var ex = Assert.Throws<ContainerException>(() =>
                _container.Resolve<NamedScopeService>());

            Assert.AreEqual(Error.NoCurrentScope, ex.Error);
        }

        [Test]
        public void Should_throw_if_matching_scope_is_not_found()
        {
            using (var s = _container.OpenScope())
            {
                var ex = Assert.Throws<ContainerException>(() => 
                    s.Resolve<NamedScopeService>());

                Assert.AreEqual(Error.NoMatchedScopeFound, ex.Error);
            }
        }
    }
}
