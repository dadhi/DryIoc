using System.Collections;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace DryIoc
{
    [MemoryDiagnoser]
    public class StructEnumerableTest
    {
        private static readonly HM _hm = new HM();
        private static readonly HM2 _hm2 = new HM2();

        [Benchmark]
        public int YieldEnumerable()
        {
            var last = 0;
            foreach (var n in _hm.Enumerate())
                last = n;
            return last;
        }

        [Benchmark(Baseline = true)]
        public int StructEnumerable()
        {
            var last = 0;
            foreach (var n in _hm2.Enumerate())
                last = n;
            return last;
        }

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
            public E Enumerate() => new E(_ints);

            public struct E : IEnumerable<int>
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
