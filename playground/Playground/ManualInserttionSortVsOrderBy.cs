using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Playground
{
    [MemoryDiagnoser]
    public class ManualInserttionSortVsOrderBy
    {
        public Thing[] things =
        {
            new Thing(1, 0),
            new Thing(5, 0),
            new Thing(3, 0),
            new Thing(4, 0),
            new Thing(6, 0),
        };

        public Thing[] things2 =
        {
            new Thing(1, 1),
            new Thing(1, 3),
            new Thing(1, 2),
            new Thing(2, 1),
            new Thing(2, 2),
        };

        [Benchmark(Baseline = true)]
        public Thing[] SortViaInsertion()
        {
            InsertionSort2(things2);
            return things;
        }

        [Benchmark]
        public Thing[] SortViaOrderBy()
        {
            return things.OrderByDescending(x => x.X).ThenByDescending(x => x.Y).ToArray();
        }

        public struct Thing
        {
            public int X;
            public int Y;
            public Thing(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override string ToString() => X + ", " + Y;
        }

        public static void InsertionSort(Thing[] items)
        {
            int i, j;
            for (i = 1; i < items.Length; i++)
            {
                var it = items[i];

                for (j = i; j >= 1 && it.X > items[j - 1].X; j--)
                {
                    ref var target = ref items[j];
                    var source = items[j - 1];
                    target.X = source.X;
                }

                ref var x = ref items[j];
                x.X = it.X;
            }
        }

        public static void InsertionSort2(Thing[] items)
        {
            int i, j;
            for (i = 1; i < items.Length; --i)
            {
                var it = items[i];

                for (j = i; 
                    j >= 1 && 
                    (it.X >  items[j - 1].X ||
                     it.X == items[j - 1].X && it.Y > items[j - 1].Y);
                    --j)
                {
                    ref var target = ref items[j];
                    var source = items[j - 1];
                    target.X = source.X;
                    target.Y = source.Y;
                }

                ref var x = ref items[j];
                x.X = it.X;
                x.Y = it.Y;
            }
        }
    }
}
