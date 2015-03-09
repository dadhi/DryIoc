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

        [Test]
        public void Without_singletons_should_work_with()
        {
            var container = new Container();
            container.Register<Melon>(Reuse.Singleton);
            var melon = container.Resolve<Melon>();

            var withoutSingletons = container.WithoutSingletonsAndCache(); // automatically drop cache
            var melonAgain = withoutSingletons.Resolve<Melon>();

            Assert.AreNotSame(melon, melonAgain);
        }

        [Test]
        public void With_registration_copy()
        {
            var container = new Container();
            container.Register<IFruit, Melon>();
            var melon = container.Resolve<IFruit>();
            Assert.That(melon, Is.InstanceOf<Melon>());

            var withRegistrationsCopy = container.WithRegistrationsCopy().WithoutCache();
            withRegistrationsCopy.Register<IFruit, Orange>(ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            var orange = withRegistrationsCopy.Resolve<IFruit>();
            Assert.That(orange, Is.InstanceOf<Orange>());
        }

        [Test]
        public void Resolving_singleton_registered_in_parent_from_second_child_with_first_child_having_resolved_and_is_disposed_do_not_throw()
        {
            var parent = new Container();
            parent.Register<Foo>(Reuse.Singleton);

            var parentFoo = parent.Resolve<Foo>(); // Stores resolved instance in Parent singletons.
            
            var firstChild = parent.CreateChildContainer(shareSingletons: false); // Child and Parent have their own singletons. 
            var firstChildFoo = firstChild.Resolve<Foo>(); // Stores resolved instance in Child singletons. 
            Assert.AreNotSame(firstChildFoo, parentFoo);   // Child and Parent singletons are different.
            
            firstChild.Dispose();                   // Disposes Child with its singletons, Parent stays intact.
            Assert.IsTrue(firstChildFoo.Disposed);  // firstFoo shouldn't be disposed (No it is by design).

            var secondChild = parent.CreateChildContainer(shareSingletons: true); // Second Child reference to Parent singletons. 
                                                                                  // So disposing Child affects Parent, that's why it is not a default option.
            var secondChildFoo = secondChild.Resolve<Foo>();                      // Resolves Parent singleton.
            Assert.AreSame(secondChildFoo, parentFoo);       
            secondChildFoo.Dispose();               // Disposes Child singletons, and effectively the Parent singletons too because of sharing.
            Assert.IsTrue(parentFoo.Disposed);
        }

        [Test, Explicit("Not supported")]
        public void Reusing_singletons_from_parent_and_not_disposing_them_with_Child()
        {
            var parent = new Container();
            parent.Register<Foo>(Reuse.Singleton);

            var firstChild = parent.CreateChildContainer(true);
            var firstFoo = firstChild.Resolve<Foo>();
            firstChild.Dispose();

            Assert.IsFalse(firstFoo.Disposed); // firstFoo shouldn't be disposed

            var secondChild = parent.CreateChildContainer(true);
            secondChild.Resolve<Foo>(); // Resolve<Foo>() shouldn't throw
        }

        [Test]
        public void Reusing_singletons_from_parent_and_not_disposing_them_in_scoped_container()
        {
            var container = new Container();

            var parent = container.OpenScopeWithoutContext("parent");
            parent.Register<Foo>(Reuse.InCurrentNamedScope("parent"));

            var firstChild = parent.OpenScopeWithoutContext();
            var firstFoo = firstChild.Resolve<Foo>();

            firstChild.Register<Blah>(Reuse.InCurrentScope);
            var firstBlah = firstChild.Resolve<Blah>();

            firstChild.Dispose();

            Assert.IsFalse(firstFoo.Disposed); // firstFoo shouldn't be disposed
            Assert.IsTrue(firstBlah.Disposed); // firstBlah should be disposed

            var secondChild = parent.OpenScopeWithoutContext();
            secondChild.Resolve<Foo>(); // Resolve<Foo>() shouldn't throw

            parent.Dispose();   // Parent scope is disposed.
            Assert.IsTrue(firstFoo.Disposed); 

            container.Dispose(); // singletons, registry, cache, is gone
        }

        #region CUT

        internal class Foo : IDisposable
        {
            public bool Disposed { get; set; }
            public void Dispose()
            {
                Disposed = true;
            }
        }

        internal class Blah : IDisposable
        {
            public bool Disposed { get; set; }
            public void Dispose()
            {
                Disposed = true;
            }
        }


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
