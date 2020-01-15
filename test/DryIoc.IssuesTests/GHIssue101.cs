using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue101
    {
        [Test]
        public void Resolve_compile_time_generated_example_service()
        {
            var c = new Container();
            c.Register<Example.RuntimeDependencyC>();
            
            var x = c.Resolve<Example.IService>();

            Assert.IsNotNull(x);
        }
    }
}
