using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture][Ignore("Fixed in v2.0.0")]
    public class IssueNo2_Can_inject_singleton_service_from_parent_container_After_it_was_resolved_from_parent
    {
        [Test]
        public void Test()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Melon), Reuse.Singleton);

            var child = parent.CreateChildContainer();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var parentFruit = parent.Resolve<IFruit>();
            var snd = child.Resolve<IJuice>();

            Assert.That(parentFruit, Is.SameAs(snd.Fruit));
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
}
