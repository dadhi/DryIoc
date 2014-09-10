using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace DryIoc.Samples
{
    [TestFixture]
    public class AutoWiring
    {
        [Test]
        public void Discover_and_register_new_plugins_from_assembly()
        {
            var container = new Container();
            var plugins = container.Resolve<Many<IPlugin>>();
            Assert.That(plugins.Items.Count(), Is.EqualTo(0));

            var pluginAssembly = typeof(AutoWiring).Assembly;

            var pluginTypes = pluginAssembly.GetTypes().Where(t => t.IsPublic && !t.IsAbstract 
                && t.GetImplementedTypes().Contains(typeof(IPlugin)));

            foreach (var pluginType in pluginTypes)
                container.Register(typeof(IPlugin), pluginType, Reuse.Singleton);

            plugins = container.Resolve<Many<IPlugin>>();
            Assert.That(plugins.Items.Count(), Is.EqualTo(2));
        }

        // Setup similar to NInject https://github.com/ninject/ninject.extensions.conventions/wiki/Overview
        [Test]
        public void Convention_setup_example()
        {
            var container = new Container();

            var implementingClasses =
                Assembly.GetExecutingAssembly() // from current executing assembly, or you can select any other assembly
                .GetTypes().Where(type =>
                    type.IsPublic &&                    // get public types 
                    !type.IsAbstract &&                 // which are not interfaces nor abstract
                    type.GetInterfaces().Length != 0);  // which implementing some interface(s)

            foreach (var implementingClass in implementingClasses)
            {
                if (implementingClass == typeof(AnotherPlugin))
                {
                    // Specific registration for some specific type
                    container.Register(implementingClass, Reuse.Transient);
                }
                else
                {   // By default register type with all of its interfaces as services. 
                    // Register with Singleton reuse.
                    container.RegisterAll(implementingClass, Reuse.Singleton, types: t => t.IsInterface);
                }
            }
        }
    }

    public interface IPlugin { }

    public class SomePlugin : IPlugin { }
    public class AnotherPlugin : IPlugin { }
}
