using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using DryIoc.ImTools;

namespace Playground
{
    [MemoryDiagnoser]
    public class MatchExperiments
    {
/*

## Match3

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19042.928/20H2/October2020Update)
Intel Core i5-8350U CPU 1.70GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

## 2 slices

```cs
private readonly int[] _items = { 42, 43, 44, 45 };
```

| Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------- |---------:|---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Match1 | 39.10 ns | 0.443 ns | 0.414 ns | 39.08 ns |  1.00 |    0.00 | 0.0204 |      64 B |        1.00 |
| Match2 | 44.66 ns | 0.988 ns | 1.138 ns | 44.56 ns |  1.14 |    0.03 | 0.0204 |      64 B |        1.00 |
| Match3 | 31.20 ns | 0.710 ns | 1.243 ns | 30.75 ns |  0.80 |    0.03 | 0.0229 |      72 B |        1.12 |
| Match4 | 29.19 ns | 0.632 ns | 1.539 ns | 28.67 ns |  0.74 |    0.03 | 0.0102 |      32 B |        0.50 |

## 5 slices

```cs
private readonly int[] _items = { 4, 1, 42, 44, 45, 46, 47, 48, 49, 52 };
```

| Method |      Mean |    Error |   StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------- |----------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
| Match1 | 111.13 ns | 2.275 ns | 3.862 ns |  1.00 |    0.00 | 0.0663 |     208 B |        1.00 |
| Match2 | 106.21 ns | 1.918 ns | 1.700 ns |  0.96 |    0.03 | 0.0663 |     208 B |        1.00 |
| Match3 |  54.01 ns | 1.053 ns | 1.126 ns |  0.49 |    0.02 | 0.0356 |     112 B |        0.54 |
| Match4 | 110.05 ns | 1.568 ns | 1.225 ns |  1.00 |    0.03 | 0.0433 |     136 B |        0.65 |
*/

        private readonly int[] _items = { 42, 43, 44, 45 };
        // private readonly int[] _items = { 4, 1, 42, 44, 45, 46, 47, 48, 49, 52 };

        [Benchmark(Baseline = true)]
        public object Match1() => _items.Match(x => (x & 1) == 0);

        [Benchmark]
        public object Match2() => _items.Match2(x => (x & 1) == 0);

        [Benchmark]
        public object Match3() => _items.Match3(x => (x & 1) == 0);

        [Benchmark]
        public object Match4() => _items.Match4(x => (x & 1) == 0);
    }

    public static class Arr
    {
        public static T[] Match4<T>(this T[] source, Func<T, bool> condition)
        {
            var inMatch = false;
            var matchesCount = 0;

            var firstMatchStart = 0;
            var firstMatchCount = 0;
            var secondMatchStart = 0;
            var secondMatchCount = 0;
            var nextMatchStart = 0;

            T[] result = null;
            for (var i = 0; (uint)i < source.Length; i++)
            {
                if (condition(source[i]))
                {
                    if (!inMatch)
                    {
                        inMatch = true;
                        ++matchesCount;
                        if (matchesCount == 1)
                            firstMatchStart = i;
                        else if (matchesCount == 2)
                            secondMatchStart = i;
                        else
                            nextMatchStart = i;
                    }
                }
                else if (inMatch)
                {
                    inMatch = false;
                    if (matchesCount == 1)
                        firstMatchCount = i - firstMatchStart;
                    else if (matchesCount == 2)
                        secondMatchCount = i - secondMatchStart;
                    else if (result == null)
                    {
                        result = new T[firstMatchCount + secondMatchCount + i - nextMatchStart];
                        Array.Copy(source, firstMatchStart, result, 0, firstMatchCount);
                        Array.Copy(source, secondMatchStart, result, firstMatchCount, secondMatchCount);
                        Array.Copy(source, nextMatchStart, result, firstMatchCount + secondMatchCount, i - nextMatchStart);
                    }
                    else
                        result = source.AppendTo(result, nextMatchStart, i - nextMatchStart);
                }
            }

            if (inMatch)
            {
                if (matchesCount == 1)
                    firstMatchCount = source.Length - firstMatchStart;
                else if (matchesCount == 2)
                    secondMatchCount = source.Length - secondMatchStart;
                else if (result == null)
                {
                    result = new T[firstMatchCount + secondMatchCount + source.Length - nextMatchStart];
                    Array.Copy(source, firstMatchStart, result, 0, firstMatchCount);
                    Array.Copy(source, secondMatchStart, result, firstMatchCount, secondMatchCount);
                    Array.Copy(source, nextMatchStart, result, firstMatchCount + secondMatchCount, source.Length - nextMatchStart);
                }
                else
                    result = source.AppendTo(result, nextMatchStart, source.Length - nextMatchStart);
            }

            if (matchesCount < 3)
            {
                result = new T[firstMatchCount + secondMatchCount];

                if (firstMatchCount == 1)
                    result[0] = source[firstMatchStart];
                else
                    Array.Copy(source, firstMatchStart, result, 0, firstMatchCount);

                if (matchesCount != 1)
                {
                    if (secondMatchCount == 1)
                        result[firstMatchCount] = source[secondMatchStart];
                    else
                        Array.Copy(source, secondMatchStart, result, firstMatchCount, secondMatchCount);
                }

                return result;
            }

            return result ?? ArrayTools.Empty<T>();
        }

        public static T[] Match3<T>(this T[] source, Func<T, bool> condition)
        {
            var results = new T[source.Length];

            int matched = 0;
            for (var i = 0; (uint)i < source.Length; i++)
                if (condition(source[i]))
                    results[matched++] = source[i];

            Array.Resize(ref results, matched);
            return results;
        }

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
                            // matches = ArrayTools.AppendTo(source, matchStart, i - matchStart, matches);
                            matches = matches == null
                                ? source.Copy(matchStart, i - matchStart)
                                : source.AppendTo(matches, matchStart, i - matchStart);
                        matchStart = i + 1; // guess the next match start will be after the non-matched item
                    }

                    ++i;
                }

                // when last match was found but not all items are matched (hence matchStart != 0)
                if (matchFound && matchStart != 0)
                    // return ArrayTools.AppendTo(source, matchStart, i - matchStart, matches);
                    return matches == null
                        ? source.Copy(matchStart, i - matchStart)
                        : source.AppendTo(matches, matchStart, i - matchStart);

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
