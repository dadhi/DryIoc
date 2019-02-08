using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;

namespace Playground
{
    [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class MatchCaseOrder
    {
        private readonly int[] _items = { 42, 43, 44, 45 };

        [Benchmark(Baseline = true)]
        public object Match0to3() => _items.Match(x => x % 2 == 0);

        [Benchmark]
        public object Match3to0() => _items.Match2(x => x % 2 == 0);
    }

    public static class Arr
    {
        public static T[] Match2<T>(this T[] source, Func<T, bool> condition)
        {
            if (source == null || source.Length == 0)
                return source;

            if (source.Length > 2)
            {
                var matchStart = 0;
                T[] matches = null;
                var matchFound = false;

                var i = 0;
                while (i < source.Length)
                {
                    matchFound = condition(source[i]);
                    if (!matchFound)
                    {
                        // for accumulated matched items
                        if (i != 0 && i > matchStart)
                            matches = ArrayTools.AppendTo(source, matchStart, i - matchStart, matches);
                        matchStart = i + 1; // guess the next match start will be after the non-matched item
                    }

                    ++i;
                }

                // when last match was found but not all items are matched (hence matchStart != 0)
                if (matchFound && matchStart != 0)
                    return ArrayTools.AppendTo(source, matchStart, i - matchStart, matches);

                if (matches != null)
                    return matches;

                if (matchStart != 0) // no matches
                    return ArrayTools.Empty<T>();

                return source;
            }

            if (source.Length == 2)
            {
                var condition0 = condition(source[0]);
                var condition1 = condition(source[1]);
                return condition0 && condition1 ? new[] { source[0], source[1] }
                    : condition0 ? new[] { source[0] }
                    : condition1 ? new[] { source[1] }
                    : ArrayTools.Empty<T>();
            }

            return condition(source[0]) ? source : ArrayTools.Empty<T>();
        }
    }
}
