using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue135_DecoratorsIgnoredInChildScope
    {
        [Test]
        public void Decorators_Work_As_Per_Parent_Container()
        {
            var parent = new Container();
            parent.Register<IFruit, Mango>();
            parent.Register<IFruit, Banana>(setup: Setup.Decorator);

            var resolved = parent.Resolve<IFruit>();
            // Correct
            Assert.That(resolved, Is.InstanceOf<Banana>());

            var child = parent.CreateFacade();

            var childResolved = child.Resolve<IFruit>();
            // Fails: Should be true
            Assert.That(childResolved, Is.InstanceOf<Banana>());
        }

        #region CUT

        public interface IFruit { }
        public class Orange : IFruit { }
        public class Mango : IFruit { }
        public class Melon : IFruit { }

        public class Banana : IFruit
        {
            private readonly IFruit _fruit;

            public Banana(IFruit fruit)
            {
                _fruit = fruit;
            }
        }

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
