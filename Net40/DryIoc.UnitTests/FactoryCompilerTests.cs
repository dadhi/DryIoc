using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class FactoryCompilerTests
    {
        [Test]
        public void Container_with_enabled_compilation_to_DynamicAssembly()
        {
            var container = new Container(rules => rules.WithFactoryDelegateCompilationToDynamicAssembly());
            container.Register<InternalService>();

            var service = container.Resolve<InternalService>();

            Assert.That(service, Is.Not.Null);
        }

        internal class InternalService { }
    }
}
