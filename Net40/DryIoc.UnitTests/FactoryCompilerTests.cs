using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FactoryCompilerTests
    {
        [Test]
        public void Container_with_enabled_compilation_to_DynamicAssembly()
        {
            var container = new Container(ResolutionRules.Default.EnableCompilationToDynamicAssembly(true));
            container.Register<InternalService>();

            // Exception is here because internal ServiceConsumer is not visible to created Dynamic Assembly.
            Assert.Throws<MethodAccessException>(() =>
                container.Resolve<InternalService>());
        }

        [Test]
        public void Container_with_disabled_compilation_to_DynamicAssembly()
        {
            var container = new Container(ResolutionRules.Default.EnableCompilationToDynamicAssembly(false));
            container.Register<InternalService>();

            var service = container.Resolve<InternalService>();

            Assert.That(service, Is.Not.Null);
        }

        internal class InternalService { }
    }
}
