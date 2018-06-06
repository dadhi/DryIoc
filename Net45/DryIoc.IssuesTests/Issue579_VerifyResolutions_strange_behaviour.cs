using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue579_VerifyResolutions_strange_behaviour
    {
        [Test]
        public void Test()
        {
            var container = new Container(rules => rules.WithoutThrowIfDependencyHasShorterReuseLifespan());
            container.Register<FiosEntities>(Reuse.InWebRequest, setup: Setup.With(openResolutionScope: true));
            var result = container.Validate();


        }
    }

    public class FiosEntities { }
}
