using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class IssueNo2_Can_inject_singleton_service_from_parent_container_After_it_was_resolved_from_parent
    {
        [Test, Explicit("Child container are not supported")]
        public void Test()
        {
            var parent = new Container();
            parent.Register(typeof(IFruit), typeof(Melon), Reuse.Singleton);

            var child = parent.CreateFacade();
            child.Register(typeof(IJuice), typeof(FruitJuice));

            var parentFruit = parent.Resolve<IFruit>();
            var snd = child.Resolve<IJuice>();

            Assert.That(parentFruit, Is.SameAs(snd.Fruit));
        }

        [Test]
        public void Test_using_open_scope()
        {
            var c = new Container();
            using (var parent = c.OpenScope("parent"))
            {
                c.Register(typeof(IFruit), typeof(Melon), Reuse.ScopedTo("parent"));

                using (var child = parent.OpenScope())
                {
                    c.Register(typeof(IJuice), typeof(FruitJuice));

                    var parentFruit = parent.Resolve<IFruit>();
                    var snd = child.Resolve<IJuice>();

                    Assert.That(parentFruit, Is.SameAs(snd.Fruit));
                }
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
}
