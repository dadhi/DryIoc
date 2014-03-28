using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class AppendableArrayTests
    {
        [Test]
        public void Append_to_end()
        {
            var store = AppendableArray<string>.Empty;
            store = store
                .Append("a")
                .Append("b")
                .Append("c")
                .Append("d");

            Assert.AreEqual("d", store.Get(3));
            Assert.AreEqual("c", store.Get(2));
            Assert.AreEqual("b", store.Get(1));
            Assert.AreEqual("a", store.Get(0));
        }

        [Test]
        public void Indexed_store_get_or_add()
        {
            var store = AppendableArray<string>.Empty;

            store = store
                .Append("a")
                .Append("b")
                .Append("c")
                .Append("d");

            var i = store.Length - 1; 

            Assert.AreEqual("d", store.Get(i));
        }

        [Test]
        public void IndexOf_with_empty_store()
        {
            var store = AppendableArray<string>.Empty;

            Assert.AreEqual(-1, store.IndexOf("a"));
        }

        [Test]
        public void IndexOf_non_existing_item()
        {
            var store = AppendableArray<string>.Empty;

            store = store.Append("a");

            Assert.AreEqual(-1, store.IndexOf("b"));
        }

        [Test]
        public void IndexOf_existing_item()
        {
            var store = AppendableArray<string>.Empty;

            store = store
                .Append("a")
                .Append("b")
                .Append("c");

            Assert.AreEqual(1, store.IndexOf("b"));
        }
    }
}