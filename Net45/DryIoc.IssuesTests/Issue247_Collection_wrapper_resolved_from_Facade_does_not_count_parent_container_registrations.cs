using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class Issue247_Collection_wrapper_resolved_from_Facade_does_not_count_parent_container_registrations
    {
        [Test]
        public void When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.UseInstance("a");

            var f = container.CreateFacade();
            f.UseInstance("b");
            var strs = f.Resolve<string[]>();

            GC.KeepAlive(container);
            Assert.AreEqual(2, strs.Length);
        }

        [Test]
        public void When_only_fallback_services_are_available()
        {
            var container = new Container();
            container.UseInstance("a");

            var f = container.CreateFacade();
            var strs = f.Resolve<string[]>();

            GC.KeepAlive(container);
            Assert.AreEqual(1, strs.Length);
        }

        [Test]
        public void For_KeyValuePair_When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.UseInstance("a", serviceKey: 1);

            var f = container.CreateFacade();
            f.UseInstance("b", serviceKey: 2);
            var strs = f.Resolve<KeyValuePair<int, string>[]>();

            GC.KeepAlive(container);
            Assert.AreEqual(2, strs.Length);
        }

        [Test]
        public void For_lazy_collection_When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.UseInstance("a");

            var f = container.CreateFacade();
            f.UseInstance("b");
            var strs = f.Resolve<LazyEnumerable<string>>().ToArrayOrSelf();

            GC.KeepAlive(container);
            Assert.AreEqual(2, strs.Length);
        }

        [Test]
        public void For_lazy_collection_When_only_fallback_services_are_available()
        {
            var container = new Container();
            container.UseInstance("a");

            var f = container.CreateFacade();
            var strs = f.Resolve<LazyEnumerable<string>>().ToArrayOrSelf();

            GC.KeepAlive(container);
            Assert.AreEqual(1, strs.Length);
        }

        [Test]
        public void For_lazy_collection_of_KeyValuePair_When_both_current_and_fallback_services_are_available()
        {
            var container = new Container();
            container.UseInstance("a", serviceKey: 1);

            var f = container.CreateFacade();
            f.UseInstance("b", serviceKey: 2);
            var strs = f.Resolve<LazyEnumerable<KeyValuePair<int, string>>>().ToArrayOrSelf();

            GC.KeepAlive(container);
            Assert.AreEqual(2, strs.Length);
        }
    }
}
