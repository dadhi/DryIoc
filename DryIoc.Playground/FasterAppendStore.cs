using ImTools;

namespace DryIoc.Playground
{
    public abstract class AppendStore<T>
    {
        public static readonly AppendStore<T> Empty = new ArrayAppendStore(0, null);

        public int Count { get; protected set; }

        public abstract AppendStore<T> Append(T value);
        public abstract object Get(int index);
        public abstract int IndexOf(object value);

        protected const int NODE_ARRAY_BITS = 31; // (11111 binary). So the array would be size of 32. Make it 15 (1111) and BIT_COUNT=4 for array of size 16
        protected const int NODE_ARRAY_BIT_COUNT = 5; // number of bits in NODE_ARRAY_MASK.

        private sealed class ArrayAppendStore : AppendStore<T>
        {
            private readonly T[] _items;

            public ArrayAppendStore(int count, T[] items)
            {
                Count = count;
                _items = items;
            }

            public override AppendStore<T> Append(T value)
            {
                return Count <= NODE_ARRAY_BITS
                    ? (AppendStore<T>)new ArrayAppendStore(Count + 1, _items.AppendOrUpdate(value))
                    : new TreeAppendStore(Count + 1, IntTree<T[]>.Empty.AddOrUpdate(0, _items).AddOrUpdate(1, new[] { value }));
            }

            public override object Get(int index)
            {
                return _items[index];
            }

            public override int IndexOf(object value)
            {
                return _items.IndexOf(x => ReferenceEquals(x, value) || Equals(x, value));
            }
        }

        private sealed class TreeAppendStore : AppendStore<T>
        {
            private readonly IntTree<T[]> _tree;

            public TreeAppendStore(int count, IntTree<T[]> tree)
            {
                Count = count;
                _tree = tree;
            }

            public override AppendStore<T> Append(T value)
            {
                return new TreeAppendStore(Count + 1, 
                    _tree.AddOrUpdate(Count >> NODE_ARRAY_BIT_COUNT, new[] { value }, ArrayTools.Append));
            }

            public override object Get(int index)
            {
                return _tree.GetValueOrDefault(index >> NODE_ARRAY_BIT_COUNT)[index & NODE_ARRAY_BITS];
            }

            public override int IndexOf(object value)
            {
                foreach (var node in _tree.Enumerate())
                {
                    var indexInNode = node.Value.IndexOf(x => ReferenceEquals(x, value) || Equals(x, value));
                    if (indexInNode != -1)
                        return node.Key << NODE_ARRAY_BIT_COUNT | indexInNode;
                }
                return -1;
            }
        }
    }
}
