using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace DryIoc
{
    [MemoryDiagnoser]
    public class StructEnumerableTest
    {
        private static readonly HM _hm = new HM();
        private static readonly HM2 _hm2 = new HM2();

        [Benchmark]
        public List<int> Test1() => _hm.Enumerate().ToList();

        [Benchmark(Baseline = true)]
        public List<int> Test2() => _hm2.Enumerate().ToList();

        class HM
        {
            private static readonly int[] _ints = { 1, 2, 3, 4, 5 };
            public IEnumerable<int> Enumerate()
            {
                for (var i = 0; i < 5; i++)
                    yield return _ints[i];
            }
        }

        class HM2
        {
            private static readonly int[] _ints = { 1, 2, 3, 4, 5 };
            public IEnumerable<int> Enumerate() => new E(_ints);

            struct E : IEnumerable<int>
            {
                private readonly int[] _ints;
                public E(int[] ints) { _ints = ints; }

                public IEnumerator<int> GetEnumerator() => new EE(0, _ints);
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            struct EE : IEnumerator<int>
            {
                private int _index;
                private readonly int[] _iis;

                public EE(int i, int[] iis)
                {
                    _index = i;
                    _iis = iis;
                }

                public bool MoveNext() => (_index++) < 5;

                public void Reset() => _index = 0;

                public int Current => _iis[_index - 1];

                object IEnumerator.Current => Current;

                public void Dispose() { }
            }
        }
    }
}
