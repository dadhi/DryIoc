using System.Linq;
using NUnit.Framework;

namespace DryIoc.MefAttributedModel.UnitTests.Playground
{
    [TestFixture]
    public class AutoWiringTests
    {
        [Test]
        public void Discover_and_register_new_plugins_from_assembly()
        {
            var container = new Container();
            var plugins = container.Resolve<Many<IPlugin>>();
            Assert.That(plugins.Items.Count(), Is.EqualTo(0));

            var pluginAssembly = typeof(AutoWiringTests).Assembly;

            var pluginTypes = pluginAssembly.GetTypes().Where(t => t.IsPublic && !t.IsAbstract 
                && t.GetImplementedTypes().Contains(typeof(IPlugin)));

            foreach (var pluginType in pluginTypes)
                container.Register(typeof(IPlugin), pluginType, Reuse.Singleton);

            plugins = container.Resolve<Many<IPlugin>>();
            Assert.That(plugins.Items.Count(), Is.EqualTo(2));
        }
    }

    public interface IPlugin { }

    public class SomePlugin : IPlugin { }
    public class AnotherPlugin : IPlugin { }
}
