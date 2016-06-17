using Xunit;

namespace DryIoc.NetCore.UnitTests
{
    public class RegisterResolveTests
    { 
        [Fact]
        public void Register_and_Resolve_should_work()
        {
            var container = new Container(Rules.Default.WithAutoConcreteTypeResolution());

            var a = container.Resolve<A>();

            Assert.NotNull(a);
        }

        public class A {}
    }
}
