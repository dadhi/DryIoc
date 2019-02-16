using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue61_Rules_SelectLastRegisteredFactory_does_not_account_for_OpenGenerics
    {
        [Test]
        public void Test_with_last_selected_factory()
        {
            var container = new Container(rules => 
                rules.WithFactorySelector(Rules.SelectLastRegisteredFactory()));

            container.Register(typeof(IX), typeof(X), Reuse.ScopedOrSingleton);
            container.Register(typeof(IA<B>), typeof(AB), Reuse.ScopedOrSingleton);
            container.Register(typeof(IA<>), typeof(A<>), Reuse.ScopedOrSingleton);

            var resolve = container.Resolve<IX>();
            var a = resolve.WhoAmI();
            Assert.AreEqual("AB", a);
        }

        [Test]
        public void Test_without_last_factory_selection_rule()
        {
            var container = new Container();

            container.Register(typeof(IX), typeof(X), Reuse.ScopedOrSingleton);
            container.Register(typeof(IA<B>), typeof(AB), Reuse.ScopedOrSingleton);
            container.Register(typeof(IA<>), typeof(A<>), Reuse.ScopedOrSingleton);

            var resolve = container.Resolve<IX>();
            var a = resolve.WhoAmI();
            Assert.AreEqual("AB", a);
        }


        internal interface IA<T> { string WhoAmI(); }

        internal class A<T> : IA<T>
        {
            public string WhoAmI() => "A";
        }

        internal class B { }

        internal class AB : IA<B>
        {
            public string WhoAmI() => "AB";
        }

        internal interface IX
        {
            string WhoAmI();
        }

        internal class X : IX
        {
            public IA<B> A1 { get; }
            public X(IA<B> a) { A1 = a; }

            public string WhoAmI() => A1.WhoAmI();
        }
    }
}
