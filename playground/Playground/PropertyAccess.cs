using System;
using BenchmarkDotNet.Attributes;

namespace Playground
{
    [MemoryDiagnoser]
    public class PropertyAccess
    {
        private static readonly NodeWithNested[] _nodesNested = CreateNodesWithNested();
        private static NodeWithNested[] CreateNodesWithNested() => new[]
        {
            new NodeWithNested(1),
            new NodeWithNested(2),
            new NodeWithNested(3),
            new NodeWithNested(4),
            new NodeWithNested(5)
        };

        private static readonly Node[] _nodesNormal = CreateNormalNodes();
        private static Node[] CreateNormalNodes() => new[]
        {
            new Node(1),
            new Node(2),
            new Node(3),
            new Node(4),
            new Node(5)
        };

        private static readonly NodeInherited[] _nodesInherited = CreateInheritedNodes();
        private static NodeInherited[] CreateInheritedNodes() => new[]
        {
            new NodeInherited(1),
            new NodeInherited(2),
            new NodeInherited(3),
            new NodeInherited(4),
            new NodeInherited(5)
        };

        [Benchmark]
        public int NodeWithNestedData()
        {
            var n = CreateNodesWithNested();
            var sum = 0;

            for (var i = 0; i < 5; i++)
                sum += n[i].Hash;

            return sum;
        }

        [Benchmark(Baseline = true)]
        public int NodeWithInlinedData()
        {
            var n = CreateNormalNodes();
            var sum = 0;

            for (var i = 0; i < 5; i++)
                sum += n[i].Hash;

            return sum;
        }

        [Benchmark]
        public int NodeWithInheritedData()
        {
            var n = CreateInheritedNodes();
            var sum = 0;

            for (var i = 0; i < 5; i++)
                sum += n[i].Hash;

            return sum;
        }

        public class Data
        {
            public readonly int Hash;
            public readonly Type Key;
            public readonly string Value;

            protected Data() { }

            public Data(int hash, Type key, string value)
            {
                Hash = hash;
                Key = key;
                Value = value;
            }
        }

        public class NodeInherited : Data
        {
            public static readonly NodeInherited Empty = new NodeInherited();

            public readonly NodeInherited Left;
            public readonly NodeInherited Right;
            public readonly int Height;

            private NodeInherited() : base()
            { }

            public NodeInherited(int hash) : base(hash, typeof(NodeWithNested), "xxx" + hash)
            {
                Left = Empty;
                Right = Empty;
                Height = 0;
            }
        }

        public class Node
        {
            public static readonly Node Empty = new Node();

            public readonly int Hash;
            public readonly Type Key;
            public readonly string Value;
            public readonly Node Left;
            public readonly Node Right;
            public readonly int Height;

            private Node() { }

            public Node(int hash)
            {
                Hash = hash;
                Key = typeof(NodeWithNested);
                Value = "xxx" + hash;
                Left = Empty;
                Right = Empty;
                Height = 0;
            }
        }

        public class NodeWithNested
        {
            public static readonly NodeWithNested Empty = new NodeWithNested();

            private readonly Data _data;
            public readonly NodeWithNested Left;
            public readonly NodeWithNested Right;
            public readonly int Height;

            private NodeWithNested() { }

            public NodeWithNested(int hash)
            {
                _data = new Data(hash, typeof(NodeWithNested), "xxx" + hash);
                Left = Empty;
                Right = Empty;
                Height = 0;
            }

            public int Hash => _data.Hash;
            public Type Key => _data.Key;

            class Data
            {
                public readonly int Hash;
                public readonly Type Key;
                public readonly string Value;

                public Data(int hash, Type key, string value)
                {
                    Hash = hash;
                    Key = key;
                    Value = value;
                }
            }
        }
    }
}
