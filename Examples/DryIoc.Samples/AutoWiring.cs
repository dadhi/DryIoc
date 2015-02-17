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
            var plugins = container.Resolve<LazyEnumerable<IPlugin>>();
            Assert.That(plugins.Count(), Is.EqualTo(0));

            var pluginAssembly = typeof(AutoWiring).GetAssembly();

            var pluginTypes = pluginAssembly.GetTypes().Where(t => t.IsPublicOrNestedPublic() && !t.IsAbstract()
                && t.GetImplementedTypes().Contains(typeof(IPlugin)));

            foreach (var pluginType in pluginTypes)
                container.Register(typeof(IPlugin), pluginType, Reuse.Singleton);

            plugins = container.Resolve<LazyEnumerable<IPlugin>>();
            Assert.That(plugins.Count(), Is.EqualTo(2));
        }

        // Setup similar to NInject https://github.com/ninject/ninject.extensions.conventions/wiki/Overview
        [Test]
        public void Convention_setup_example()
        {
            var container = new Container();
            container.RegisterMany(new[] { Assembly.GetExecutingAssembly() },
                typeof(IPlugin).Equals,
                (r, types, type, proceed) =>
                {
                    if (type == typeof(AnotherPlugin)) // custom registration of specific type
                        r.Register<IPlugin, AnotherPlugin>(Reuse.Singleton, named: "another");
                    else
                        proceed(types, type);
                });

            var plugin = container.Resolve<IPlugin>("another");
            Assert.NotNull(plugin);
            Assert.AreSame(plugin, container.Resolve<IPlugin>("another"));
            Assert.AreEqual(2, container.Resolve<IPlugin[]>().Length);
        }
    }

    public interface IPlugin { }
    public class SomePlugin : IPlugin { }
    public class AnotherPlugin : IPlugin { }
}
