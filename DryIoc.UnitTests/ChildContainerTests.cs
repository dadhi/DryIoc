using System;
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
        public void Can_resolve_service_wrapper_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Orange));

            var child = parent.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var juice = child.Resolve<Func<IJuice>>();

            Assert.That(juice, Is.InstanceOf<Func<IJuice>>());
        }

        [Test]
        public void Can_inject_singleton_service_from_parent_container()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Mango), Reuse.Singleton);

            var child = parent.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var fst = child.Resolve<IJuice>();
            var snd = child.Resolve<IJuice>();

            Assert.That(fst.Fruit, Is.SameAs(snd.Fruit));
        }

        [Test]
        public void Can_share_resolved_singletons_from_parent_container()
        {
            var parent = new Container();
            parent.Register<IFruit, Mango>(Reuse.Singleton);

            var child = parent.CreateChildContainer(shareSingletons: true);
            child.Register<IJuice, FruitJuice>();

            var parentFruit = parent.Resolve<IFruit>();
            var childJuice = child.Resolve<IJuice>();

            Assert.That(parentFruit, Is.SameAs(childJuice.Fruit));
        }

        [Test]
        public void Context_scope_is_not_copied_to_child_container_created_from_opened_scope()
        {
            var container = new Container();
            container.Register<IFruit, Mango>(Reuse.InCurrentScope);

            using (var scoped = container.OpenScope())
            {
                var child = container.CreateChildContainer();
                child.Register<IJuice, FruitJuice>();

                scoped.Resolve<IFruit>();
                
                var ex = Assert.Throws<ContainerException>(() => child.Resolve<IJuice>());
                Assert.AreEqual(ex.Error, Error.NO_CURRENT_SCOPE);
            }
        }

        [Test]
        public void Can_inject_current_scope_service_from_parent_container_After_it_was_resolved_from_parent2()
        {
            var container = new Container();
            container.Register<IFruit, Mango>(Reuse.InCurrentScope);

            var child = container.CreateChildContainer();
            child.Register<IJuice, FruitJuice>();

            using (var scoped = child.OpenScope())
            {
                var parentFruit = scoped.Resolve<IFruit>();
                var childJuice = child.Resolve<IJuice>();

                Assert.That(parentFruit, Is.SameAs(childJuice.Fruit));
            }

            Assert.That(() => child.Resolve<IJuice>(), Throws.InstanceOf<ContainerException>());
        }

        [Test]
        public void Can_fallback_to_parent_and_return_back_to_child()
        {
            var container = new Container();
            container.Register<FruitJuice>();
            container.Register<IFruit, Melon>();

            var childContainer = container.CreateChildContainer();
            childContainer.Register<IFruit, Orange>();

            Assert.That(container.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Melon>());
            Assert.That(childContainer.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Orange>());
        }

        [Test]
        public void Child_may_throw_if_parent_disposed()
        {
            var container = new Container();
            container.Register<FruitJuice>();
            container.Register<IFruit, Melon>();

            var childContainer = container.CreateChildContainer();
            childContainer.Register<IFruit, Orange>();

            container.Dispose();

            var ex = Assert.Throws<ContainerException>(() =>
            childContainer.Resolve<FruitJuice>());

            Assert.AreEqual(ex.Error, Error.CONTAINER_IS_DISPOSED);
        }

        [Test]
        public void Attach_multiple_parents_with_single_rule()
        {
            IContainer parent = new Container();
            parent.Register<FruitJuice>();

            IContainer anotherParent = new Container();
            anotherParent.Register<IFruit, Melon>();

            var container = new Container();

            var attachParents = Container.ResolveFromParents(parent, anotherParent);
            var childContainer = container.With(rules => 
                rules.WithUnknownServiceResolver(attachParents));

            Assert.That(childContainer.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Melon>());
        }

        [Test]
        public void Parent_attachment_should_not_affect_original_container()
        {
            IContainer parent = new Container();
            parent.Register<FruitJuice>();
            parent.Register<IFruit, Melon>();

            var container = new Container();
            container.Register<IFruit, Orange>();

            var childContainer = container.With(rules => 
                rules.WithUnknownServiceResolver(Container.ResolveFromParents(parent)));

            Assert.That(parent.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Melon>());
            Assert.That(childContainer.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Orange>());

            Assert.Throws<ContainerException>(() => 
                container.Resolve<FruitJuice>());
        }

        [Test]
        public void Can_detach_parent()
        {
            IContainer parent = new Container();
            parent.Register<FruitJuice>();
            parent.Register<IFruit, Melon>();

            var container = new Container();
            container.Register<IFruit, Orange>();

            var resolveFromParents = Container.ResolveFromParents(parent) ;
            var childContainer = container.With(rules => rules.WithUnknownServiceResolver(resolveFromParents));

            Assert.That(parent.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Melon>());
            Assert.That(childContainer.Resolve<FruitJuice>().Fruit, Is.InstanceOf<Orange>());

            var detachedChild = childContainer.With(rules => rules.WithoutUnknownServiceResolver(resolveFromParents));

            Assert.Throws<ContainerException>(() =>
                detachedChild.Resolve<FruitJuice>());
        }

        #region CUT

        public interface IFruit { }
        public class Orange : IFruit { }
        public class Mango : IFruit { }
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
}
