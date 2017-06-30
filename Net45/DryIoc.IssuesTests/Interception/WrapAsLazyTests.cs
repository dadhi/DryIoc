using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DryIoc.IssuesTests.Interception
{
    [TestFixture]
    public class WrapAsLazyTests
    {
        [Test]
        public void Service_can_be_registered_as_always_lazy()
        {
            var c = new Container();
            c.RegisterAsLazy<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Interface_can_be_resolved_as_always_lazy()
        {
            var c = new Container();
            c.ResolveAsLazy<IAlwaysLazy>();
            c.Register<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test]
        public void Interface_not_available_at_compile_time_can_be_resolved_as_always_lazy()
        {
            var c = new Container();
            var typeLoadedFromExternalAssembly = Assembly.GetExecutingAssembly().GetType("DryIoc.IssuesTests.Interception.IAlwaysLazy");
            c.ResolveAsLazy(typeLoadedFromExternalAssembly);

            c.Register<IAlwaysLazy, LazyService>();

            LazyService.LastValue = "NotCreated";

            var proxy = c.Resolve<IAlwaysLazy>();
            Assert.AreEqual("NotCreated", LazyService.LastValue);

            proxy.Test("Created!");
            Assert.AreEqual("Created!", LazyService.LastValue);
        }

        [Test, ExpectedException(typeof(ContainerException))]
        public void Circular_dependency_is_normally_not_allowed()
        {
            var container = new Container();
            container.Register<IChicken, Chicken>();
            container.Register<IEgg, Egg>();

            // this call doesn't detect the circular dependency
            container.Validate();

            // but the resolution fails
            var e = container.Resolve<IEgg>();
        }

        [Test]
        public void Circular_dependency_is_allowed_when_services_are_registered_as_lazy()
        {
            var container = new Container();
            container.RegisterAsLazy<IChicken, Chicken>();
            container.RegisterAsLazy<IEgg, Egg>();

            var e = container.Resolve<IEgg>();
            Assert.NotNull(e);
            Assert.NotNull(e.Chicken);

            var c = container.Resolve<IChicken>();
            Assert.NotNull(c);
            Assert.NotNull(c.Egg);
        }

        [Test, Ignore]
        public void Circular_dependency_handling_doesnt_actually_work_for_the_deeper_levels()
        {
            var container = new Container();
            container.RegisterAsLazy<IChicken, Chicken>();
            container.RegisterAsLazy<IEgg, Egg>();

            var e = container.Resolve<IEgg>();
            Assert.NotNull(e);
            Assert.NotNull(e.Chicken);
            Assert.NotNull(e.Chicken.Egg); // this call throws a circular dependency exception
            Assert.NotNull(e.Chicken.Egg.Chicken);
            Assert.NotNull(e.Chicken.Egg.Chicken.Egg);
            Assert.NotNull(e.Chicken.Egg.Chicken.Egg.Chicken);
        }
    }

    public interface IAlwaysLazy
    {
        void Test(string x);
    }

    class LazyService : IAlwaysLazy
    {
        public LazyService()
        {
            LastValue = "LazyServiceCreated";
        }

        public static string LastValue { get; set; }

        public void Test(string x)
        {
            LastValue = x;
        }
    }

    public interface IChicken { IEgg Egg { get; } }

    public interface IEgg { IChicken Chicken { get; } }

    public class Chicken : IChicken
    {
        public Chicken(IEgg egg)
        {
            Egg = egg;
        }

        public IEgg Egg { get; private set; }
    }

    public class Egg : IEgg
    {
        public Egg(IChicken chicken)
        {
            Chicken = chicken;
        }

        public IChicken Chicken { get; private set; }
    }
}
