using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DryIoc.UnitTests.Playground
{
    public sealed class TypeTree<V> : IEnumerable<TypeTree<V>>
    {
        public static readonly TypeTree<V> Empty = new TypeTree<V>(IntTree<KV>.Empty);

        public TypeTree<V> AddOrUpdate(Type key, V value)
        {
            return new TypeTree<V>(_tree.AddOrUpdate(key.GetHashCode(), new KV { Key = key, Value = value }, Update));
        }

        public V TryGet(Type key)
        {
            var item = _tree.GetValueOrDefault(key.GetHashCode());
            return item != null && key == item.Key ? item.Value : TryGetConflicted(item, key);
        }

        public IEnumerator<TypeTree<V>> GetEnumerator()
        {
            return _tree.Select(t => new TypeTree<V>(t)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Implementation

        private readonly IntTree<KV> _tree;

        private TypeTree(IntTree<KV> tree)
        {
            _tree = tree;
        }

        private static KV Update(KV old, KV added)
        {
            var conflicts = old is KVWithConflicts ? ((KVWithConflicts)old).Conflicts : null;

            if (old.Key == added.Key)
                return conflicts == null ? added
                    : new KVWithConflicts { Key = added.Key, Value = added.Value, Conflicts = conflicts };

            var newConflicts = conflicts == null ? new[] { added }
                : conflicts.AppendOrUpdate(added, Array.FindIndex(conflicts, x => x.Key == added.Key));

            return new KVWithConflicts { Key = old.Key, Value = old.Value, Conflicts = newConflicts };
        }

        private static V TryGetConflicted(KV item, Type key)
        {
            var conflicts = item is KVWithConflicts ? ((KVWithConflicts)item).Conflicts : null;
            if (conflicts != null)
                for (var i = 0; i < conflicts.Length; i++)
                    if (conflicts[i].Key == key)
                        return conflicts[i].Value;
            return default(V);
        }

        private class KV
        {
            public Type Key;
            public V Value;
        }

        private sealed class KVWithConflicts : KV
        {
            public KV[] Conflicts;
        }

        #endregion
    }

}
