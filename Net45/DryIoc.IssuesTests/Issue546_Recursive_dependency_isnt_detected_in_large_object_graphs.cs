using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue546_Recursive_dependency_isnt_detected_in_large_object_graphs
    {
        [Test, Ignore("Fails")]
        public void Circular_dependencies_in_large_object_graphs_should_be_detected()
        {
            // ordinary registration — simulate large object graph by lowering the max size
            var nonLazyContainer = new Container().WithMef().With(r => r.WithMaxObjectGraphSize(1));
            nonLazyContainer.RegisterExports(new[] { typeof(Issue546_Recursive_dependency_isnt_detected_in_large_object_graphs).Assembly });

            // check that importing as non-lazy actually detects the circular dependency
            Assert.Throws<ContainerException>(() =>
            {
                var cmds = nonLazyContainer.Resolve<CircularDependencyRoot>();
                Assert.IsNotNull(cmds.Service);
            });
        }

        [Test, Ignore("Fails")]
        public void Circular_dependencies_in_large_object_graphs_should_be_detected_even_for_dynamic_registrations()
        {
            // register dynamically
            var assembly = typeof(Issue546_Recursive_dependency_isnt_detected_in_large_object_graphs).Assembly;
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
            // simulate large object graph by lowering the max size
            var container = new Container().WithMef()
                .With(rules => rules.WithDynamicRegistrations(dynamicRegistrations)
                .WithMaxObjectGraphSize(1));

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
