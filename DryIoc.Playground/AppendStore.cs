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
            return new AppendStore<T>(Count + 1,
                _nodes.AddOrUpdate(Count >> NODE_ARRAY_BIT_COUNT, new[] { value }, ArrayTools.Append));
        }

        public int IndexOf(object value, int defaultIndex = -1)
        {
            foreach (var node in _nodes.Enumerate())
            {
                var indexInNode = node.Value.IndexOf(x => ReferenceEquals(x, value) || Equals(x, value));
                if (indexInNode != -1)
                    return node.Key << NODE_ARRAY_BIT_COUNT | indexInNode;
            }

            return defaultIndex;
        }

        public object Get(int index)
        {
            return _nodes.GetValueOrDefault(index >> NODE_ARRAY_BIT_COUNT)[index & NODE_ARRAY_MASK];
        }

        #region Implementation

        private const int NODE_ARRAY_MASK = 31; // (11111 binary). So the array would be size of 32. Make it 15 (1111) and BIT_COUNT=4 for array of size 16
        private const int NODE_ARRAY_BIT_COUNT = 5; // number of bits in NODE_ARRAY_MASK.

        private readonly HashTree<T[]> _nodes;

        private AppendStore(int count, HashTree<T[]> nodes)
        {
            Count = count;
            _nodes = nodes;
        }

        #endregion
    }
}