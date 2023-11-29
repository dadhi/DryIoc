using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue543_Dynamic_Registrations_dont_respect_shared_creation_policy : ITest
    {
        public int Run()
        {
            DryIoc_normal_import_respects_shared_creation_policy();
            DryIoc_lazy_import_should_respect_shared_creation_policy_via_Resolve();
            DryIoc_lazy_import_should_respect_shared_creation_policy_via_InjectPropertiesAndFields();
            return 3;
        }

        [Test]
        public void DryIoc_normal_import_respects_shared_creation_policy()
        {
            // ordinary registration, container uses transient reuse by default
            var nonLazyContainer = new Container().WithMef().With(r => r.WithDefaultReuse(Reuse.Transient));
            nonLazyContainer.RegisterExports(new[] { typeof(Issue543_Dynamic_Registrations_dont_respect_shared_creation_policy).Assembly });

            var exp1 = nonLazyContainer.Resolve<IBaseService>();
            using (var scope = nonLazyContainer.OpenScope())
            {
                var exp2 = scope.Resolve<IDerivedService>();
                Assert.AreEqual(exp1.InstanceID, exp2.InstanceID);
            }

            var use1 = new UseBaseService();
            nonLazyContainer.InjectPropertiesAndFields(use1);
            Assert.AreEqual(exp1.InstanceID, use1.Service.InstanceID);

            var use2 = new UseDerivedService();
            nonLazyContainer.InjectPropertiesAndFields(use2);
            Assert.AreEqual(exp1.InstanceID, use2.Service.InstanceID);
        }

        private IContainer CreateContainerWithDynamicRegistrations()
        {
            // dynamic registration
            var assembly = typeof(Issue543_Dynamic_Registrations_dont_respect_shared_creation_policy).Assembly;
            var registrations = AttributedModel.Scan(new[] { assembly });
            var lazyRegistrations = registrations.MakeLazyAndEnsureUniqueServiceKeys();

            // use shared service exports to compose multiple providers
            var serviceExports = new Dictionary<string, IList<KeyValuePair<object, ExportedRegistrationInfo>>>();

            // create a separate DynamicRegistrationProvider for each lazy registration
            // to simulate that each ICommand is located in a different assembly
            var dynamicRegistrations = lazyRegistrations
                .Select(r => new[] { r }
                    .GetLazyTypeRegistrationProvider(
                        otherServiceExports: serviceExports,
                        typeProvider: t => assembly.GetType(t)))
                .ToArray();

            return new Container().WithMef()
                .With(r => r.WithDefaultReuse(Reuse.Transient).WithDynamicRegistrations(dynamicRegistrations));
        }

        [Test]
        public void DryIoc_lazy_import_should_respect_shared_creation_policy_via_Resolve()
        {
            var container = CreateContainerWithDynamicRegistrations();
            var exp1 = container.Resolve<IBaseService>();
            using (var scope = container.OpenScope())
            {
                var exp2 = scope.Resolve<IDerivedService>();
                Assert.AreEqual(exp1.InstanceID, exp2.InstanceID);
            }
        }

        [Test]
        public void DryIoc_lazy_import_should_respect_shared_creation_policy_via_InjectPropertiesAndFields()
        {
            // make sure that UseBaseService and UseDerivedService classes are available without loading the lazy assembly
            var container = CreateContainerWithDynamicRegistrations();
            container.RegisterExports(typeof(UseBaseService), typeof(UseDerivedService));

            var use1 = new UseBaseService();
            container.InjectPropertiesAndFields(use1);

            var exp1 = container.Resolve<IBaseService>();
            Assert.AreEqual(exp1.InstanceID, use1.Service.InstanceID);

            var use2 = new UseDerivedService();
            container.InjectPropertiesAndFields(use2);
            Assert.AreEqual(exp1.InstanceID, use2.Service.InstanceID);
        }

        public interface IBaseService { Guid InstanceID { get; } }

        public interface IDerivedService : IBaseService { }

        [Export(typeof(IBaseService))]
        [Export(typeof(IDerivedService))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        public class DerivedService : IDerivedService, IBaseService
        {
            public Guid InstanceID { get; } = Guid.NewGuid();
        }

        public class UseBaseService
        {
            [Import] public IBaseService Service { get; set; }
        }

        public class UseDerivedService
        {
            [Import] public IDerivedService Service { get; set; }
        }
    }
}
