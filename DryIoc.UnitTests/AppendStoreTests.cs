using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class AppendStoreTests
    {
        [Test]
        public void Append_to_end()
        {
            var store = AppendStore<string>.Empty;
            int key;
            store = store
                .Append("a", out key)
                .Append("b", out key)
                .Append("c", out key)
                .Append("d", out key);

            Assert.AreEqual("d", store.Get(3));
            Assert.AreEqual("c", store.Get(2));
            Assert.AreEqual("b", store.Get(1));
            Assert.AreEqual("a", store.Get(0));
        }

        [Test]
        public void Indexed_store_get_or_add()
        {
            var store = AppendStore<string>.Empty;

            int i; 
            store = store
                .Append("a", out i)
                .Append("b", out i)
                .Append("c", out i)
                .Append("d", out i);

            Assert.AreEqual("d", store.Get(i));
        }

        [Test]
        public void IndexOf_with_empty_store()
        {
            var store = AppendStore<string>.Empty;

            Assert.AreEqual(-1, store.IndexOf("a"));
        }

        [Test]
        public void IndexOf_non_existing_item()
        {
            var store = AppendStore<string>.Empty;

            int i;
            store = store
                .Append("a", out i);

            Assert.AreEqual(-1, store.IndexOf("b"));
        }

        [Test]
        public void IndexOf_existing_item()
        {
            var store = AppendStore<string>.Empty;

            int i;
            store = store
                .Append("a", out i)
                .Append("b", out i)
                .Append("c", out i);

            Assert.AreEqual(1, store.IndexOf("b"));
        }
    }
}