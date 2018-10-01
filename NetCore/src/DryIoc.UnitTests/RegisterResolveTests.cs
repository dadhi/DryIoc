using NUnit.Framework;

namespace DryIoc.NetCore.UnitTests
{
    [TestFixture]
    public class RegisterResolveTests
    { 
        [Test]
        public void Register_and_Resolve_should_work()
        {
            var container = new Container(Rules.Default.WithAutoConcreteTypeResolution());

            var a = container.Resolve<A>();

            Assert.NotNull(a);
        }

        public class A {}
    }
}
