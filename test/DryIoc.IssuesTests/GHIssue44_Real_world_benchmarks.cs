using NUnit.Framework;
using static DryIoc.IssuesTests.Realistic_unit_of_work_slash_web_controller_example;

namespace DryIoc.IssuesTests
{
    [TestFixture]
    public class GHIssue44_Real_world_benchmarks
    {
        [Test]
        public void Test_realistic_unit_of_work_slash_web_controller_example()
        {
            var container = new Container();

            var x = container.PrepareDryIoc().Measure();

            Assert.IsInstanceOf<R>(x);
        }
    }

    public static class Realistic_unit_of_work_slash_web_controller_example
    {
        public static IContainer PrepareDryIoc(this IContainer container)
        {
            container.Register<R>(Reuse.Scoped);

            container.Register<Scoped1>(Reuse.Scoped);
            container.Register<Scoped2>(Reuse.Scoped);

            container.Register<Trans1>(Reuse.Transient);
            container.Register<Trans2>(Reuse.Transient);

            container.Register<Single1>(Reuse.Singleton);
            container.Register<Single2>(Reuse.Singleton);

            container.RegisterDelegate(r => new ScopedFac1(r.Resolve<Scoped1>(), r.Resolve<Scoped3>(), r.Resolve<Single1>(), r.Resolve<SingleObj1>()));
            container.RegisterDelegate(r => new ScopedFac2(r.Resolve<Scoped2>(), r.Resolve<Scoped4>(), r.Resolve<Single2>(), r.Resolve<SingleObj2>()));

            container.RegisterInstance(new SingleObj1());
            container.RegisterInstance(new SingleObj2());

            // level 2

            container.Register<Scoped3>(Reuse.Scoped);
            container.Register<Scoped4>(Reuse.Scoped);

            container.Register<Scoped12>(Reuse.Scoped);
            container.Register<Scoped22>(Reuse.Scoped);

            container.Register<Single12>(Reuse.Singleton);
            container.Register<Single22>(Reuse.Singleton);

            container.Register<Trans12>(Reuse.Transient);
            container.Register<Trans22>(Reuse.Transient);

            container.RegisterDelegate(r => new ScopedFac12());
            container.RegisterDelegate(r => new ScopedFac22());

            container.RegisterInstance(new SingleObj12());
            container.RegisterInstance(new SingleObj22());

            return container;
        }

        public static object Measure(this IContainer container)
        {
            using (var scope = container.OpenScope())
                return scope.Resolve<R>();
        }


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

        public class Scoped3
        {
        }

        public class Scoped4
        {
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
        }

        public class Trans2
        {
        }

        // ## Level 2

        public class Single22
        {
        }

        public class Single12
        {
        }

        public class SingleObj12
        {
        }

        public class SingleObj22
        {
        }

        public class Scoped12
        {
        }

        public class Scoped22
        {
        }


        public class ScopedFac12
        {
        }

        public class Trans12
        {
        }

        public class Trans22
        {
        }

        public class ScopedFac22
        {
        }
    }
}
