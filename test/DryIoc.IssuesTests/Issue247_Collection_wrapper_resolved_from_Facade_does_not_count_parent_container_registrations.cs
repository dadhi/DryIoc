using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ImTools;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue247_Collection_wrapper_resolved_from_Facade_does_not_count_parent_container_registrations
    {
        [Test]
        public void When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.RegisterInstance("a");

            var f = container.CreateFacade();
            f.RegisterInstance("b", serviceKey: ContainerTools.FacadeKey);
            var strs = f.Resolve<string[]>();

            CollectionAssert.AreEquivalent(new[] { "b", "a" }, strs);
        }

        [Test]
        public void When_only_fallback_services_are_available()
        {
            var container = new Container();
            container.RegisterInstance("a");

            var f = container.CreateFacade();
            var strs = f.Resolve<string[]>();

            CollectionAssert.AreEqual(new[] { "a" }, strs);
        }

        [Test]
        public void For_KeyValuePair_When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.RegisterInstance("a", serviceKey: 1);

            var f = container.CreateFacade();
            f.RegisterInstance("b", serviceKey: 2);
            var strs = f.Resolve<KeyValuePair<int, string>[]>();

            CollectionAssert.AreEquivalent(new[] { "b", "a" }, strs.Select(it => it.Value));
        }

        [Test]
        public void For_lazy_collection_When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.RegisterInstance("a");

            var f = container.CreateFacade();
            f.RegisterInstance("b", serviceKey: ContainerTools.FacadeKey);
            var strs = f.Resolve<LazyEnumerable<string>>().ToArrayOrSelf();

            CollectionAssert.AreEquivalent(new[] { "b", "a" }, strs);
        }

        [Test]
        public void For_lazy_collection_When_only_fallback_services_are_available()
        {
            var container = new Container();
            container.RegisterInstance("a");

            var f = container.CreateFacade();
            var strs = f.Resolve<LazyEnumerable<string>>().ToArrayOrSelf();

            Assert.AreEqual(1, strs.Length);
        }

        [Test]
        public void For_lazy_collection_of_KeyValuePair_When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.RegisterInstance("a", serviceKey: 1);

            var f = container.CreateFacade();
            f.RegisterInstance("b", serviceKey: 2);
            var strs = f.Resolve<LazyEnumerable<KeyValuePair<int, string>>>().ToArrayOrSelf();

            Assert.AreEqual(2, strs.Length);
        }
    }
}
