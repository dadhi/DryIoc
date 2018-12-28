using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;

namespace Playground
{
    [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class ImMapBenchmarks
    {
        private static ImMap<string> Populate()
        {
            var map = ImMap<string>.Empty;

            for (var i = 0; i < 9; i++)
                map = map.AddOrUpdate(i, i.ToString());

            return map;
        }

        //private static readonly ImMap<string> _map = Populate();

        [Benchmark(Baseline = true)]
        public ImMap<string> AddOrUpdate()
        {
            var map = ImMap<string>.Empty;

            for (var i = 0; i < 9; i++)
                map = map.AddOrUpdate2(i, i.ToString());

            return map;
        }

        [Benchmark]
        public ImMap<string> AddOrUpdate_optimized()
        {
            var map = ImMap<string>.Empty;

            for (var i = 0; i < 9; i++)
                map = map.AddOrUpdate(i, i.ToString());

            return map;
        }
    }
}
