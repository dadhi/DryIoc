using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue297_Can_RegisterMany_ignore_already_registered_services_based_on_reuse
    {
        [Test]
        public void RegisterMany_may_skip_IsRegistered_as_Scoped()
        {
            var c = new Container();

            c.Register<A>(Reuse.Scoped);    // should be kept
            c.Register<B>(Reuse.Singleton); // should be replaced by RegisterMany below

            var implTypes = new[] {typeof(A), typeof(B)}; // or can be loaded from Assembly

            c.RegisterMany(implTypes, Reuse.Transient, nonPublicServiceTypes: true,
                serviceTypeCondition: t => !c.IsRegistered(t, condition: factory => factory.Reuse == Reuse.Scoped),
                ifAlreadyRegistered: IfAlreadyRegistered.Replace);

            Assert.IsTrue(c.IsRegistered<A>(condition: f => f.Reuse == Reuse.Scoped));
            Assert.IsTrue(c.IsRegistered<B>(condition: f => f.Reuse == Reuse.Transient)); // replaced
        }

        class A {}
        class B {}
    }
}
