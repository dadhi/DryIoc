using NUnit.Framework;

namespace DryIoc.Playground
{
    [TestFixture]
    //[Ignore]
    public partial class IntTreeTests
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

    public sealed class AppendStore<T>
    {
        public static readonly AppendStore<T> Empty = new AppendStore<T>(0, HashTree<T[]>.Empty);

        public readonly int Count;

        public AppendStore<T> Append(T value, out int index)
        {
            index = Count;
            return new AppendStore<T>(Count + 1, _nodes.AddOrUpdate(Count >> 5, new[] { value }, ArrayTools.Append));
        }

        public int IndexOf(object value, int defaultIndex = -1)
        {
            foreach (var node in _nodes.Enumerate())
            {
                var indexInNode = node.Value.IndexOf(x => ReferenceEquals(x, value) || Equals(x, value));
                if (indexInNode != -1)
                    return node.Key << 5 | indexInNode;
            }

            return defaultIndex;
        }

        public object Get(int index)
        {
            return _nodes.GetValueOrDefault(index >> 5)[index & 31];
        }

        #region Implementation

        private const int NODE_ARRAY_SIZE = 32;

        private readonly HashTree<T[]> _nodes;

        private AppendStore(int count, HashTree<T[]> nodes)
        {
            Count = count;
            _nodes = nodes;
        }

        #endregion
    }
}