using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue541_Dynamic_Registrations_dont_detect_circular_dependencies
    {
        [Test]
        public void Lazy_import_should_detect_circular_dependencies()
        {
            // ordinary registration
            var nonLazyContainer = new Container().WithMef();
            nonLazyContainer.RegisterExports(new[] { typeof(Issue541_Dynamic_Registrations_dont_detect_circular_dependencies).Assembly });

            // check that importing as non-lazy actually detects the circular dependency
            Assert.Throws<ContainerException>(() =>
            {
                var cmds = nonLazyContainer.Resolve<CircularDependencyRoot>();
                Assert.IsNotNull(cmds.Service);
            });

            // register dynamically
            var assembly = typeof(Issue541_Dynamic_Registrations_dont_detect_circular_dependencies).Assembly;
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

            // Test that dynamic resolution also detects the circular dependency
            //==================================================================
            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations));

            // make sure that CircularDependencyRoot itself is available without loading the lazy assembly
            container.RegisterExports(typeof(CircularDependencyRoot));
            Assert.Throws<ContainerException>(() => container.Resolve<CircularDependencyRoot>());
        }

        [Export]
        public class CircularDependencyRoot
        {
            [Import]
            public IFirstLevelDependency Service { get; set; }
        }

        public interface IFirstLevelDependency { }

        [Export(typeof(IFirstLevelDependency))]
        public class FirstLevelDependency : IFirstLevelDependency
        {
            [Import]
            public ISecondLevelDependency Service { get; set; }
        }

        public interface ISecondLevelDependency { }

        [Export(typeof(ISecondLevelDependency))]
        public class SecondLevelDependency : ISecondLevelDependency
        {
            [Import]
            public IFirstLevelDependency Service { get; set; }
        }
    }
}
