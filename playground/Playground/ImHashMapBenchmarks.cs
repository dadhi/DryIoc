using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using ImTools;
using Microsoft.Collections.Extensions;

namespace Playground
{
    public class ImHashMapBenchmarks
    {
        private static readonly Type[] _keys = typeof(Dictionary<,>).Assembly.GetTypes().Take(1000).ToArray();

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Populate
        {
            /*
            ## 15.01.2019:

                     Method |     Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
            --------------- |---------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                AddOrUpdate | 15.84 us | 0.1065 us | 0.0944 us |  1.00 |    0.00 |      7.3242 |           - |           - |            33.87 KB |
             AddOrUpdate_v1 | 27.00 us | 0.1792 us | 0.1588 us |  1.71 |    0.02 |      7.7515 |           - |           - |            35.77 KB |

            
            ## 16.01.2019: Total test against ImHashMap V1, System ImmutableDictionary and ConcurrentDictionary

BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.523 (1803/April2018Update/Redstone4)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
Frequency=2156249 Hz, Resolution=463.7683 ns, Timer=TSC
.NET Core SDK=2.2.100
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


         Method | Count |           Mean |         Error |        StdDev |         Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |---------------:|--------------:|--------------:|---------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |    10 |       987.6 ns |      36.53 ns |      99.99 ns |       934.3 ns |  0.94 |    0.17 |      0.5589 |           - |           - |             2.58 KB |
    AddOrUpdate |    10 |     1,067.5 ns |      50.43 ns |     136.33 ns |     1,073.5 ns |  1.00 |    0.00 |      0.4044 |           - |           - |             1.87 KB |
 ConcurrentDict |    10 |     1,943.9 ns |      92.31 ns |      81.83 ns |     1,921.3 ns |  1.85 |    0.25 |      0.6371 |           - |           - |             2.95 KB |
 AddOrUpdate_v2 |    10 |     2,688.3 ns |     229.79 ns |     677.55 ns |     2,342.9 ns |  2.50 |    0.77 |      0.4349 |           - |           - |             2.02 KB |
  ImmutableDict |    10 |     5,903.1 ns |     749.28 ns |     801.72 ns |     5,607.8 ns |  5.66 |    1.06 |      0.5875 |           - |           - |             2.73 KB |
                |       |                |               |               |                |       |         |             |             |             |                     |
 ConcurrentDict |   100 |    14,476.1 ns |   1,184.05 ns |   1,215.93 ns |    14,193.3 ns |  0.48 |    0.21 |      3.6011 |      0.0305 |           - |            16.66 KB |
 AddOrUpdate_v1 |   100 |    16,999.4 ns |   1,522.75 ns |   4,441.93 ns |    14,281.8 ns |  0.59 |    0.25 |      8.4686 |           - |           - |            39.05 KB |
 AddOrUpdate_v2 |   100 |    28,695.4 ns |      41.78 ns |      32.62 ns |    28,697.6 ns |  0.94 |    0.33 |      7.7515 |           - |           - |            35.81 KB |
    AddOrUpdate |   100 |    31,854.4 ns |   2,882.03 ns |   8,497.72 ns |    36,547.9 ns |  1.00 |    0.00 |      7.3242 |           - |           - |            33.91 KB |
  ImmutableDict |   100 |    89,602.2 ns |   1,873.19 ns |   2,229.89 ns |    88,767.5 ns |  2.98 |    1.05 |      9.3994 |           - |           - |            43.68 KB |
                |       |                |               |               |                |       |         |             |             |             |                     |
 ConcurrentDict |  1000 |   219,064.7 ns |     559.14 ns |     466.91 ns |   218,894.7 ns |  0.69 |    0.01 |     49.3164 |     17.8223 |           - |           254.29 KB |
 AddOrUpdate_v1 |  1000 |   297,651.6 ns |   1,073.37 ns |     838.02 ns |   297,421.1 ns |  0.93 |    0.01 |    120.6055 |      3.4180 |           - |           556.41 KB |
    AddOrUpdate |  1000 |   319,478.3 ns |   2,768.11 ns |   2,161.16 ns |   319,079.8 ns |  1.00 |    0.00 |    113.2813 |      0.9766 |           - |           526.48 KB |
 AddOrUpdate_v2 |  1000 |   615,321.8 ns |  72,410.43 ns | 213,503.80 ns |   467,207.0 ns |  1.97 |    0.66 |    118.6523 |      0.4883 |           - |            547.3 KB |
  ImmutableDict |  1000 | 1,516,613.7 ns | 107,376.35 ns | 290,298.20 ns | 1,387,325.2 ns |  4.95 |    1.19 |    140.6250 |      1.9531 |           - |           648.02 KB |

## 21.01.2019 - all versions compared

         Method | Count |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |    30 |   3.326 us | 0.0098 us | 0.0092 us |  0.84 |    0.01 |      2.0409 |           - |           - |             9.42 KB |
 AddOrUpdate_v2 |    30 |   3.515 us | 0.0700 us | 0.0688 us |  0.89 |    0.02 |      1.9302 |           - |           - |             8.91 KB |
    AddOrUpdate |    30 |   3.945 us | 0.0261 us | 0.0231 us |  1.00 |    0.00 |      2.1591 |           - |           - |             9.98 KB |
 AddOrUpdate_v3 |    30 |   4.607 us | 0.0875 us | 0.0819 us |  1.17 |    0.02 |      1.7624 |           - |           - |             8.13 KB |
                |       |            |           |           |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   150 |  24.883 us | 0.4840 us | 0.4527 us |  0.86 |    0.02 |     13.5193 |           - |           - |            62.39 KB |
 AddOrUpdate_v2 |   150 |  26.862 us | 0.5042 us | 0.4717 us |  0.92 |    0.02 |     13.7024 |           - |           - |             63.2 KB |
    AddOrUpdate |   150 |  29.054 us | 0.1205 us | 0.1127 us |  1.00 |    0.00 |     15.1978 |           - |           - |            70.17 KB |
 AddOrUpdate_v3 |   150 |  35.585 us | 0.3740 us | 0.3498 us |  1.22 |    0.01 |     12.5732 |           - |           - |            58.05 KB |
                |       |            |           |           |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   500 | 124.728 us | 0.7225 us | 0.6759 us |  0.91 |    0.01 |     54.4434 |           - |           - |           251.95 KB |
 AddOrUpdate_v2 |   500 | 128.650 us | 1.4938 us | 1.3973 us |  0.94 |    0.01 |     58.1055 |      0.2441 |           - |           267.97 KB |
    AddOrUpdate |   500 | 137.325 us | 1.9010 us | 1.7782 us |  1.00 |    0.00 |     63.2324 |      0.2441 |           - |           291.42 KB |
 AddOrUpdate_v3 |   500 | 166.994 us | 1.7109 us | 1.6004 us |  1.22 |    0.02 |     52.7344 |           - |           - |           243.81 KB |

## Inlining With and removing not necessary call to Balance in case of Update. 
 
         Method | Count |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |    30 |   3.323 us | 0.0121 us | 0.0107 us |  0.91 |    0.00 |      2.0409 |           - |           - |             9.42 KB |
    AddOrUpdate |    30 |   3.647 us | 0.0111 us | 0.0093 us |  1.00 |    0.00 |      2.0409 |           - |           - |             9.42 KB |
                |       |            |           |           |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   150 |  24.605 us | 0.2023 us | 0.1690 us |  0.92 |    0.02 |     13.5193 |           - |           - |            62.39 KB |
    AddOrUpdate |   150 |  26.832 us | 0.5300 us | 0.5443 us |  1.00 |    0.00 |     13.5193 |           - |           - |            62.39 KB |
                |       |            |           |           |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   500 | 121.799 us | 0.9309 us | 0.8252 us |  0.92 |    0.01 |     54.4434 |           - |           - |           251.95 KB |
    AddOrUpdate |   500 | 132.886 us | 2.3470 us | 2.0805 us |  1.00 |    0.00 |     54.4434 |           - |           - |           251.95 KB |


## Special fast logic for adding to empty branch.

         Method | Count |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |    30 |   3.369 us | 0.0102 us | 0.0090 us |  0.94 |    0.01 |      2.0409 |           - |           - |             9.42 KB |
    AddOrUpdate |    30 |   3.587 us | 0.0244 us | 0.0228 us |  1.00 |    0.00 |      2.0409 |           - |           - |             9.42 KB |
                |       |            |           |           |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   150 |  25.025 us | 0.4927 us | 0.4609 us |  0.95 |    0.02 |     13.5193 |           - |           - |            62.39 KB |
    AddOrUpdate |   150 |  26.235 us | 0.1058 us | 0.0989 us |  1.00 |    0.00 |     13.5193 |           - |           - |            62.39 KB |
                |       |            |           |           |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   500 | 126.106 us | 1.0204 us | 0.9545 us |  1.00 |    0.01 |     54.4434 |           - |           - |           251.95 KB |
    AddOrUpdate |   500 | 126.844 us | 0.8788 us | 0.7339 us |  1.00 |    0.00 |     54.4434 |           - |           - |           251.95 KB |

## Removing not necessary imbalanced tree creation before balance - first memory win.

         Method | Count |       Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
    AddOrUpdate |    30 |   3.357 us | 0.0163 us | 0.0145 us |  1.00 |      1.9417 |           - |           - |             8.95 KB |
 AddOrUpdate_v1 |    30 |   3.359 us | 0.0124 us | 0.0116 us |  1.00 |      2.0409 |           - |           - |             9.42 KB |
                |       |            |           |           |       |             |             |             |                     |
 AddOrUpdate_v1 |   150 |  24.513 us | 0.0420 us | 0.0393 us |  0.98 |     13.5193 |           - |           - |            62.39 KB |
    AddOrUpdate |   150 |  25.074 us | 0.1160 us | 0.1085 us |  1.00 |     12.9395 |      0.0305 |           - |            59.72 KB |
                |       |            |           |           |       |             |             |             |                     |
    AddOrUpdate |   500 | 122.624 us | 0.6553 us | 0.6130 us |  1.00 |     52.4902 |      0.2441 |           - |           242.06 KB |
 AddOrUpdate_v1 |   500 | 122.656 us | 0.8018 us | 0.7500 us |  1.00 |     54.4434 |           - |           - |           251.95 KB |

## More variety to benchmark input

         Method | Count |         Mean |        Error |       StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|-------------:|------:|------------:|------------:|------------:|--------------------:|
    AddOrUpdate |     5 |     472.4 ns |     2.139 ns |     1.786 ns |  1.00 |      0.2537 |           - |           - |             1.17 KB |
 AddOrUpdate_v1 |     5 |     488.1 ns |     2.258 ns |     2.112 ns |  1.03 |      0.2737 |           - |           - |             1.27 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |    40 |   4,855.4 ns |    19.395 ns |    17.194 ns |  0.98 |      2.9678 |           - |           - |            13.69 KB |
    AddOrUpdate |    40 |   4,974.1 ns |    17.098 ns |    15.157 ns |  1.00 |      2.8000 |           - |           - |            12.94 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |   200 |  35,267.4 ns |   100.767 ns |    84.145 ns |  0.98 |     18.7378 |      0.0610 |           - |            86.53 KB |
    AddOrUpdate |   200 |  35,874.3 ns |   173.786 ns |   162.560 ns |  1.00 |     18.0054 |           - |           - |            83.02 KB |
                |       |              |              |              |       |             |             |             |                     |
    AddOrUpdate |  1000 | 302,900.4 ns | 1,116.252 ns | 1,044.143 ns |  1.00 |    115.7227 |      2.4414 |           - |           535.08 KB |
 AddOrUpdate_v1 |  1000 | 303,552.8 ns |   906.769 ns |   803.827 ns |  1.00 |    120.6055 |      3.4180 |           - |           556.41 KB |

## Remove unnecessary temporary left leaf(y) branch creation before balancing - memory win

         Method | Count |         Mean |        Error |       StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|-------------:|------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |     5 |     491.4 ns |     2.121 ns |     1.984 ns |  1.00 |      0.2737 |           - |           - |             1.27 KB |
    AddOrUpdate |     5 |     493.4 ns |     2.199 ns |     2.057 ns |  1.00 |      0.2537 |           - |           - |             1.17 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |    40 |   4,871.0 ns |    26.907 ns |    23.852 ns |  0.98 |      2.9678 |           - |           - |            13.69 KB |
    AddOrUpdate |    40 |   4,949.1 ns |    10.957 ns |     9.713 ns |  1.00 |      2.7542 |           - |           - |             12.7 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |   200 |  35,540.5 ns |   271.146 ns |   240.364 ns |  0.95 |     18.7378 |      0.0610 |           - |            86.53 KB |
    AddOrUpdate |   200 |  37,344.9 ns |   127.081 ns |   118.871 ns |  1.00 |     17.6392 |           - |           - |            81.38 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |  1000 | 308,033.2 ns | 1,643.015 ns | 1,536.877 ns |  0.98 |    120.6055 |      3.4180 |           - |           556.41 KB |
    AddOrUpdate |  1000 | 314,370.2 ns | 1,781.632 ns | 1,666.540 ns |  1.00 |    113.7695 |      0.4883 |           - |           525.09 KB |


## Remove unnecessary temporary right leaf(y) branch creation before balancing - memory win

         Method | Count |         Mean |        Error |       StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|-------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v1 |     5 |     489.0 ns |     2.395 ns |     2.241 ns |  0.99 |    0.01 |      0.2737 |           - |           - |             1.27 KB |
    AddOrUpdate |     5 |     496.0 ns |     3.869 ns |     3.619 ns |  1.00 |    0.00 |      0.2432 |           - |           - |             1.13 KB |
                |       |              |              |              |       |         |             |             |             |                     |
 AddOrUpdate_v1 |    40 |   4,873.1 ns |    35.313 ns |    33.032 ns |  0.91 |    0.01 |      2.9678 |           - |           - |            13.69 KB |
    AddOrUpdate |    40 |   5,346.9 ns |    18.590 ns |    16.480 ns |  1.00 |    0.00 |      2.6779 |           - |           - |            12.38 KB |
                |       |              |              |              |       |         |             |             |             |                     |
 AddOrUpdate_v1 |   200 |  36,484.4 ns |   309.583 ns |   274.437 ns |  0.90 |    0.01 |     18.7378 |      0.0610 |           - |            86.53 KB |
    AddOrUpdate |   200 |  40,641.9 ns |   628.973 ns |   588.342 ns |  1.00 |    0.00 |     17.1509 |      0.0610 |           - |            79.08 KB |
                |       |              |              |              |       |         |             |             |             |                     |
 AddOrUpdate_v1 |  1000 | 316,678.8 ns | 5,722.780 ns | 5,353.092 ns |  0.98 |    0.02 |    120.6055 |      3.4180 |           - |           556.41 KB |
    AddOrUpdate |  1000 | 323,403.6 ns | 1,483.940 ns | 1,315.474 ns |  1.00 |    0.00 |    111.8164 |     32.7148 |           - |           515.39 KB |


## Parity with v1 via handling one more special case - where `Height == 1`

         Method | Count |         Mean |        Error |       StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|-------------:|------:|------------:|------------:|------------:|--------------------:|
    AddOrUpdate |     5 |     480.3 ns |     3.645 ns |     3.410 ns |  1.00 |      0.2432 |           - |           - |             1.13 KB |
 AddOrUpdate_v1 |     5 |     487.1 ns |     2.547 ns |     2.382 ns |  1.01 |      0.2737 |           - |           - |             1.27 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |    40 |   4,844.9 ns |    23.494 ns |    21.976 ns |  0.99 |      2.9678 |           - |           - |            13.69 KB |
    AddOrUpdate |    40 |   4,908.8 ns |    29.374 ns |    27.477 ns |  1.00 |      2.6779 |           - |           - |            12.38 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |   200 |  35,727.8 ns |    84.717 ns |    75.099 ns |  0.99 |     18.7378 |      0.0610 |           - |            86.53 KB |
    AddOrUpdate |   200 |  36,231.4 ns |   136.671 ns |   121.156 ns |  1.00 |     17.1509 |      0.0610 |           - |            79.08 KB |
                |       |              |              |              |       |             |             |             |                     |
    AddOrUpdate |  1000 | 304,238.0 ns | 1,636.331 ns | 1,366.410 ns |  1.00 |    111.8164 |     32.7148 |           - |           515.39 KB |
 AddOrUpdate_v1 |  1000 | 307,233.3 ns | 1,487.373 ns | 1,391.290 ns |  1.01 |    120.6055 |      3.4180 |           - |           556.41 KB |


## Some fixes for updating things

         Method | Count |         Mean |        Error |       StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|-------------:|------:|------------:|------------:|------------:|--------------------:|
    AddOrUpdate |     5 |     469.8 ns |     4.613 ns |     3.602 ns |  1.00 |      0.2432 |           - |           - |             1.13 KB |
 AddOrUpdate_v1 |     5 |     480.6 ns |     2.648 ns |     2.477 ns |  1.02 |      0.2737 |           - |           - |             1.27 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |    40 |   4,731.4 ns |    22.512 ns |    21.058 ns |  0.96 |      2.9678 |           - |           - |            13.69 KB |
    AddOrUpdate |    40 |   4,945.0 ns |    12.489 ns |    10.429 ns |  1.00 |      2.6779 |           - |           - |            12.38 KB |
                |       |              |              |              |       |             |             |             |                     |
 AddOrUpdate_v1 |   200 |  35,094.6 ns |    70.945 ns |    66.362 ns |  0.98 |     18.7378 |      0.0610 |           - |            86.53 KB |
    AddOrUpdate |   200 |  35,821.9 ns |   129.417 ns |   121.057 ns |  1.00 |     17.1509 |      0.0610 |           - |            79.08 KB |
                |       |              |              |              |       |             |             |             |                     |
    AddOrUpdate |  1000 | 303,448.5 ns | 2,017.835 ns | 1,887.484 ns |  1.00 |    111.8164 |     32.7148 |           - |           515.39 KB |
 AddOrUpdate_v1 |  1000 | 304,471.0 ns | 1,549.817 ns | 1,449.700 ns |  1.00 |    120.6055 |      3.4180 |           - |           556.41 KB |


## Inlining the Balance and removing its not used if-branches

         Method | Count |         Mean |        Error |       StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|-------------:|------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v2 |     5 |     418.2 ns |     2.540 ns |     2.376 ns |  0.90 |      0.2170 |           - |           - |              1024 B |
    AddOrUpdate |     5 |     464.6 ns |     2.301 ns |     2.039 ns |  1.00 |      0.2437 |           - |           - |              1152 B |
 AddOrUpdate_v1 |     5 |     487.7 ns |     1.769 ns |     1.655 ns |  1.05 |      0.2737 |           - |           - |              1296 B |
 AddOrUpdate_v3 |     5 |     516.5 ns |     2.289 ns |     2.141 ns |  1.11 |      0.1993 |           - |           - |               944 B |
                |       |              |              |              |       |             |             |             |                     |
    AddOrUpdate |    40 |   4,621.6 ns |    19.899 ns |    18.614 ns |  1.00 |      2.6779 |           - |           - |             12672 B |
 AddOrUpdate_v1 |    40 |   4,910.9 ns |    19.805 ns |    18.525 ns |  1.06 |      2.9678 |           - |           - |             14016 B |
 AddOrUpdate_v2 |    40 |   5,115.0 ns |    26.410 ns |    23.411 ns |  1.11 |      2.8458 |           - |           - |             13456 B |
 AddOrUpdate_v3 |    40 |   6,953.3 ns |    24.391 ns |    22.815 ns |  1.50 |      2.6016 |           - |           - |             12304 B |
                |       |              |              |              |       |             |             |             |                     |
    AddOrUpdate |   200 |  34,250.7 ns |    87.221 ns |    81.586 ns |  1.00 |     17.1509 |      0.0610 |           - |             80976 B |
 AddOrUpdate_v1 |   200 |  35,425.8 ns |   137.918 ns |   129.008 ns |  1.03 |     18.7378 |      0.0610 |           - |             88608 B |
 AddOrUpdate_v2 |   200 |  37,942.8 ns |   188.112 ns |   175.960 ns |  1.11 |     19.2261 |           - |           - |             91008 B |
 AddOrUpdate_v3 |   200 |  50,640.8 ns |    66.485 ns |    55.518 ns |  1.48 |     17.6392 |      0.0610 |           - |             83352 B |
                |       |              |              |              |       |             |             |             |                     |
    AddOrUpdate |  1000 | 290,830.8 ns |   915.770 ns |   811.806 ns |  1.00 |    111.8164 |     32.7148 |           - |            527760 B |
 AddOrUpdate_v2 |  1000 | 308,260.3 ns | 2,278.822 ns | 2,020.116 ns |  1.06 |    130.8594 |      0.9766 |           - |            619056 B |
 AddOrUpdate_v1 |  1000 | 308,355.6 ns | 2,046.461 ns | 1,914.261 ns |  1.06 |    120.6055 |      3.4180 |           - |            569760 B |
 AddOrUpdate_v3 |  1000 | 375,880.3 ns | 1,710.716 ns | 1,600.205 ns |  1.29 |    118.6523 |      0.4883 |           - |            560440 B |


         Method | Count |         Mean |        Error |        StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------- |------ |-------------:|-------------:|--------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 AddOrUpdate_v2 |     5 |     404.8 ns |     2.504 ns |     2.0907 ns |  0.91 |    0.01 |      0.2170 |           - |           - |                1 KB |
    AddOrUpdate |     5 |     447.1 ns |     4.673 ns |     4.1429 ns |  1.00 |    0.00 |      0.2437 |           - |           - |             1.13 KB |
 AddOrUpdate_v1 |     5 |     481.5 ns |     1.112 ns |     0.9854 ns |  1.08 |    0.01 |      0.2737 |           - |           - |             1.27 KB |
                |       |              |              |               |       |         |             |             |             |                     |
    AddOrUpdate |    40 |   4,563.0 ns |    91.302 ns |    89.6712 ns |  1.00 |    0.00 |      2.6779 |           - |           - |            12.38 KB |
 AddOrUpdate_v1 |    40 |   4,712.5 ns |    27.017 ns |    25.2716 ns |  1.03 |    0.02 |      2.9678 |           - |           - |            13.69 KB |
 AddOrUpdate_v2 |    40 |   4,943.3 ns |    22.715 ns |    21.2474 ns |  1.08 |    0.02 |      2.8458 |           - |           - |            13.14 KB |
                |       |              |              |               |       |         |             |             |             |                     |
    AddOrUpdate |   200 |  33,629.4 ns |   647.198 ns |   635.6353 ns |  1.00 |    0.00 |     17.1509 |      0.0610 |           - |            79.08 KB |
 AddOrUpdate_v1 |   200 |  34,643.5 ns |   155.549 ns |   145.5005 ns |  1.03 |    0.02 |     18.7378 |      0.0610 |           - |            86.53 KB |
 AddOrUpdate_v2 |   200 |  36,726.4 ns |   419.812 ns |   372.1521 ns |  1.10 |    0.01 |     19.2261 |           - |           - |            88.88 KB |
                |       |              |              |               |       |         |             |             |             |                     |
    AddOrUpdate |  1000 | 291,376.3 ns | 3,419.191 ns | 3,198.3136 ns |  1.00 |    0.00 |    111.8164 |     32.7148 |           - |           515.39 KB |
 AddOrUpdate_v2 |  1000 | 302,027.2 ns | 4,981.373 ns | 4,659.5798 ns |  1.04 |    0.02 |    130.8594 |      0.9766 |           - |           604.55 KB |
 AddOrUpdate_v1 |  1000 | 304,899.6 ns | 4,634.673 ns | 4,335.2763 ns |  1.05 |    0.02 |    120.6055 |      3.4180 |           - |           556.41 KB |

 */

            [Params(10, 100, 1000)]
            public int Count;

            [Benchmark(Baseline = true)]
            public ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a", out _, out _);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _);
            }

            [Benchmark]
            public V1.ImHashMap<Type, string> AddOrUpdate_v1()
            {
                var map = V1.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a");

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!");
            }

            //[Benchmark]
            public V2.ImHashMap<Type, string> AddOrUpdate_v2()
            {
                var map = V2.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a", out _, out _);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _);
            }

            //[Benchmark]
            public V3.ImHashMap<Type, string> AddOrUpdate_v3()
            {
                var map = V3.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a", out _, out _);

                return map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _);
            }

            [Benchmark]
            public ImmutableDictionary<Type, string> ImmutableDict()
            {
                var map = ImmutableDictionary<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.Add(key, "a");

                return map.Add(typeof(ImHashMapBenchmarks), "!");
            }

            [Benchmark]
            public ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys.Take(Count))
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!");

                return map;
            }
        }

        [MemoryDiagnoser, Orderer(SummaryOrderPolicy.FastestToSlowest)]
        public class Lookup
        {
            /*
## 21.01.2019: All versions.

               Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 GetValueOrDefault_v1 | 13.74 ns | 0.0686 ns | 0.0642 ns |  0.79 |           - |           - |           - |                   - |
    GetValueOrDefault | 17.43 ns | 0.0924 ns | 0.0864 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 | 19.15 ns | 0.0786 ns | 0.0656 ns |  1.10 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 | 25.73 ns | 0.0711 ns | 0.0665 ns |  1.48 |           - |           - |           - |                   - |

## For some reason dropping lookup speed with only changes to AddOrUpdate

               Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 GetValueOrDefault_v1 | 13.89 ns | 0.0938 ns | 0.0877 ns |  0.80 |           - |           - |           - |                   - |
    GetValueOrDefault | 17.40 ns | 0.0888 ns | 0.0831 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 | 19.04 ns | 0.0712 ns | 0.0666 ns |  1.09 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 | 25.93 ns | 0.0474 ns | 0.0420 ns |  1.49 |           - |           - |           - |                   - |

## Got back some perf by moving GetValueOrDefault to static method and specializing for Type

               Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
 GetValueOrDefault_v1 | 13.87 ns | 0.0400 ns | 0.0355 ns |  0.85 |           - |           - |           - |                   - |
    GetValueOrDefault | 16.34 ns | 0.0932 ns | 0.0826 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 | 19.18 ns | 0.0460 ns | 0.0430 ns |  1.17 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 | 25.96 ns | 0.0756 ns | 0.0707 ns |  1.59 |           - |           - |           - |                   - |

## Benchmark against variety of inputs on par with Populate benchmark

               Method | Count |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 GetValueOrDefault_v1 |     5 |  6.155 ns | 0.0321 ns | 0.0301 ns |  0.98 |    0.01 |           - |           - |           - |                   - |
    GetValueOrDefault |     5 |  6.267 ns | 0.0510 ns | 0.0452 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 |     5 |  7.439 ns | 0.0763 ns | 0.0676 ns |  1.19 |    0.02 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 |     5 |  9.558 ns | 0.0409 ns | 0.0383 ns |  1.52 |    0.01 |           - |           - |           - |                   - |
                      |       |           |           |           |       |         |             |             |             |                     |
 GetValueOrDefault_v1 |    40 | 10.897 ns | 0.0673 ns | 0.0629 ns |  0.95 |    0.01 |           - |           - |           - |                   - |
    GetValueOrDefault |    40 | 11.467 ns | 0.0325 ns | 0.0304 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 |    40 | 14.012 ns | 0.1092 ns | 0.1022 ns |  1.22 |    0.01 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 |    40 | 19.945 ns | 0.1032 ns | 0.0965 ns |  1.74 |    0.01 |           - |           - |           - |                   - |
                      |       |           |           |           |       |         |             |             |             |                     |
 GetValueOrDefault_v1 |   200 | 13.664 ns | 0.0291 ns | 0.0258 ns |  0.97 |    0.00 |           - |           - |           - |                   - |
    GetValueOrDefault |   200 | 14.051 ns | 0.0524 ns | 0.0491 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 |   200 | 16.722 ns | 0.0568 ns | 0.0531 ns |  1.19 |    0.01 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 |   200 | 24.473 ns | 0.0792 ns | 0.0702 ns |  1.74 |    0.01 |           - |           - |           - |                   - |
                      |       |           |           |           |       |         |             |             |             |                     |
 GetValueOrDefault_v1 |  1000 | 14.213 ns | 0.1528 ns | 0.1354 ns |  0.96 |    0.01 |           - |           - |           - |                   - |
    GetValueOrDefault |  1000 | 14.805 ns | 0.0518 ns | 0.0485 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v2 |  1000 | 16.645 ns | 0.0447 ns | 0.0419 ns |  1.12 |    0.01 |           - |           - |           - |                   - |
 GetValueOrDefault_v3 |  1000 | 27.489 ns | 0.0890 ns | 0.0832 ns |  1.86 |    0.01 |           - |           - |           - |                   - |

## Adding aggressive inlining to the Data { Hash, Key, Value } properties

               Method | Count |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |------ |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
    GetValueOrDefault |     5 |  5.853 ns | 0.0685 ns | 0.0607 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |     5 |  5.913 ns | 0.0373 ns | 0.0349 ns |  1.01 |           - |           - |           - |                   - |
                      |       |           |           |           |       |             |             |             |                     |
    GetValueOrDefault |    40 |  9.649 ns | 0.0235 ns | 0.0220 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |    40 | 10.266 ns | 0.0236 ns | 0.0221 ns |  1.06 |           - |           - |           - |                   - |
                      |       |           |           |           |       |             |             |             |                     |
    GetValueOrDefault |   200 | 11.613 ns | 0.0554 ns | 0.0491 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |   200 | 12.052 ns | 0.0555 ns | 0.0520 ns |  1.04 |           - |           - |           - |                   - |

## Using  `!= Empty` instead of `.Height != 0` drops some perf

               Method | Count |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
 GetValueOrDefault_v1 |     5 |  5.933 ns | 0.0310 ns | 0.0290 ns |  0.93 |    0.03 |           - |           - |           - |                   - |
    GetValueOrDefault |     5 |  6.386 ns | 0.1807 ns | 0.1602 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
                      |       |           |           |           |       |         |             |             |             |                     |
    GetValueOrDefault |    40 |  9.820 ns | 0.0521 ns | 0.0488 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |    40 | 10.257 ns | 0.0300 ns | 0.0266 ns |  1.05 |    0.01 |           - |           - |           - |                   - |
                      |       |           |           |           |       |         |             |             |             |                     |
    GetValueOrDefault |   200 | 11.717 ns | 0.0689 ns | 0.0644 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |   200 | 12.104 ns | 0.0548 ns | 0.0486 ns |  1.03 |    0.01 |           - |           - |           - |                   - |

## Removing `.Height != 0` check completely did not change much, but let it stay cause less code is better

               Method | Count |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |------ |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
    GetValueOrDefault |     5 |  5.903 ns | 0.0612 ns | 0.0573 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |     5 |  5.931 ns | 0.0503 ns | 0.0470 ns |  1.00 |           - |           - |           - |                   - |
                      |       |           |           |           |       |             |             |             |                     |
    GetValueOrDefault |    40 |  9.636 ns | 0.0419 ns | 0.0392 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |    40 | 10.231 ns | 0.0333 ns | 0.0312 ns |  1.06 |           - |           - |           - |                   - |
                      |       |           |           |           |       |             |             |             |                     |
    GetValueOrDefault |   200 | 11.637 ns | 0.0721 ns | 0.0602 ns |  1.00 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |   200 | 12.042 ns | 0.0607 ns | 0.0568 ns |  1.03 |           - |           - |           - |                   - |

## TryFind

             Method | Count |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
        ----------- |------ |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
         TryFind_v1 |     5 |  5.229 ns | 0.0307 ns | 0.0257 ns |  1.00 |           - |           - |           - |                   - |
            TryFind |     5 |  6.766 ns | 0.0695 ns | 0.0650 ns |  1.30 |           - |           - |           - |                   - |
                    |       |           |           |           |       |             |             |             |                     |
         TryFind_v1 |    40 |  9.268 ns | 0.0116 ns | 0.0108 ns |  1.00 |           - |           - |           - |                   - |
            TryFind |    40 |  9.755 ns | 0.0219 ns | 0.0205 ns |  1.05 |           - |           - |           - |                   - |
                    |       |           |           |           |       |             |             |             |                     |
         TryFind_v1 |   200 | 11.773 ns | 0.0558 ns | 0.0494 ns |  1.00 |           - |           - |           - |                   - |
            TryFind |   200 | 12.212 ns | 0.0456 ns | 0.0380 ns |  1.04 |           - |           - |           - |                   - |

     Method | Count |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
----------- |------ |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
    TryFind |     5 |  5.906 ns | 0.0191 ns | 0.0178 ns |  0.97 |           - |           - |           - |                   - |
 TryFind_v1 |     5 |  6.079 ns | 0.0947 ns | 0.0839 ns |  1.00 |           - |           - |           - |                   - |
            |       |           |           |           |       |             |             |             |                     |
    TryFind |    40 |  9.211 ns | 0.0214 ns | 0.0200 ns |  0.87 |           - |           - |           - |                   - |
 TryFind_v1 |    40 | 10.566 ns | 0.0149 ns | 0.0132 ns |  1.00 |           - |           - |           - |                   - |
            |       |           |           |           |       |             |             |             |                     |
    TryFind |   200 | 11.400 ns | 0.1152 ns | 0.1078 ns |  0.88 |           - |           - |           - |                   - |
 TryFind_v1 |   200 | 12.929 ns | 0.0712 ns | 0.0666 ns |  1.00 |           - |           - |           - |                   - |

    ## GetOrDefault a bit optimized
    
               Method | Count |      Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------- |------ |----------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
    GetValueOrDefault |     5 |  6.782 ns | 0.0449 ns | 0.0398 ns |  0.96 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |     5 |  7.042 ns | 0.0659 ns | 0.0616 ns |  1.00 |           - |           - |           - |                   - |
                      |       |           |           |           |       |             |             |             |                     |
    GetValueOrDefault |    40 | 10.962 ns | 0.0866 ns | 0.0768 ns |  0.99 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |    40 | 11.094 ns | 0.0973 ns | 0.0813 ns |  1.00 |           - |           - |           - |                   - |
                      |       |           |           |           |       |             |             |             |                     |
    GetValueOrDefault |   200 | 13.329 ns | 0.0338 ns | 0.0299 ns |  0.97 |           - |           - |           - |                   - |
 GetValueOrDefault_v1 |   200 | 13.722 ns | 0.0537 ns | 0.0448 ns |  1.00 |           - |           - |           - |                   - |

    ## The whole result for the docs

                Method | Count |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
---------------------- |------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
            TryFind_v1 |    10 |  7.274 ns | 0.0410 ns | 0.0384 ns |  0.98 |    0.01 |           - |           - |           - |                   - |
               TryFind |    10 |  7.422 ns | 0.0237 ns | 0.0222 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 ConcurrentDict_TryGet |    10 | 21.664 ns | 0.0213 ns | 0.0189 ns |  2.92 |    0.01 |           - |           - |           - |                   - |
  ImmutableDict_TryGet |    10 | 71.199 ns | 0.1312 ns | 0.1228 ns |  9.59 |    0.03 |           - |           - |           - |                   - |
                       |       |           |           |           |       |         |             |             |             |                     |
            TryFind_v1 |   100 |  8.426 ns | 0.0236 ns | 0.0221 ns |  0.91 |    0.00 |           - |           - |           - |                   - |
               TryFind |   100 |  9.304 ns | 0.0305 ns | 0.0270 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
 ConcurrentDict_TryGet |   100 | 21.791 ns | 0.1072 ns | 0.0951 ns |  2.34 |    0.01 |           - |           - |           - |                   - |
  ImmutableDict_TryGet |   100 | 74.985 ns | 0.1053 ns | 0.0879 ns |  8.06 |    0.03 |           - |           - |           - |                   - |
                       |       |           |           |           |       |         |             |             |             |                     |
               TryFind |  1000 | 13.837 ns | 0.0291 ns | 0.0272 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
            TryFind_v1 |  1000 | 16.108 ns | 0.0415 ns | 0.0367 ns |  1.16 |    0.00 |           - |           - |           - |                   - |
 ConcurrentDict_TryGet |  1000 | 21.876 ns | 0.0325 ns | 0.0288 ns |  1.58 |    0.00 |           - |           - |           - |                   - |
  ImmutableDict_TryGet |  1000 | 83.563 ns | 0.1046 ns | 0.0873 ns |  6.04 |    0.01 |           - |           - |           - |                   - |


    ## 2019-03-28: Comparing vs `Dictionary<K, V>`:

                           Method | Count |      Mean |     Error |    StdDev |    Median | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------------------- |------ |----------:|----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
                          TryFind |    10 |  7.722 ns | 0.0451 ns | 0.0422 ns |  7.718 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
           Dictionary_TryGetValue |    10 | 18.475 ns | 0.0502 ns | 0.0470 ns | 18.470 ns |  2.39 |    0.01 |           - |           - |           - |                   - |
 ConcurrentDictionary_TryGetValue |    10 | 22.661 ns | 0.0463 ns | 0.0433 ns | 22.653 ns |  2.93 |    0.02 |           - |           - |           - |                   - |
             ImmutableDict_TryGet |    10 | 72.911 ns | 1.5234 ns | 2.1355 ns | 74.134 ns |  9.25 |    0.23 |           - |           - |           - |                   - |
                                  |       |           |           |           |           |       |         |             |             |             |                     |
                          TryFind |   100 |  9.987 ns | 0.0543 ns | 0.0508 ns |  9.978 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
           Dictionary_TryGetValue |   100 | 18.110 ns | 0.0644 ns | 0.0602 ns | 18.088 ns |  1.81 |    0.01 |           - |           - |           - |                   - |
 ConcurrentDictionary_TryGetValue |   100 | 24.402 ns | 0.0978 ns | 0.0915 ns | 24.435 ns |  2.44 |    0.02 |           - |           - |           - |                   - |
             ImmutableDict_TryGet |   100 | 76.689 ns | 0.3632 ns | 0.3397 ns | 76.704 ns |  7.68 |    0.04 |           - |           - |           - |                   - |
                                  |       |           |           |           |           |       |         |             |             |             |                     |
                          TryFind |  1000 | 12.600 ns | 0.1506 ns | 0.1335 ns | 12.551 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
           Dictionary_TryGetValue |  1000 | 19.023 ns | 0.0575 ns | 0.0538 ns | 19.036 ns |  1.51 |    0.02 |           - |           - |           - |                   - |
 ConcurrentDictionary_TryGetValue |  1000 | 22.651 ns | 0.1238 ns | 0.1097 ns | 22.613 ns |  1.80 |    0.02 |           - |           - |           - |                   - |
             ImmutableDict_TryGet |  1000 | 83.608 ns | 0.3105 ns | 0.2904 ns | 83.612 ns |  6.64 |    0.07 |           - |           - |           - |                   - |

    ## 2019-03-29: Comparing vs `DictionarySlim<K, V>`:

                           Method | Count |      Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
--------------------------------- |------ |----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
       DictionarySlim_TryGetValue |    10 |  8.228 ns | 0.0682 ns | 0.0604 ns |  1.00 |    0.01 |           - |           - |           - |                   - |
                          TryFind |    10 |  8.257 ns | 0.0796 ns | 0.0706 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
           Dictionary_TryGetValue |    10 | 19.615 ns | 0.0251 ns | 0.0209 ns |  2.38 |    0.02 |           - |           - |           - |                   - |
 ConcurrentDictionary_TryGetValue |    10 | 22.339 ns | 0.0922 ns | 0.0863 ns |  2.71 |    0.03 |           - |           - |           - |                   - |
             ImmutableDict_TryGet |    10 | 69.872 ns | 0.2699 ns | 0.2524 ns |  8.46 |    0.07 |           - |           - |           - |                   - |
                                  |       |           |           |           |       |         |             |             |             |                     |
       DictionarySlim_TryGetValue |   100 |  8.351 ns | 0.1613 ns | 0.1508 ns |  0.69 |    0.01 |           - |           - |           - |                   - |
                          TryFind |   100 | 12.144 ns | 0.0570 ns | 0.0533 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
           Dictionary_TryGetValue |   100 | 17.985 ns | 0.0880 ns | 0.0823 ns |  1.48 |    0.01 |           - |           - |           - |                   - |
 ConcurrentDictionary_TryGetValue |   100 | 22.312 ns | 0.0564 ns | 0.0471 ns |  1.84 |    0.01 |           - |           - |           - |                   - |
             ImmutableDict_TryGet |   100 | 75.374 ns | 0.3042 ns | 0.2846 ns |  6.21 |    0.04 |           - |           - |           - |                   - |
                                  |       |           |           |           |       |         |             |             |             |                     |
       DictionarySlim_TryGetValue |  1000 |  8.202 ns | 0.0713 ns | 0.0667 ns |  0.55 |    0.01 |           - |           - |           - |                   - |
                          TryFind |  1000 | 14.919 ns | 0.1101 ns | 0.0919 ns |  1.00 |    0.00 |           - |           - |           - |                   - |
           Dictionary_TryGetValue |  1000 | 18.073 ns | 0.2415 ns | 0.2141 ns |  1.21 |    0.02 |           - |           - |           - |                   - |
 ConcurrentDictionary_TryGetValue |  1000 | 22.406 ns | 0.1039 ns | 0.0921 ns |  1.50 |    0.01 |           - |           - |           - |                   - |
             ImmutableDict_TryGet |  1000 | 84.215 ns | 0.2835 ns | 0.2513 ns |  5.65 |    0.04 |           - |           - |           - |                   - |

*/
            [Params(10, 100, 1000)]// the 1000 does not add anything as the LookupKey stored higher in the tree, 1000)]
            public int Count;

            [GlobalSetup]
            public void Populate()
            {
                _map = AddOrUpdate();
                _mapV1 = AddOrUpdate_v1();
                //_mapV2 = AddOrUpdate_v2();
                //_mapV3 = AddOrUpdate_v3();
                _dict = Dict();
                _dictSlim = DictSlim();
                _concurrentDict = ConcurrentDict();
                _immutableDict = ImmutableDict();
            }

            public ImHashMap<Type, string> AddOrUpdate()
            {
                var map = ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a", out _, out _);

                map = map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!", out _, out _);

                return map;
            }

            private ImHashMap<Type, string> _map;

            public V1.ImHashMap<Type, string> AddOrUpdate_v1()
            {
                var map = V1.ImHashMap<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.AddOrUpdate(key, "a");

                map = map.AddOrUpdate(typeof(ImHashMapBenchmarks), "!");

                return map;
            }

            private V1.ImHashMap<Type, string> _mapV1;

            public Dictionary<Type, string> Dict()
            {
                var map = new Dictionary<Type, string>();

                foreach (var key in _keys.Take(Count))
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!");

                return map;
            }

            public struct TypeVal : IEquatable<TypeVal>
            {
                public static implicit operator TypeVal(Type t) => new TypeVal(t);

                public readonly Type Type;
                public TypeVal(Type type) => Type = type;
                public bool Equals(TypeVal other) => Type == other.Type;
                public override bool Equals(object obj) => !ReferenceEquals(null, obj) && obj is TypeVal other && Equals(other);
                public override int GetHashCode() => Type.GetHashCode();
            }

            private Dictionary<Type, string> _dict;

            public DictionarySlim<TypeVal, string> DictSlim()
            {
                var dict = new DictionarySlim<TypeVal, string>();

                foreach (var key in _keys.Take(Count))
                    dict.GetOrAddValueRef(key) = "a";

                dict.GetOrAddValueRef(typeof(ImHashMapBenchmarks)) = "!";

                return dict;
            }

            private DictionarySlim<TypeVal, string> _dictSlim;

            public ConcurrentDictionary<Type, string> ConcurrentDict()
            {
                var map = new ConcurrentDictionary<Type, string>();

                foreach (var key in _keys.Take(Count))
                    map.TryAdd(key, "a");

                map.TryAdd(typeof(ImHashMapBenchmarks), "!");

                return map;
            }

            private ConcurrentDictionary<Type, string> _concurrentDict;

            public ImmutableDictionary<Type, string> ImmutableDict()
            {
                var map = ImmutableDictionary<Type, string>.Empty;

                foreach (var key in _keys.Take(Count))
                    map = map.Add(key, "a");

                return map.Add(typeof(ImHashMapBenchmarks), "!");
            }

            private ImmutableDictionary<Type, string> _immutableDict;

            public static Type LookupKey = typeof(ImHashMapBenchmarks);

            //[Benchmark]
            public string TryFind_v1()
            {
                _mapV1.TryFind(LookupKey, out var result);
                return result;
            }

            [Benchmark(Baseline = true)]
            public string TryFind()
            {
                _map.TryFind(LookupKey, out var result);
                return result;
            }

            [Benchmark]
            public string Dictionary_TryGetValue()
            {
                _dict.TryGetValue(LookupKey, out var result);
                return result;
            }

            [Benchmark]
            public string DictionarySlim_TryGetValue()
            {
                _dictSlim.TryGetValue(LookupKey, out var result);
                return result;
            }

            [Benchmark]
            public string ConcurrentDictionary_TryGetValue()
            {
                _concurrentDict.TryGetValue(LookupKey, out var result);
                return result;
            }

            [Benchmark]
            public string ImmutableDict_TryGet()
            {
                _immutableDict.TryGetValue(LookupKey, out var result);
                return result;
            }

            //[Benchmark]
            //public string TryFind_v2()
            //{
            //    V2.ImHashMap.TryFind(_mapV2, LookupKey, out var result);
            //    return result;
            //}

            //[Benchmark]
            //public string GetValueOrDefault() => _map.GetValueOrDefault(LookupKey);

            //[Benchmark(Baseline = true)]
            //public string GetValueOrDefault_v1() => _mapV1.GetValueOrDefault(LookupKey);

            //[Benchmark]
            //public string GetValueOrDefault_v2() => V2.ImHashMap.GetValueOrDefault(_mapV2, LookupKey);

            //[Benchmark]
            //public string GetValueOrDefault_v3() => V3.ImHashMap.GetValueOrDefault(_mapV3, LookupKey);

            //[Benchmark]
            //public string ConcurrentDict_TryGet()
            //{
            //    _concurDict.TryGetValue(LookupKey, out var result);
            //    return result;
            //}

            //public string ImmutableDict_TryGet()
            //{
            //    _immutableDict.TryGetValue(LookupKey, out var result);
            //    return result;
            //}
        }
    }
}

namespace V1
{
    using System.Runtime.CompilerServices; // For [MethodImpl(AggressiveInlining)]

    /// <summary>Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.</summary>
    public sealed class ImHashMap<K, V>
    {
        /// <summary>Empty tree to start with.</summary>
        public static readonly ImHashMap<K, V> Empty = new ImHashMap<K, V>();

        /// <summary>Calculated key hash.</summary>
        public int Hash => _data.Hash;

        /// <summary>Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>.</summary>
        public K Key => _data.Key;

        /// <summary>Value of any type V.</summary>
        public V Value => _data.Value;

        /// <summary>In case of <see cref="Hash"/> conflicts for different keys contains conflicted keys with their values.</summary>
        public KV<K, V>[] Conflicts => _data.Conflicts;

        /// <summary>Left sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Left;

        /// <summary>Right sub-tree/branch, or empty.</summary>
        public readonly ImHashMap<K, V> Right;

        /// <summary>Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.</summary>
        public readonly int Height;

        /// <summary>Returns true if tree is empty.</summary>
        public bool IsEmpty => Height == 0;

        /// <summary>Returns new tree with added key-value. 
        /// If value with the same key is exist then the value is replaced.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value) =>
            AddOrUpdate(key.GetHashCode(), key, value);

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">Update handler.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update) =>
            AddOrUpdate(key.GetHashCode(), key, value, update);

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImHashMap<K, V> Update(K key, V value, Update<V> update = null) =>
            Update(key.GetHashCode(), key, value, update);

        /// <summary>Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.</summary>
        /// <param name="key">Key to look for.</param> <param name="defaultValue">(optional) Value to return if key is not found.</param>
        /// <returns>Found value or <paramref name="defaultValue"/>.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public V GetValueOrDefault(K key, V defaultValue = default(V))
        {
            var t = this;
            var hash = key.GetHashCode();
            while (t.Height != 0 && t.Hash != hash)
                t = hash < t.Hash ? t.Left : t.Right;
            return t.Height != 0 && (ReferenceEquals(key, t.Key) || key.Equals(t.Key))
                ? t.Value : t.GetConflictedValueOrDefault(key, defaultValue);
        }

        /// <summary>Returns true if key is found and sets the value.</summary>
        /// <param name="key">Key to look for.</param> <param name="value">Result value</param>
        /// <returns>True if key found, false otherwise.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public bool TryFind(K key, out V value)
        {
            var hash = key.GetHashCode();

            var t = this;
            while (t.Height != 0 && t._data.Hash != hash)
                t = hash < t._data.Hash ? t.Left : t.Right;

            if (t.Height != 0 && (ReferenceEquals(key, t._data.Key) || key.Equals(t._data.Key)))
            {
                value = t._data.Value;
                return true;
            }

            return t.TryFindConflictedValue(key, out value);
        }

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (Height == 0)
                yield break;

            var parents = new ImHashMap<K, V>[Height];

            var node = this;
            var parentCount = -1;
            while (node.Height != 0 || parentCount != -1)
            {
                if (node.Height != 0)
                {
                    parents[++parentCount] = node;
                    node = node.Left;
                }
                else
                {
                    node = parents[parentCount--];
                    yield return new KV<K, V>(node.Key, node.Value);

                    if (node.Conflicts != null)
                        for (var i = 0; i < node.Conflicts.Length; i++)
                            yield return node.Conflicts[i];

                    node = node.Right;
                }
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        public ImHashMap<K, V> Remove(K key) =>
            Remove(key.GetHashCode(), key);

        /// <summary>Outputs key value pair</summary>
        public override string ToString() => IsEmpty ? "empty" : (Key + ":" + Value);

        #region Implementation

        private sealed class Data
        {
            public readonly int Hash;
            public readonly K Key;
            public readonly V Value;

            public readonly KV<K, V>[] Conflicts;

            public Data() { }

            public Data(int hash, K key, V value, KV<K, V>[] conflicts = null)
            {
                Hash = hash;
                Key = key;
                Value = value;
                Conflicts = conflicts;
            }
        }

        private readonly Data _data;

        private ImHashMap() { _data = new Data(); }

        private ImHashMap(Data data)
        {
            _data = data;
            Left = Empty;
            Right = Empty;
            Height = 1;
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = 1 + (left.Height > right.Height ? left.Height : right.Height);
        }

        private ImHashMap(Data data, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
        {
            _data = data;
            Left = left;
            Right = right;
            Height = height;
        }

        // todo: made public for benchmarking
        /// <summary>It is fine</summary>
        public ImHashMap<K, V> AddOrUpdate(int hash, K key, V value)
        {
            return Height == 0  // add new node
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update found node
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, value, Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, null, false))
                    : (hash < Hash  // search for node
                        ? (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                new ImHashMap<K, V>(new Data(hash, key, value)), Right, height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left.AddOrUpdate(hash, key, value), Right).KeepBalance())
                        : (Height == 1
                            ? new ImHashMap<K, V>(_data,
                                Left, new ImHashMap<K, V>(new Data(hash, key, value)), height: 2)
                            : new ImHashMap<K, V>(_data,
                                Left, Right.AddOrUpdate(hash, key, value)).KeepBalance())));
        }

        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update)
        {
            return Height == 0
                ? new ImHashMap<K, V>(new Data(hash, key, value))
                : (hash == Hash // update
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, false))
                    : (hash < Hash
                        ? With(Left.AddOrUpdate(hash, key, value, update), Right)
                        : With(Left, Right.AddOrUpdate(hash, key, value, update)))
                    .KeepBalance());
        }

        // todo: made public for benchmarking
        /// <summary>It is fine</summary>
        public ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update)
        {
            return Height == 0 ? this
                : (hash == Hash
                    ? (ReferenceEquals(Key, key) || Key.Equals(key)
                        ? new ImHashMap<K, V>(new Data(hash, key, update == null ? value : update(Value, value), Conflicts), Left, Right)
                        : UpdateValueAndResolveConflicts(key, value, update, true))
                    : (hash < Hash
                        ? With(Left.Update(hash, key, value, update), Right)
                        : With(Left, Right.Update(hash, key, value, update)))
                    .KeepBalance());
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly ? this
                    : new ImHashMap<K, V>(new Data(Hash, Key, Value, new[] { new KV<K, V>(key, value) }), Left, Right);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly) return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ImHashMap<K, V>(new Data(Hash, Key, Value, newConflicts), Left, Right);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, conflicts), Left, Right);
        }

        // todo: temporary made public for benchmarking
        /// <summary>It is fine</summary>
        public V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                        return Conflicts[i].Value;
            return defaultValue;
        }

        private bool TryFindConflictedValue(K key, out V value)
        {
            if (Height != 0 && Conflicts != null)
                for (var i = Conflicts.Length - 1; i >= 0; --i)
                    if (Equals(Conflicts[i].Key, key))
                    {
                        value = Conflicts[i].Value;
                        return true;
                    }

            value = default(V);
            return false;
        }

        private ImHashMap<K, V> KeepBalance()
        {
            var delta = Left.Height - Right.Height;
            if (delta >= 2) // left is longer by 2, rotate left
            {
                var left = Left;
                var leftLeft = left.Left;
                var leftRight = left.Right;
                if (leftRight.Height - leftLeft.Height == 1)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return new ImHashMap<K, V>(leftRight._data,
                        left: new ImHashMap<K, V>(left._data,
                            left: leftLeft, right: leftRight.Left), right: new ImHashMap<K, V>(_data,
                            left: leftRight.Right, right: Right));
                }

                // todo: do we need this?
                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                return new ImHashMap<K, V>(left._data,
                    left: leftLeft, right: new ImHashMap<K, V>(_data,
                        left: leftRight, right: Right));
            }

            if (delta <= -2)
            {
                var right = Right;
                var rightLeft = right.Left;
                var rightRight = right.Right;
                if (rightLeft.Height - rightRight.Height == 1)
                {
                    return new ImHashMap<K, V>(rightLeft._data,
                        left: new ImHashMap<K, V>(_data,
                            left: Left, right: rightLeft.Left), right: new ImHashMap<K, V>(right._data,
                            left: rightLeft.Right, right: rightRight));
                }

                return new ImHashMap<K, V>(right._data,
                    left: new ImHashMap<K, V>(_data,
                        left: Left, right: rightLeft), right: rightRight);
            }

            return this;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right) =>
            left == Left && right == Right ? this : new ImHashMap<K, V>(_data, left, right);

        internal ImHashMap<K, V> Remove(int hash, K key, bool ignoreKey = false)
        {
            if (Height == 0)
                return this;

            ImHashMap<K, V> result;
            if (hash == Hash) // found node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey && Conflicts != null)
                        return ReplaceRemovedWithConflicted();

                    if (Height == 1) // remove node
                        return Empty;

                    if (Right.IsEmpty)
                        result = Left;
                    else if (Left.IsEmpty)
                        result = Right;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = Right;
                        while (!successor.Left.IsEmpty) successor = successor.Left;
                        result = new ImHashMap<K, V>(successor._data,
                            Left, Right.Remove(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
                result = new ImHashMap<K, V>(_data, Left.Remove(hash, key, ignoreKey), Right);
            else
                result = new ImHashMap<K, V>(_data, Left, Right.Remove(hash, key, ignoreKey));

            if (result.Height == 1)
                return result;

            return result.KeepBalance();
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Key, Value), Left, Right);
            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) shrinkedConflicts[newIndex++] = Conflicts[i];
            return new ImHashMap<K, V>(new Data(Hash, Key, Value, shrinkedConflicts), Left, Right);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            if (Conflicts.Length == 1)
                return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value), Left, Right);
            var shrinkedConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, shrinkedConflicts, 0, shrinkedConflicts.Length);
            return new ImHashMap<K, V>(new Data(Hash, Conflicts[0].Key, Conflicts[0].Value, shrinkedConflicts), Left, Right);
        }

        #endregion
    }
}

namespace V2
{
    using System.Runtime.CompilerServices; // For [MethodImpl(AggressiveInlining)]

    /// Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.
    public class ImHashMap<K, V>
    {
        /// Empty tree to start with.
        public static readonly ImHashMap<K, V> Empty = new Branch();

        /// Returns true if tree is empty. Valid for a `Branch`.
        public bool IsEmpty => this == Empty;

        /// Calculated key hash.
        public readonly int Hash;

        /// Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>
        public readonly K Key;

        /// Value of any type V.
        public readonly V Value;

        /// The height.
        public int Height => this is Branch b ? b.BranchHeight : 1;

        /// Left branch.
        public ImHashMap<K, V> Left => (this as Branch)?.LeftNode;

        /// Right branch.
        public ImHashMap<K, V> Right => (this as Branch)?.RightNode;

        /// Conflicts
        public readonly KV<K, V>[] Conflicts;

        /// The branch node
        public sealed class Branch : ImHashMap<K, V>
        {
            /// Left sub-tree/branch, or `null`!
            public readonly ImHashMap<K, V> LeftNode;

            /// Right sub-tree/branch, or `null`!
            public readonly ImHashMap<K, V> RightNode;

            /// Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.
            public readonly int BranchHeight;

            /// Empty map
            public Branch() { }

            /// Constructs the branch node
            public Branch(int hash, K key, V value, KV<K, V>[] conflicts, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
                : base(hash, key, value, conflicts)
            {
                LeftNode = left;
                RightNode = right;
                BranchHeight = height;
            }

            /// Works on the branch
            public Branch AddOrUpdateBranch(int hash, K key, V value, Update<K, V> update, ref bool isUpdated, ref V oldValue)
            {
                if (hash == Hash)
                {
                    if (ReferenceEquals(Key, key) || Key.Equals(key))
                    {
                        if (update != null)
                            value = update(Key, Value, value);
                        if (ReferenceEquals(value, Value) || value?.Equals(Value) == true)
                            return this;

                        isUpdated = true;
                        oldValue = Value;
                        return new Branch(hash, key, value, Conflicts, LeftNode, RightNode, BranchHeight);
                    }

                    return Conflicts == null
                        ? new Branch(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, LeftNode, RightNode, BranchHeight)
                        : UpdateBranchValueAndResolveConflicts(key, value, update, false, ref isUpdated, ref oldValue);
                }

                if (hash < Hash)
                {
                    var left = LeftNode;
                    if (left == null)
                        return new Branch(Hash, Key, Value, Conflicts,
                            new ImHashMap<K, V>(hash, key, value), RightNode, BranchHeight);

                    var leftBranch = left as Branch;

                    // Left is the Leaf node without producing garbage intermediate results
                    if (leftBranch == null)
                    {
                        if (hash == left.Hash)
                        {
                            if (ReferenceEquals(left.Key, key) || left.Key.Equals(key))
                            {
                                if (update != null)
                                    value = update(left.Key, left.Value, value);
                                if (ReferenceEquals(value, left.Value) || value?.Equals(left.Value) == true)
                                    return this;

                                isUpdated = true;
                                oldValue = left.Value;
                                return new Branch(Hash, Key, Value, Conflicts,
                                    new ImHashMap<K, V>(hash, key, value, left.Conflicts), RightNode, BranchHeight);
                            }

                            return new Branch(Hash, Key, Value, Conflicts,
                                left.Conflicts == null
                                    ? new ImHashMap<K, V>(left.Hash, left.Key, left.Value, new[] { new KV<K, V>(key, value) })
                                    : left.UpdateLeafValueAndResolveConflicts(key, value, update, false, ref isUpdated, ref oldValue),
                                RightNode, BranchHeight);
                        }

                        if (hash < left.Hash)
                        {
                            // single rotation:
                            //      5     =>     2
                            //   2            1     5
                            // 1
                            if (RightNode == null)
                                return new Branch(left.Hash, left.Key, left.Value, left.Conflicts,
                                    new ImHashMap<K, V>(hash, key, value),
                                    new ImHashMap<K, V>(Hash, Key, Value, Conflicts), 2);

                            leftBranch = new Branch(left.Hash, left.Key, left.Value, left.Conflicts,
                                new ImHashMap<K, V>(hash, key, value), null, 2);
                        }
                        else
                        {
                            // double rotation
                            //      5     =>    5     =>    4
                            //   2           4           2     5
                            //    4         2
                            if (RightNode == null)
                                return new Branch(hash, key, value, null,
                                    new ImHashMap<K, V>(left.Hash, left.Key, left.Value, left.Conflicts),
                                    new ImHashMap<K, V>(Hash, Key, Value, Conflicts), 2);

                            leftBranch = new Branch(left.Hash, left.Key, left.Value, left.Conflicts,
                                null, new ImHashMap<K, V>(hash, key, value), 2);
                        }
                    }
                    else
                    {
                        var oldLeftBranch = leftBranch;
                        leftBranch = leftBranch.AddOrUpdateBranch(hash, key, value, update, ref isUpdated, ref oldValue);
                        if (isUpdated)
                            return new Branch(Hash, Key, Value, Conflicts, leftBranch, RightNode, BranchHeight);
                        if (leftBranch == oldLeftBranch)
                            return this;
                    }

                    var rightHeight = (RightNode as Branch)?.BranchHeight ?? 1;
                    var leftHeight = leftBranch.BranchHeight;
                    if (leftHeight - rightHeight <= 1)
                        return CreateBranch(Hash, Key, Value, Conflicts, leftHeight, leftBranch, rightHeight, RightNode);

                    // Also means that left is branch and not the leaf
                    // ReSharper disable once PossibleNullReferenceException
                    var leftLeft = leftBranch.LeftNode;
                    var leftRight = leftBranch.RightNode;

                    // The left is at least have height >= 2 and assuming that it is balanced and no node is updated (handled early),
                    // it should not have `null` left or right
                    //Debug.Assert(leftLeft != null && leftRight != null);
                    var llb = leftLeft as Branch;
                    var lrb = leftRight as Branch;
                    var leftLeftHeight = llb?.BranchHeight ?? 1;
                    var leftRightHeight = lrb?.BranchHeight ?? 1;
                    if (leftRightHeight > leftLeftHeight)
                    {
                        // double rotation:
                        //      5     =>     5     =>     4
                        //   2     6      4     6      2     5
                        // 1   4        2   3        1   3     6
                        //    3        1
                        // ReSharper disable once PossibleNullReferenceException
                        return CreateBranch(leftRight.Hash, leftRight.Key, leftRight.Value, leftRight.Conflicts,
                            CreateBranch(left.Hash, left.Key, left.Value, left.Conflicts, leftLeftHeight, leftLeft, lrb?.LeftNode),
                            CreateBranch(Hash, Key, Value, Conflicts, lrb?.RightNode, rightHeight, RightNode));
                    }

                    // one rotation:
                    //      5     =>     2
                    //   2     6      1     5
                    // 1   4              4   6
                    var newRightHeight = leftRightHeight > rightHeight ? leftRightHeight + 1 : rightHeight + 1;
                    return CreateBranch(left.Hash, left.Key, left.Value, left.Conflicts,
                        leftLeftHeight, leftLeft,
                        newRightHeight, new Branch(Hash, Key, Value, Conflicts, leftRight, RightNode, newRightHeight));
                }
                else
                {
                    var right = RightNode;
                    if (right == null)
                        return new Branch(Hash, Key, Value, Conflicts,
                            LeftNode, new ImHashMap<K, V>(hash, key, value), BranchHeight);

                    var rightBranch = right as Branch;
                    if (rightBranch == null)
                    {
                        if (hash == right.Hash)
                        {
                            if (ReferenceEquals(right.Key, key) || right.Key.Equals(key))
                            {
                                if (update != null)
                                    value = update(right.Key, right.Value, value);
                                if (ReferenceEquals(value, right.Value) || value?.Equals(right.Value) == true)
                                    return this;

                                isUpdated = true;
                                oldValue = right.Value;
                                return new Branch(Hash, Key, Value, Conflicts,
                                    LeftNode, new ImHashMap<K, V>(hash, key, value, right.Conflicts), BranchHeight);
                            }

                            return new Branch(Hash, Key, Value, Conflicts, LeftNode, right.Conflicts == null
                                ? new ImHashMap<K, V>(right.Hash, right.Key, right.Value, new[] { new KV<K, V>(key, value) })
                                : right.UpdateLeafValueAndResolveConflicts(key, value, update, false, ref isUpdated, ref oldValue),
                                BranchHeight);
                        }

                        if (hash > right.Hash)
                        {
                            // single rotation:
                            //      5     =>      6
                            //        6        5     7
                            //          7
                            if (LeftNode == null)
                            {
                                return new Branch(right.Hash, right.Key, right.Value, right.Conflicts,
                                    new ImHashMap<K, V>(Hash, Key, Value, Conflicts),
                                    new ImHashMap<K, V>(hash, key, value), 2);
                            }

                            rightBranch = new Branch(right.Hash, right.Key, right.Value, right.Conflicts,
                                null, new ImHashMap<K, V>(hash, key, value), 2);
                        }
                        else
                        {
                            // double rotation
                            //      5     =>    5     =>     6
                            //         7          6        5   7
                            //        6            7
                            if (LeftNode == null)
                                return new Branch(hash, key, value, null,
                                    new ImHashMap<K, V>(Hash, Key, Value, Conflicts),
                                    new ImHashMap<K, V>(right.Hash, right.Key, right.Value, right.Conflicts), 2);

                            rightBranch = new Branch(right.Hash, right.Key, right.Value, right.Conflicts,
                                new ImHashMap<K, V>(hash, key, value), null, 2);
                        }
                    }
                    else
                    {
                        var oldRightBranch = rightBranch;
                        rightBranch = rightBranch.AddOrUpdateBranch(hash, key, value, update, ref isUpdated, ref oldValue);
                        if (isUpdated)
                            return new Branch(Hash, Key, Value, Conflicts, LeftNode, rightBranch, BranchHeight);
                        if (rightBranch == oldRightBranch)
                            return this;
                    }

                    var leftHeight = (LeftNode as Branch)?.BranchHeight ?? 1;
                    var rightHeight = rightBranch.BranchHeight;
                    if (rightHeight - leftHeight <= 1)
                        return CreateBranch(Hash, Key, Value, Conflicts, leftHeight, LeftNode, rightHeight, rightBranch);

                    // ReSharper disable once PossibleNullReferenceException
                    var rightLeft = rightBranch.LeftNode;
                    var rightRight = rightBranch.RightNode;

                    var rlb = rightLeft as Branch;
                    var rrb = rightRight as Branch;
                    var rightLeftHeight = rlb?.BranchHeight ?? 1;
                    var rightRightHeight = rrb?.BranchHeight ?? 1;
                    if (rightLeftHeight > rightRightHeight)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        return CreateBranch(rightLeft.Hash, rightLeft.Key, rightLeft.Value, rightLeft.Conflicts,
                            CreateBranch(Hash, Key, Value, Conflicts, leftHeight, LeftNode, rlb?.LeftNode),
                            CreateBranch(right.Hash, right.Key, right.Value, right.Conflicts, rlb?.RightNode, rightRightHeight, rightRight));
                    }

                    var newLeftHeight = leftHeight > rightLeftHeight ? leftHeight + 1 : rightLeftHeight + 1;
                    return CreateBranch(right.Hash, right.Key, right.Value, right.Conflicts,
                        newLeftHeight, new Branch(Hash, Key, Value, Conflicts, LeftNode, rightLeft, newLeftHeight),
                        rightRightHeight, rightRight);
                }
            }

            private Branch UpdateBranchValueAndResolveConflicts(
                K key, V value, Update<K, V> update, bool updateOnly, ref bool isUpdated, ref V oldValue)
            {
                if (Conflicts == null) // add only if updateOnly is false.
                    return updateOnly
                        ? this
                        : new Branch(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, LeftNode, RightNode, BranchHeight);

                var found = Conflicts.Length - 1;
                while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
                if (found == -1)
                {
                    if (updateOnly)
                        return this;

                    var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                    Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                    newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                    return new Branch(Hash, Key, Value, newConflicts, LeftNode, RightNode, BranchHeight);
                }

                var conflicts = new KV<K, V>[Conflicts.Length];
                Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);

                var conflict = conflicts[found];
                if (update != null)
                    value = update(conflict.Key, conflict.Value, value);
                if (ReferenceEquals(value, conflict.Value) || value?.Equals(conflict.Value) == true)
                    return this;

                isUpdated = true;
                oldValue = conflict.Value;
                conflicts[found] = new KV<K, V>(key, value);
                return new Branch(Hash, Key, Value, conflicts, LeftNode, RightNode, BranchHeight);
            }
        }

        /// <summary>Returns new tree with added key-value. 
        /// If value with the same key is exist then the value is replaced.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value)
        {
            var isUpdated = false;
            var oldValue = default(V);
            return this == Empty
                ? new ImHashMap<K, V>(key.GetHashCode(), key, value)
                : this is Branch branch
                    ? branch.AddOrUpdateBranch(key.GetHashCode(), key, value, null, ref isUpdated, ref oldValue)
                    : AddOrUpdateLeaf(key.GetHashCode(), key, value, null, ref isUpdated, ref oldValue);
        }

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">Update handler.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update)
        {
            var isUpdated = false;
            var oldValue = default(V);
            return this == Empty
                ? new ImHashMap<K, V>(key.GetHashCode(), key, value)
                : this is Branch branch
                    ? branch.AddOrUpdateBranch(key.GetHashCode(), key, value, (_, a, b) => update(a, b), ref isUpdated, ref oldValue)
                    : AddOrUpdateLeaf(key.GetHashCode(), key, value, (_, old, x) => update(old, x), ref isUpdated, ref oldValue);
        }

        /// Returns the previous value if updated.
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value, out bool isUpdated, out V oldValue, Update<K, V> update = null)
        {
            isUpdated = false;
            oldValue = default(V);
            return this == Empty
                ? new ImHashMap<K, V>(key.GetHashCode(), key, value)
                : this is Branch branch
                    ? branch.AddOrUpdateBranch(key.GetHashCode(), key, value, update, ref isUpdated, ref oldValue)
                    : AddOrUpdateLeaf(key.GetHashCode(), key, value, update, ref isUpdated, ref oldValue);
        }

        private ImHashMap<K, V> AddOrUpdateLeaf(int hash, K key, V value, Update<K, V> update, ref bool isUpdated, ref V oldValue)
        {
            if (hash == Hash)
            {
                if (ReferenceEquals(Key, key) || Key.Equals(key))
                {
                    if (update != null)
                        value = update(Key, Value, value);
                    if (ReferenceEquals(value, Value) || value?.Equals(Value) == true)
                        return this;

                    isUpdated = true;
                    oldValue = Value;
                    return new ImHashMap<K, V>(hash, key, value, Conflicts);
                }

                return Conflicts == null
                    ? new ImHashMap<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) })
                    : UpdateLeafValueAndResolveConflicts(key, value, update, false, ref isUpdated, ref oldValue);
            }

            return hash < Hash
                ? new Branch(Hash, Key, Value, Conflicts, new ImHashMap<K, V>(hash, key, value), null, 2)
                : new Branch(Hash, Key, Value, Conflicts, null, new ImHashMap<K, V>(hash, key, value), 2);
        }

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImHashMap<K, V> Update(K key, V value, Update<V> update = null) =>
            Update(key.GetHashCode(), key, value, update);

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (this == Empty)
                yield break;

            var node = this;
            var height = node is Branch b ? b.BranchHeight : 1;
            var parents = new ImHashMap<K, V>[height];
            var parentCount = -1;
            while (height != 0 || parentCount != -1)
            {
                if (height != 0)
                {
                    parents[++parentCount] = node;
                    node = (node as Branch)?.LeftNode; // null is fine cause GetHeight handles the null
                }
                else
                {
                    node = parents[parentCount--];
                    yield return new KV<K, V>(node.Key, node.Value);

                    var conflicts = node.Conflicts;
                    if (conflicts != null)
                        for (var i = 0; i < conflicts.Length; i++)
                            yield return conflicts[i];

                    node = (node as Branch)?.RightNode;
                }

                height = node == null ? 0 : node is Branch br ? br.BranchHeight : 1;
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> Remove(K key) =>
            Remove(key.GetHashCode(), key);

        /// <summary>Outputs key value pair</summary>
        public override string ToString() => IsEmpty ? "empty" : (Key + ":" + Value);

        #region Implementation

        private ImHashMap() { }

        /// Creates the leaf node
        protected ImHashMap(int hash, K key, V value, KV<K, V>[] conflicts = null)
        {
            Hash = hash;
            Key = key;
            Value = value;
            Conflicts = conflicts;
        }

        /// It is fine.
        public ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update = null)
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (height == 0)
                return this;

            return hash == Hash
                ? (ReferenceEquals(Key, key) || Key.Equals(key)
                    ? CreateNode(hash, key, update == null ? value : update(Value, value), Conflicts,
                        branch?.LeftNode, branch?.RightNode, height)
                    : UpdateValueAndResolveConflicts(key, value, update, true))
                : (hash < Hash
                    ? With(branch?.LeftNode?.Update(hash, key, value, update), branch?.RightNode)
                    : With(branch?.LeftNode, branch?.RightNode?.Update(hash, key, value, update)));
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            var br = this as Branch;
            var height = br?.BranchHeight ?? 1;
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly
                    ? this
                    : CreateNode(Hash, Key, Value, new[] { new KV<K, V>(key, value) }, br?.LeftNode, br?.RightNode, height);

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly)
                    return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);

                return CreateNode(Hash, Key, Value, newConflicts, br?.LeftNode, br?.RightNode, height);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));

            return CreateNode(Hash, Key, Value, conflicts, br?.LeftNode, br?.RightNode, height);
        }

        private ImHashMap<K, V> UpdateLeafValueAndResolveConflicts(
            K key, V value, Update<K, V> update, bool updateOnly, ref bool isUpdated, ref V oldValue)
        {
            if (Conflicts == null) // add only if updateOnly is false.
                return updateOnly
                    ? this
                    : new ImHashMap<K, V>(Hash, Key, Value, new[] { new KV<K, V>(key, value) });

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly)
                    return this;

                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);
                return new ImHashMap<K, V>(Hash, Key, Value, newConflicts);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);

            var conflict = conflicts[found];
            if (update != null)
                value = update(conflict.Key, conflict.Value, value);
            if (ReferenceEquals(value, conflict.Value) || value?.Equals(conflict.Value) == true)
                return this;

            isUpdated = true;
            oldValue = conflict.Value;
            conflicts[found] = new KV<K, V>(key, value);
            return new ImHashMap<K, V>(Hash, Key, Value, conflicts);
        }

        /// It is fine.
        public V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
            {
                var conflicts = Conflicts;
                for (var i = conflicts.Length - 1; i >= 0; --i)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            }
            return defaultValue;
        }

        /// It is fine.
        internal bool TryFindConflictedValue(K key, out V value)
        {
            if (Conflicts != null)
            {
                var conflicts = Conflicts;
                for (var i = conflicts.Length - 1; i >= 0; --i)
                    if (Equals(conflicts[i].Key, key))
                    {
                        value = conflicts[i].Value;
                        return true;
                    }
            }

            value = default(V);
            return false;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            var branch = this as Branch;
            return left == branch?.LeftNode && right == branch?.RightNode
                ? this
                : Balance(Hash, Key, Value, Conflicts, left, right);
        }

        private static ImHashMap<K, V> CreateNode(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right, int height) =>
            height == 1
                ? new ImHashMap<K, V>(hash, key, value, conflicts)
                : new Branch(hash, key, value, conflicts, left, right, height);

        [MethodImpl((MethodImplOptions)256)]
        private static Branch CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            int leftHeight, ImHashMap<K, V> left, ImHashMap<K, V> right) =>
            CreateBranch(hash, key, value, conflicts,
                leftHeight, left, right == null ? 0 : (right as Branch)?.BranchHeight ?? 1, right);

        [MethodImpl((MethodImplOptions)256)]
        private static Branch CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, int rightHeight, ImHashMap<K, V> right) =>
            CreateBranch(hash, key, value, conflicts,
                left == null ? 0 : (left as Branch)?.BranchHeight ?? 1, left,
                rightHeight, right);

        [MethodImpl((MethodImplOptions)256)]
        private static Branch CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            int leftHeight, ImHashMap<K, V> left, int rightHeight, ImHashMap<K, V> right) =>
            new Branch(hash, key, value, conflicts, left, right, leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1);

        [MethodImpl((MethodImplOptions)256)]
        private static Branch CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right) =>
            CreateBranch(hash, key, value, conflicts,
                left == null ? 0 : (left as Branch)?.BranchHeight ?? 1, left,
                right == null ? 0 : (right as Branch)?.BranchHeight ?? 1, right);

        private static ImHashMap<K, V> Balance(
            int hash, K key, V value, KV<K, V>[] conflicts, ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            var leftBranch = left as Branch;
            var leftHeight = left == null ? 0 : leftBranch?.BranchHeight ?? 1;

            var rightBranch = right as Branch;
            var rightHeight = right == null ? 0 : rightBranch?.BranchHeight ?? 1;

            var delta = leftHeight - rightHeight;
            if (delta > 1) // left is longer by 2, rotate left
            {
                // Also means that left is branch and not the leaf
                // ReSharper disable once PossibleNullReferenceException
                var leftLeft = leftBranch.LeftNode;
                var leftRight = leftBranch.RightNode;

                var llb = leftLeft as Branch;
                var lrb = leftRight as Branch;

                // The left is at least have height >= 2 and assuming that it is balanced, it should not have `null` left or right
                var leftLeftHeight = leftLeft == null ? 0 : llb?.BranchHeight ?? 1;
                var leftRightHeight = leftRight == null ? 0 : lrb?.BranchHeight ?? 1;
                if (leftRightHeight > leftLeftHeight)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    return CreateBranch(leftRight.Hash, leftRight.Key, leftRight.Value, leftRight.Conflicts,
                        leftLeft == null && lrb?.LeftNode == null
                            ? new ImHashMap<K, V>(left.Hash, left.Key, left.Value, left.Conflicts)
                            : CreateBranch(left.Hash, left.Key, left.Value, left.Conflicts, leftLeftHeight, leftLeft, lrb?.LeftNode),
                        lrb?.RightNode == null && right == null
                            ? new ImHashMap<K, V>(hash, key, value, conflicts)
                            : CreateBranch(hash, key, value, conflicts, lrb?.RightNode, rightHeight, right));
                }

                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                var newRightHeight = leftRightHeight > rightHeight ? leftRightHeight + 1 : rightHeight + 1;
                ImHashMap<K, V> right1 = CreateNode(hash, key, value, conflicts, leftRight, right, newRightHeight);
                int height = leftLeftHeight > newRightHeight ? leftLeftHeight + 1 : newRightHeight + 1;
                return new Branch(left.Hash, left.Key, left.Value, left.Conflicts, leftLeft, right1, height);
            }

            if (delta < -1)
            {
                // ReSharper disable once PossibleNullReferenceException
                var rightLeft = rightBranch.LeftNode;
                var rightRight = rightBranch.RightNode;
                var rlb = rightLeft as Branch;
                var rrb = rightRight as Branch;

                var rightLeftHeight = rightLeft == null ? 0 : rlb?.BranchHeight ?? 1;
                var rightRightHeight = rightRight == null ? 0 : rrb?.BranchHeight ?? 1;
                if (rightLeftHeight > rightRightHeight)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    return CreateBranch(rightLeft.Hash, rightLeft.Key, rightLeft.Value, rightLeft.Conflicts,
                        left == null && rlb?.LeftNode == null
                            ? new ImHashMap<K, V>(hash, key, value, conflicts)
                            : CreateBranch(hash, key, value, conflicts, leftHeight, left, rlb?.LeftNode),
                        rlb?.RightNode == null && rightRight == null
                            ? new ImHashMap<K, V>(right.Hash, right.Key, right.Value, right.Conflicts)
                            : CreateBranch(right.Hash, right.Key, right.Value, right.Conflicts, rlb?.RightNode, rightRightHeight, rightRight));
                }

                var newLeftHeight = leftHeight > rightLeftHeight ? leftHeight + 1 : rightLeftHeight + 1;
                ImHashMap<K, V> left1 = CreateNode(hash, key, value, conflicts, left, rightLeft, newLeftHeight);
                int height = newLeftHeight > rightRightHeight ? newLeftHeight + 1 : rightRightHeight + 1;
                return new Branch(right.Hash, right.Key, right.Value, right.Conflicts, left1, rightRight, height);
            }

            int height1 = leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1;
            return new Branch(hash, key, value, conflicts, left, right, height1);
        }

        internal ImHashMap<K, V> Remove(int hash, K key, bool ignoreKey = false)
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (height == 0)
                return this;

            ImHashMap<K, V> result;
            if (hash == Hash) // found node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey && Conflicts != null)
                        return ReplaceRemovedWithConflicted();

                    if (height == 1) // remove node
                        return Empty;

                    if (branch?.RightNode == null)
                        result = branch?.LeftNode;
                    else if (branch.LeftNode == null)
                        result = branch.RightNode;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = branch?.RightNode;
                        while ((successor as Branch)?.LeftNode != null)
                            successor = (successor as Branch)?.LeftNode;

                        var successorBranch = successor as Branch;
                        var rightNode = successorBranch?.RightNode.Remove(successor.Hash, default(K), ignoreKey: true);
                        result = rightNode == null && successorBranch?.LeftNode == null
                            ? new ImHashMap<K, V>(successor.Hash, successor.Key, successor.Value, successor.Conflicts)
                            : CreateBranch(successor.Hash, successor.Key, successor.Value, successor.Conflicts, successorBranch.LeftNode, rightNode);
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
            {
                result = Balance(Hash, Key, Value, Conflicts, branch?.LeftNode.Remove(hash, key, ignoreKey), branch?.RightNode);
            }
            else
            {
                result = Balance(Hash, Key, Value, Conflicts, branch?.LeftNode, branch?.RightNode.Remove(hash, key, ignoreKey));
            }

            return result;
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (Conflicts.Length == 1)
                return CreateNode(Hash, Key, Value, null, branch?.LeftNode, branch?.RightNode, height);

            var lessConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) lessConflicts[newIndex++] = Conflicts[i];
            return CreateNode(Hash, Key, Value, lessConflicts, branch?.LeftNode, branch?.RightNode, height);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;

            if (Conflicts.Length == 1)
                return CreateNode(Hash, Conflicts[0].Key, Conflicts[0].Value, null, branch?.LeftNode, branch?.RightNode, height);

            var lessConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, lessConflicts, 0, lessConflicts.Length);
            return CreateNode(Hash, Conflicts[0].Key, Conflicts[0].Value, lessConflicts, branch?.LeftNode, branch?.RightNode, height);
        }

        #endregion
    }

    /// Map methods
    public static class ImHashMap
    {
        /// Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.
        [MethodImpl((MethodImplOptions)256)]
        public static V GetValueOrDefault<K, V>(this ImHashMap<K, V> map, K key, V defaultValue = default(V))
        {
            var hash = key.GetHashCode();
            while (map != null)
            {
                if (hash == map.Hash)
                    return ReferenceEquals(key, map.Key) || key.Equals(map.Key)
                        ? map.Value
                        : map.GetConflictedValueOrDefault(key, defaultValue);

                var br = map as ImHashMap<K, V>.Branch;
                map = br == null ? null : hash < map.Hash ? br.LeftNode : br.RightNode;
            }

            return defaultValue;
        }

        /// Looks for the `Type` key in a tree and returns the value if found, or <paramref name="defaultValue"/> otherwise.
        [MethodImpl((MethodImplOptions)256)]
        public static V GetValueOrDefault<V>(this ImHashMap<Type, V> map, Type key, V defaultValue = default(V))
        {
            var hash = key.GetHashCode();
            while (map != null)
            {
                if (hash == map.Hash)
                    return ReferenceEquals(key, map.Key)
                        ? map.Value
                        : map.GetConflictedValueOrDefault(key, defaultValue);

                var br = map as ImHashMap<Type, V>.Branch;
                map = br == null ? null : hash < map.Hash ? br.LeftNode : br.RightNode;
            }

            return defaultValue;
        }

        /// Returns true if key is found and sets the value.
        [MethodImpl((MethodImplOptions)256)]
        public static bool TryFind<K, V>(this ImHashMap<K, V> map, K key, out V value)
        {
            var hash = key.GetHashCode();
            while (map != null)
            {
                if (map.Hash == hash)
                {
                    if (ReferenceEquals(key, map.Key) || key.Equals(map.Key))
                    {
                        value = map.Value;
                        return true;
                    }
                    return map.TryFindConflictedValue(key, out value);
                }

                map = hash < map.Hash
                    ? (map as ImHashMap<K, V>.Branch)?.LeftNode
                    : (map as ImHashMap<K, V>.Branch)?.RightNode;
            }

            value = default(V);
            return false;
        }

        /// Returns true if `Type` key is found and sets the value.
        [MethodImpl((MethodImplOptions)256)]
        public static bool TryFind<V>(this ImHashMap<Type, V> map, Type key, out V value)
        {
            var hash = key.GetHashCode();
            while (map != null)
            {
                if (ReferenceEquals(map.Key, key))
                {
                    value = map.Value;
                    return true;
                }

                var delta = hash - map.Hash;
                if (delta < 0)
                    map = (map as ImHashMap<Type, V>.Branch)?.LeftNode;
                else if (delta > 0)
                    map = (map as ImHashMap<Type, V>.Branch)?.RightNode;
                else
                    return map.TryFindConflictedValue(key, out value);
            }

            value = default(V);
            return false;
        }
    }
}

namespace V3
{
    using System.Runtime.CompilerServices; // For [MethodImpl(AggressiveInlining)]

    /// Immutable http://en.wikipedia.org/wiki/AVL_tree 
    /// where node key is the hash code of <typeparamref name="K"/>.
    public class ImHashMap<K, V>
    {
        /// Empty tree to start with.
        public static readonly ImHashMap<K, V> Empty = new Branch();

        /// Returns true if tree is empty. Valid for a `Branch`.
        public bool IsEmpty => this == Empty;

        /// Calculated key hash.
        public readonly int Hash;

        /// Key of type K that should support <see cref="object.Equals(object)"/> and <see cref="object.GetHashCode"/>
        public readonly K Key;

        /// Value of any type V.
        public readonly V Value;

        /// The height.
        public int Height => this is Branch b ? b.BranchHeight : 1;

        /// Left branch.
        public ImHashMap<K, V> Left => (this as Branch)?.LeftNode;

        /// Right branch.
        public ImHashMap<K, V> Right => (this as Branch)?.RightNode;

        /// Conflicts
        public virtual KV<K, V>[] Conflicts => null;

        /// The branch node
        public class Branch : ImHashMap<K, V>
        {
            /// Left sub-tree/branch, or `null`!
            public readonly ImHashMap<K, V> LeftNode;

            /// Right sub-tree/branch, or `null`!
            public readonly ImHashMap<K, V> RightNode;

            /// Height of longest sub-tree/branch plus 1. It is 0 for empty tree, and 1 for single node tree.
            public readonly int BranchHeight;

            /// Empty map
            public Branch() { }

            /// Constructs the branch node
            public Branch(int hash, K key, V value, ImHashMap<K, V> left, ImHashMap<K, V> right)
                : base(hash, key, value)
            {
                LeftNode = left;
                RightNode = right;
                var leftHeight = left == null ? 0 : left is Branch lb ? lb.BranchHeight : 1;
                var rightHeight = right == null ? 0 : right is Branch rb ? rb.BranchHeight : 1;
                BranchHeight = leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1;
            }

            /// Creates branch with known heights of left and right
            public Branch(int hash, K key, V value, int leftHeight, ImHashMap<K, V> left, int rightHeight, ImHashMap<K, V> right)
                : base(hash, key, value)
            {
                LeftNode = left;
                RightNode = right;
                BranchHeight = leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1;
            }

            /// Creates branch with known height of left sub-tree
            public Branch(int hash, K key, V value, int leftHeight, ImHashMap<K, V> left, ImHashMap<K, V> right)
                : base(hash, key, value)
            {
                LeftNode = left;
                RightNode = right;
                var rightHeight = right == null ? 0 : right is Branch rb ? rb.BranchHeight : 1;
                BranchHeight = leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1;
            }

            /// Creates branch with known height of right sub-tree
            public Branch(int hash, K key, V value, ImHashMap<K, V> left, int rightHeight, ImHashMap<K, V> right)
                : base(hash, key, value)
            {
                LeftNode = left;
                RightNode = right;
                var leftHeight = left == null ? 0 : left is Branch lb ? lb.BranchHeight : 1;
                BranchHeight = leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1;
            }

            /// Creates the branch node with known height
            public Branch(int hash, K key, V value, ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
                : base(hash, key, value)
            {
                LeftNode = left;
                RightNode = right;
                BranchHeight = height;
            }
        }

        /// Branch with the conflicts
        public sealed class ConflictsLeaf : ImHashMap<K, V>
        {
            /// In case of Hash conflicts for different keys contains conflicted keys with their values.
            public override KV<K, V>[] Conflicts { get; }

            /// Creates the branch node
            internal ConflictsLeaf(
                int hash, K key, V value, KV<K, V>[] conflicts) : base(hash, key, value)
            {
                Conflicts = conflicts;
            }
        }

        /// Branch with the conflicts
        public sealed class ConflictsBranch : Branch
        {
            /// In case of Hash conflicts for different keys contains conflicted keys with their values.
            public override KV<K, V>[] Conflicts { get; }

            /// Creates the branch node
            internal ConflictsBranch(
                int hash, K key, V value, KV<K, V>[] conflicts,
                ImHashMap<K, V> left, ImHashMap<K, V> right) :
                base(hash, key, value, left, right)
            {
                Conflicts = conflicts;
            }

            /// Creates the branch node with known height
            internal ConflictsBranch(
                int hash, K key, V value, KV<K, V>[] conflicts,
                ImHashMap<K, V> left, ImHashMap<K, V> right, int height) :
                base(hash, key, value, left, right, height)
            {
                Conflicts = conflicts;
            }
        }

        /// <summary>Returns new tree with added key-value. 
        /// If value with the same key is exist then the value is replaced.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value) =>
            this == Empty
                ? new ImHashMap<K, V>(key.GetHashCode(), key, value)
                : AddOrUpdate(key.GetHashCode(), key, value);

        /// <summary>Returns new tree with added key-value. If value with the same key is exist, then
        /// if <paramref name="update"/> is not specified: then existing value will be replaced by <paramref name="value"/>;
        /// if <paramref name="update"/> is specified: then update delegate will decide what value to keep.</summary>
        /// <param name="key">Key to add.</param><param name="value">Value to add.</param>
        /// <param name="update">Update handler.</param>
        /// <returns>New tree with added or updated key-value.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value, Update<V> update) =>
            this == Empty
                ? new ImHashMap<K, V>(key.GetHashCode(), key, value)
                : AddOrUpdate(key.GetHashCode(), key, value, update);

        /// Returns the previous value if updated.
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> AddOrUpdate(K key, V value, out bool isUpdated, out V oldValue, Update<K, V> update = null)
        {
            isUpdated = false;
            oldValue = default(V);
            return this == Empty
                ? new ImHashMap<K, V>(key.GetHashCode(), key, value)
                : AddOrUpdate(key.GetHashCode(), key, value, update, ref isUpdated, ref oldValue);
        }

        /// <summary>Looks for <paramref name="key"/> and replaces its value with new <paramref name="value"/>, or 
        /// runs custom update handler (<paramref name="update"/>) with old and new value to get the updated result.</summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">New value to replace key value with.</param>
        /// <param name="update">(optional) Delegate for custom update logic, it gets old and new <paramref name="value"/>
        /// as inputs and should return updated value as output.</param>
        /// <returns>New tree with updated value or the SAME tree if no key found.</returns>
        public ImHashMap<K, V> Update(K key, V value, Update<V> update = null) =>
            Update(key.GetHashCode(), key, value, update);

        /// <summary>Depth-first in-order traversal as described in http://en.wikipedia.org/wiki/Tree_traversal
        /// The only difference is using fixed size array instead of stack for speed-up (~20% faster than stack).</summary>
        /// <returns>Sequence of enumerated key value pairs.</returns>
        public IEnumerable<KV<K, V>> Enumerate()
        {
            if (this == Empty)
                yield break;

            var node = this;
            var height = node is Branch b ? b.BranchHeight : 1;
            var parents = new ImHashMap<K, V>[height];
            var parentCount = -1;
            while (height != 0 || parentCount != -1)
            {
                if (height != 0)
                {
                    parents[++parentCount] = node;
                    node = (node as Branch)?.LeftNode; // null is fine cause GetHeight handles the null
                }
                else
                {
                    node = parents[parentCount--];
                    yield return new KV<K, V>(node.Key, node.Value);

                    var conflicts = node.Conflicts;
                    if (conflicts != null)
                        for (var i = 0; i < conflicts.Length; i++)
                            yield return conflicts[i];

                    node = (node as Branch)?.RightNode;
                }

                height = node == null ? 0 : node is Branch br ? br.BranchHeight : 1;
            }
        }

        /// <summary>Removes or updates value for specified key, or does nothing if key is not found.
        /// Based on Eric Lippert http://blogs.msdn.com/b/ericlippert/archive/2008/01/21/immutability-in-c-part-nine-academic-plus-my-avl-tree-implementation.aspx </summary>
        /// <param name="key">Key to look for.</param> 
        /// <returns>New tree with removed or updated value.</returns>
        [MethodImpl((MethodImplOptions)256)]
        public ImHashMap<K, V> Remove(K key) =>
            Remove(key.GetHashCode(), key);

        /// <summary>Outputs key value pair</summary>
        public override string ToString() => IsEmpty ? "empty" : (Key + ":" + Value);

        #region Implementation

        private ImHashMap() { }

        /// Creates the leaf node
        protected ImHashMap(int hash, K key, V value)
        {
            Hash = hash;
            Key = key;
            Value = value;
        }

        /// It is fine
        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<V> update = null)
        {
            var branch = this as Branch;
            if (hash == Hash)
            {
                if (ReferenceEquals(Key, key) || Key.Equals(key))
                    return CreateNode(hash, key, update == null ? value : update(Value, value),
                        Conflicts, branch?.LeftNode, branch?.RightNode);
                return UpdateValueAndResolveConflicts(key, value, update, false);
            }

            return hash < Hash
                ? With(branch?.LeftNode?.AddOrUpdate(hash, key, value, update) ?? new ImHashMap<K, V>(hash, key, value), branch?.RightNode)
                : With(branch?.LeftNode, branch?.RightNode?.AddOrUpdate(hash, key, value, update) ?? new ImHashMap<K, V>(hash, key, value));
        }

        private ImHashMap<K, V> AddOrUpdate(int hash, K key, V value, Update<K, V> update, ref bool isUpdated, ref V oldValue)
        {
            var branch = this as Branch;
            if (hash == Hash)
            {
                if (ReferenceEquals(Key, key) || Key.Equals(key))
                {
                    if (update != null)
                        value = update(Key, Value, value);
                    if (ReferenceEquals(value, Value) || value?.Equals(Value) == true)
                        return this;

                    isUpdated = true;
                    oldValue = Value;
                    return branch == null
                        ? CreateLeaf(hash, key, value, Conflicts)
                        : CreateBranch(hash, key, value, Conflicts, branch.LeftNode, branch.RightNode, branch.BranchHeight);
                }

                if (Conflicts == null) // add only if updateOnly is false.
                    return branch == null
                        ? (ImHashMap<K, V>)new ConflictsLeaf(Hash, Key, Value, new[] { new KV<K, V>(key, value) })
                        : new ConflictsBranch(Hash, Key, Value, new[] { new KV<K, V>(key, value) },
                            branch.LeftNode, branch.RightNode, branch.BranchHeight);

                return UpdateValueAndResolveConflicts(key, value, update, false, ref isUpdated, ref oldValue);
            }

            if (hash < Hash)
            {
                if (branch == null)
                    return CreateBranch(Hash, Key, Value, Conflicts, new ImHashMap<K, V>(hash, key, value), null, 2);

                var oldLeft = branch.LeftNode;
                if (oldLeft == null)
                    return CreateBranch(Hash, Key, Value, Conflicts, new ImHashMap<K, V>(hash, key, value), branch.RightNode, branch.BranchHeight);

                var newLeft = oldLeft.AddOrUpdate(hash, key, value, update, ref isUpdated, ref oldValue);
                return newLeft == oldLeft ? this : Balance(Hash, Key, Value, Conflicts, newLeft, branch.RightNode);
            }
            else
            {
                if (branch == null)
                    return CreateBranch(Hash, Key, Value, Conflicts, null, new ImHashMap<K, V>(hash, key, value), 2);

                var oldRight = branch.RightNode;
                if (oldRight == null)
                    return CreateBranch(Hash, Key, Value, Conflicts, branch.LeftNode, new ImHashMap<K, V>(hash, key, value), branch.BranchHeight);

                var newRight = oldRight.AddOrUpdate(hash, key, value, update, ref isUpdated, ref oldValue);
                return newRight == oldRight ? this : Balance(Hash, Key, Value, Conflicts, branch.LeftNode, newRight);
            }
        }

        /// It is fine.
        public ImHashMap<K, V> Update(int hash, K key, V value, Update<V> update = null)
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (height == 0)
                return this;

            return hash == Hash
                ? (ReferenceEquals(Key, key) || Key.Equals(key)
                    ? CreateNode(hash, key, update == null ? value : update(Value, value), Conflicts,
                        branch?.LeftNode, branch?.RightNode, height)
                    : UpdateValueAndResolveConflicts(key, value, update, true))
                : (hash < Hash
                    ? With(branch?.LeftNode?.Update(hash, key, value, update), branch?.RightNode)
                    : With(branch?.LeftNode, branch?.RightNode?.Update(hash, key, value, update)));
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(K key, V value, Update<V> update, bool updateOnly)
        {
            var br = this as Branch;
            var height = br?.BranchHeight ?? 1;
            if (Conflicts == null) // add only if updateOnly is false.
            {
                if (updateOnly)
                    return this;
                return CreateConflictsNode(Hash, Key, Value, new[] { new KV<K, V>(key, value) },
                    br?.LeftNode, br?.RightNode, height);
            }

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly)
                    return this;
                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);

                return CreateConflictsNode(Hash, Key, Value, newConflicts, br?.LeftNode, br?.RightNode, height);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);
            conflicts[found] = new KV<K, V>(key, update == null ? value : update(Conflicts[found].Value, value));

            return CreateConflictsNode(Hash, Key, Value, conflicts, br?.LeftNode, br?.RightNode, height);
        }

        private ImHashMap<K, V> UpdateValueAndResolveConflicts(
            K key, V value, Update<K, V> update, bool updateOnly, ref bool isUpdated, ref V oldValue)
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (Conflicts == null) // add only if updateOnly is false.
            {
                if (updateOnly)
                    return this;
                return CreateConflictsNode(Hash, Key, Value, new[] { new KV<K, V>(key, value) },
                    branch?.LeftNode, branch?.RightNode, height);
            }

            var found = Conflicts.Length - 1;
            while (found >= 0 && !Equals(Conflicts[found].Key, Key)) --found;
            if (found == -1)
            {
                if (updateOnly)
                    return this;

                var newConflicts = new KV<K, V>[Conflicts.Length + 1];
                Array.Copy(Conflicts, 0, newConflicts, 0, Conflicts.Length);
                newConflicts[Conflicts.Length] = new KV<K, V>(key, value);

                return CreateConflictsNode(Hash, Key, Value, newConflicts, branch?.LeftNode, branch?.RightNode, height);
            }

            var conflicts = new KV<K, V>[Conflicts.Length];
            Array.Copy(Conflicts, 0, conflicts, 0, Conflicts.Length);

            if (update == null)
                conflicts[found] = new KV<K, V>(key, value);
            else
            {
                var conflict = conflicts[found];
                var newValue = update(conflict.Key, conflict.Value, value);
                if (ReferenceEquals(newValue, conflict.Value) || newValue?.Equals(conflict.Value) == true)
                    return this;

                isUpdated = true;
                oldValue = conflict.Value;
                conflicts[found] = new KV<K, V>(key, newValue);
            }

            return CreateConflictsNode(Hash, Key, Value, conflicts, branch?.LeftNode, branch?.RightNode, height);
        }

        /// It is fine.
        public V GetConflictedValueOrDefault(K key, V defaultValue)
        {
            if (Conflicts != null)
            {
                var conflicts = Conflicts;
                for (var i = conflicts.Length - 1; i >= 0; --i)
                    if (Equals(conflicts[i].Key, key))
                        return conflicts[i].Value;
            }
            return defaultValue;
        }

        /// It is fine.
        internal bool TryFindConflictedValue(K key, out V value)
        {
            if (Conflicts != null)
            {
                var conflicts = Conflicts;
                for (var i = conflicts.Length - 1; i >= 0; --i)
                    if (Equals(conflicts[i].Key, key))
                    {
                        value = conflicts[i].Value;
                        return true;
                    }
            }

            value = default(V);
            return false;
        }

        private ImHashMap<K, V> With(ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            var branch = this as Branch;
            if (left == branch?.LeftNode && right == branch?.RightNode)
                return this;
            return Balance(Hash, Key, Value, Conflicts, left, right);
        }

        private static ImHashMap<K, V> CreateNode(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            if (left == null && right == null)
                return conflicts == null
                    ? new ImHashMap<K, V>(hash, key, value)
                    : new ConflictsLeaf(hash, key, value, conflicts);
            return conflicts == null
                ? new Branch(hash, key, value, left, right)
                : new ConflictsBranch(hash, key, value, conflicts, left, right);
        }

        private static ImHashMap<K, V> CreateNode(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right, int height)
        {
            if (height == 1)
                return conflicts == null
                    ? new ImHashMap<K, V>(hash, key, value)
                    : new ConflictsLeaf(hash, key, value, conflicts);
            return conflicts == null
                ? new Branch(hash, key, value, left, right, height)
                : new ConflictsBranch(hash, key, value, conflicts, left, right, height);
        }

        [MethodImpl((MethodImplOptions)256)]
        private static ImHashMap<K, V> CreateNode(int hash, K key, V value,
            ImHashMap<K, V> left, ImHashMap<K, V> right, int height) =>
            height == 1
                ? new ImHashMap<K, V>(hash, key, value)
                : new Branch(hash, key, value, left, right, height);

        [MethodImpl((MethodImplOptions)256)]
        private static ImHashMap<K, V> CreateConflictsNode(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right, int height) =>
            height == 1
                ? (ImHashMap<K, V>)new ConflictsLeaf(hash, key, value, conflicts)
                : new ConflictsBranch(hash, key, value, conflicts, left, right, height);

        [MethodImpl((MethodImplOptions)256)]
        private static Branch CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right, int height) =>
            conflicts == null
                ? new Branch(hash, key, value, left, right, height)
                : new ConflictsBranch(hash, key, value, conflicts, left, right, height);

        [MethodImpl((MethodImplOptions)256)]
        private static Branch CreateBranch(int hash, K key, V value, KV<K, V>[] conflicts,
            ImHashMap<K, V> left, ImHashMap<K, V> right) =>
            conflicts == null
                ? new Branch(hash, key, value, left, right)
                : new ConflictsBranch(hash, key, value, conflicts, left, right);

        [MethodImpl((MethodImplOptions)256)]
        private static ImHashMap<K, V> CreateLeaf(int hash, K key, V value, KV<K, V>[] conflicts) =>
            conflicts == null
                ? new ImHashMap<K, V>(hash, key, value)
                : new ConflictsLeaf(hash, key, value, conflicts);

        private static ImHashMap<K, V> Balance(
            int hash, K key, V value, KV<K, V>[] conflicts, ImHashMap<K, V> left, ImHashMap<K, V> right)
        {
            var leftBranch = left as Branch;
            var leftHeight = left == null ? 0 : leftBranch?.BranchHeight ?? 1;

            var rightBranch = right as Branch;
            var rightHeight = right == null ? 0 : rightBranch?.BranchHeight ?? 1;

            var delta = leftHeight - rightHeight;
            if (delta > 1) // left is longer by 2, rotate left
            {
                // Also means that left is branch and not the leaf
                // ReSharper disable once PossibleNullReferenceException
                var leftLeft = leftBranch.LeftNode;
                var leftRight = leftBranch.RightNode;

                var llb = leftLeft as Branch;
                var lrb = leftRight as Branch;

                // The left is at least have height >= 2 and assuming that it is balanced, it should not have `null` left or right
                var leftLeftHeight = leftLeft == null ? 0 : llb?.BranchHeight ?? 1;
                var leftRightHeight = leftRight == null ? 0 : lrb?.BranchHeight ?? 1;
                if (leftRightHeight > leftLeftHeight)
                {
                    // double rotation:
                    //      5     =>     5     =>     4
                    //   2     6      4     6      2     5
                    // 1   4        2   3        1   3     6
                    //    3        1
                    if (lrb == null) // leftRight is a leaf node and the leftLeft is null
                    {
                        return CreateBranch(leftRight.Hash, leftRight.Key, leftRight.Value, leftRight.Conflicts,
                            CreateLeaf(left.Hash, left.Key, left.Value, left.Conflicts),
                            CreateNode(hash, key, value, conflicts, null, right, rightHeight + 1),
                            rightHeight + 2);
                    }

                    return CreateBranch(leftRight.Hash, leftRight.Key, leftRight.Value, leftRight.Conflicts,
                        CreateNode(left.Hash, left.Key, left.Value, left.Conflicts, leftLeft, lrb.LeftNode),
                        CreateNode(hash, key, value, conflicts, lrb.RightNode, right));
                }

                // one rotation:
                //      5     =>     2
                //   2     6      1     5
                // 1   4              4   6
                var newRightHeight = leftRightHeight > rightHeight ? leftRightHeight + 1 : rightHeight + 1;
                return CreateBranch(left.Hash, left.Key, left.Value, left.Conflicts,
                    leftLeft, CreateNode(hash, key, value, conflicts, leftRight, right, newRightHeight),
                    leftLeftHeight > newRightHeight ? leftLeftHeight + 1 : newRightHeight + 1);
            }

            if (delta < -1)
            {
                // ReSharper disable once PossibleNullReferenceException
                var rightLeft = rightBranch.LeftNode;
                var rightRight = rightBranch.RightNode;
                var rlb = rightLeft as Branch;
                var rrb = rightRight as Branch;

                var rightLeftHeight = rightLeft == null ? 0 : rlb?.BranchHeight ?? 1;
                var rightRightHeight = rightRight == null ? 0 : rrb?.BranchHeight ?? 1;
                if (rightLeftHeight > rightRightHeight)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    return CreateBranch(rightLeft.Hash, rightLeft.Key, rightLeft.Value, rightLeft.Conflicts,
                        CreateNode(hash, key, value, conflicts, left, rlb?.LeftNode),
                        CreateNode(right.Hash, right.Key, right.Value, right.Conflicts, rlb?.RightNode, rightRight));
                }

                var newLeftHeight = leftHeight > rightLeftHeight ? leftHeight + 1 : rightLeftHeight + 1;
                return CreateBranch(right.Hash, right.Key, right.Value, right.Conflicts,
                    CreateNode(hash, key, value, conflicts, left, rightLeft, newLeftHeight), rightRight,
                    newLeftHeight > rightRightHeight ? newLeftHeight + 1 : rightRightHeight + 1);
            }

            return CreateBranch(hash, key, value, conflicts, left, right, leftHeight > rightHeight ? leftHeight + 1 : rightHeight + 1);
        }

        internal ImHashMap<K, V> Remove(int hash, K key, bool ignoreKey = false)
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (height == 0)
                return this;

            ImHashMap<K, V> result;
            if (hash == Hash) // found node
            {
                if (ignoreKey || Equals(Key, key))
                {
                    if (!ignoreKey && Conflicts != null)
                        return ReplaceRemovedWithConflicted();

                    if (height == 1) // remove node
                        return Empty;

                    if (branch?.RightNode == null)
                        result = branch?.LeftNode;
                    else if (branch.LeftNode == null)
                        result = branch.RightNode;
                    else
                    {
                        // we have two children, so remove the next highest node and replace this node with it.
                        var successor = branch?.RightNode;
                        while ((successor as Branch)?.LeftNode != null)
                            successor = (successor as Branch)?.LeftNode;

                        result = CreateNode(
                            successor.Hash, successor.Key, successor.Value, successor.Conflicts,
                            (successor as Branch)?.LeftNode,
                            (successor as Branch)?.RightNode.Remove(successor.Hash, default(K), ignoreKey: true));
                    }
                }
                else if (Conflicts != null)
                    return TryRemoveConflicted(key);
                else
                    return this; // if key is not matching and no conflicts to lookup - just return
            }
            else if (hash < Hash)
            {
                result = Balance(Hash, Key, Value, Conflicts, branch?.LeftNode.Remove(hash, key, ignoreKey), branch?.RightNode);
            }
            else
            {
                result = Balance(Hash, Key, Value, Conflicts, branch?.LeftNode, branch?.RightNode.Remove(hash, key, ignoreKey));
            }

            return result;
        }

        private ImHashMap<K, V> TryRemoveConflicted(K key)
        {
            var index = Conflicts.Length - 1;
            while (index >= 0 && !Equals(Conflicts[index].Key, key)) --index;
            if (index == -1) // key is not found in conflicts - just return
                return this;

            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;
            if (Conflicts.Length == 1)
                return CreateNode(Hash, Key, Value, branch?.LeftNode, branch?.RightNode, height);

            var lessConflicts = new KV<K, V>[Conflicts.Length - 1];
            var newIndex = 0;
            for (var i = 0; i < Conflicts.Length; ++i)
                if (i != index) lessConflicts[newIndex++] = Conflicts[i];
            return CreateConflictsNode(Hash, Key, Value, lessConflicts, branch?.LeftNode, branch?.RightNode, height);
        }

        private ImHashMap<K, V> ReplaceRemovedWithConflicted()
        {
            var branch = this as Branch;
            var height = branch?.BranchHeight ?? 1;

            if (Conflicts.Length == 1)
                return CreateNode(Hash, Conflicts[0].Key, Conflicts[0].Value, branch?.LeftNode, branch?.RightNode, height);

            var lessConflicts = new KV<K, V>[Conflicts.Length - 1];
            Array.Copy(Conflicts, 1, lessConflicts, 0, lessConflicts.Length);
            return CreateConflictsNode(Hash, Conflicts[0].Key, Conflicts[0].Value, lessConflicts, branch?.LeftNode, branch?.RightNode, height);
        }

        #endregion
    }

    /// Map methods
    public static class ImHashMap
    {
        /// Looks for key in a tree and returns the key value if found, or <paramref name="defaultValue"/> otherwise.
        [MethodImpl((MethodImplOptions)256)]
        public static V GetValueOrDefault<K, V>(this ImHashMap<K, V> map, K key, V defaultValue = default(V))
        {
            var hash = key.GetHashCode();
            while (map != null)
            {
                if (map.Hash == hash)
                    return ReferenceEquals(key, map.Key) || key.Equals(map.Key)
                        ? map.Value
                        : map.GetConflictedValueOrDefault(key, defaultValue);

                map = hash < map.Hash
                    ? (map as ImHashMap<K, V>.Branch)?.LeftNode
                    : (map as ImHashMap<K, V>.Branch)?.RightNode;
            }

            return defaultValue;
        }

        /// Returns true if key is found and sets the value.
        [MethodImpl((MethodImplOptions)256)]
        public static bool TryFind<K, V>(this ImHashMap<K, V> map, K key, out V value)
        {
            var hash = key.GetHashCode();
            while (map != null)
            {
                if (map.Hash == hash)
                {
                    if (ReferenceEquals(key, map.Key) || key.Equals(map.Key))
                    {
                        value = map.Value;
                        return true;
                    }
                    return map.TryFindConflictedValue(key, out value);
                }

                map = hash < map.Hash
                    ? (map as ImHashMap<K, V>.Branch)?.LeftNode
                    : (map as ImHashMap<K, V>.Branch)?.RightNode;
            }

            value = default(V);
            return false;
        }
    }
}
