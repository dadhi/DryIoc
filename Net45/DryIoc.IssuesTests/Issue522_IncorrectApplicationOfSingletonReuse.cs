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
        public void MefCreatesDifferentInstancesOfNamedDriverUsingStaticFactory()
        {
            var catalog = new TypeCatalog(typeof(Driver.StaticDriverFactory));
            var container = new CompositionContainer(catalog);

            var factory = container.GetExport<Func<string, IDriver>>().Value;
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void MefCreatesDifferentInstancesOfNamedDriverUsingNonStaticFactory()
        {
            var catalog = new TypeCatalog(typeof(Driver.DriverFactory));
            var container = new CompositionContainer(catalog);

            var factory = container.GetExport<IDriverFactory>().Value;
            var driver1 = factory.Create("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory.Create("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithDefaultSettingsCreatesDifferentInstancesOfNamedDriverUsingStaticFactory()
        {
            var container = new Container().WithMef().With(rules => rules
                .WithIgnoringReuseForFuncWithArgs());
            container.RegisterExports(typeof(Driver.StaticDriverFactory));

            var factory = container.Resolve<Func<string, IDriver>>();
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithDefaultSettingsCreatesDifferentInstancesOfNamedDriverUsingNonStaticFactory()
        {
            var container = new Container().WithMef();
            container.RegisterExports(typeof(Driver.DriverFactory));

            var factory = container.Resolve<IDriverFactory>();
            var driver1 = factory.Create("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory.Create("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithTransientDefaultReuseCreatesDifferentInstancesOfNamedDriverUsingStaticFactory()
        {
            var container = new Container().WithMef().With(rules => rules.WithDefaultReuse(Reuse.Transient));
            container.RegisterExports(typeof(Driver.StaticDriverFactory));

            var factory = container.Resolve<Func<string, IDriver>>();
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithTransientDefaultReuseCreatesDifferentInstancesOfNamedDriverUsingNonStaticFactory()
        {
            var container = new Container().WithMef().With(rules => rules.WithDefaultReuse(Reuse.Transient));
            container.RegisterExports(typeof(Driver.DriverFactory));

            var factory = container.Resolve<IDriverFactory>();
            var driver1 = factory.Create("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory.Create("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithScopedDefaultReuseCreatesDifferentInstancesOfNamedDriverUsingStaticFactory()
        {
            var container = new Container().WithMef().With(rules => rules
                .WithDefaultReuse(Reuse.ScopedOrSingleton)
                .WithIgnoringReuseForFuncWithArgs());

            container.RegisterExports(typeof(Driver.StaticDriverFactory));

            var factory = container.Resolve<Func<string, IDriver>>();
            var driver1 = factory("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        [Test]
        public void DryIocWithScopedDefaultReuseCreatesDifferentInstancesOfNamedDriverUsingNonStaticFactory()
        {
            var container = new Container().WithMef().With(rules => rules.WithDefaultReuse(Reuse.ScopedOrSingleton));
            container.RegisterExports(typeof(Driver.DriverFactory));

            var factory = container.Resolve<IDriverFactory>();
            var driver1 = factory.Create("One");
            Assert.AreEqual("One", driver1.Name);

            var driver2 = factory.Create("Two");
            Assert.AreEqual("Two", driver2.Name);
            Assert.AreNotSame(driver1, driver2);
        }

        interface IDriver
        {
            string Name { get; }
        }

        interface IDriverFactory
        {
            IDriver Create(string name);
        }

        class Driver : IDriver
        {
            public Driver(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public static class StaticDriverFactory
            {
                [Export]
                public static IDriver Create(string Name)
                {
                    return new Driver(Name);
                }
            }

            [Export(typeof(IDriverFactory))]
            public class DriverFactory : IDriverFactory
            {
                public IDriver Create(string Name)
                {
                    return new Driver(Name);
                }
            }
        }
    }
}
