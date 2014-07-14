using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class ResolveUnregisteredFromTests
    {
        [Test]
        public void Can_resolve_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Orange));
            var child = new Container();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            child.ResolveUnregisteredFrom(parent);
            var juice = child.Resolve<IJuice>();

            Assert.That(juice, Is.InstanceOf<FruitJuice>());
        }

        [Test]
        public void Can_inject_singleton_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Melon), Reuse.Singleton);

            var child = new Container();
            child.ResolveUnregisteredFrom(parent);
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var fst = child.Resolve<IJuice>();
            var snd = child.Resolve<IJuice>();

            Assert.That(fst.Fruit, Is.SameAs(snd.Fruit));
        }

        [Test]
        public void Once_resolved_I_can_NOT_stop_resolving_services_from_parent_container()
        {
            var parentContainer = new Container();
            parentContainer.Register(typeof(IFruit), typeof(Orange));

            var container = new Container();
            container.Register(typeof(IJuice), typeof(FruitJuice));

            var useRegistrationsFromParent = container.ResolveUnregisteredFrom(parentContainer);
            var juice = container.Resolve<IJuice>();
            Assert.That(juice, Is.InstanceOf<FruitJuice>());

            container.ResolutionRules.UnregisteredServices =
                container.ResolutionRules.UnregisteredServices.Append(useRegistrationsFromParent);

            Assert.DoesNotThrow(
                () => container.Resolve<IJuice>());
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
