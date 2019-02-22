using System;

namespace RealisticUnitOfWork
{
    public class R
    {
        public Single1 Single1 { get; }
        public Single2 Single2 { get; }

        public Scoped1 Scoped1 { get; }
        public Scoped2 Scoped2 { get; }

        public Trans1 Trans1 { get; }
        public Trans2 Trans2 { get; }

        public ScopedFac1 ScopedFac1 { get; }
        public ScopedFac2 ScopedFac2 { get; }

        public SingleObj1 SingleObj1 { get; }
        public SingleObj2 SingleObj2 { get; }

        public R(
            Single1 single1,
            Single2 single2,
            Scoped1 scoped1,
            Scoped2 scoped2,
            Trans1 trans1,
            Trans2 trans2,
            ScopedFac1 scopedFac1,
            ScopedFac2 scopedFac2,
            SingleObj1 singleObj1,
            SingleObj2 singleObj2
        )
        {
            Single1 = single1;
            Single2 = single2;
            Scoped1 = scoped1;
            Scoped2 = scoped2;
            Trans1 = trans1;
            Trans2 = trans2;
            ScopedFac1 = scopedFac1;
            ScopedFac2 = scopedFac2;
            SingleObj1 = singleObj1;
            SingleObj2 = singleObj2;
        }
    }

    public class Single1
    {
        public Single12 Single12 { get; }
        public Single22 Single22 { get; }
        public SingleObj12 SingleObj12 { get; }
        public SingleObj22 SingleObj22 { get; }

        public Single1(
            Single12 single12,
            Single22 single22,
            SingleObj12 singleObj12,
            SingleObj22 singleObj22
            )
        {
            Single12 = single12;
            Single22 = single22;
            SingleObj12 = singleObj12;
            SingleObj22 = singleObj22;
        }
    }

    public class Single2
    {
        public Single12 Single12 { get; }
        public Single22 Single22 { get; }
        public SingleObj12 SingleObj12 { get; }
        public SingleObj22 SingleObj22 { get; }
        public Single2(
            Single12 single12,
            Single22 single22,
            SingleObj12 singleObj12,
            SingleObj22 singleObj22
        )
        {
            Single12 = single12;
            Single22 = single22;
            SingleObj12 = singleObj12;
            SingleObj22 = singleObj22;
        }
    }

    public class Scoped1
    {
        public Single12 Single12 { get; }
        public SingleObj12 SingleObj12 { get; }
        public Scoped12 Scoped12 { get; }
        public ScopedFac12 ScopedFac12 { get; }
        public Trans12 Trans12 { get; }

        public Single1 Single1 { get; }
        public SingleObj1 SingleObj1 { get; }

        public Scoped1(Single12 single12, SingleObj12 singleObj12, ScopedFac12 scopedFac12, Trans12 trans12, Single1 single1, SingleObj1 singleObj1, Scoped12 scoped12)
        {
            Single12 = single12;
            SingleObj12 = singleObj12;
            ScopedFac12 = scopedFac12;
            Trans12 = trans12;
            Single1 = single1;
            SingleObj1 = singleObj1;
            Scoped12 = scoped12;
        }
    }

    public class Scoped2
    {
        public Single22 Single22 { get; }
        public SingleObj22 SingleObj22 { get; }
        public Scoped22 Scoped22 { get; }
        public ScopedFac22 ScopedFac22 { get; }
        public Trans22 Trans22 { get; }

        public Single2 Single2 { get; }
        public SingleObj2 SingleObj2 { get; }

        public Scoped2(Single22 single22, SingleObj22 singleObj22, ScopedFac22 scopedFac22, Trans22 trans22, Single2 single2, SingleObj2 singleObj2, Scoped22 scoped22)
        {
            Single22 = single22;
            SingleObj22 = singleObj22;
            ScopedFac22 = scopedFac22;
            Trans22 = trans22;
            Single2 = single2;
            SingleObj2 = singleObj2;
            Scoped22 = scoped22;
        }
    }

    public class Scoped3 : IDisposable
    {
        public void Dispose() { }
    }

    public class Scoped4 : IDisposable
    {
        public void Dispose() { }
    }

    public class SingleObj1
    {
    }

    public class SingleObj2
    {
    }

    public class ScopedFac1
    {
        public Scoped1 Scoped1 { get; }
        public Scoped3 Scoped3 { get; }
        public Single1 Single1 { get; }
        public SingleObj1 SingleObj1 { get; }

        public ScopedFac1(Scoped1 scoped1, Scoped3 scoped3, Single1 single1, SingleObj1 singleObj1)
        {
            Scoped1 = scoped1;
            Scoped3 = scoped3;
            Single1 = single1;
            SingleObj1 = singleObj1;
        }
    }

    public class ScopedFac2
    {
        public Scoped2 Scoped2 { get; }
        public Scoped4 Scoped4 { get; }
        public Single2 Single2 { get; }
        public SingleObj2 SingleObj2 { get; }

        public ScopedFac2(Scoped2 scoped2, Scoped4 scoped4, Single2 single2, SingleObj2 singleObj2)
        {
            Scoped2 = scoped2;
            Scoped4 = scoped4;
            Single2 = single2;
            SingleObj2 = singleObj2;
        }
    }

    public class Trans1
    {
        public Single1 Single1 { get; }
        public SingleObj1 SingleObj1 { get; }

        public Trans13 Trans13 { get; }
        public Trans23 Trans23 { get; }

        public Single13 Single13 { get; }

        public Trans1(Trans13 trans13, Trans23 trans23, Single13 single13, Single1 single1, SingleObj1 singleObj1)
        {
            Trans13 = trans13;
            Trans23 = trans23;
            Single13 = single13;
            Single1 = single1;
            SingleObj1 = singleObj1;
        }
    }

    public class Trans2
    {
        public Single2 Single2 { get; }
        public SingleObj2 SingleObj2 { get; }

        public Trans13 Trans13 { get; }
        public Trans23 Trans23 { get; }

        public Single23 Single23 { get; }

        public Trans2(Trans13 trans13, Trans23 trans23, Single23 single23, Single2 single2, SingleObj2 singleObj2)
        {
            Trans13 = trans13;
            Trans23 = trans23;
            Single23 = single23;
            Single2 = single2;
            SingleObj2 = singleObj2;
        }
    }

    // ## Level 2

    public class Single12 : IDisposable
    {
        public Single14 Single14 { get; }
        public SingleObj14 SingleObj14 { get; }

        public Single12(Single14 single14, SingleObj14 singleObj14)
        {
            Single14 = single14;
            SingleObj14 = singleObj14;
        }

        public void Dispose() { }
    }

    public class Single22 : IDisposable
    {
        public Single24 Single24 { get; }
        public SingleObj24 SingleObj24 { get; }

        public Single22(Single24 single24, SingleObj24 singleObj24)
        {
            Single24 = single24;
            SingleObj24 = singleObj24;
        }

        public void Dispose() { }
    }

    public class SingleObj12
    {
    }

    public class SingleObj22
    {
    }

    public class Scoped12 : IDisposable
    {
        public Single13 Single13 { get; }
        public SingleObj13 SingleObj13 { get; }
        public Scoped13 Scoped13 { get; }
        public ScopedFac13 ScopedFac13 { get; }
        public Trans13 Trans13 { get; }

        public Single1 Single1 { get; }
        public SingleObj1 SingleObj1 { get; }

        public Scoped12(Single13 single13, SingleObj13 singleObj13, Scoped13 scoped13, ScopedFac13 scopedFac13, Trans13 trans13, Single1 single1, SingleObj1 singleObj1)
        {
            Single13 = single13;
            SingleObj13 = singleObj13;
            Scoped13 = scoped13;
            ScopedFac13 = scopedFac13;
            Trans13 = trans13;
            Single1 = single1;
            SingleObj1 = singleObj1;
        }

        public void Dispose() { }
    }

    public class Scoped22 : IDisposable
    {
        public Single23 Single23 { get; }
        public SingleObj23 SingleObj23 { get; }
        public Scoped23 Scoped23 { get; }
        public ScopedFac23 ScopedFac23 { get; }
        public Trans23 Trans23 { get; }

        public Single2 Single2 { get; }
        public SingleObj2 SingleObj2 { get; }

        public Scoped22(Single23 single23, SingleObj23 singleObj23, Scoped23 scoped23, ScopedFac23 scopedFac23, Trans23 trans23, Single2 single2, SingleObj2 singleObj2)
        {
            Single23 = single23;
            SingleObj23 = singleObj23;
            Scoped23 = scoped23;
            ScopedFac23 = scopedFac23;
            Trans23 = trans23;
            Single2 = single2;
            SingleObj2 = singleObj2;
        }

        public void Dispose() { }
    }

    public class ScopedFac12 : IDisposable
    {
        public Scoped13 Scoped3 { get; }
        public Single1 Single1 { get; }
        public SingleObj13 SingleObj1 { get; }

        public ScopedFac12(Scoped13 scoped3, Single1 single1, SingleObj13 singleObj1)
        {
            Scoped3 = scoped3;
            Single1 = single1;
            SingleObj1 = singleObj1;
        }

        public void Dispose() { }
    }

    public class ScopedFac22 : IDisposable
    {
        public Scoped23 Scoped23 { get; }
        public Single2 Single2 { get; }
        public SingleObj23 SingleObj23 { get; }

        public ScopedFac22(Scoped23 scoped23, Single2 single2, SingleObj23 singleObj23)
        {
            Scoped23 = scoped23;
            Single2 = single2;
            SingleObj23 = singleObj23;
        }

        public void Dispose() { }
    }

    public class Trans12
    {
        public Trans13 Trans13 { get; }
        public Single13 Single13 { get; }
        public SingleObj13 SingleObj13 { get; }

        public Trans12(Trans13 trans13, Single13 single13, SingleObj13 singleObj13)
        {
            Trans13 = trans13;
            Single13 = single13;
            SingleObj13 = singleObj13;
        }
    }

    public class Trans22
    {
        public Trans23 Trans23 { get; }
        public Single23 Single23 { get; }
        public SingleObj23 SingleObj23 { get; }

        public Trans22(Trans23 trans23, Single23 single23, SingleObj23 singleObj23)
        {
            Trans23 = trans23;
            Single23 = single23;
            SingleObj23 = singleObj23;
        }
    }

    // ## Level 3

    public class Trans13
    {
        public Single14 Single14 { get; }
        public Trans14 Trans14 { get; }

        public Trans13(Single14 single14, Trans14 trans14)
        {
            Single14 = single14;
            Trans14 = trans14;
        }
    }

    public class Trans23
    {
        public Single24 Single24 { get; }
        public Trans24 Trans24 { get; }

        public Trans23(Single24 single24, Trans24 trans24)
        {
            Single24 = single24;
            Trans24 = trans24;
        }
    }

    public class Single13
    {
        public Single14 Single14 { get; }

        public Single13(Single14 single14)
        {
            Single14 = single14;
        }
    }

    public class Single23
    {
        public Single14 Single24 { get; }

        public Single23(Single14 single24)
        {
            Single24 = single24;
        }
    }

    public class SingleObj13 { }
    public class SingleObj23 { }

    public class Scoped13
    {
        public Single1 Single1 { get; }
        public Scoped14 Scoped14 { get; }

        public Scoped13(Single1 single1, Scoped14 scoped14)
        {
            Single1 = single1;
            Scoped14 = scoped14;
        }
    }

    public class Scoped23 : IDisposable
    {
        public Single2 Single2 { get; }
        public Scoped24 Scoped24 { get; }

        public Scoped23(Single2 single2, Scoped24 scoped24)
        {
            Single2 = single2;
            Scoped24 = scoped24;
        }

        public void Dispose() { }
    }

    public class ScopedFac13
    {
        public Single1 Single1 { get; }
        public Scoped14 Scoped14 { get; }
        public ScopedFac14 ScopedFac14 { get; }

        public ScopedFac13(Single1 single1, Scoped14 scoped14, ScopedFac14 scopedFac14)
        {
            Single1 = single1;
            Scoped14 = scoped14;
            ScopedFac14 = scopedFac14;
        }
    }

    public class ScopedFac23 : IDisposable
    {
        public Single2 Single1 { get; }
        public Scoped24 Scoped14 { get; }
        public ScopedFac24 ScopedFac14 { get; }

        public ScopedFac23(Single2 single1, Scoped24 scoped14, ScopedFac24 scopedFac14)
        {
            Single1 = single1;
            Scoped14 = scoped14;
            ScopedFac14 = scopedFac14;
        }

        public void Dispose() { }
    }

    // ## Level 4

    public class Trans14 { }
    public class Trans24 { }

    public class Single14 { }
    public class Single24 { }

    public class SingleObj14 { }
    public class SingleObj24 { }

    public class Scoped14 : IDisposable
    {
        public void Dispose() { }
    }

    public class Scoped24 { }

    public class ScopedFac14 : IDisposable
    {
        public void Dispose() { }
    }

    public class ScopedFac24 { }

    // ## Dummy Population

    public class D1 { }
    public class D2 { }
    public class D3 { }
    public class D4 { }
    public class D5 { }
    public class D6 { }
    public class D7 { }
    public class D8 { }
    public class D9 { }
    public class D10 { }
    public class D11 { }
    public class D12 { }

    public class D13 { }
    public class D14 { }
    public class D15 { }
    public class D16 { }
    public class D17 { }
    public class D18 { }
    public class D19 { }
    public class D20 { }
}
