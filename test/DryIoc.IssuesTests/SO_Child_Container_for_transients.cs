using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class SO_Child_Container_for_transients : ITest
    {
        public int Run()
        {
            ScopedTransientDryIOC();
            return 1;
        }

        [Test]
        public void ScopedTransientDryIOC()
        {
            var container = new Container(scopeContext: new AsyncExecutionFlowScopeContext());
            
            container.Register<IFeature, StandardFeature>(reuse: Reuse.Transient);

            var standardFeature = container.Resolve<IFeature>();

            Assert.That(standardFeature, Is.TypeOf<StandardFeature>(), "In container");

            using (var child = container.CreateChild(IfAlreadyRegistered.Replace))
            {
                // Without overriding resolves the standard implementation from the parent scope
                var customizedFeature = container.Resolve<IFeature>();
                Assert.That(
                    customizedFeature,
                    Is.TypeOf<StandardFeature>(),
                    "In OpenScope 1st level, before local registration");

                child.Register<IFeature, CustomizedFeature>(reuse: Reuse.Transient);

                // After overriding it should resolve the customized implementation
                customizedFeature =  child.Resolve<IFeature>();
                Assert.That(
                    customizedFeature,
                    Is.TypeOf<CustomizedFeature>(),
                    "In OpenScope 1st level, after local registration");
            }

            // When the overriding scope is disposed resolve again the standard implementation from the root scope
            var standardFeatureClosed = container.Resolve<IFeature>();
            Assert.That(standardFeatureClosed, Is.TypeOf<StandardFeature>(), "After 1st level scope disposed");
        }

        public interface IFeature { }
        public class StandardFeature : IFeature { }
        public class CustomizedFeature : IFeature { }
    }
}
