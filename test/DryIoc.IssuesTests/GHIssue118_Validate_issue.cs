using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue118_Validate_issue
    {
        [Test]
        public void Test_without_Validate()
        {
            using (var container = new Container())
            {
                var outerScoped = Reuse.ScopedTo("Outer");
                container.Register<SomethingDefinedInOuterScope>(outerScoped);
                
                var innerScoped = Reuse.ScopedTo("Inner", false, outerScoped.Lifespan - 1);
                container.Register<SomethingDefinedInInnerScope>(innerScoped);
                //container.Validate();

                using (var outerScopedContainer = container.OpenScope(outerScoped.Name))
                {
                    using (var innerScopedContainer = outerScopedContainer.OpenScope(innerScoped.Name))
                    {
                        var ex = Assert.Throws<ContainerException>(() => 
                            innerScopedContainer.Resolve<SomethingDefinedInOuterScope>());
                            Assert.AreEqual(Error.NameOf(Error.DependencyHasShorterReuseLifespan), ex.ErrorName);
                    }
                }
            }
        }

        [Test]
        public void Test_with_Validate()
        {
            using (var container = new Container())
            {
                var outerScoped = Reuse.ScopedTo("Outer");
                container.Register<SomethingDefinedInOuterScope>(outerScoped);
                
                var innerScoped = Reuse.ScopedTo("Inner", false, outerScoped.Lifespan - 1);
                container.Register<SomethingDefinedInInnerScope>(innerScoped);
                
                var errors = container.Validate();
                Assert.AreEqual(1, errors.Length);
                Assert.AreEqual(
                    Error.NameOf(Error.DependencyHasShorterReuseLifespan),
                    errors[0].Value.ErrorName);
            }
        }

        class SomethingDefinedInOuterScope
        {
            public SomethingDefinedInOuterScope(SomethingDefinedInInnerScope dependency) // we were expecting DryIoC to not allow that, it's wrong since the dependency has a tighter lifetime
            {
            }
        }

        class SomethingDefinedInInnerScope
        {
        }
    }
}
