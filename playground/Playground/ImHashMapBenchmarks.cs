using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;

namespace Playground
{
    public class ImHashMapBenchmarks
    {
        private static readonly Type[] _keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(100).ToArray();

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Populate
        {
            [Benchmark]
            public ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a", out _, out _, (type, _, v) => v);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _, (type, _, v) => v);
            }

            //[Benchmark(Baseline = true)]
            //public ImHashMap<Type, string> AddOrUpdate_original()
            //{
            //    var map = ImHashMap<Type, string>.Empty;

            //    foreach (var key in _keys)
            //        map = map.AddOrUpdate_original(key, "a", out _, out _, (type, _, v) => v);

            //    return map.AddOrUpdate_original(typeof(ImHashMapBenchmarks), "!", out _, out _, (type, _, v) => v);
            //}

            //[Benchmark]
            public ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys)
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!!!");

                return map;
            }
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Lookup
        {
            public static ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys)
                    map = map.AddOrUpdate(key, "a");

                map = map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!!!");

                return map;
            }

            private static readonly ImHashMap<Type, string> _map = AddOrUpdate();

            public static ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys)
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!!!");

                return map;
            }

            private static readonly ConcurrentDictionary<Type, string> _dict = ConcurrentDict();

            public static Type LookupKey = typeof(ImHashMapBenchmarks);

            //[Benchmark]
            //public string TryFind_instance()
            //{
            //    _map.TryFind_old(LookupKey, out var result);
            //    return result;
            //}

            [Benchmark(Baseline = true)]
            public string TryFind_static()
            {
                _map.TryFind(LookupKey, out var result);
                return result;
            }

            //[Benchmark]
            //public string GetValueOrDefault_instance()
            //{
            //    return _map.GetValueOrDefault_old(LookupKey);
            //}

            [Benchmark]
            public string GetValueOrDefault_static()
            {
                return _map.GetValueOrDefault(LookupKey);
            }

            //[Benchmark]
            public string ConcurrentDict_TryGet()
            {
                _dict.TryGetValue(LookupKey, out var result);
                return result;
            }
        }
    }
}
