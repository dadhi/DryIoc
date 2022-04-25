using NUnit.Framework;
using Example;

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

        [Test]
        public void Emulate_compile_time_generated_example_service_in_runtime()
        {
            var container = new Container();

            container.Register<IService, MyService>();
            container.Register<IDependencyA, DependencyA>();

            container.Register(typeof(DependencyB<>), setup: Setup.With(asResolutionCall: true));

            container.RegisterPlaceholder<RuntimeDependencyC>();

            container.Register<Example.RuntimeDependencyC>();

            var x = container.Resolve<Example.IService>();

            Assert.IsNotNull(x);
        }
    }
}
