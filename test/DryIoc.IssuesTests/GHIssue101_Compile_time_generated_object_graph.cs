using NUnit.Framework;
using Example;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue101_Compile_time_generated_object_graph : ITest
    {
        public int Run()
        {
            Resolve_compile_time_generated_example_service();
            Emulate_compile_time_generated_example_service_in_runtime();
            return 2;
        }

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

            var exprs = container.GenerateResolutionExpressions(ServiceInfo.Of<IService>());
            Assert.AreEqual(0, exprs.Errors.Count);
            Assert.AreEqual(1, exprs.Roots.Count);
            Assert.AreEqual(typeof(IService), exprs.Roots[0].Key.ServiceType);
            Assert.AreEqual(1, exprs.ResolveDependencies.Count);
            Assert.AreEqual(typeof(DependencyB<string>), exprs.ResolveDependencies[0].Key.ServiceType);

            container.Register<Example.RuntimeDependencyC>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var x = container.Resolve<Example.IService>();

            Assert.IsNotNull(x);
        }
    }
}
