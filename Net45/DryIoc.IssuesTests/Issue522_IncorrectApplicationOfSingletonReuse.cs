using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using DryIoc.MefAttributedModel;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    class Issue522_IncorrectApplicationOfSingletonReuse
    {
        [Test]
        public void MefCreatesDifferentInstancesOfNamedDriver()
        {
            var catalog = new TypeCatalog(typeof(Driver.DriverFactory));
            var container = new CompositionContainer(catalog);

            var factory = container.GetExport<Func<string, IDriver>>().Value;
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test, Ignore("TODO: fix")]
        public void DryIocWithDefaultSettingsCreatesDifferentInstancesOfNamedDriver()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Driver.DriverFactory));

            var factory = container.Resolve<Func<string, IDriver>>();
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithTransientDefaultReuseCreatesDifferentInstancesOfNamedDriver()
        {
            var container = new Container().WithMef().With(rules => rules.WithDefaultReuse(Reuse.Transient));
            container.RegisterExports(typeof(Driver.DriverFactory));

            var factory1 = container.Resolve<Func<string, IDriver>>();
            var driver1 = factory1("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory1("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test, Ignore("TODO: fix")]
        public void DryIocWithScopedDefaultReuseCreatesDifferentInstancesOfNamedDriver()
        {
            var container = new Container().WithMef().With(rules => rules.WithImplicitRootOpenScope().WithDefaultReuseInsteadOfTransient(Reuse.InCurrentScope));
            container.RegisterExports(typeof(Driver.DriverFactory));

            var factory = container.Resolve<Func<string, IDriver>>();
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        interface IDriver
        {
            string Name { get; }
        }

        class Driver : IDriver
        {
            public Driver(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public static class DriverFactory
            {
                [Export]
                public static IDriver Create(string Name)
                {
                    return new Driver(Name);
                }
            }
        }
    }
}
