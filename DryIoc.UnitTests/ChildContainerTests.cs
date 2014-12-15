using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ChildContainerTests
    {
        [Test]
        public void Can_resolve_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Orange));

            var child = parent.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var juice = child.Resolve<IJuice>();

            Assert.That(juice, Is.InstanceOf<FruitJuice>());
        }

        [Test]
        public void Can_inject_singleton_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Melon), Reuse.Singleton);

            var child = parent.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var fst = child.Resolve<IJuice>();
            var snd = child.Resolve<IJuice>();

            Assert.That(fst.Fruit, Is.SameAs(snd.Fruit));
        }

        [Test]
        public void Can_inject_singleton_service_from_parent_container_After_it_was_resolved_from_parent()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Melon), Reuse.Singleton);

            var child = parent.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var parentFruit = parent.Resolve<IFruit>();
            var childJuice = child.Resolve<IJuice>();

            Assert.That(parentFruit, Is.SameAs(childJuice.Fruit));
        }

        [Test, Ignore]
        public void Can_inject_current_scope_service_from_parent_container_After_it_was_resolved_from_parent()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Melon), Reuse.InCurrentScope);
            var scoped = parent.OpenScope();

            var child = scoped.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var parentFruit = scoped.Resolve<IFruit>();
            var childJuice = child.Resolve<IJuice>();

            Assert.That(parentFruit, Is.SameAs(childJuice.Fruit));
        }
    }

    #region CUT

    public interface IFruit { }
    public class Orange : IFruit { }
    public class Melon : IFruit { }

    public interface IJuice
    {
        IFruit Fruit { get; }
    }

    public class FruitJuice : IJuice
    {
        public IFruit Fruit { get; set; }

        public FruitJuice(IFruit fruit)
        {
            Fruit = fruit;
        }
    }

    #endregion
}
